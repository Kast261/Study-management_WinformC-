using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_GiaoVien : ReaLTaiizor.Forms.MaterialForm
    {
        #region Dữ liệu & Trạng thái

        private readonly ILopHocService _lopService = new LopHocService();
        private readonly IDeThiService _deThiService = new DeThiService();
        private readonly IKhoaHocService _khService = new KhoaHocService();
        private readonly ITaiKhoanService _tkService = new TaiKhoanService();
        private int _selectedLopHocID = 0;
        private int _selectedCauHoiID = 0;
        private int _selectedDeThiID   = 0;  
        private bool _isSyncingMonHoc = false;
        private bool _dangTaiDeThi;
        private bool _dangNhapExcel;
        private bool _dangGiaoDeThi;
        private bool _daTaiTrangChu;
        private DataTable _khoaHoc;
        private DataTable _lopHoc;
        private DataTable _ketQua;

        #endregion

        #region 1. Khởi tạo & Đăng ký sự kiện

        public frm_GiaoVien()
        {
            InitializeComponent();
            if (System.ComponentModel.LicenseManager.UsageMode != System.ComponentModel.LicenseUsageMode.Designtime)
            {
                this.DrawerTabControl = materialTabControl1;
                HienThiTheHoSo(new GiaoVien { HoTen = Session.HoTen ?? Session.TenDangNhap });
            }

            DataGridViewPager.AttachAll(this, KhiDoiTrang);
        }

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            try
            {
                LoadThongTinCaNhan();
                LoadTrangChu();
                _daTaiTrangChu = true;

                InitDropdowns();

                // Load dữ liệu ban đầu cho các tab
                LoadDanhSachLopQuanLy();
                cboDK_LocTheoLop_SelectedIndexChanged(null, null);
                LoadDeThiCombobox();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trang giáo viên: " + ex.Message, "Thông báo");
            }
        }

        private void InitDropdowns()
        {
            // Khóa học cho form Lớp
            _khoaHoc = _khService.LayTatCa();
            var tenKhoaHoc = new AutoCompleteStringCollection();
            foreach (DataRow row in _khoaHoc.Rows) tenKhoaHoc.Add(row["TenKhoaHoc"].ToString());
            txtLH_KhoaHoc.AutoCompleteCustomSource = tenKhoaHoc;

            // Nạp môn học (khóa học) mà giáo viên đang dạy cho cả Tab Câu hỏi (cmbCH_MonHoc) và Tab Bài thi (cmbDT_MonHoc)
            LoadMonHocGiaoVien();
        }

        /// <summary>
        /// Nạp danh sách các môn học / khóa học mà giáo viên hiện tại đang dạy
        /// vào cả cmbCH_MonHoc (Tab Câu hỏi) và cmbDT_MonHoc (Tab Bài thi)
        /// </summary>
        private void LoadMonHocGiaoVien()
        {
            try
            {
                DataTable dtMonHoc = _khService.LayKhoaHocTheoGiaoVien(Session.NguoiDungID);

                string selectedCH = cmbCH_MonHoc.SelectedItem?.ToString();
                string selectedDe = cmbDT_MonHoc.SelectedItem?.ToString();

                _isSyncingMonHoc = true;
                cmbCH_MonHoc.Items.Clear();
                cmbDT_MonHoc.Items.Clear();

                if (dtMonHoc != null && dtMonHoc.Rows.Count > 0)
                {
                    foreach (DataRow r in dtMonHoc.Rows)
                    {
                        string tenKH = r["TenKhoaHoc"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(tenKH))
                        {
                            if (!cmbCH_MonHoc.Items.Contains(tenKH))
                                cmbCH_MonHoc.Items.Add(tenKH);
                            if (!cmbDT_MonHoc.Items.Contains(tenKH))
                                cmbDT_MonHoc.Items.Add(tenKH);
                        }
                    }
                }

                if (cmbCH_MonHoc.Items.Count > 0)
                {
                    if (!string.IsNullOrEmpty(selectedCH) && cmbCH_MonHoc.Items.Contains(selectedCH))
                        cmbCH_MonHoc.SelectedItem = selectedCH;
                    else
                        cmbCH_MonHoc.SelectedIndex = 0;
                }

                if (cmbDT_MonHoc.Items.Count > 0)
                {
                    if (!string.IsNullOrEmpty(selectedDe) && cmbDT_MonHoc.Items.Contains(selectedDe))
                        cmbDT_MonHoc.SelectedItem = selectedDe;
                    else
                        cmbDT_MonHoc.SelectedIndex = 0;
                }

                _isSyncingMonHoc = false;
                LoadLopGiaoDeThi();
            }
            catch (Exception ex)
            {
                _isSyncingMonHoc = false;
                System.Diagnostics.Debug.WriteLine("Lỗi nạp môn học giáo viên: " + ex.Message);
            }
        }

        private void KhiDoiTrang(DataGridView grid, PageChangeReason reason)
        {
            if (grid == dgvLopHoc)
            {
                _selectedLopHocID = 0;
                headerLabel4.Text = "Đang chọn: chưa chọn lớp";
            }
            if (grid == dgvCauHoi)
            {
                _selectedCauHoiID = 0;
                materialLabel15.Text = "Đang chỉnh sửa: câu hỏi mới";
            }
            if (grid == dgvGVDeThi && reason != PageChangeReason.Data)
            {
                _selectedDeThiID = 0;
                cmbDT_DeThi.SelectedIndex = -1;
                dgvCauHoi.DataSource = null;
            }
        }

        #endregion

        #region 2. Tab Trang Chủ & Quản Lý Lớp Học

        private void LoadTrangChu()
        {
            var vi = CultureInfo.GetCultureInfo("vi-VN");
            lblTCGV_Chao.Text = "Xin chào, " + (Session.HoTen ?? Session.TenDangNhap);
            lblTCGV_NgayThangHT.Text = DateTime.Today.ToString("dddd, dd/MM/yyyy", vi);
            try
            {
                DataTable classes = _lopService.LayTheoGiaoVien(Session.NguoiDungID);
                DataTable results = _deThiService.LayTongHopKetQuaGanDay(Session.NguoiDungID);
                using (DataTable summary = new DashboardService().LayTongQuanGiaoVien(Session.NguoiDungID))
                {
                    DataRow row = summary.Rows[0];
                    long classCount = Convert.ToInt64(row["SoLopHoc"]);
                    long studentCount = Convert.ToInt64(row["SoHocVien"]);
                    long pending = Convert.ToInt64(row["SoChoDuyet"]);
                    long openExams = Convert.ToInt64(row["SoBaiThiDangMo"]);
                    dgvBaiThiDangMo.DataSource = classes;
                    dgvTCBaiThi.DataSource = results;
                    lblTCGV_LopDH.Text = $"Lớp đang dạy\n{classCount:N0}";
                    lblTCGV_HocVien.Text = $"Học viên\n{studentCount:N0}";
                    lblTCGV_ChoDuyet.Text = $"Yêu cầu chờ duyệt\n{pending:N0}";
                    lblTCGV_BaiThiDangMo.Text = $"Bài thi đang mở\n{openExams:N0}";
                    lblTCGV_SoLopQuanLy.Text = $"{classCount:N0} lớp đang quản lý";
                    lblTCGV_SoYeuCauChoDuyet.Text = $"{pending:N0} yêu cầu đăng ký đang chờ duyệt";
                    lblTCHV_TatCaBaiThi.Text = $"Bạn đang quản lý {classCount:N0} lớp và có {pending:N0} yêu cầu chờ duyệt.";
                    btnTCGV_DuyetNgay.Enabled = pending > 0;
                }
            }
            catch (Exception ex)
            {
                dgvBaiThiDangMo.DataSource = dgvTCBaiThi.DataSource = null;
                lblTCGV_LopDH.Text = "Lớp đang dạy\n--";
                lblTCGV_HocVien.Text = "Học viên\n--";
                lblTCGV_ChoDuyet.Text = "Yêu cầu chờ duyệt\n--";
                lblTCGV_BaiThiDangMo.Text = "Bài thi đang mở\n--";
                lblTCGV_SoLopQuanLy.Text = "Chưa tải được danh sách lớp";
                lblTCGV_SoYeuCauChoDuyet.Text = "Chưa tải được yêu cầu đăng ký";
                lblTCHV_TatCaBaiThi.Text = "Không tải được dữ liệu trang chủ. Mở lại tab để thử lại.";
                btnTCGV_DuyetNgay.Enabled = false;
                MessageBox.Show("Không thể tải trang chủ giáo viên: " + ex.Message, "Lỗi");
            }
        }

        private void btnTCGV_GiaoBai_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = pageGV_QuanLyDeThi;
        }

        private void btnTCGV_QuanLyLop_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = pageGV_LopHoc;
        }

        private void btnTCGV_DuyetNgay_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = pageGV_DuyetHocVien;
        }

        private void lblTCGV_XemTatCaBaiThi_Click(object sender, EventArgs e)
        {
            materialTabControl1.SelectedTab = pageGV_KetQuaThi;
        }

        private void dgvTCBaiThi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || !(dgvTCBaiThi.Rows[e.RowIndex].DataBoundItem is DataRowView row)) return;
            materialTabControl1.SelectedTab = pageGV_KetQuaThi;
            cmbKQ_DeThi.SelectedValue = Convert.ToInt32(row["DeThiID"]);
        }

        private void LoadDanhSachLopQuanLy()
        {
            try
            {
                DataTable dt = _lopService.LayTheoGiaoVien(Session.NguoiDungID);
                _lopHoc = dt;
                HienThiLopHoc();

                // Load danh sách lớp vào combobox duyệt & giao bài thi
                DataTable dtDuyet = dt.Copy();
                DataRow drAll = dtDuyet.NewRow();
                drAll["LopHocID"] = 0;
                drAll["TenLop"] = "-- Tất cả các lớp --";
                dtDuyet.Rows.InsertAt(drAll, 0);

                cboDK_LocTheoLop.DataSource = dtDuyet;

                // Nạp các lớp giáo viên phụ trách để nhận bài thi
                LoadLopGiaoDeThi();
            }
            catch(Exception ex) {
                MessageBox.Show(
      "Lỗi tải danh sách lớp học:\n" + ex.Message,
      "Lỗi",
      MessageBoxButtons.OK,
      MessageBoxIcon.Error);
            }
        }

        private void HienThiLopHoc()
        {
            if (_lopHoc == null) return;
            string timKiem = txtLH_TimKiem.Text.Trim().Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            var view = new DataView(_lopHoc);
            if (timKiem.Length > 0)
                view.RowFilter = $"TenLop LIKE '%{timKiem}%' OR TenKhoaHoc LIKE '%{timKiem}%'";
            dgvLopHoc.DataSource = view;
            hlbl_SoLuong.Text = $"Số lượng lớp học: {view.Count:N0}";
        }

        private void txtLH_TimKiem_TextChanged(object sender, EventArgs e)
        {
            HienThiLopHoc();
        }

        private void dgvLopHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLopHoc.Rows.Count)
            {
                var row = dgvLopHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedLopHocID = Convert.ToInt32(drv["LopHocID"]);
                    txtLH_TenLop.Text = drv["TenLop"]?.ToString();
                    txtGV_LichHoc.Text = drv["LichHoc"]?.ToString();

                    DatePickerInput.SetValue(dateLH_NgayBatDau, drv["NgayBatDau"]);
                    DatePickerInput.SetValue(dateLH_NgayKetThuc, drv["NgayKetThuc"]);
                    nUDLH_SiSo.Value = drv["SiSoToiDa"] == DBNull.Value ? 40
                        : Math.Max(nUDLH_SiSo.Minimum, Math.Min(nUDLH_SiSo.Maximum, Convert.ToDecimal(drv["SiSoToiDa"])));
                    txtLH_KhoaHoc.Text = drv["TenKhoaHoc"].ToString();
                    txtLH_SoDangKi.Text = drv["SiSo"].ToString();
                    headerLabel4.Text = "Đang chọn: " + drv["TenLop"];
                }
            }
        }

        private bool LayKhoaHocNhap(out int khoaHocID)
        {
            khoaHocID = 0;
            if (_khoaHoc != null)
                foreach (DataRow row in _khoaHoc.Rows)
                    if (string.Equals(row["TenKhoaHoc"].ToString().Trim(), txtLH_KhoaHoc.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        khoaHocID = Convert.ToInt32(row["KhoaHocID"]);
                        return true;
                    }
            MessageBox.Show("Vui lòng nhập tên khóa học có trong danh mục quản trị viên đã tạo.", "Thông báo");
            txtLH_KhoaHoc.Focus();
            return false;
        }

        private bool KiemTraNgayLopHoc(out DateTime ngayBatDau, out DateTime ngayKetThuc)
        {
            ngayBatDau = dateLH_NgayBatDau.Value.Date;
            ngayKetThuc = dateLH_NgayKetThuc.Value.Date;
            if (!dateLH_NgayBatDau.Checked || !dateLH_NgayKetThuc.Checked)
            {
                MessageBox.Show("Vui lòng chọn đủ ngày bắt đầu và ngày kết thúc!", "Thông báo");
                return false;
            }
            if (ngayKetThuc < ngayBatDau)
            {
                MessageBox.Show("Ngày kết thúc không được trước ngày bắt đầu!", "Thông báo");
                return false;
            }
            return true;
        }

        private void btnLH_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLH_TenLop.Text))
            {
                MessageBox.Show("Vui lòng nhập tên lớp học!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLH_TenLop.Focus();
                return;
            }

            if (!Validation.IsValidSiSo(nUDLH_SiSo.Value.ToString(CultureInfo.InvariantCulture), out int siSo, out string errSiSo))
            {
                MessageBox.Show(errSiSo, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nUDLH_SiSo.Focus();
                return;
            }

            if (!LayKhoaHocNhap(out int khID)) return;

            if (!KiemTraNgayLopHoc(
                out DateTime nbd,
                out DateTime nkt))
            {
                return;
            }

            LopHoc lop = new LopHoc
            {
                TenLop = txtLH_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                LichHoc = txtGV_LichHoc.Text.Trim(),
                SiSoToiDa = siSo,
                TrangThai = "Đang mở",
                NgayBatDau = nbd,
                NgayKetThuc = nkt
            };

            if (_lopService.ThemLop(lop))
            {
                MessageBox.Show("Tạo lớp học mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopQuanLy();
                LoadTrangChu();
            }
        }

        private void btnLH_CapNhap_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLH_TenLop.Text))
            {
                MessageBox.Show("Tên lớp không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLH_TenLop.Focus();
                return;
            }

            if (!Validation.IsValidSiSo(nUDLH_SiSo.Value.ToString(CultureInfo.InvariantCulture), out int siSo, out string errSiSo))
            {
                MessageBox.Show(errSiSo, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                nUDLH_SiSo.Focus();
                return;
            }

            if (!LayKhoaHocNhap(out int khID)) return;

            if (!KiemTraNgayLopHoc(
                out DateTime nbd,
                out DateTime nkt))
            {
                return;
            }

            LopHoc lop = new LopHoc
            {
                LopHocID = _selectedLopHocID,
                TenLop = txtLH_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                LichHoc = txtGV_LichHoc.Text.Trim(),
                SiSoToiDa = siSo,
                NgayBatDau = nbd,
                NgayKetThuc = nkt
            };

            if (_lopService.SuaLop(lop))
            {
                MessageBox.Show("Cập nhật thông tin lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopQuanLy();
            }
            else
            {
                MessageBox.Show("Không thể cập nhật lớp. Sĩ số tối đa không được nhỏ hơn số học viên đang học.", "Thông báo");
            }
        }

        private void btnLH_DongDK_Click(object sender, EventArgs e)
        {
            var selected = dgvLopHoc.CurrentRow?.DataBoundItem as DataRowView;
            if (selected == null || dgvLopHoc.SelectedRows.Count != 1 || Session.NguoiDungID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần đóng đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int lopHocID = Convert.ToInt32(selected["LopHocID"]);
            string tenLop = ExamSelection.ClassDisplayName(selected.Row);
            if (MessageBox.Show($"Đóng đăng ký cho {tenLop}? Học viên mới sẽ không thể đăng ký vào lớp này. Các lớp khác vẫn mở đăng ký.", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopService.DongDangKyLop(lopHocID, Session.NguoiDungID))
                {
                    MessageBox.Show($"Đã đóng đăng ký {tenLop}.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDanhSachLopQuanLy();
                    LoadTrangChu();
                }
                else
                    MessageBox.Show("Lớp đã đóng hoặc không còn do bạn phụ trách. Vui lòng tải lại danh sách.", "Thông báo");
            }
        }

        private void btnLH_TaiLai_Click(object sender, EventArgs e)
        {
            _selectedLopHocID = 0;
            txtLH_TimKiem.Text = "";
            txtLH_TenLop.Clear();
            txtLH_KhoaHoc.Clear();
            txtGV_LichHoc.Clear();
            txtLH_SoDangKi.Text = "0";
            dateLH_NgayBatDau.Checked = dateLH_NgayKetThuc.Checked = false;
            nUDLH_SiSo.Value = 40;
            headerLabel4.Text = "Đang chọn: chưa chọn lớp";
            LoadDanhSachLopQuanLy();
            LoadTrangChu();
        }

        #endregion

        #region 3. Tab Duyệt Học Viên Đăng Ký

        private void cboDK_LocTheoLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int lopID = 0;
                if (cboDK_LocTheoLop.SelectedValue is int idVal)
                {
                    lopID = idVal;
                }
                else if (cboDK_LocTheoLop.SelectedValue is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    int.TryParse(drv["LopHocID"]?.ToString(), out lopID);
                }
                else if (cboDK_LocTheoLop.SelectedValue != null)
                {
                    int.TryParse(cboDK_LocTheoLop.SelectedValue.ToString(), out lopID);
                }

                DataTable dt = _lopService.LayDangKyChoDuyet(Session.NguoiDungID, lopID);

                dgvDSYeuCau.DataSource = null;
                dgvDSYeuCau.DataSource = dt;
                lblDK_SoChoDuyet.Text = $"Số yêu cầu chờ duyệt: {dt.Rows.Count:N0}";
                lblDK_Lop.Text = "Lớp: " + cboDK_LocTheoLop.Text;
            }
            catch { }
        }

        private void dgvDSYeuCau_SelectionChanged(object sender, EventArgs e)
        {
            var row = dgvDSYeuCau.CurrentRow?.DataBoundItem as DataRowView;
            lblDK_ChonHS.Text = "Đang chọn: " + (row == null ? "chưa chọn học viên" : row["HoTen"].ToString());
        }

        private void btnDK_Duyet_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDSYeuCau.CurrentRow != null && dgvDSYeuCau.CurrentRow.DataBoundItem is DataRowView drv)
                {
                    int dkID = Convert.ToInt32(drv["DangKyID"]);
                    int lopID = Convert.ToInt32(drv["LopHocID"]);
                    int hsID = Convert.ToInt32(drv["HocSinhID"]);

                    if (_lopService.DuyetDangKy(dkID, lopID, hsID, Session.NguoiDungID))
                    {
                        MessageBox.Show("Đã duyệt học viên vào lớp thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cboDK_LocTheoLop_SelectedIndexChanged(null, null);
                        LoadDanhSachLopQuanLy();
                    }
                    else
                    {
                        MessageBox.Show("Không thể duyệt: lớp đã đủ sĩ số, đã đóng hoặc đơn đăng ký đã thay đổi.", "Thông báo");
                        cboDK_LocTheoLop_SelectedIndexChanged(null, null);
                    }
                }
                else
                {
                    MessageBox.Show("Vui lòng chọn học viên cần duyệt từ danh sách!", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnDK_TuChoi_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDSYeuCau.CurrentRow != null && dgvDSYeuCau.CurrentRow.DataBoundItem is DataRowView drv)
                {
                    int dkID = Convert.ToInt32(drv["DangKyID"]);
                    if (MessageBox.Show("Từ chối đơn đăng ký này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        if (_lopService.TuChoiDangKy(dkID, Session.NguoiDungID))
                        {
                            MessageBox.Show("Đã từ chối đơn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            cboDK_LocTheoLop_SelectedIndexChanged(null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 4. Tab Ngân Hàng Câu Hỏi

        private void cmbCH_MonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSyncingMonHoc) return;
            try
            {
                _isSyncingMonHoc = true;
                string monHoc = cmbCH_MonHoc.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(monHoc) && cmbDT_MonHoc.Items.Contains(monHoc))
                {
                    cmbDT_MonHoc.SelectedItem = monHoc;
                    LoadLopGiaoDeThi();
                }
            }
            finally
            {
                _isSyncingMonHoc = false;
            }
        }

        private void dgvCauHoi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvCauHoi.Rows.Count)
            {
                CauHoi q = dgvCauHoi.Rows[e.RowIndex].DataBoundItem as CauHoi;
                if (q != null)
                {
                    _selectedCauHoiID = q.CauHoiID;
                    materialLabel15.Text = "Đang chỉnh sửa câu hỏi: " + q.ThuTu;
                    txtCH_NhapCauHoi.Text = q.NoiDung;
                    txtCH_A.Text = q.DapAnA;
                    txtCH_B.Text = q.DapAnB;
                    txtCH_C.Text = q.DapAnC;
                    txtCH_D.Text = q.DapAnD;
                    cmb_DapAn.SelectedItem = q.DapAnDung;
                }
            }
        }

        private void LamMoiFormCauHoi()
        {
            _selectedCauHoiID = 0;
            materialLabel15.Text = "Đang chỉnh sửa: câu hỏi mới";
            txtCH_NhapCauHoi.Clear();
            txtCH_A.Clear();
            txtCH_B.Clear();
            txtCH_C.Clear();
            txtCH_D.Clear();
            cmb_DapAn.SelectedIndex = 0;
        }

        // -----------------------------------------------------------------------
        // btnCH_TaoDe (Tab Ngân Hàng Câu Hỏi):
        //   Tạo đề thi mới với tên nhập ở txtCH_TenDeThi.
        //   Sau khi tạo, chuyển combobox sang đề mới và reset form câu hỏi.
        // -----------------------------------------------------------------------
        private void btnCH_TaoDe_Click(object sender, EventArgs e)
        {
            string tenDe = txtCH_TenDeThi?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tenDe))
            {
                MessageBox.Show("Vui lòng nhập tên đề thi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCH_TenDeThi?.Focus();
                return;
            }

            string monHoc = cmbCH_MonHoc.SelectedItem?.ToString() ?? cmbCH_MonHoc.Text?.Trim();
            if (string.IsNullOrWhiteSpace(monHoc))
            {
                monHoc = cmbDT_MonHoc.SelectedItem?.ToString() ?? "Lập trình C# WinForms";
            }

            int.TryParse(cmbDT_DeThi.SelectedValue?.ToString(), out int currentID);
            DataRow existing = ExamSelection.FindDraft(_deThiService.LayDeThiTheoGiaoVien(Session.NguoiDungID),
                tenDe, monHoc, currentID);
            if (existing != null)
            {
                LoadDeThiCombobox(Convert.ToInt32(existing["DeThiID"]));
                LamMoiFormCauHoi();
                return;
            }

            DeThi de = new DeThi
            {
                TenDeThi   = tenDe,
                MonHoc     = monHoc,
                GiaoVienID = Session.NguoiDungID,
                SoCau      = 0,
                ThoiGian   = 45,
                TrangThai  = "Chưa giao"
            };

            int newID = _deThiService.TaoDeThiMoiLayID(de);
            if (newID > 0)
            {
                MessageBox.Show(
                    $"Tạo đề thi \"{ tenDe }\" ({ monHoc }) thành công!\nBây giờ bạn có thể nhập các câu hỏi kèm đáp án cho đề thi này.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm mới danh sách đề thi và chọn đề vừa tạo
                _selectedDeThiID = newID;
                LoadDeThiCombobox(newID);
                // Reset form câu hỏi để bắt đầu nhập câu mới
                LamMoiFormCauHoi();

                // Nạp lưới câu hỏi (lúc mới tạo là 0 câu)
                dgvCauHoi.DataSource = _deThiService.LayDanhSachCauHoi(newID);

                // Xóa tên đề thi đã nhập
                if (txtCH_TenDeThi != null) txtCH_TenDeThi.Clear();
            }
            else
            {
                MessageBox.Show("Tạo đề thi thất bại! Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCH_ThemCH_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCH_NhapCauHoi.Text))
                {
                    MessageBox.Show("Vui lòng nhập nội dung câu hỏi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCH_NhapCauHoi.Focus();
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCH_A.Text) ||
                    string.IsNullOrWhiteSpace(txtCH_B.Text) ||
                    string.IsNullOrWhiteSpace(txtCH_C.Text) ||
                    string.IsNullOrWhiteSpace(txtCH_D.Text))
                {
                    MessageBox.Show("Vui lòng nhập đầy đủ cả 4 phương án đáp án (A, B, C, D)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string tenDeNhap = txtCH_TenDeThi?.Text?.Trim();
                string monHocDaChon = cmbCH_MonHoc.SelectedItem?.ToString() ?? cmbCH_MonHoc.Text?.Trim();
                if (string.IsNullOrWhiteSpace(monHocDaChon))
                {
                    MessageBox.Show("Vui lòng chọn môn học cho câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                int deID = 0;

                // Nếu người dùng có nhập tên đề vào ô txtCH_TenDeThi
                if (!string.IsNullOrEmpty(tenDeNhap))
                {
                    // Kiểm tra xem đề thi này đã có trong danh sách chưa
                    int.TryParse(cmbDT_DeThi.SelectedValue?.ToString(), out int currentID);
                    DataRow existing = ExamSelection.FindDraft(_deThiService.LayDeThiTheoGiaoVien(Session.NguoiDungID),
                        tenDeNhap, monHocDaChon, currentID);
                    if (existing != null)
                    {
                        deID = Convert.ToInt32(existing["DeThiID"]);
                        cmbDT_DeThi.SelectedValue = deID;
                    }

                    // Nếu chưa có đề thi này, tự động tạo mới luôn
                    if (deID <= 0)
                    {
                        DeThi deMoi = new DeThi
                        {
                            TenDeThi = tenDeNhap,
                            MonHoc = monHocDaChon,
                            GiaoVienID = Session.NguoiDungID,
                            SoCau = 0,
                            ThoiGian = 45,
                            TrangThai = "Chưa giao"
                        };
                        deID = _deThiService.TaoDeThiMoiLayID(deMoi);
                        if (deID > 0)
                        {
                            LoadDeThiCombobox(deID);
                        }
                    }
                }

                // Nếu không gõ ô txtCH_TenDeThi thì lấy từ ComboBox
                if (deID <= 0 && cmbDT_DeThi.SelectedValue != null)
                {
                    int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out deID);
                    DeThi deDaChon = deID > 0 ? _deThiService.LayTheoID(deID) : null;
                    if (deDaChon == null || !string.Equals(deDaChon.MonHoc?.Trim(), monHocDaChon.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        MessageBox.Show("Đề thi đang chọn thuộc môn khác. Vui lòng nhập tên đề mới hoặc chọn đề đúng môn học!",
                            "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                if (deID <= 0)
                {
                    MessageBox.Show("Vui lòng nhập tên đề thi hoặc chọn đề thi cần thêm câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int soCauHienCo = _deThiService.DemSoCauHoiHienTai(deID);

                CauHoi ch = new CauHoi
                {
                    DeThiID = deID,
                    NoiDung = txtCH_NhapCauHoi.Text.Trim(),
                    DapAnA = txtCH_A.Text.Trim(),
                    DapAnB = txtCH_B.Text.Trim(),
                    DapAnC = txtCH_C.Text.Trim(),
                    DapAnD = txtCH_D.Text.Trim(),
                    DapAnDung = cmb_DapAn.SelectedItem?.ToString() ?? "A",
                    ThuTu = soCauHienCo + 1
                };

                if (_deThiService.ThemCauHoi(ch, Session.NguoiDungID))
                {
                    _deThiService.CapNhatSoCau(deID);
                    MessageBox.Show($"Thêm câu hỏi thứ {ch.ThuTu} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Đồng bộ cập nhật lại danh sách đề thi ở cả 2 tab
                    LoadDeThiCombobox(deID);
                    LamMoiFormCauHoi();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCH_CapNhap_Click(object sender, EventArgs e)
        {
            try
            {
                if (_selectedCauHoiID <= 0)
                {
                    MessageBox.Show("Vui lòng chọn câu hỏi cần sửa!", "Thông báo");
                    return;
                }

                if (string.IsNullOrWhiteSpace(txtCH_NhapCauHoi.Text))
                {
                    MessageBox.Show("Nội dung câu hỏi không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtCH_NhapCauHoi.Focus();
                    return;
                }

                CauHoi ch = new CauHoi
                {
                    CauHoiID = _selectedCauHoiID,
                    NoiDung = txtCH_NhapCauHoi.Text.Trim(),
                    DapAnA = txtCH_A.Text.Trim(),
                    DapAnB = txtCH_B.Text.Trim(),
                    DapAnC = txtCH_C.Text.Trim(),
                    DapAnD = txtCH_D.Text.Trim(),
                    DapAnDung = cmb_DapAn.SelectedItem?.ToString() ?? "A"
                };

                if (_deThiService.SuaCauHoi(ch, Session.NguoiDungID))
                {
                    MessageBox.Show("Cập nhật câu hỏi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cmbDT_DeThi_SelectedIndexChanged(null, null);
                    LamMoiFormCauHoi();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Không thể thực hiện", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void btnCH_XoaCH_Click(object sender, EventArgs e)
        {
            if (_selectedCauHoiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần xóa!", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xóa câu hỏi này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_deThiService.XoaCauHoi(_selectedCauHoiID, Session.NguoiDungID))
                    {
                        if (cmbDT_DeThi.SelectedValue != null && int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out int deID))
                        {
                            _deThiService.CapNhatSoCau(deID);
                        }
                        MessageBox.Show("Đã xóa câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDeThiCombobox();
                        cmbDT_DeThi_SelectedIndexChanged(null, null);
                        LamMoiFormCauHoi();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private DeThi ChonDeThiNhapExcel()
        {
            string ten = txtCH_TenDeThi.Text.Trim();
            int.TryParse(cmbDT_DeThi.SelectedValue?.ToString(), out int deID);
            DataTable table = cmbDT_DeThi.DataSource as DataTable;
            DataRow selected = table?.Rows.Cast<DataRow>().FirstOrDefault(r => Convert.ToInt32(r["DeThiID"]) == deID);

            if (!string.IsNullOrWhiteSpace(ten)
                && (selected == null || !string.Equals(selected["TenDeThi"].ToString().Trim(), ten, StringComparison.OrdinalIgnoreCase)))
            {
                DataRow[] matches = table?.Rows.Cast<DataRow>().Where(r =>
                    string.Equals(r["TenDeThi"].ToString().Trim(), ten, StringComparison.OrdinalIgnoreCase)).ToArray()
                    ?? new DataRow[0];
                if (matches.Length > 1)
                    throw new InvalidOperationException("Có nhiều đề cùng tên. Hãy chọn chính xác đề trong danh sách trước khi nhập Excel.");
                selected = matches.FirstOrDefault();
                deID = selected == null ? 0 : Convert.ToInt32(selected["DeThiID"]);
            }

            string monHoc = (cmbCH_MonHoc.SelectedItem?.ToString() ?? cmbCH_MonHoc.Text).Trim();
            if (deID <= 0 && (string.IsNullOrWhiteSpace(ten) || ten.Length > 150
                || string.IsNullOrWhiteSpace(monHoc) || monHoc.Length > 100))
                throw new InvalidOperationException("Hãy nhập tên đề mới (tối đa 150 ký tự) và chọn môn học, hoặc chọn một đề đã có.");
            return new DeThi
            {
                DeThiID = deID, GiaoVienID = Session.NguoiDungID,
                TenDeThi = selected?["TenDeThi"].ToString() ?? ten, MonHoc = monHoc
            };
        }

        private void LuuFileExcelMau_Click(object sender, EventArgs e)
        {
            try
            {
                string template = Path.Combine(Application.StartupPath, "Templates", "CauHoi_Mau.xlsx");
                using (var dialog = new SaveFileDialog
                {
                    Title = "Lưu file mẫu nhập câu hỏi", Filter = "Excel (*.xlsx)|*.xlsx",
                    FileName = "CauHoi_Mau.xlsx", DefaultExt = "xlsx", RestoreDirectory = true
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    if (!string.Equals(Path.GetFullPath(template), Path.GetFullPath(dialog.FileName), StringComparison.OrdinalIgnoreCase))
                        File.Copy(template, dialog.FileName, true);
                    MessageBox.Show(this, "Đã lưu file mẫu. Hãy điền câu hỏi vào sheet CauHoi, lưu lại rồi chọn Nhập Excel.", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể lưu file mẫu: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void BtnCH_Excel_Click(object sender, EventArgs e)
        {
            if (_dangNhapExcel) return;
            if (!Session.IsGiaoVien || Session.TaiKhoanID <= 0 || Session.NguoiDungID <= 0)
            {
                MessageBox.Show(this, "Vui lòng đăng nhập bằng tài khoản giáo viên đã liên kết hồ sơ.", "Thông báo");
                return;
            }
            int taiKhoanID = Session.TaiKhoanID;
            int giaoVienID = Session.NguoiDungID;
            Control[] inputs = { btnCH_Excel, btnCH_ThemCH, btnCH_CapNhap, btnCH_XoaCH,
                btnCH_TaoDe, btnDT_GiaoBaiThi, cmbDT_DeThi, cmbCH_MonHoc, txtCH_TenDeThi };
            bool[] enabled = inputs.Select(control => control.Enabled).ToArray();
            _dangNhapExcel = true;
            try
            {
                DeThi target = ChonDeThiNhapExcel();
                foreach (Control input in inputs) input.Enabled = false;
                string path;
                using (var dialog = new OpenFileDialog
                {
                    Title = "Chọn file Excel câu hỏi", Filter = "File Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
                    CheckFileExists = true, Multiselect = false, RestoreDirectory = true
                })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    path = dialog.FileName;
                }
                UseWaitCursor = true;
                var questions = await Task.Run(() => CauHoiExcelReader.Read(path));
                if (IsDisposed || Disposing) return;
                if (!Session.IsGiaoVien || Session.TaiKhoanID != taiKhoanID || Session.NguoiDungID != giaoVienID)
                    throw new InvalidOperationException("Phiên đăng nhập đã thay đổi. Vui lòng đăng nhập lại trước khi nhập câu hỏi.");
                UseWaitCursor = false;
                string destination = target.DeThiID > 0
                    ? $"Thêm vào cuối đề \"{target.TenDeThi}\". Các câu hỏi hiện có được giữ nguyên."
                    : $"Tạo đề mới \"{target.TenDeThi}\" thuộc môn \"{target.MonHoc}\" (chưa giao cho lớp).";
                if (MessageBox.Show(this, $"File: {Path.GetFileName(path)}\nĐã kiểm tra {questions.Count:N0} câu hỏi hợp lệ.\n\n{destination}\n\nBạn có muốn nhập không?",
                    "Xác nhận nhập câu hỏi", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

                UseWaitCursor = true;
                KetQuaNhapCauHoi result = await Task.Run(() => new CauHoiImportService().Nhap(taiKhoanID, target, questions));
                if (IsDisposed || Disposing || Session.TaiKhoanID != taiKhoanID || Session.NguoiDungID != giaoVienID) return;
                try
                {
                    _selectedDeThiID = result.DeThiID;
                    LoadDeThiCombobox(result.DeThiID);
                    if (!int.TryParse(cmbDT_DeThi.SelectedValue?.ToString(), out int displayedID) || displayedID != result.DeThiID)
                        throw new InvalidOperationException("Chưa tải được đề vừa nhập trong danh sách.");
                    _selectedDeThiID = result.DeThiID;
                    cmbDT_DeThi_SelectedIndexChanged(null, null);
                    LamMoiFormCauHoi();
                }
                catch (Exception ex)
                {
                    UseWaitCursor = false;
                    MessageBox.Show(this, $"Đã lưu {result.SoCauDaNhap:N0} câu hỏi vào đề \"{result.TenDeThi}\", nhưng chưa làm mới được giao diện.\nHãy mở lại form để tải dữ liệu; không cần nhập lại file.\n\n{ex.Message}",
                        "Đã lưu câu hỏi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                UseWaitCursor = false;
                MessageBox.Show(this, $"Đã nhập {result.SoCauDaNhap:N0} câu hỏi vào đề \"{result.TenDeThi}\".\nTổng số câu hiện tại: {result.TongSoCau:N0}.",
                    "Nhập Excel thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                if (!IsDisposed && !Disposing)
                {
                    UseWaitCursor = false;
                    MessageBox.Show(this, "Không thể nhập câu hỏi từ Excel:\n" + ex.Message, "Lỗi nhập Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                _dangNhapExcel = false;
                if (!IsDisposed && !Disposing)
                {
                    UseWaitCursor = false;
                    for (int i = 0; i < inputs.Length; i++)
                        if (!inputs[i].IsDisposed) inputs[i].Enabled = enabled[i];
                }
            }
        }

        #endregion

        #region 5. Tab Quản Lý & Giao Đề Thi

        private void LoadDeThiCombobox(int preferredDeID = 0)
        {
            try
            {
                int currentDeID = preferredDeID;
                if (currentDeID <= 0 && cmbDT_DeThi.SelectedValue != null)
                    int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out currentDeID);
                if (currentDeID <= 0) currentDeID = _selectedDeThiID;
                int resultDeID = cmbKQ_DeThi.SelectedValue is int id ? id : 0;
                DataTable dt = _deThiService.LayDeThiTheoGiaoVien(Session.NguoiDungID);
                DataTable resultOptions = _deThiService.LayDeThiTheoGiaoVien(Session.NguoiDungID, true);
                resultOptions.Columns.Add("TenHienThiKetQua", typeof(string));
                foreach (DataRow row in resultOptions.Rows)
                    row["TenHienThiKetQua"] = Convert.ToString(row["TenDeThi"])
                        + (Convert.ToString(row["TrangThai"]) == "Đã xóa" ? " (đã xóa)" : "");
                DataTable deThiCoCauHoi = ExamSelection.WithQuestions(dt);
                // Binding temporarily selects the first exam. Do not let that
                // selection replace the subject/name while restoring the real exam.
                _dangTaiDeThi = true;
                try
                {
                    cmbDT_DeThi.DisplayMember = dt.Columns["TenDeThi"].ColumnName;
                    cmbDT_DeThi.ValueMember = dt.Columns["DeThiID"].ColumnName;
                    // Đề vừa tạo phải có trong ComboBox ngay cả khi chưa thêm câu hỏi.
                    cmbDT_DeThi.DataSource = dt.Copy();
                    cmbKQ_DeThi.DisplayMember = "TenHienThiKetQua";
                    cmbKQ_DeThi.ValueMember = resultOptions.Columns["DeThiID"].ColumnName;
                    cmbKQ_DeThi.DataSource = resultOptions;

                    dgvGVDeThi.DataSource = deThiCoCauHoi;
                    lblDT_SoDeThi.Text = $"Số đề thi: {deThiCoCauHoi.Rows.Count:N0}";

                    foreach (DataRow row in dt.Rows)
                    {
                        int deID = Convert.ToInt32(row["DeThiID"]);
                        if (deID == currentDeID) cmbDT_DeThi.SelectedValue = deID;
                    }
                    foreach (DataRow row in resultOptions.Rows)
                        if (Convert.ToInt32(row["DeThiID"]) == resultDeID)
                            cmbKQ_DeThi.SelectedValue = resultDeID;
                }
                finally { _dangTaiDeThi = false; }

                // Apply metadata only after the final selection is in place.
                cmbDT_DeThi_SelectedIndexChanged(null, EventArgs.Empty);
                cmbKQ_DeThi_SelectedIndexChanged(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách đề thi: " + ex.Message,
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Lớp nhận đề là các lớp giáo viên phụ trách, không phụ thuộc tên khóa học.
        /// </summary>
        private void LoadLopGiaoDeThi(bool preferSubject = false)
        {
            int selectedClass = cmbDT_GiaoChoLop.SelectedValue is int id ? id : 0;
            try
            {
                DataTable classes = _lopService.LayTheoGiaoVien(Session.NguoiDungID);
                cmbDT_GiaoChoLop.DataSource = null;
                if (classes == null)
                {
                    cmbDT_GiaoChoLop.Enabled = false;
                    return;
                }
                classes.Columns.Add("NhanLopNhanDe", typeof(string));
                foreach (DataRow classroom in classes.Rows)
                    classroom["NhanLopNhanDe"] = ExamSelection.ClassDisplayName(classroom);
                cmbDT_GiaoChoLop.DisplayMember = "NhanLopNhanDe";
                cmbDT_GiaoChoLop.ValueMember = classes.Columns["LopHocID"].ColumnName;
                cmbDT_GiaoChoLop.DataSource = classes;
                string subject = cmbDT_MonHoc.SelectedItem?.ToString() ?? cmbDT_MonHoc.Text;
                int preferred = ExamSelection.PreferredClassID(classes, subject, selectedClass, preferSubject);
                if (preferred > 0) cmbDT_GiaoChoLop.SelectedValue = preferred;
                else cmbDT_GiaoChoLop.SelectedIndex = -1;
                cmbDT_GiaoChoLop.Enabled = classes.Rows.Count > 0;
            }
            catch (Exception ex)
            {
                cmbDT_GiaoChoLop.DataSource = null;
                cmbDT_GiaoChoLop.Enabled = false;
                MessageBox.Show("Không tải được lớp nhận đề: " + ex.Message,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void cmbDT_MonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSyncingMonHoc) return;
            try
            {
                _isSyncingMonHoc = true;
                string monHoc = cmbDT_MonHoc.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(monHoc))
                {
                    if (cmbCH_MonHoc.Items.Contains(monHoc))
                        cmbCH_MonHoc.SelectedItem = monHoc;
                    LoadLopGiaoDeThi(true);
                }
            }
            finally
            {
                _isSyncingMonHoc = false;
            }
        }

        private void cmbDT_DeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dangTaiDeThi) return;
            if (cmbDT_DeThi.SelectedValue != null && int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out int deID) && deID > 0)
            {
                _selectedDeThiID = deID;
                _selectedCauHoiID = 0;
                var listCH = _deThiService.LayDanhSachCauHoi(deID);
                dgvCauHoi.DataSource = listCH;
                txtDT_SoCauHoi.Text = listCH.Count.ToString();
                lblCH_TongCauHoi.Text = $"Số câu hỏi: {listCH.Count:N0}";

                DeThi de = _deThiService.LayTheoID(deID);
                if (de != null)
                {
                    txtDT_ThoiGian.Text = de.ThoiGian.ToString();
                    if (!string.IsNullOrEmpty(de.MonHoc))
                    {
                        _isSyncingMonHoc = true;
                        try
                        {
                            // DropDownList không nhận Text nếu môn của đề chưa nằm trong Items.
                            if (!cmbDT_MonHoc.Items.Contains(de.MonHoc)) cmbDT_MonHoc.Items.Add(de.MonHoc);
                            if (!cmbCH_MonHoc.Items.Contains(de.MonHoc)) cmbCH_MonHoc.Items.Add(de.MonHoc);
                            cmbDT_MonHoc.SelectedItem = de.MonHoc;
                            cmbCH_MonHoc.SelectedItem = de.MonHoc;
                        }
                        finally { _isSyncingMonHoc = false; }

                        LoadLopGiaoDeThi(true);
                    }
                    // Khi chọn một lượt đã giao, hiển thị đúng lớp của chính lượt đó.
                    if (de.LopHocID > 0) cmbDT_GiaoChoLop.SelectedValue = de.LopHocID;

                    if (txtCH_TenDeThi != null && !string.IsNullOrEmpty(de.TenDeThi))
                    {
                        txtCH_TenDeThi.Text = de.TenDeThi;
                        materialLabel45.Text = "Đang chọn: " + de.TenDeThi;
                    }
                }

                SyncSelectedRowInDgvDanhSachDeThi(deID);
            }
            else
            {
                _selectedDeThiID = 0;
                _selectedCauHoiID = 0;
                dgvCauHoi.DataSource = null;
                lblCH_TongCauHoi.Text = "Số câu hỏi: 0";
                materialLabel45.Text = "Đang chọn: chưa chọn đề thi";
                txtDT_SoCauHoi.Clear();
            }
        }

        private void SyncSelectedRowInDgvDanhSachDeThi(int deID)
        {
            // Pager chọn/cuộn ở lượt UI kế tiếp; không cuộn trực tiếp trong sự kiện ComboBox.
            DataGridViewPager.ShowItem(dgvGVDeThi, "DeThiID", deID);
        }

        // -----------------------------------------------------------------------
        // dgvGVDeThi_CellClick (Tab Quản Lý Đề Thi):
        //   Khi chọn một đề thi trong bảng, hiển thị số câu hiện có vào txtDT_SoCauHoi.
        // -----------------------------------------------------------------------
        private void dgvGVDeThi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvGVDeThi.Rows.Count) return;
            if (!(dgvGVDeThi.Rows[e.RowIndex].DataBoundItem is DataRowView row)) return;
            int deID = Convert.ToInt32(row["DeThiID"]);
            if (cmbDT_DeThi.SelectedValue is int currentID && currentID == deID)
                cmbDT_DeThi_SelectedIndexChanged(null, EventArgs.Empty);
            else
                cmbDT_DeThi.SelectedValue = deID;
        }

        private void dgvGVDeThi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvGVDeThi.Rows.Count) return;

            var row = dgvGVDeThi.Rows[e.RowIndex];
            if (row.DataBoundItem is DataRowView drv)
            {
                int deID = 0;
                if (drv.Row.Table.Columns.Contains("DeThiID") && drv["DeThiID"] != DBNull.Value)
                    deID = Convert.ToInt32(drv["DeThiID"]);
                else if (drv.Row.Table.Columns.Contains("dethiid") && drv["dethiid"] != DBNull.Value)
                    deID = Convert.ToInt32(drv["dethiid"]);

                if (deID > 0)
                {
                    _selectedDeThiID = deID;
                    try { cmbDT_DeThi.SelectedValue = deID; } catch { }

                    // Chuyển sang Tab Câu Hỏi để giáo viên xem/soạn câu hỏi cho đề này
                    if (pageGV_NganHangCauHoi != null && materialTabControl1 != null)
                    {
                        materialTabControl1.SelectedTab = pageGV_NganHangCauHoi;
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // btnDT_GiaoBaiThi (Tab Quản Lý Đề Thi):
        //   Giao đề thi đang chọn trong bảng cho lớp học được chọn.
        //   txtDT_SoCauHoi hiển thị sẵn số câu, chỉ đọc để giáo viên xem.
        // -----------------------------------------------------------------------
        private void btnDT_GiaoBaiThi_Click(object sender, EventArgs e)
        {
            if (_dangGiaoDeThi || _dangNhapExcel) return;
            _dangGiaoDeThi = true;
            btnDT_GiaoBaiThi.Enabled = false;
            btnDT_GiaoBaiThi.Text = "Đang kiểm tra đề...";
            btnDT_GiaoBaiThi.Refresh();
            try
            {
                // Đọc bản ghi đang chọn, không phụ thuộc mã cache ở tab Câu hỏi.
                var exam = cmbDT_DeThi.SelectedItem as DataRowView;
                var classroom = cmbDT_GiaoChoLop.SelectedItem as DataRowView;
                int deNguonID = exam == null ? 0 : Convert.ToInt32(exam["DeThiID"]);
                int lopHocID = classroom == null ? 0 : Convert.ToInt32(classroom["LopHocID"]);
                if (deNguonID <= 0 || lopHocID <= 0)
                {
                    MessageBox.Show(this, "Vui lòng chọn đề thi và lớp nhận bài trong hai ô Đề thi, Giao cho lớp.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int soCauHienCo = _deThiService.DemSoCauHoiHienTai(deNguonID);
                if (soCauHienCo <= 0)
                {
                    MessageBox.Show(this, "Đề đang chọn chưa có câu hỏi đã lưu. Hãy chọn đề đã soạn câu hỏi hoặc thêm câu hỏi vào đúng đề này.",
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int.TryParse(txtDT_ThoiGian?.Text, out int thoiGian);
                if (thoiGian <= 0)
                {
                    MessageBox.Show(this, "Vui lòng nhập thời gian làm bài lớn hơn 0 phút.", "Thông báo");
                    return;
                }
                string monHocGiao = (cmbDT_MonHoc.SelectedItem?.ToString() ?? cmbDT_MonHoc.Text).Trim();
                if (string.IsNullOrWhiteSpace(monHocGiao) || monHocGiao.Length > 100)
                {
                    MessageBox.Show(this, "Vui lòng chọn môn học hợp lệ trước khi giao đề.", "Thông báo");
                    return;
                }

                string tenLop = ExamSelection.ClassDisplayName(classroom.Row);
                if (MessageBox.Show(this,
                        $"Giao đề \"{exam["TenDeThi"]}\" cho lớp {tenLop}?\nMôn học: {monHocGiao}\nSố câu: {soCauHienCo} | Thời gian: {thoiGian} phút\nMỗi lần giao là một bài thi riêng, hạn nộp sau 7 ngày.",
                        "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                    return;

                UseWaitCursor = true;
                btnDT_GiaoBaiThi.Text = "Đang giao bài...";
                int lanGiaoID = _deThiService.TaoLanGiaoDeThi(Session.TaiKhoanID, Session.NguoiDungID,
                    deNguonID, lopHocID, thoiGian, DateTime.Now.AddDays(7), monHocGiao);
                UseWaitCursor = false;
                MessageBox.Show(this, $"Đã giao đề cho lớp {tenLop} (lượt #{lanGiaoID}).\nHọc viên được duyệt vào lớp trước hoặc sau khi giao đều thấy bài thi này.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                try
                {
                    LoadDeThiCombobox(lanGiaoID);
                    cmbDT_GiaoChoLop.SelectedValue = lopHocID;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, $"Lượt giao #{lanGiaoID} đã lưu, nhưng chưa tải lại được danh sách. Không cần giao lại.\n" + ex.Message,
                        "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "Không thể giao đề thi: " + ex.Message,
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                UseWaitCursor = false;
                btnDT_GiaoBaiThi.Text = "Giao Bài Thi";
                btnDT_GiaoBaiThi.Enabled = true;
                _dangGiaoDeThi = false;
            }
        }

        // -----------------------------------------------------------------------
        // btnDT_XoaDeThi (Tab Quản Lý Đề Thi):
        //   Ẩn đề/lượt đang chọn, giữ lại kết quả và câu hỏi để xem lịch sử bài làm.
        // -----------------------------------------------------------------------
        private void btnDT_XoaDeThi_Click(object sender, EventArgs e)
        {
            // 1. Xác định ID đề thi cần xóa
            int deID = _selectedDeThiID;
            if (deID <= 0 && cmbDT_DeThi.SelectedValue != null)
            {
                int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out deID);
            }

            if (deID <= 0 && dgvGVDeThi.CurrentRow != null && dgvGVDeThi.CurrentRow.DataBoundItem is DataRowView drv)
            {
                string colName = drv.Row.Table.Columns.Contains("DeThiID") ? "DeThiID" : "dethiid";
                if (drv.Row.Table.Columns.Contains(colName) && drv[colName] != DBNull.Value)
                {
                    deID = Convert.ToInt32(drv[colName]);
                }
            }

            if (deID <= 0)
            {
                MessageBox.Show("Vui lòng chọn đề thi cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Lấy thông tin đề thi để hiển thị xác nhận và kiểm tra
            DeThi de = _deThiService.LayTheoID(deID);
            string tenDeThi = de?.TenDeThi ?? $"Mã {deID}";

            // 4. Hỏi xác nhận người dùng
            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa đề thi \"{tenDeThi}\" (Mã đề: {deID}) khỏi danh sách giao bài không?\n\nĐiểm, đáp án và lịch sử bài làm của học sinh vẫn được giữ để xem lại.",
                "Xác nhận xóa đề thi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            // 5. Thực hiện xóa
            if (_deThiService.XoaDeThi(deID, Session.TaiKhoanID, Session.NguoiDungID, out string thongBao))
            {
                MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                _selectedDeThiID = 0;
                txtDT_SoCauHoi.Clear();
                txtDT_ThoiGian.Clear();
                if (txtCH_TenDeThi != null) txtCH_TenDeThi.Clear();
                dgvCauHoi.DataSource = null;

                LoadDeThiCombobox();
            }
            else
            {
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        #endregion

        #region 6. Tab Kết Quả Thi & Xuất Báo Cáo

        private void cmbKQ_DeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dangTaiDeThi) return;
            if (cmbKQ_DeThi.SelectedValue != null && int.TryParse(cmbKQ_DeThi.SelectedValue.ToString(), out int deID))
                _ketQua = _deThiService.LayKetQuaTheoDeThi(deID);
            else
                _ketQua = null;
            HienThiKetQua();
        }

        private void txtKQ_TenHocVien_TextChanged(object sender, EventArgs e)
        {
            HienThiKetQua();
        }

        private void HienThiKetQua()
        {
            if (_ketQua == null)
            {
                dgvKetQua.DataSource = null;
                lblKQ_SoBaiNop.Text = "Số bài nộp: 0";
                return;
            }
            var view = new DataView(_ketQua);
            string ten = txtKQ_TenHocVien.Text.Trim().Replace("'", "''").Replace("[", "[[]").Replace("%", "[%]").Replace("*", "[*]");
            if (ten.Length > 0) view.RowFilter = $"HoTen LIKE '%{ten}%'";
            dgvKetQua.DataSource = view;
            lblKQ_SoBaiNop.Text = $"Số bài nộp: {view.Count:N0}";
        }

        private void dgvKetQua_SelectionChanged(object sender, EventArgs e)
        {
            btnKQ_XuatBaoCao.Enabled = dgvKetQua.CurrentRow?.DataBoundItem is DataRowView;
        }

        private void btnKQ_XuatExcel_Click(object sender, EventArgs e)
        {
            if (dgvKetQua.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenDeThi = cmbKQ_DeThi.Text;
            if (string.IsNullOrWhiteSpace(tenDeThi) || tenDeThi == "Đề Thi")
            {
                tenDeThi = "Tất cả đề thi";
            }

            string subTitle = $"Đề thi: {tenDeThi} | Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm} | Người xuất: {Session.TenDangNhap}";

            ExcelExportHelper.ExportDataGridView(
                dgvKetQua,
                $"KetQuaThi_{DateTime.Now:yyyyMMdd_HHmmss}.xls",
                "BÁO CÁO KẾT QUẢ BÀI THI",
                subTitle
            );
        }

        private void btnKQ_XuatBaoCao_Click(object sender, EventArgs e)
        {
            // Kiểm tra đã chọn dòng học sinh nào trong grid chưa
            if (dgvKetQua.CurrentRow == null || dgvKetQua.Rows.Count == 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn một học sinh trong danh sách trước khi xuất báo cáo!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DataGridViewRow selectedRow = dgvKetQua.CurrentRow;

            // Lấy HocSinhID từ DataSource của grid
            int hocSinhID = 0;
            string tenHocSinh = "";
            try
            {
                DataRowView drv = selectedRow.DataBoundItem as DataRowView;
                if (drv != null)
                {
                    hocSinhID  = Convert.ToInt32(drv["HocSinhID"]);
                    tenHocSinh = drv["HoTen"]?.ToString() ?? "";
                }
            }
            catch
            {
                MessageBox.Show("Không thể lấy thông tin học sinh từ dòng đang chọn!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (hocSinhID <= 0)
            {
                MessageBox.Show("Không xác định được học sinh. Vui lòng chọn lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Report selection keeps the existing Excel path available.
                using (DataTable dtBangDiem = _deThiService.LayBangDiemHocSinh(hocSinhID))
                using (var report = new BLT_Quan_Ly_Hoc_Tap.Forms.BaoCaoDialog(
                    dtBangDiem, tenHocSinh, Session.TenDangNhap))
                    report.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất báo cáo: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 7. Tab Thông Tin Cá Nhân & Đổi Mật Khẩu

        private void LoadThongTinCaNhan()
        {
            try
            {
                GiaoVien gv = new GiaoVienService().LayTheoID(Session.NguoiDungID);
                if (gv != null)
                {
                    txtGVTT_HovaTen.Text = gv.HoTen;
                    txtTTGV_Email.Text = gv.Email;
                    txtTTGV_DienThoai.Text = gv.SoDienThoai;
                    txtTTGV_DiaChi.Text = gv.DiaChi;
                    cmbTTGV_GioiTinh.SelectedIndex = -1;
                    foreach (object item in cmbTTGV_GioiTinh.Items)
                        if (string.Equals(item.ToString(), gv.GioiTinh?.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            cmbTTGV_GioiTinh.SelectedItem = item;
                            break;
                        }
                    HienThiTheHoSo(gv);
                }
                else
                {
                    MessageBox.Show("Không tìm thấy hồ sơ giáo viên đang đăng nhập.", "Thông báo");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể tải hồ sơ giáo viên: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private static string GiaTriHoSo(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? "Chưa cập nhật" : value.Trim();
        }

        private void HienThiTheHoSo(GiaoVien gv)
        {
            string hoTen = string.IsNullOrWhiteSpace(gv.HoTen) ? GiaTriHoSo(Session.TenDangNhap) : gv.HoTen.Trim();
            lblGVTT_HovaTen.Text = hoTen;
            lblTTGV_ChuyenMon.Text = "Chuyên môn: " + GiaTriHoSo(gv.ChuyenMon);
            lblTTGV_Email.Text = "Email: " + GiaTriHoSo(gv.Email);
            lblTTGV_SDT.Text = "Số điện thoại: " + GiaTriHoSo(gv.SoDienThoai);
            Session.HoTen = hoTen;
            lblTCGV_Chao.Text = "Xin chào, " + hoTen;
        }

        private void btnTTGV_LuuThayDoi_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtGVTT_HovaTen.Text))
                {
                    MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGVTT_HovaTen.Focus();
                    return;
                }

                if (!Validation.IsValidEmail(txtTTGV_Email.Text, out string errEmail))
                {
                    MessageBox.Show(errEmail, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTTGV_Email.Focus();
                    return;
                }

                if (!Validation.IsValidPhone(txtTTGV_DienThoai.Text, out string errPhone))
                {
                    MessageBox.Show(errPhone, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtTTGV_DienThoai.Focus();
                    return;
                }

                GiaoVien gv = new GiaoVien
                {
                    GiaoVienID = Session.NguoiDungID,
                    HoTen = txtGVTT_HovaTen.Text.Trim(),
                    Email = txtTTGV_Email.Text.Trim(),
                    SoDienThoai = txtTTGV_DienThoai.Text.Trim(),
                    DiaChi = txtTTGV_DiaChi.Text.Trim(),
                    GioiTinh = cmbTTGV_GioiTinh.SelectedItem?.ToString()
                };

                if (new GiaoVienService().CapNhatHoSoCaNhan(Session.TaiKhoanID, gv))
                {
                    MessageBox.Show("Cập nhật thông tin cá nhân thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    // Đọc lại cả chuyên môn được giữ nguyên trong database, thay vì dùng model chỉnh sửa thiếu trường này.
                    LoadThongTinCaNhan();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnTTGV_DoiMK_Click(object sender, EventArgs e)
        {
            string cu = txtTTGV_MatKhauHienTai.Text;
            string moi = txtTTGV_MKMoi.Text;
            if (!Validation.IsValidPasswordChange(cu, moi, txtTTGV_XacNhanMK.Text, out string loi))
            {
                MessageBox.Show(loi, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            try
            {
                if (!_tkService.DoiMatKhauGiaoVien(Session.TaiKhoanID, cu, moi))
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng hoặc tài khoản đã bị khóa.", "Không thể đổi mật khẩu");
                    return;
                }
                _tkService.XoaGhiNho(Session.TenDangNhap);
                txtTTGV_MatKhauHienTai.Clear();
                txtTTGV_MKMoi.Clear();
                txtTTGV_XacNhanMK.Clear();
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể đổi mật khẩu: " + ex.Message, "Lỗi");
            }
        }

        #endregion

        #region 8. Điều Hướng & Đóng Form

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_daTaiTrangChu) return;
            if (materialTabControl1.SelectedTab == null) return;

            string tabText = materialTabControl1.SelectedTab.Text.Trim();

            // Xử lý đăng xuất
            if (tabText.Contains("Đăng Xuất") || materialTabControl1.SelectedTab == pageGV_DangXuat)
            {
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Session.Clear();
                    frm_DangNhap loginForm = Application.OpenForms["frm_DangNhap"] as frm_DangNhap;
                    if (loginForm == null || loginForm.IsDisposed)
                    {
                        loginForm = new frm_DangNhap();
                    }
                    loginForm.Show();
                    loginForm.BringToFront();
                    this.Hide();
                }
                else
                {
                    materialTabControl1.SelectedIndex = 0;
                }
                return;
            }

            // Tự động load dữ liệu mới nhất khi chuyển tab
            if (materialTabControl1.SelectedTab == pageGV_TrangChu)
            {
                // SelectedIndexChanged có thể chạy ngay trong InitializeComponent.
                if (_daTaiTrangChu)
                {
                    LoadTrangChu();
                }
            }
            else if (materialTabControl1.SelectedTab == pageGV_LopHoc)
            {
                LoadDanhSachLopQuanLy();
            }
            else if (materialTabControl1.SelectedTab == pageGV_DuyetHocVien)
            {
                LoadDanhSachLopQuanLy();
                cboDK_LocTheoLop_SelectedIndexChanged(null, null);
            }
            else if (materialTabControl1.SelectedTab == pageGV_NganHangCauHoi)
            {
                LoadMonHocGiaoVien();
                LoadDeThiCombobox();
                cmbDT_DeThi_SelectedIndexChanged(null, null);
            }
            else if (materialTabControl1.SelectedTab == pageGV_QuanLyDeThi)
            {
                LoadMonHocGiaoVien();
                LoadDanhSachLopQuanLy();
                LoadDeThiCombobox();
            }
            else if (materialTabControl1.SelectedTab == pageGV_KetQuaThi)
            {
                LoadDeThiCombobox();
                cmbKQ_DeThi_SelectedIndexChanged(null, null);
            }
            else if (materialTabControl1.SelectedTab == pageGV_ThongTin)
            {
                LoadThongTinCaNhan();
            }
        }

        private void Frm_GiaoVien_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (Application.OpenForms["frm_DangNhap"] != null && Application.OpenForms["frm_DangNhap"].Visible)
            {
                return;
            }

            bool hasVisibleForm = false;
            foreach (Form f in Application.OpenForms)
            {
                if (f.Visible && f != this)
                {
                    hasVisibleForm = true;
                    break;
                }
            }
            if (!hasVisibleForm)
            {
                Application.Exit();
            }
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);
            foreach (Form f in Application.OpenForms)
            {
                if (f.Visible && f != this)
                    return;
            }
            Application.Exit();
        }

        #endregion

    }
}
