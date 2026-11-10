using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        #region Dữ liệu & Trạng thái

        private readonly ILopHocService _lopService = new LopHocService();
        private readonly IDeThiService _deThiService = new DeThiService();
        private readonly IKetQuaService _ketQuaService = new KetQuaService();
        private readonly IHocSinhService _hocSinhService = new HocSinhService();
        private readonly ITaiKhoanService _taiKhoanService = new TaiKhoanService();
        private int _selectedLopDangKyID = 0;
        private int _selectedLopDangHocID = 0;
        private int _selectedDeThiID = 0;
        // Lưu trữ danh sách đề thi gốc để hỗ trợ việc lọc bằng ComboBox
        private DataTable _dtDeThi;
        private DataTable _dtBaiThiDangMo;
        private DataTable _lopTrangChu;
        private bool _dangTaiLopLocKetQua;
        private bool _daTaiHocVien;
        private bool _dangLamMoiTrang;
        private bool _dangXuLyDangKyLop;

        #endregion

        #region 1. Khởi tạo & Đăng ký sự kiện

        public HocVien()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                this.DrawerTabControl = tabHocVien;
                HienThiTheHoSo(new HocSinh { HoTen = Session.HoTen ?? Session.TenDangNhap });
            }
            btnTCHV_BatDauLamBai.Enabled = btnTCHV_LamBai.Enabled = false;
            lblTCHV_NgayThangHT.Text = DateTime.Today.ToString("dddd, dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
            DataGridViewPager.AttachAll(this, KhiDoiTrang);

            CapNhatTrangBaiThi();
            dgvBaiThiDangMo_SelectionChanged(null, EventArgs.Empty);
            dgvHVDeThi_SelectionChanged(null, EventArgs.Empty);
            dgvHVKetQua_SelectionChanged(null, EventArgs.Empty);
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
        }

        private void HocVien_Load(object sender, EventArgs e)
        {
            if (!LoadThongTinHocVien()) return;
            try
            {
                LoadLopHoc();
                LoadDanhSachDeThi();
                LoadLichSuThi();
                LoadTabLopHocCuaToi();
                _daTaiHocVien = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin học viên: " + ex.Message, "Thông báo");
            }
        }

        private void KhiDoiTrang(DataGridView grid, PageChangeReason reason)
        {
            if (grid == dgvLopDangKi)
            {
                _selectedLopDangKyID = 0;
                lblHV_DangChon.Text = "Đang chọn: chưa chọn lớp";
            }
            if (grid == dgvLopThamGia)
            {
                _selectedLopDangHocID = 0;
                lblHV_DangChon.Text = "Đang chọn: chưa chọn lớp";
            }
            if (grid == dgvHVDeThi) _selectedDeThiID = 0;
            if (grid == dgvBaiThiDangMo) CapNhatTrangBaiThi();
        }

        #endregion

        #region 2. Tab Trang Chủ

        private void LoadLopHoc()
        {
            lblTCHV_NgayThangHT.Text = DateTime.Today.ToString("dddd, dd/MM/yyyy", CultureInfo.GetCultureInfo("vi-VN"));
            try
            {
                _lopTrangChu = _lopService.LayTheoHocSinh(Session.NguoiDungID);
                _lopTrangChu.Columns.Add("ThoiGianHoc", typeof(string));
                foreach (DataRow row in _lopTrangChu.Rows)
                {
                    string start = row.IsNull("NgayBatDau") ? "" : Convert.ToDateTime(row["NgayBatDau"]).ToString("dd/MM/yyyy");
                    string end = row.IsNull("NgayKetThuc") ? "" : Convert.ToDateTime(row["NgayKetThuc"]).ToString("dd/MM/yyyy");
                    row["ThoiGianHoc"] = start.Length > 0 && end.Length > 0 ? start + " – " + end : start + end;
                }
                dgvLopDangHoc.DataSource = _lopTrangChu;
                lblTCHV_LopDH.Text = $"Lớp đang học\n{_lopTrangChu.Rows.Count:N0}";
            }
            catch (Exception ex)
            {
                _lopTrangChu = null;
                dgvLopDangHoc.DataSource = null;
                lblTCHV_LopDH.Text = "Lớp đang học\n--";
                MessageBox.Show("Không thể tải lớp đang học: " + ex.Message, "Lỗi");
            }
        }

        private void CapNhatThongKeTrangChu()
        {
            if (_dtBaiThiDangMo == null) return;
            try
            {
                using (DataTable results = _ketQuaService.LayLichSuTheoHocSinh(Session.NguoiDungID))
                {
                    var summary = HomeDashboardSummary.ForStudent(_lopTrangChu, _dtBaiThiDangMo, results);
                    lblTCHV_DaHoanThanh.Text = $"Đã hoàn thành\n{summary.CompletedExamCount:N0}";
                    lblTCHV_DiemTB.Text = "Điểm trung bình\n" + (summary.AverageScore.HasValue
                        ? summary.AverageScore.Value.ToString("0.0", CultureInfo.GetCultureInfo("vi-VN")) + " / 10" : "--");
                    prcTCHV_DaHoanThanh.Value = summary.CompletionPercent;
                    lblTCHV_PhanTram.Text = summary.CompletionPercent + "%";
                    materialLabel25.Text = $"{summary.CompletedExamCount:N0} bài đã hoàn thành · {summary.OpenExamCount:N0} bài đang mở";
                }
            }
            catch (Exception ex)
            {
                lblTCHV_DaHoanThanh.Text = "Đã hoàn thành\n--";
                lblTCHV_DiemTB.Text = "Điểm trung bình\n--";
                lblTCHV_PhanTram.Text = "--";
                prcTCHV_DaHoanThanh.Value = 0;
                materialLabel25.Text = "Chưa tải được thống kê kết quả.";
                MessageBox.Show("Không thể tải thống kê học tập: " + ex.Message, "Lỗi");
            }
        }

        private void CapNhatTrangBaiThi()
        {
            DataGridViewPager.TryGetPageInfo(dgvBaiThiDangMo, out int page, out int pages);
            lblTCHV_SoTrang.Text = $"Trang {page} / {pages}";
        }

        private void dgvBaiThiDangMo_SelectionChanged(object sender, EventArgs e)
        {
            var row = dgvBaiThiDangMo.CurrentRow?.DataBoundItem as DataRowView;
            lblTCHV_DangChon.Text = "Đang chọn: " + (row == null ? "chưa chọn bài thi" : row["TenDeThi"].ToString());
            btnTCHV_LamBai.Enabled = row != null;
        }

        private void lblTCHV_XemTatCa_Click(object sender, EventArgs e)
        {
            tabHocVien.SelectedTab = pageHV_BaiThi;
        }

        private void lblTCHV_XemLopDangHoc_Click(object sender, EventArgs e)
        {
            tabHocVien.SelectedTab = pageHV_LopHoc;
        }

        private void btnTCHV_LamBai_Click(object sender, EventArgs e)
        {
            if (_dtBaiThiDangMo == null || _dtBaiThiDangMo.Rows.Count == 0) return;
            var selected = dgvBaiThiDangMo.CurrentRow?.DataBoundItem as DataRowView;
            DataRow row = selected?.Row ?? _dtBaiThiDangMo.Rows[0];
            BatDauThi(Convert.ToInt32(row["DeThiID"]));
        }

        private void dgvBaiThiDangMo_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var selected = dgvBaiThiDangMo.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (selected != null) BatDauThi(Convert.ToInt32(selected["DeThiID"]));
        }

        #endregion

        #region 3. Tab Lớp Học & Đăng Ký

        private void LoadTabLopHocCuaToi()
        {
            try
            {
                DataTable dtLopMo = _lopService.LayLopChuaDangKy(Session.NguoiDungID);
                if (dgvLopDangKi != null) dgvLopDangKi.DataSource = dtLopMo;

                DataTable dtLopDangHoc = _lopService.LayLopHocVienDangHocVaChoDuyet(Session.NguoiDungID);
                if (dgvLopThamGia != null) dgvLopThamGia.DataSource = dtLopDangHoc;

                _selectedLopDangKyID = 0;
                _selectedLopDangHocID = 0;
                lblHV_DangChon.Text = "Đang chọn: chưa chọn lớp";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp học: " + ex.Message, "Thông báo");
            }
        }

        private void dgvLopDangKi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLopDangKi.Rows.Count)
            {
                var row = dgvLopDangKi.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    _selectedLopDangKyID = Convert.ToInt32(drv["LopHocID"]);
                    lblHV_DangChon.Text = "Đang chọn: " + drv["TenLop"];
                }
            }
        }

        private void dgvLopThamGia_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLopThamGia.Rows.Count)
            {
                var row = dgvLopThamGia.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    _selectedLopDangHocID = Convert.ToInt32(drv["LopHocID"]);
                    lblHV_DangChon.Text = "Đang chọn: " + drv["TenLop"];
                }
            }
        }

        private void btnDangKyLop_Click(object sender, EventArgs e)
        {
            if (_dangXuLyDangKyLop) return;
            var selected = dgvLopDangKi.CurrentRow?.DataBoundItem as DataRowView;
            if (selected == null || dgvLopDangKi.SelectedRows.Count != 1)
            {
                MessageBox.Show("Vui lòng chọn lớp học muốn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // MessageBox đóng có thể kích hoạt OnActivated và tải lại bảng.
            // Giữ mã lớp/học viên trước khi mở hộp thoại, không đọc lại cache sau đó.
            int lopHocID = Convert.ToInt32(selected["LopHocID"]);
            int hocSinhID = Session.NguoiDungID;
            string tenLop = ExamSelection.ClassDisplayName(selected.Row);
            _dangXuLyDangKyLop = true;
            try
            {
                if (MessageBox.Show(this, $"Đăng ký {tenLop}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                if (_lopService.DangKyLop(lopHocID, hocSinhID, out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTabLopHocCuaToi();
                    LoadLopHoc();
                    LoadDanhSachDeThi();
                }
                else
                {
                    MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể đăng ký lớp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _dangXuLyDangKyLop = false; }
        }

        private void btnHV_HuyDK_Click(object sender, EventArgs e)
        {
            if (_dangXuLyDangKyLop) return;
            var selected = dgvLopThamGia.CurrentRow?.DataBoundItem as DataRowView;
            if (selected == null || dgvLopThamGia.SelectedRows.Count != 1)
            {
                MessageBox.Show("Vui lòng chọn lớp học muốn hủy đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- LOGIC MỚI THÊM: Chặn hủy đăng ký nếu lớp "Đã duyệt" ---
            if (dgvLopThamGia.CurrentRow != null && dgvLopThamGia.CurrentRow.DataBoundItem is DataRowView drv)
            {
                // Lấy tên cột trạng thái (có thể là TrangThaiDangKy hoặc TrangThai tùy vào câu SQL ở DAL)
                string trangThai = "";
                if (drv.Row.Table.Columns.Contains("TrangThaiDangKy"))
                    trangThai = drv["TrangThaiDangKy"].ToString();
                else if (drv.Row.Table.Columns.Contains("TrangThai"))
                    trangThai = drv["TrangThai"].ToString();

                // Nếu trạng thái là Đã duyệt thì chặn lại
                if (trangThai.Trim().Equals("Đã duyệt", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Lớp học này đã được giáo viên phê duyệt. Bạn không thể hủy đăng ký nữa!", "Từ chối", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return; // Dừng luôn, không chạy xuống dưới
                }
            }
            // -------------------------------------------------------------

            int lopHocID = Convert.ToInt32(selected["LopHocID"]);
            int hocSinhID = Session.NguoiDungID;
            string tenLop = ExamSelection.ClassDisplayName(selected.Row);
            _dangXuLyDangKyLop = true;
            try
            {
                if (MessageBox.Show(this, $"Hủy đăng ký {tenLop}?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;
                if (_lopService.HuyDangKyLop(lopHocID, hocSinhID, out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Tải lại các bảng dữ liệu sau khi hủy
                    LoadTabLopHocCuaToi();
                    LoadLopHoc();
                    LoadDanhSachDeThi();
                }
                else
                {
                    MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể hủy đăng ký lớp: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally { _dangXuLyDangKyLop = false; }
        }

        #endregion

        #region 4. Tab Bài Thi & Làm Bài

        private void LoadDanhSachDeThi()
        {
            try
            {
                string monHocDaLoc = materialComboBox1?.SelectedItem?.ToString();
                int deDaChon = _selectedDeThiID;
                var baiTrangChu = dgvBaiThiDangMo.CurrentRow?.DataBoundItem as DataRowView;
                int deTrangChu = baiTrangChu == null ? 0 : Convert.ToInt32(baiTrangChu["DeThiID"]);
                DataTable exams = _deThiService.LayDeThiChoHocSinh(Session.NguoiDungID);
                DataTable openExams = _deThiService.LayBaiThiDangMoChoHocSinh(Session.NguoiDungID);
                _selectedDeThiID = 0;
                _dtDeThi = exams;
                _dtBaiThiDangMo = openExams;
                dgvHVDeThi.DataSource = _dtDeThi;
                lblHV_TongBaiThi.Text = $"Tổng bài thi: {_dtDeThi.Rows.Count:N0}";
                // Nguồn riêng để lọc môn học ở tab Bài Thi không làm mất bài trên Trang chủ.
                dgvBaiThiDangMo.DataSource = _dtBaiThiDangMo;
                CapNhatTrangBaiThi();
                lblTCHV_BaiThiDangMo.Text = $"Bài thi đang mở\n{_dtBaiThiDangMo.Rows.Count:N0}";
                CapNhatThongKeTrangChu();
                bool coBaiThi = _dtBaiThiDangMo.Rows.Count > 0;
                btnTCHV_BatDauLamBai.Enabled = coBaiThi;
                dgvBaiThiDangMo_SelectionChanged(null, EventArgs.Empty);
                lblTCHV_TatCaBaiThi.Text = coBaiThi
                    ? $"Bạn có {_dtBaiThiDangMo.Rows.Count:N0} bài thi đang mở chưa làm."
                    : "Bạn chưa có bài thi đang mở để làm.";

                if (materialComboBox1 != null)
                {
                    materialComboBox1.Items.Clear();
                    materialComboBox1.Items.Add("-- Tất cả môn học --");

                    if (_dtDeThi != null)
                    {
                        var dsMonHoc = new HashSet<string>();
                        foreach (DataRow row in _dtDeThi.Rows)
                        {
                            string mon = row["MonHoc"].ToString();
                            if (!string.IsNullOrEmpty(mon)) dsMonHoc.Add(mon);
                        }

                        foreach (var mon in dsMonHoc)
                        {
                            materialComboBox1.Items.Add(mon);
                        }
                    }
                    if (!string.IsNullOrEmpty(monHocDaLoc) && materialComboBox1.Items.Contains(monHocDaLoc))
                        materialComboBox1.SelectedItem = monHocDaLoc;
                    else
                        materialComboBox1.SelectedIndex = 0;
                }
                if (deDaChon > 0 && DataGridViewPager.ShowItem(dgvHVDeThi, "DeThiID", deDaChon))
                    _selectedDeThiID = deDaChon;
                if (deTrangChu > 0) DataGridViewPager.ShowItem(dgvBaiThiDangMo, "DeThiID", deTrangChu);
            }
            catch (Exception ex)
            {
                _selectedDeThiID = 0;
                _dtDeThi = _dtBaiThiDangMo = null;
                dgvHVDeThi.DataSource = null;
                dgvBaiThiDangMo.DataSource = null;
                btnTCHV_BatDauLamBai.Enabled = btnTCHV_LamBai.Enabled = false;
                lblTCHV_BaiThiDangMo.Text = "Bài thi đang mở\n--";
                lblTCHV_PhanTram.Text = "--";
                prcTCHV_DaHoanThanh.Value = 0;
                materialLabel25.Text = "Chưa tải được danh sách bài thi để tính tiến độ.";
                lblTCHV_TatCaBaiThi.Text = "Không thể tải danh sách bài thi.";
                MessageBox.Show("Không thể tải danh sách bài thi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void materialComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtDeThi == null) return;

            string selectedMonHoc = materialComboBox1.SelectedItem?.ToString();

            if (string.IsNullOrEmpty(selectedMonHoc) || selectedMonHoc == "-- Tất cả môn học --")
            {
                _dtDeThi.DefaultView.RowFilter = string.Empty;
            }
            else
            {
                selectedMonHoc = selectedMonHoc.Replace("'", "''");
                _dtDeThi.DefaultView.RowFilter = $"MonHoc = '{selectedMonHoc}'";
            }
        }

        private void dgvHVDeThi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvHVDeThi.Rows.Count)
            {
                var row = dgvHVDeThi.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("DeThiID"))
                {
                    _selectedDeThiID = Convert.ToInt32(drv["DeThiID"]);
                }
                else if (dgvHVDeThi.Columns.Contains("DeThiID") && row.Cells["DeThiID"].Value != null)
                {
                    _selectedDeThiID = Convert.ToInt32(row.Cells["DeThiID"].Value);
                }
            }
        }

        private void dgvHVDeThi_SelectionChanged(object sender, EventArgs e)
        {
            var row = dgvHVDeThi.CurrentRow?.DataBoundItem as DataRowView;
            _selectedDeThiID = row == null ? 0 : Convert.ToInt32(row["DeThiID"]);
            lblHVDT_ChonDT.Text = "Đang chọn: " + (row == null ? "chưa chọn đề thi" : row["TenDeThi"].ToString());
            lblHVDT_ThongDT.Text = row == null ? "Chọn một đề thi để xem thông tin."
                : $"{row["MonHoc"]} · {row["SoCau"]} câu · {row["ThoiGian"]} phút";
            btnHVDT_LamBai.Enabled = row != null;
        }

        private void btnHVDT_LamBai_Click(object sender, EventArgs e)
        {
            if (_selectedDeThiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 đề thi từ danh sách bên trên trước khi bắt đầu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra trực tiếp Trạng Thái của dòng đang chọn
            if (dgvHVDeThi.CurrentRow != null && dgvHVDeThi.CurrentRow.DataBoundItem is DataRowView drv)
            {
                string trangThai = drv["TrangThai"].ToString();

                if (trangThai == "Đã làm")
                {
                    MessageBox.Show("Bạn đã hoàn thành bài thi này rồi! Bạn có thể xem lại điểm ở cột bên cạnh hoặc trong tab Lịch sử.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (trangThai == "Hết hạn")
                {
                    MessageBox.Show("Bài thi này đã quá hạn nộp, bạn không thể làm bài được nữa!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            BatDauThi(_selectedDeThiID);
        }

        private void BatDauThi(int deThiID)
        {
            try
            {
                if (!_deThiService.CoTheBatDauThi(Session.NguoiDungID, deThiID))
                {
                    MessageBox.Show("Bài thi không còn khả dụng. Bài có thể đã đóng, hết hạn, đã nộp hoặc bạn không còn thuộc lớp được giao.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDanhSachDeThi();
                    return;
                }

                using (var frm = new Frm_KiemTra(deThiID))
                {
                    frm.ShowDialog(this);
                }

                LoadLichSuThi();
                LoadDanhSachDeThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể mở bài thi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 5. Tab Kết Quả Thi & Xuất Báo Cáo

        private void LoadLichSuThi()
        {
            int selectedClass = cmbKQHV_BaiThi.SelectedValue is int value ? value : 0;
            _dangTaiLopLocKetQua = true;
            try
            {
                using (DataTable classes = _lopService.LayTheoHocSinh(Session.NguoiDungID))
                {
                    DataTable options = classes.DefaultView.ToTable(true, "LopHocID", "TenLop");
                    DataRow all = options.NewRow();
                    all["LopHocID"] = 0;
                    all["TenLop"] = "Tất cả lớp đang học";
                    options.Rows.InsertAt(all, 0);
                    bool exists = false;
                    foreach (DataRow row in options.Rows)
                        if (Convert.ToInt32(row["LopHocID"]) == selectedClass) exists = true;
                    cmbKQHV_BaiThi.DataSource = null;
                    // Reapply binding members after detaching the previous source.
                    // PostgreSQL may return lowercase column names.
                    cmbKQHV_BaiThi.DisplayMember = options.Columns["TenLop"].ColumnName;
                    cmbKQHV_BaiThi.ValueMember = options.Columns["LopHocID"].ColumnName;
                    cmbKQHV_BaiThi.DataSource = options;
                    cmbKQHV_BaiThi.SelectedValue = exists ? selectedClass : 0;
                    cmbKQHV_BaiThi.Enabled = options.Rows.Count > 1;
                }
            }
            catch (Exception ex)
            {
                dgvHVKetQua.DataSource = null;
                btnHVKQ_XuatFile.Enabled = false;
                btnHVKQ_BaoCao.Enabled = false;
                lblHVKQ_TongBaiThi.Text = "Không tải được danh sách lớp đang học.";
                MessageBox.Show("Không tải được lớp để lọc kết quả: " + ex.Message, "Lỗi");
                return;
            }
            finally { _dangTaiLopLocKetQua = false; }
            HienThiKetQuaTheoLop();
        }

        private void cmbKQHV_BaiThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dangTaiLopLocKetQua || !(cmbKQHV_BaiThi.SelectedValue is int)) return;
            HienThiKetQuaTheoLop();
        }

        private void HienThiKetQuaTheoLop()
        {
            try
            {
                int lopID = cmbKQHV_BaiThi.SelectedValue is int value ? value : 0;
                DataTable results = _ketQuaService.LayKetQuaTheoLopDangHoc(Session.NguoiDungID, lopID);
                dgvHVKetQua.DataSource = results;
                btnHVKQ_XuatFile.Enabled = results.Rows.Count > 0;
                btnHVKQ_BaoCao.Enabled = results.Rows.Count > 0;
                dgvHVKetQua_SelectionChanged(null, EventArgs.Empty);
                lblHVKQ_TongBaiThi.Text = !cmbKQHV_BaiThi.Enabled ? "Bạn chưa có lớp đang học."
                    : results.Rows.Count == 0 ? "Chưa có bài thi đã nộp trong lớp đang chọn."
                    : $"{results.Rows.Count:N0} kết quả · {cmbKQHV_BaiThi.Text}";
            }
            catch (Exception ex)
            {
                dgvHVKetQua.DataSource = null;
                btnHVKQ_XuatFile.Enabled = false;
                btnHVKQ_BaoCao.Enabled = false;
                lblHVKQ_TongBaiThi.Text = "Không tải được kết quả. Hãy mở lại tab để thử lại.";
                MessageBox.Show("Không tải được kết quả bài thi: " + ex.Message, "Lỗi");
            }
        }

        private void dgvHVKetQua_SelectionChanged(object sender, EventArgs e)
        {
            var row = dgvHVKetQua.CurrentRow?.DataBoundItem as DataRowView;
            lblHVKQ_ChonDT.Text = "Đang chọn: " + (row == null ? "chưa chọn bài thi" : row["TenDeThi"].ToString());
            lblHVKQ_TTDT.Text = row == null ? "Thông tin đề thi" : row["MonHoc"].ToString();
            lblKQHV_Diem.Text = row == null || row["Diem"] == DBNull.Value
                ? "Chưa có điểm" : Convert.ToDecimal(row["Diem"]).ToString("0.00") + "/10";
            btnHVKQ_XuatFile.Enabled = row != null;
        }

        private void dgvHVKetQua_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !(dgvHVKetQua.Rows[e.RowIndex].DataBoundItem is DataRowView row)) return;
            try
            {
                using (var form = new Forms.Frm_XemLaiBaiThi(Convert.ToInt32(row["KetQuaID"])))
                    form.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xem chi tiết bài làm: " + ex.Message, "Lỗi");
            }
        }

        private void btnHVKQ_XuatFile_Click(object sender, EventArgs e)
        {
            if (dgvHVKetQua.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn bài thi đã làm trong bảng lịch sử!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var row = dgvHVKetQua.CurrentRow;
                int ketQuaID = 0;

                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("KetQuaID"))
                {
                    ketQuaID = Convert.ToInt32(drv["KetQuaID"]);
                }
                else if (dgvHVKetQua.Columns.Contains("KetQuaID") && row.Cells["KetQuaID"].Value != null)
                {
                    ketQuaID = Convert.ToInt32(row.Cells["KetQuaID"].Value);
                }

                if (ketQuaID <= 0)
                {
                    MessageBox.Show("Không tìm thấy dữ liệu kết quả bài thi!", "Thông báo");
                    return;
                }

                DataTable dtChiTiet = _ketQuaService.LayChiTietBaiLamTheoKetQua(ketQuaID);
                if (dtChiTiet == null || dtChiTiet.Rows.Count == 0)
                {
                    MessageBox.Show("Không có dữ liệu chi tiết bài làm!", "Thông báo");
                    return;
                }

                string tenDeThi = "";
                string diem = "";
                string thoiGianNop = "";

                if (row.DataBoundItem is DataRowView drvRow)
                {
                    if (drvRow.Row.Table.Columns.Contains("TenDeThi")) tenDeThi = drvRow["TenDeThi"]?.ToString() ?? "";
                    if (drvRow.Row.Table.Columns.Contains("Diem")) diem = drvRow["Diem"]?.ToString() ?? "";
                    if (drvRow.Row.Table.Columns.Contains("ThoiGianNop")) thoiGianNop = drvRow["ThoiGianNop"]?.ToString() ?? "";
                }

                XuatChiTietBaiLamRaExcel(dtChiTiet, tenDeThi, diem, thoiGianNop);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file Excel: " + ex.Message, "Lỗi");
            }
        }

        private void XuatChiTietBaiLamRaExcel(DataTable dt, string tenDeThi, string diem, string thoiGianNop)
        {
            var columns = new List<ExcelExportHelper.ColumnDefinition>
            {
                new ExcelExportHelper.ColumnDefinition { FieldName = "ThuTu", HeaderText = "STT", Width = 45, Alignment = "Center", IsNumeric = true },
                new ExcelExportHelper.ColumnDefinition { FieldName = "CauHoi", HeaderText = "Câu Hỏi", Width = 320, Alignment = "Left" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnA", HeaderText = "Đáp Án A", Width = 150, Alignment = "Left" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnB", HeaderText = "Đáp Án B", Width = 150, Alignment = "Left" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnC", HeaderText = "Đáp Án C", Width = 150, Alignment = "Left" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnD", HeaderText = "Đáp Án D", Width = 150, Alignment = "Left" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnDung", HeaderText = "Đáp Án Đúng", Width = 95, Alignment = "Center" },
                new ExcelExportHelper.ColumnDefinition { FieldName = "DapAnChon", HeaderText = "Đáp Án Chọn", Width = 100, Alignment = "Center" },
                new ExcelExportHelper.ColumnDefinition {
                    FieldName = "LaDung",
                    HeaderText = "Kết Quả",
                    Width = 90,
                    Alignment = "Center",
                    ValueFormatter = val => {
                        if (val == null || val == DBNull.Value) return "";
                        bool isTrue = false;
                        if (val is bool b) isTrue = b;
                        else bool.TryParse(val.ToString(), out isTrue);
                        return isTrue ? "ĐÚNG" : "SAI";
                    }
                }
            };

            string subTitle = $"Đề thi: {tenDeThi} | Điểm: {diem} | Nộp lúc: {thoiGianNop} | Học viên: {Session.TenDangNhap}";
            ExcelExportHelper.ExportDataTable(
                dt,
                columns,
                $"ChiTietBaiLam_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                "BẢNG KẾT QUẢ CHI TIẾT BÀI THI",
                subTitle
            );
        }

        private void btnHVKQ_BaoCao_Click(object sender, EventArgs e)
        {
            try
            {
                int lopID = cmbKQHV_BaiThi.SelectedValue is int value ? value : 0;
                using (DataTable results = _ketQuaService.LayKetQuaTheoLopDangHoc(Session.NguoiDungID, lopID))
                using (var report = new BLT_Quan_Ly_Hoc_Tap.Forms.BaoCaoDialog(results,
                    Session.HoTen ?? Session.TenDangNhap, Session.TenDangNhap))
                    report.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất báo cáo kết quả: " + ex.Message, "Lỗi");
            }
        }

        #endregion

        #region 6. Tab Thông Tin Cá Nhân & Đổi Mật Khẩu

        private bool CoPhienHocSinhHopLe()
        {
            return Session.IsHocSinh && Session.TaiKhoanID > 0;
        }

        private bool LoadThongTinHocVien()
        {
            btnTTHV_LuuThayDoi.Enabled = false;
            btnTTHV_DoiMK.Enabled = CoPhienHocSinhHopLe();
            if (!CoPhienHocSinhHopLe())
            {
                MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "Thông báo");
                return false;
            }

            try
            {
                HocSinh hs = _hocSinhService.LayTheoTaiKhoan(Session.TaiKhoanID);
                if (hs == null)
                {
                    MessageBox.Show("Không tìm thấy hồ sơ học sinh được liên kết với tài khoản. Vui lòng liên hệ quản trị viên.", "Thông báo");
                    return false;
                }

                Session.NguoiDungID = hs.HocSinhID;
                HienThiHoSo(hs);
                btnTTHV_LuuThayDoi.Enabled = true;
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải hồ sơ học sinh: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private void HienThiHoSo(HocSinh hs)
        {
            string ngaySinh = hs.NgaySinh?.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) ?? "";
            txtTTHV_HovaTen.Text = hs.HoTen ?? "";
            txtTTHV_NgaySinh.Text = ngaySinh;
            txtTTHV_Email.Text = hs.Email ?? "";
            txtTTHV_SDT.Text = hs.SoDienThoai ?? "";
            txtTTHV_DiaChi.Text = hs.DiaChi ?? "";
            cmbTTHV_GioiTinh.SelectedIndex = -1;
            foreach (object item in cmbTTHV_GioiTinh.Items)
            {
                if (string.Equals(item.ToString(), hs.GioiTinh?.Trim(), StringComparison.OrdinalIgnoreCase))
                {
                    cmbTTHV_GioiTinh.SelectedItem = item;
                    break;
                }
            }

            HienThiTheHoSo(hs);
        }

        private static string GiaTriHoSo(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value.Trim();
        }

        private void HienThiTheHoSo(HocSinh hs)
        {
            string hoTen = string.IsNullOrWhiteSpace(hs.HoTen) ? GiaTriHoSo(Session.TenDangNhap) : hs.HoTen.Trim();
            lblTTHV_HoTen.Text = hoTen;
            lblTTHV_DiaChi.Text = "Địa chỉ: " + GiaTriHoSo(hs.DiaChi);
            lblTTHV_Email.Text = "Email: " + GiaTriHoSo(hs.Email);
            lblTTHV_SDT.Text = "Số điện thoại: " + GiaTriHoSo(hs.SoDienThoai);
            Session.HoTen = hoTen;
            lblTCHV_Chao.Text = "Xin chào, " + hoTen;
        }

        private void BtnHV_LuuProfile_Click(object sender, EventArgs e)
        {
            if (!CoPhienHocSinhHopLe() || Session.NguoiDungID <= 0)
            {
                MessageBox.Show("Không xác định được hồ sơ học sinh. Vui lòng đăng nhập lại.", "Thông báo");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtTTHV_HovaTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTTHV_HovaTen.Focus();
                return;
            }

            if (!Validation.IsValidProfileBirthDate(txtTTHV_NgaySinh.Text, out DateTime? ngaySinh, out string loi))
            {
                MessageBox.Show(loi, "Ngày sinh không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTTHV_NgaySinh.Focus();
                return;
            }
            if (cmbTTHV_GioiTinh.SelectedItem == null)
            {
                MessageBox.Show("Vui lòng chọn giới tính.", "Thông báo");
                cmbTTHV_GioiTinh.Focus();
                return;
            }
            if (!Validation.IsValidEmail(txtTTHV_Email.Text, out loi))
            {
                MessageBox.Show(loi, "Email không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTTHV_Email.Focus();
                return;
            }
            if (!Validation.IsValidPhone(txtTTHV_SDT.Text, out loi))
            {
                MessageBox.Show(loi, "Số điện thoại không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTTHV_SDT.Focus();
                return;
            }

            HocSinh hs = new HocSinh
            {
                HocSinhID = Session.NguoiDungID,
                HoTen = txtTTHV_HovaTen.Text.Trim(),
                Email = txtTTHV_Email.Text.Trim(),
                SoDienThoai = txtTTHV_SDT.Text.Trim(),
                DiaChi = txtTTHV_DiaChi.Text.Trim(),
                GioiTinh = cmbTTHV_GioiTinh.SelectedItem?.ToString(),
                NgaySinh = ngaySinh
            };

            btnTTHV_LuuThayDoi.Enabled = false;
            try
            {
                if (!_hocSinhService.CapNhatHoSoCaNhan(Session.TaiKhoanID, hs))
                {
                    MessageBox.Show("Không thể cập nhật hồ sơ. Tài khoản hoặc liên kết học sinh có thể đã thay đổi. Vui lòng đăng nhập lại.", "Thông báo");
                    return;
                }
                HienThiHoSo(hs);
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể lưu hồ sơ: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTTHV_LuuThayDoi.Enabled = CoPhienHocSinhHopLe();
            }
        }

        private void BtnHV_DoiMatKhau_Click(object sender, EventArgs e)
        {
            if (!CoPhienHocSinhHopLe())
            {
                MessageBox.Show("Phiên đăng nhập không hợp lệ. Vui lòng đăng nhập lại.", "Thông báo");
                return;
            }

            string matKhauCu = txtTTHV_MatKhauHienTai.Text;
            string matKhauMoi = txtTTHV_MKMoi.Text;
            if (!Validation.IsValidPasswordChange(matKhauCu, matKhauMoi, txtTTHV_XacNhanMK.Text, out string loi))
            {
                MessageBox.Show(loi, "Thông tin mật khẩu không hợp lệ", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            btnTTHV_DoiMK.Enabled = false;
            try
            {
                if (!_taiKhoanService.DoiMatKhauCaNhan(Session.TaiKhoanID, matKhauCu, matKhauMoi))
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng hoặc tài khoản đã thay đổi. Vui lòng kiểm tra lại.", "Không thể đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _taiKhoanService.XoaGhiNho(Session.TenDangNhap);
                txtTTHV_MatKhauHienTai.Clear();
                txtTTHV_MKMoi.Clear();
                txtTTHV_XacNhanMK.Clear();
                MessageBox.Show("Đổi mật khẩu thành công. Hãy dùng mật khẩu mới trong lần đăng nhập tiếp theo.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đổi mật khẩu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnTTHV_DoiMK.Enabled = CoPhienHocSinhHopLe();
            }
        }

        #endregion

        #region 7. Điều Hướng & Sự Kiện Form

        private void LamMoiTrangDangXem()
        {
            if (!_daTaiHocVien || _dangLamMoiTrang || _dangXuLyDangKyLop) return;
            _dangLamMoiTrang = true;
            try
            {
                if (tabHocVien.SelectedTab == pageHV_TrangChu)
                {
                    LoadLopHoc();
                    LoadDanhSachDeThi();
                }
                else if (tabHocVien.SelectedTab == pageHV_BaiThi)
                    LoadDanhSachDeThi();
                else if (tabHocVien.SelectedTab == pageHV_KetQua)
                    LoadLichSuThi();
                else if (tabHocVien.SelectedTab == pageHV_LopHoc)
                {
                    LoadTabLopHocCuaToi();
                    LoadDanhSachDeThi();
                }
                else if (tabHocVien.SelectedTab == pageHV_ThongTin)
                    LoadThongTinHocVien();
            }
            finally { _dangLamMoiTrang = false; }
        }

        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);
            if (!_daTaiHocVien || _dangLamMoiTrang || _dangXuLyDangKyLop || !Visible) return;
            // Giáo viên có thể giao bài/duyệt lớp trong khi cửa sổ học viên đang mở.
            if (tabHocVien.SelectedTab == pageHV_TrangChu
                || tabHocVien.SelectedTab == pageHV_BaiThi || tabHocVien.SelectedTab == pageHV_LopHoc)
                LamMoiTrangDangXem();
        }

        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_daTaiHocVien) return;
            LamMoiTrangDangXem();
            if (tabHocVien.SelectedTab != null && tabHocVien.SelectedTab.Text.Trim().Contains("Đăng Xuất"))
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Session.Clear();
                    frm_DangNhap loginForm = Application.OpenForms["frm_DangNhap"] as frm_DangNhap ?? new frm_DangNhap();
                    loginForm.Show();
                    this.Hide();
                }
                else
                {
                    tabHocVien.SelectedIndex = 0;
                }
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (Form f in Application.OpenForms)
            {
                if (f.Visible && f != this) return;
            }
            Application.Exit();
        }

        // HÀM NÀY ĐỂ FIX LỖI DESIGNER (Tương ứng với ảnh lỗi bạn cung cấp)
        private void tableLayoutPanel2_Paint(object sender, PaintEventArgs e)
        {
        }

        #endregion

    }
}
