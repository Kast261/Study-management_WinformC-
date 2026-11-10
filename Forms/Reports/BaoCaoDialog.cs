using System;
using System.Data;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Helpers;

namespace BLT_Quan_Ly_Hoc_Tap.Forms
{
    /// <summary>
    /// Crystal is an optional Windows runtime. No SAP binaries or fake .rpt are
    /// bundled. The Excel path remains usable without SAP installed.
    /// </summary>
    public sealed class BaoCaoDialog : Form
    {
        public BaoCaoDialog(DataTable results, string tenHocVien, string nguoiLap)
        {
            Text = "Báo cáo kết quả học tập";
            Width = 470; Height = 180;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false; MinimizeBox = false;
            StartPosition = FormStartPosition.CenterParent;
            var caption = new Label { Text = "Chọn cách xem hoặc xuất bảng điểm", AutoSize = true, Left = 24, Top = 22 };
            var crystal = new Button { Text = "Xem / in Crystal Reports", Left = 24, Top = 62, Width = 220, Height = 35 };
            var excel = new Button { Text = "Xuất Excel", Left = 260, Top = 62, Width = 160, Height = 35 };
            excel.Click += (s, e) => ExcelExportHelper.ExportBangDiemHocSinh(results,
                tenHocVien, null, null, null, nguoiLap);
            crystal.Click += (s, e) => XemCrystal(results, tenHocVien, nguoiLap);
            Controls.AddRange(new Control[] { caption, crystal, excel });
        }

        private void XemCrystal(DataTable results, string tenHocVien, string nguoiLap)
        {
            object report = null;
            Form window = null;
            Control viewer = null;
            DataSet data = null;
            try
            {
                if (results.Rows.Count == 0)
                {
                    MessageBox.Show(this, "Chưa có kết quả để lập báo cáo.", "Báo cáo");
                    return;
                }
                Type reportType = CrystalType("CrystalDecisions.CrystalReports.Engine.ReportDocument", "CrystalDecisions.CrystalReports.Engine");
                Type viewerType = CrystalType("CrystalDecisions.Windows.Forms.CrystalReportViewer", "CrystalDecisions.Windows.Forms");
                string path = Path.Combine(Application.StartupPath, "Reports", "BangDiem.rpt");
                if (!File.Exists(path))
                {
                    using (var select = new OpenFileDialog { Title = "Chọn mẫu bảng điểm Crystal Reports", Filter = "Crystal Reports (*.rpt)|*.rpt", CheckFileExists = true })
                    {
                        if (select.ShowDialog(this) != DialogResult.OK) return;
                        path = select.FileName;
                    }
                }
                IBaoCaoService service = new BaoCaoService();
                data = service.TaoDuLieuBangDiem(results, tenHocVien, nguoiLap);
                report = Activator.CreateInstance(reportType);
                reportType.GetMethod("Load", new[] { typeof(string) }).Invoke(report, new object[] { path });
                reportType.GetMethod("SetDataSource", new[] { typeof(DataSet) }).Invoke(report, new object[] { data });
                viewer = (Control)Activator.CreateInstance(viewerType);
                viewer.Dock = DockStyle.Fill;
                viewerType.GetProperty("ReportSource").SetValue(viewer, report, null);
                window = new Form { Text = "Bảng điểm — " + tenHocVien, Width = 1100, Height = 750, StartPosition = FormStartPosition.CenterParent };
                window.Controls.Add(viewer);
                window.ShowDialog(this);
            }
            catch (Exception ex)
            {
                Exception cause = ex is TargetInvocationException && ex.InnerException != null ? ex.InnerException : ex;
                MessageBox.Show(this, "Không thể mở báo cáo Crystal. Máy cần SAP Crystal Reports runtime đúng kiến trúc và mẫu BangDiem.rpt theo Reports/README.md.\n\n" + cause.Message,
                    "Báo cáo chưa sẵn sàng", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            finally
            {
                // Dispose viewer before releasing its report and pushed dataset.
                window?.Dispose();
                viewer?.Dispose();
                try { (report as IDisposable)?.Dispose(); }
                finally { data?.Dispose(); }
            }
        }

        private static Type CrystalType(string type, string assembly)
        {
            // SAP CR for VS uses these assembly versions across service packs.
            foreach (string version in new[] { "13.0.4000.0", "13.0.3500.0", "13.0.2000.0" })
            {
                Type resolved = Type.GetType(type + ", " + assembly + ", Version=" + version + ", Culture=neutral, PublicKeyToken=692fbea5521e1304", false);
                if (resolved != null) return resolved;
            }
            throw new FileNotFoundException("Chưa tìm thấy SAP Crystal Reports runtime tương thích.");
        }
    }
}
