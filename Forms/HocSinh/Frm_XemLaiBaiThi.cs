using BLT_Quan_Ly_Hoc_Tap.DAL;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Forms
{
    public partial class Frm_XemLaiBaiThi : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly int _ketQuaID;
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private DataGridView dgvChiTiet;

        public Frm_XemLaiBaiThi(int ketQuaID)
        {
            _ketQuaID = ketQuaID;
            InitializeComponent();
            InitCustomControls();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            LoadChiTiet();
        }

        private void InitCustomControls()
        {
            this.dgvChiTiet = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).BeginInit();
            this.SuspendLayout();

            this.dgvChiTiet.Dock = DockStyle.Fill;
            this.dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTiet.ReadOnly = true;
            this.dgvChiTiet.RowHeadersVisible = false;
            this.dgvChiTiet.AllowUserToAddRows = false;
            this.dgvChiTiet.RowPrePaint += DgvChiTiet_RowPrePaint;

            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.dgvChiTiet);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "CHI TIẾT BÀI LÀM - KẾT QUẢ THI";

            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).EndInit();
            this.ResumeLayout(false);
        }

        private void LoadChiTiet()
        {
            DataTable dt = _ketQuaDAL.LayChiTietBaiLamTheoKetQua(_ketQuaID);
            dgvChiTiet.DataSource = dt;
        }

        private void DgvChiTiet_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvChiTiet.Rows.Count)
            {
                var row = dgvChiTiet.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    bool laDung = drv["LaDung"] != DBNull.Value && Convert.ToBoolean(drv["LaDung"]);
                    if (laDung)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); // Xanh nhạt: Đúng
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235); // Đỏ nhạt: Sai
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }
            }
        }

        private void Frm_XemLaiBaiThi_Load(object sender, EventArgs e)
        {
        }
    }
}
