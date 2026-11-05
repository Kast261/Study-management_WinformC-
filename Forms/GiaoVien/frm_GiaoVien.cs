using BLT_Quan_Ly_Hoc_Tap.DAL;
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

        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KhoaHocDAL _khDAL = new KhoaHocDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();
        private int _selectedLopHocID = 0;
        private int _selectedCauHoiID = 0;
        private int _selectedDeThiID   = 0;  
        private bool _isSyncingMonHoc = false;
        private bool _dangNhapExcel;
        private bool _daTaiTrangChu;
        private DataTable _khoaHoc;
        private DataTable _lopHoc;
        private DataTable _ketQua;
        private bool _isEventHandlersSetup = false;

        #endregion

        #region 1. Khởi tạo & Đăng ký sự kiện

        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            CauHinhLuoi();
            CauHinhNhapLieu();
            CauHinhKetQuaThiGanDay();
            DataGridViewPager.AttachAll(this, KhiDoiTrang);
            this.DrawerTabControl = materialTabControl1;
            this.FormClosed += Frm_GiaoVien_FormClosed;
            SetupEventHandlers();
        }

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            try
            {
                lblGV_LoiChao.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";
                LoadKetQuaThiGanDay();
                _daTaiTrangChu = true;

                InitDropdowns();
               

                // Load dữ liệu ban đầu cho các tab
                LoadLopHocCuaToi();
                LoadDanhSachLopQuanLy();
                cboDK_LocTheoLop_SelectedIndexChanged(null, null);
                LoadDeThiCombobox();
                LoadThongTinCaNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trang giáo viên: " + ex.Message, "Thông báo");
            }
        }

        private void CauHinhLuoi()
        {
            dgvGV_LopCuaToi.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn7.DataPropertyName = "TenLop";
            dataGridViewTextBoxColumn8.DataPropertyName = "SiSo";
            dataGridViewTextBoxColumn9.DataPropertyName = "LichHoc";
            dgvLopHoc.AutoGenerateColumns = false;
            maGV.DataPropertyName = "TenLop";
            HoTen.DataPropertyName = "TenKhoaHoc";
            Column1.DataPropertyName = "SiSo";
            Column2.DataPropertyName = "TrangThai";
            Column3.DataPropertyName = "NgayBatDau";
            Column4.DataPropertyName = "NgayKetThuc";
            Column8.DataPropertyName = "LichHoc";
            Column3.DefaultCellStyle.Format = Column4.DefaultCellStyle.Format = "dd/MM/yyyy";
            Column3.DefaultCellStyle.NullValue = Column4.DefaultCellStyle.NullValue = "";
            dgvDSYeuCau.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn13.DataPropertyName = "HoTen";
            dataGridViewTextBoxColumn14.DataPropertyName = "Email";
            dataGridViewTextBoxColumn15.DataPropertyName = "SoDienThoai";
            dataGridViewTextBoxColumn16.DataPropertyName = "TenLop";
            dataGridViewTextBoxColumn17.DataPropertyName = "SiSo";
            dataGridViewTextBoxColumn18.DataPropertyName = "NgayDangKy";
            dataGridViewTextBoxColumn18.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dgvGVDeThi.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn19.DataPropertyName = "TenDeThi";
            dataGridViewTextBoxColumn20.DataPropertyName = "MonHoc";
            dataGridViewTextBoxColumn21.DataPropertyName = "ThoiGian";
            dataGridViewTextBoxColumn22.DataPropertyName = "TenLop";
            dataGridViewTextBoxColumn32.DataPropertyName = "SoCau";
            dgvKetQua.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn25.DataPropertyName = "HoTen";
            dataGridViewTextBoxColumn26.DataPropertyName = "TenDeThi";
            dataGridViewTextBoxColumn27.DataPropertyName = "SoCauDung";
            dataGridViewTextBoxColumn28.DataPropertyName = "Diem";
            dataGridViewTextBoxColumn29.DataPropertyName = "ThoiGianNop";
            dataGridViewTextBoxColumn28.DefaultCellStyle.Format = "0.00";
            dataGridViewTextBoxColumn29.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            CauHinhLuoiCauHoi();
            foreach (DataGridView grid in new[] { dgvLopHoc, dgvCauHoi, dgvGVDeThi, dgvDSYeuCau, dgvKetQua })
            {
                grid.ReadOnly = true;
                grid.MultiSelect = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            }
        }

        private void CauHinhNhapLieu()
        {
            txtLH_TenLop.Clear();
            txtLH_KhoaHoc.Clear();
            txtLH_TimKiem.Text = "";
            txtLH_SoDangKi.ReadOnly = true;
            txtDT_SoCauHoi.ReadOnly = true;
            nUDLH_SiSo.Minimum = 1;
            nUDLH_SiSo.Maximum = 500;
            nUDLH_SiSo.Value = 40;
            DatePickerInput.Configure(dateLH_NgayBatDau);
            DatePickerInput.Configure(dateLH_NgayKetThuc);
            txtCH_TenDeThi.Hint = "Tên đề mới / chọn đề có sẵn ở tab Đề Thi";
            txtCH_TenDeThi.Clear();
            txtCH_NhapCauHoi.Clear();
            foreach (var input in new[] { txtTTGV_MatKhauHienTai, txtTTGV_MKMoi, txtTTGV_XacNhanMK })
                input.UseSystemPasswordChar = true;
            lblGVTT_HovaTen.Text = Session.HoTen ?? Session.TenDangNhap ?? "";
            materialLabel54.Text = "GV";
            txtLH_SoDangKi.Text = "0";
            lblDK_ChonHS.Text = "Đang chọn: chưa chọn học viên";
            lblDK_SoChoDuyet.Text = "Số yêu cầu chờ duyệt: 0";
            lblCH_TongCauHoi.Text = "Số câu hỏi: 0";
            lblDT_SoDeThi.Text = "Số đề thi: 0";
            lblKQ_SoBaiNop.Text = "Số bài nộp: 0";
        }

        private void InitDropdowns()
        {
            // Dropdown đáp án đúng của câu hỏi
            cmb_DapAn.Items.Clear();
            cmb_DapAn.Items.AddRange(new object[] { "A", "B", "C", "D" });
            cmb_DapAn.SelectedIndex = 0;

            // Giới tính profile
            cmbTTGV_GioiTinh.Items.Clear();
            cmbTTGV_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cmbTTGV_GioiTinh.SelectedIndex = 0;

            // Khóa học cho form Lớp
            _khoaHoc = _khDAL.LayTatCa();
            var tenKhoaHoc = new AutoCompleteStringCollection();
            foreach (DataRow row in _khoaHoc.Rows) tenKhoaHoc.Add(row["TenKhoaHoc"].ToString());
            txtLH_KhoaHoc.AutoCompleteCustomSource = tenKhoaHoc;
            txtLH_KhoaHoc.AutoCompleteSource = AutoCompleteSource.CustomSource;
            txtLH_KhoaHoc.AutoCompleteMode = AutoCompleteMode.SuggestAppend;

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
                DataTable dtMonHoc = _khDAL.LayKhoaHocTheoGiaoVien(Session.NguoiDungID);

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
                FilterLopTheoMonHoc(cmbDT_MonHoc.SelectedItem?.ToString());
            }
            catch (Exception ex)
            {
                _isSyncingMonHoc = false;
                System.Diagnostics.Debug.WriteLine("Lỗi nạp môn học giáo viên: " + ex.Message);
            }
        }

        private void SetupEventHandlers()
        {
            if (_isEventHandlersSetup) return;
            _isEventHandlersSetup = true;

            // Tab Lớp học
            btnLH_Them.Click += btnLH_Them_Click;
            btnLH_CapNhap.Click += btnLH_CapNhap_Click;
            btnLH_DongDK.Click += btnLH_DongDK_Click;
            btnLH_TaiLai.Click += btnLH_TaiLai_Click;
            txtLH_TimKiem.TextChanged += txtLH_TimKiem_TextChanged;
            dgvDSYeuCau.SelectionChanged += dgvDSYeuCau_SelectionChanged;
            dgvKetQua.SelectionChanged += dgvKetQua_SelectionChanged;
            txtKQ_TenHocVien.TextChanged += (sender, e) => HienThiKetQua();
            dgvLopHoc.CellClick += dgvLopHoc_CellClick;

            // Tab Duyệt đăng ký
            cboDK_LocTheoLop.SelectedIndexChanged += cboDK_LocTheoLop_SelectedIndexChanged;
            btnDK_Duyet.Click += btnDK_Duyet_Click;
            btnDK_TuChoi.Click += btnDK_TuChoi_Click;

            // Tab Câu hỏi
            cmbDT_DeThi.SelectedIndexChanged += cmbDT_DeThi_SelectedIndexChanged;
            cmbCH_MonHoc.SelectedIndexChanged += cmbCH_MonHoc_SelectedIndexChanged;
            btnCH_ThemCH.Click += btnCH_ThemCH_Click;
            btnCH_CapNhap.Click += btnCH_CapNhap_Click;
            btnCH_XoaCH.Click += btnCH_XoaCH_Click;
            btnCH_Excel.Click += BtnCH_Excel_Click;
            if (components == null) components = new System.ComponentModel.Container();
            var excelMenu = new ContextMenuStrip(components);
            excelMenu.Items.Add("Lưu file Excel mẫu...", null, LuuFileExcelMau_Click);
            btnCH_Excel.ContextMenuStrip = excelMenu;
            var excelTip = new ToolTip(components);
            excelTip.SetToolTip(btnCH_Excel, "Nhập câu hỏi từ .xlsx hoặc .xls. Nhấp chuột phải để lưu file Excel mẫu.");
            dgvCauHoi.CellClick += dgvCauHoi_CellClick;

            // Tab Câu hỏi – Tạo đề thi mới
            btnCH_TaoDe.Click += btnCH_TaoDe_Click;

            // Tab Đề thi – Giao đề thi đã tạo cho lớp & Xóa đề thi
            cmbDT_MonHoc.SelectedIndexChanged += cmbDT_MonHoc_SelectedIndexChanged;
            btnDT_GiaoBaiThi.Click        += btnDT_GiaoBaiThi_Click;
            btnDT_XoaDeThi.Click        += btnDT_XoaDeThi_Click;
            dgvGVDeThi.CellClick  += dgvGVDeThi_CellClick;
            dgvGVDeThi.CellDoubleClick += dgvGVDeThi_CellDoubleClick;

            // Tab Kết quả thi
            cmbKQ_DeThi.SelectedIndexChanged += cmbKQ_DeThi_SelectedIndexChanged;
            btnKQ_XuatExcel.Click += btnKQ_XuatExcel_Click;
            btnKQ_XuatBaoCao.Click   += btnKQ_XuatBaoCao_Click;

            // Tab Thông tin cá nhân & Mật khẩu
            btnTTGV_LuuThayDoi.Click += btnTTGV_LuuThayDoi_Click;
            btnTTGV_DoiMK.Click += btnTTGV_DoiMK_Click;
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
            if (grid == dgvGVDeThi)
            {
                _selectedDeThiID = 0;
                if (reason != PageChangeReason.Data)
                {
                    cmbDT_DeThi.SelectedIndex = -1;
                    dgvCauHoi.DataSource = null;
                }
            }
        }

        #endregion

        #region 2. Tab Trang Chủ & Quản Lý Lớp Học

        private void CauHinhKetQuaThiGanDay()
        {
            // Cấu hình trước khi AttachAll bind nguồn rỗng để giữ nguyên cột Designer.
            dgvGV_LichDay.AutoGenerateColumns = false;
            dgvGV_LichDay.ReadOnly = true;
            dgvGV_LichDay.AllowUserToDeleteRows = false;
            dataGridViewTextBoxColumn10.DataPropertyName = "TenDeThi";
            dataGridViewTextBoxColumn11.DataPropertyName = "DaNop";
            dataGridViewTextBoxColumn11.DefaultCellStyle.Format = "N0";
            dataGridViewTextBoxColumn12.DataPropertyName = "DiemTrungBinh";
            dataGridViewTextBoxColumn12.DefaultCellStyle.Format = "0.00";
            dataGridViewTextBoxColumn12.DefaultCellStyle.NullValue = "—";
        }

        private void LoadKetQuaThiGanDay()
        {
            try
            {
                DataTable results = _deThiDAL.LayTongHopKetQuaGanDay(Session.NguoiDungID);
                dgvGV_LichDay.DataSource = results;
                materialLabel34.Text = results.Rows.Count > 0
                    ? "Kết quả bài thi gần đây"
                    : "Kết quả bài thi gần đây — chưa có bài nộp";
            }
            catch (Exception ex)
            {
                dgvGV_LichDay.DataSource = null;
                materialLabel34.Text = "Kết quả bài thi gần đây — không tải được";
                MessageBox.Show("Không thể tải kết quả thi gần đây: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLopHocCuaToi()
        {
            try
            {
                DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                if (dgvGV_LopCuaToi != null) dgvGV_LopCuaToi.DataSource = dt;
            }
            catch { }
        }

        private void LoadDanhSachLopQuanLy()
        {
            try
            {
                DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                _lopHoc = dt;
                HienThiLopHoc();

                // Load danh sách lớp vào combobox duyệt & giao bài thi
                DataTable dtDuyet = dt.Copy();
                DataRow drAll = dtDuyet.NewRow();
                drAll["LopHocID"] = 0;
                drAll["TenLop"] = "-- Tất cả các lớp --";
                dtDuyet.Rows.InsertAt(drAll, 0);

                cboDK_LocTheoLop.DataSource = dtDuyet;
                cboDK_LocTheoLop.DisplayMember = "TenLop";
                cboDK_LocTheoLop.ValueMember = "LopHocID";

                // Cập nhật lớp học cho form Đề thi theo môn học hiện tại
                FilterLopTheoMonHoc(cmbDT_MonHoc.SelectedItem?.ToString());
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

            if (_lopDAL.ThemLop(lop))
            {
                MessageBox.Show("Tạo lớp học mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopQuanLy();
                LoadLopHocCuaToi();
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

            if (_lopDAL.SuaLop(lop))
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
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần đóng đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Đóng đăng ký cho lớp này? Học viên mới sẽ không thể đăng ký vào nữa.", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.DongDangKyLop(_selectedLopHocID))
                {
                    MessageBox.Show("Đã đóng đăng ký lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDanhSachLopQuanLy();
                }
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
            LoadLopHocCuaToi();
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

                DataTable dt = _lopDAL.LayDangKyChoDuyet(Session.NguoiDungID, lopID);

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

                    if (_lopDAL.DuyetDangKy(dkID, lopID, hsID, Session.NguoiDungID))
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
                        if (_lopDAL.TuChoiDangKy(dkID, Session.NguoiDungID))
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

        private void CauHinhLuoiCauHoi()
        {
            dgvCauHoi.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn23.DataPropertyName = "DeThiID";
            dataGridViewTextBoxColumn24.DataPropertyName = "NoiDung";
            dataGridViewTextBoxColumn30.DataPropertyName = "DapAnDung";
            dataGridViewTextBoxColumn31.DataPropertyName = "ThuTu";
        }

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
                    FilterLopTheoMonHoc(monHoc);
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

            DeThi de = new DeThi
            {
                TenDeThi   = tenDe,
                MonHoc     = monHoc,
                GiaoVienID = Session.NguoiDungID,
                SoCau      = 0,
                ThoiGian   = 45,
                TrangThai  = "Chưa giao"
            };

            int newID = _deThiDAL.TaoDeThiMoiLayID(de);
            if (newID > 0)
            {
                MessageBox.Show(
                    $"Tạo đề thi \"{ tenDe }\" ({ monHoc }) thành công!\nBây giờ bạn có thể nhập các câu hỏi kèm đáp án cho đề thi này.",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Làm mới danh sách đề thi và chọn đề vừa tạo
                _selectedDeThiID = newID;
                LoadDeThiCombobox();
                try
                {
                    cmbDT_DeThi.SelectedValue = newID;
                    _selectedDeThiID = newID;
                }
                catch { }

                // Reset form câu hỏi để bắt đầu nhập câu mới
                LamMoiFormCauHoi();

                // Nạp lưới câu hỏi (lúc mới tạo là 0 câu)
                dgvCauHoi.DataSource = _deThiDAL.LayDanhSachCauHoi(newID);

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
                int deID = 0;

                // Nếu người dùng có nhập tên đề vào ô txtCH_TenDeThi
                if (!string.IsNullOrEmpty(tenDeNhap))
                {
                    // Kiểm tra xem đề thi này đã có trong danh sách chưa
                    if (cmbDT_DeThi.DataSource is DataTable dtDe)
                    {
                        foreach (DataRow r in dtDe.Rows)
                        {
                            if (r["TenDeThi"]?.ToString()?.Trim().Equals(tenDeNhap, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                deID = Convert.ToInt32(r["DeThiID"]);
                                cmbDT_DeThi.SelectedValue = deID;
                                _selectedDeThiID = deID;
                                break;
                            }
                        }
                    }

                    // Nếu chưa có đề thi này, tự động tạo mới luôn
                    if (deID <= 0)
                    {
                        string monHoc = cmbCH_MonHoc.SelectedItem?.ToString() ?? cmbCH_MonHoc.Text?.Trim();
                        if (string.IsNullOrWhiteSpace(monHoc))
                            monHoc = cmbDT_MonHoc.SelectedItem?.ToString() ?? "Lập trình C# WinForms";

                        DeThi deMoi = new DeThi
                        {
                            TenDeThi = tenDeNhap,
                            MonHoc = monHoc,
                            GiaoVienID = Session.NguoiDungID,
                            SoCau = 0,
                            ThoiGian = 45,
                            TrangThai = "Chưa giao"
                        };
                        deID = _deThiDAL.TaoDeThiMoiLayID(deMoi);
                        if (deID > 0)
                        {
                            LoadDeThiCombobox();
                            try { cmbDT_DeThi.SelectedValue = deID; _selectedDeThiID = deID; } catch { }
                        }
                    }
                }

                // Nếu không gõ ô txtCH_TenDeThi thì lấy từ ComboBox
                if (deID <= 0 && cmbDT_DeThi.SelectedValue != null)
                {
                    int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out deID);
                }

                if (deID <= 0)
                {
                    MessageBox.Show("Vui lòng nhập tên đề thi hoặc chọn đề thi cần thêm câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int soCauHienCo = _deThiDAL.DemSoCauHoiHienTai(deID);

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

                if (_deThiDAL.ThemCauHoi(ch, Session.NguoiDungID))
                {
                    _deThiDAL.CapNhatSoCau(deID);
                    MessageBox.Show($"Thêm câu hỏi thứ {ch.ThuTu} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Đồng bộ cập nhật lại danh sách đề thi ở cả 2 tab
                    LoadDeThiCombobox();
                    try { cmbDT_DeThi.SelectedValue = deID; } catch { }
                    cmbDT_DeThi_SelectedIndexChanged(null, null);
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

                if (_deThiDAL.SuaCauHoi(ch, Session.NguoiDungID))
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
                    if (_deThiDAL.XoaCauHoi(_selectedCauHoiID, Session.NguoiDungID))
                    {
                        if (cmbDT_DeThi.SelectedValue != null && int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out int deID))
                        {
                            _deThiDAL.CapNhatSoCau(deID);
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
                btnCH_TaoDe, cmbDT_DeThi, cmbCH_MonHoc, txtCH_TenDeThi };
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
                KetQuaNhapCauHoi result = await Task.Run(() => new CauHoiImportDAL().Nhap(taiKhoanID, target, questions));
                if (IsDisposed || Disposing || Session.TaiKhoanID != taiKhoanID || Session.NguoiDungID != giaoVienID) return;
                try
                {
                    _selectedDeThiID = result.DeThiID;
                    LoadDeThiCombobox();
                    cmbDT_DeThi.SelectedValue = result.DeThiID;
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

        private void LoadDeThiCombobox()
        {
            try
            {
                int currentDeID = 0;
                if (cmbDT_DeThi.SelectedValue != null)
                {
                    int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out currentDeID);
                }
                if (currentDeID <= 0 && _selectedDeThiID > 0)
                {
                    currentDeID = _selectedDeThiID;
                }

                DataTable dt = _deThiDAL.LayDeThiTheoGiaoVien(Session.NguoiDungID);

                // Cài đặt DisplayMember & ValueMember TRƯỚC khi gán DataSource
                cmbDT_DeThi.DisplayMember = "TenDeThi";
                cmbDT_DeThi.ValueMember = "DeThiID";
                cmbDT_DeThi.DataSource = dt.Copy();

                cmbKQ_DeThi.DisplayMember = "TenDeThi";
                cmbKQ_DeThi.ValueMember = "DeThiID";
                cmbKQ_DeThi.DataSource = dt.Copy();

                // Chỉ lấy các đề thi đã có câu hỏi
                DataTable deThiCoCauHoi = dt.Clone();

                foreach (DataRow r in dt.Rows)
                {
                    if (r["SoCau"] != DBNull.Value &&
                        Convert.ToInt32(r["SoCau"]) > 0)
                    {
                        deThiCoCauHoi.ImportRow(r);
                    }
                }

                // Hiển thị danh sách đề thi có câu hỏi
                dgvGVDeThi.DataSource = null;
                dgvGVDeThi.DataSource = deThiCoCauHoi;
                lblDT_SoDeThi.Text = $"Số đề thi: {deThiCoCauHoi.Rows.Count:N0}";

                // Khôi phục lại đề thi đang chọn nếu có
                if (currentDeID > 0)
                {
                    try { cmbDT_DeThi.SelectedValue = currentDeID; } catch { }
                    _selectedDeThiID = currentDeID;
                }
                else if (dt.Rows.Count > 0)
                {
                    _selectedDeThiID = Convert.ToInt32(dt.Rows[0]["DeThiID"]);
                }

                // Cập nhật số câu hiển thị
                if (_selectedDeThiID > 0 && dt != null)
                {
                    DataGridViewPager.ShowItem(dgvGVDeThi, "DeThiID", _selectedDeThiID);
                    // Đọc metadata từ DataTable gốc, không từ các hàng đang chờ bind.
                    foreach (DataRow rowView in dt.Rows)
                    {
                        string colName = rowView.Table.Columns.Contains("DeThiID") ? "DeThiID" : "dethiid";
                        if (rowView.Table.Columns.Contains(colName) &&
                            rowView[colName] != DBNull.Value &&
                            Convert.ToInt32(rowView[colName]) == _selectedDeThiID)
                        {
                            txtDT_SoCauHoi.Text = _deThiDAL.DemSoCauHoiHienTai(_selectedDeThiID).ToString();

                            string monHoc = rowView.Table.Columns.Contains("MonHoc") ? rowView["MonHoc"]?.ToString() : "";
                            if (!string.IsNullOrEmpty(monHoc))
                            {
                                _isSyncingMonHoc = true;
                                if (cmbDT_MonHoc.Items.Contains(monHoc)) cmbDT_MonHoc.SelectedItem = monHoc;
                                else cmbDT_MonHoc.Text = monHoc;

                                if (cmbCH_MonHoc.Items.Contains(monHoc)) cmbCH_MonHoc.SelectedItem = monHoc;
                                else cmbCH_MonHoc.Text = monHoc;
                                _isSyncingMonHoc = false;

                                FilterLopTheoMonHoc(monHoc);
                            }

                            if (rowView.Table.Columns.Contains("ThoiGian") && rowView["ThoiGian"] != DBNull.Value)
                                txtDT_ThoiGian.Text = rowView["ThoiGian"].ToString();

                            if (rowView.Table.Columns.Contains("TenDeThi") && txtCH_TenDeThi != null)
                                txtCH_TenDeThi.Text = rowView["TenDeThi"]?.ToString() ?? "";

                            if (rowView.Table.Columns.Contains("LopHocID") && rowView["LopHocID"] != DBNull.Value)
                            {
                                int lopID = Convert.ToInt32(rowView["LopHocID"]);
                                if (lopID > 0)
                                {
                                    try { cmbDT_GiaoChoLop.SelectedValue = lopID; } catch { }
                                }
                            }
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Lỗi tải danh sách đề thi: " + ex.Message,
                    "Lỗi",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        /// <summary>
        /// Lọc danh sách lớp học ở cmbDT_GiaoChoLop theo môn học đang chọn
        /// </summary>
        private void FilterLopTheoMonHoc(string tenMonHoc)
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                if (dtLop == null || dtLop.Rows.Count == 0)
                {
                    cmbDT_GiaoChoLop.DataSource = null;
                    return;
                }

                if (string.IsNullOrWhiteSpace(tenMonHoc))
                {
                    cmbDT_GiaoChoLop.DataSource = dtLop.Copy();
                    cmbDT_GiaoChoLop.DisplayMember = "TenLop";
                    cmbDT_GiaoChoLop.ValueMember = "LopHocID";
                    return;
                }

                DataView dv = dtLop.DefaultView;
                dv.RowFilter = $"TenKhoaHoc = '{tenMonHoc.Replace("'", "''")}'";
                DataTable dtFiltered = dv.ToTable();

                if (dtFiltered.Rows.Count > 0)
                {
                    cmbDT_GiaoChoLop.DataSource = dtFiltered;
                }
                else
                {
                    cmbDT_GiaoChoLop.DataSource = dtLop.Copy();
                }
                cmbDT_GiaoChoLop.DisplayMember = "TenLop";
                cmbDT_GiaoChoLop.ValueMember = "LopHocID";
            }
            catch { }
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
                    FilterLopTheoMonHoc(monHoc);
                }
            }
            finally
            {
                _isSyncingMonHoc = false;
            }
        }

        private void cmbDT_DeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbDT_DeThi.SelectedValue != null && int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out int deID) && deID > 0)
            {
                _selectedDeThiID = deID;
                _selectedCauHoiID = 0;
                var listCH = _deThiDAL.LayDanhSachCauHoi(deID);
                dgvCauHoi.DataSource = listCH;
                txtDT_SoCauHoi.Text = listCH.Count.ToString();
                lblCH_TongCauHoi.Text = $"Số câu hỏi: {listCH.Count:N0}";

                DeThi de = _deThiDAL.LayTheoID(deID);
                if (de != null)
                {
                    txtDT_ThoiGian.Text = de.ThoiGian.ToString();
                    if (!string.IsNullOrEmpty(de.MonHoc))
                    {
                        _isSyncingMonHoc = true;
                        if (cmbDT_MonHoc.Items.Contains(de.MonHoc)) cmbDT_MonHoc.SelectedItem = de.MonHoc;
                        else cmbDT_MonHoc.Text = de.MonHoc;

                        if (cmbCH_MonHoc.Items.Contains(de.MonHoc)) cmbCH_MonHoc.SelectedItem = de.MonHoc;
                        else cmbCH_MonHoc.Text = de.MonHoc;
                        _isSyncingMonHoc = false;

                        FilterLopTheoMonHoc(de.MonHoc);
                    }

                    if (de.LopHocID > 0)
                    {
                        try { cmbDT_GiaoChoLop.SelectedValue = de.LopHocID; } catch { }
                    }

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

                    // Hiển thị số câu hiện có của đề thi đang chọn
                    int soCauHienCo = _deThiDAL.DemSoCauHoiHienTai(_selectedDeThiID);
                    txtDT_SoCauHoi.Text = soCauHienCo.ToString();

                    // Cập nhật Môn học cho cả 2 tab
                    string monHoc = drv.Row.Table.Columns.Contains("MonHoc") ? drv["MonHoc"]?.ToString() : "";
                    if (!string.IsNullOrEmpty(monHoc))
                    {
                        _isSyncingMonHoc = true;
                        if (cmbDT_MonHoc.Items.Contains(monHoc)) cmbDT_MonHoc.SelectedItem = monHoc;
                        else cmbDT_MonHoc.Text = monHoc;

                        if (cmbCH_MonHoc.Items.Contains(monHoc)) cmbCH_MonHoc.SelectedItem = monHoc;
                        else cmbCH_MonHoc.Text = monHoc;
                        _isSyncingMonHoc = false;

                        FilterLopTheoMonHoc(monHoc);
                    }

                    // Cập nhật Thời gian
                    if (drv.Row.Table.Columns.Contains("ThoiGian") && drv["ThoiGian"] != DBNull.Value)
                        txtDT_ThoiGian.Text = drv["ThoiGian"].ToString();

                    // Cập nhật Tên đề thi lên Tab Câu hỏi
                    string tenDe = drv.Row.Table.Columns.Contains("TenDeThi") ? drv["TenDeThi"]?.ToString() : "";
                    if (!string.IsNullOrEmpty(tenDe) && txtCH_TenDeThi != null)
                        txtCH_TenDeThi.Text = tenDe;

                    // Cập nhật lớp học nếu đã giao
                    if (drv.Row.Table.Columns.Contains("LopHocID") && drv["LopHocID"] != DBNull.Value)
                    {
                        int lopID = Convert.ToInt32(drv["LopHocID"]);
                        if (lopID > 0)
                        {
                            try { cmbDT_GiaoChoLop.SelectedValue = lopID; } catch { }
                        }
                    }

                    // Tải câu hỏi lên dgvCauHoi
                    dgvCauHoi.DataSource = _deThiDAL.LayDanhSachCauHoi(deID);
                }
            }
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
            // 1. Tự động lấy DeThiID từ ComboBox nếu chưa click chọn dòng trong bảng
            if (_selectedDeThiID <= 0 && cmbDT_DeThi.SelectedValue != null)
            {
                int.TryParse(cmbDT_DeThi.SelectedValue.ToString(), out _selectedDeThiID);
            }

            // 2. Nếu vẫn <= 0, lấy từ dòng hiện tại của dgvGVDeThi
            if (_selectedDeThiID <= 0 && dgvGVDeThi.CurrentRow != null && dgvGVDeThi.CurrentRow.DataBoundItem is DataRowView drv)
            {
                if (drv.Row.Table.Columns.Contains("DeThiID") && drv["DeThiID"] != DBNull.Value)
                    _selectedDeThiID = Convert.ToInt32(drv["DeThiID"]);
                else if (drv.Row.Table.Columns.Contains("dethiid") && drv["dethiid"] != DBNull.Value)
                    _selectedDeThiID = Convert.ToInt32(drv["dethiid"]);
            }

            if (_selectedDeThiID <= 0)
            {
                MessageBox.Show(
                    "Vui lòng chọn đề thi từ danh sách hoặc ComboBox!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbDT_GiaoChoLop.SelectedValue == null)
            {
                MessageBox.Show(
                    "Vui lòng chọn lớp học để giao bài!",
                    "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 3. Kiểm tra đề thi có câu hỏi chưa
            int soCauHienCo = _deThiDAL.DemSoCauHoiHienTai(_selectedDeThiID);
            if (soCauHienCo == 0)
            {
                MessageBox.Show(
                    $"Đề thi (Mã: {_selectedDeThiID}) hiện chưa có câu hỏi nào trong cơ sở dữ liệu!\nVui lòng chuyển sang Tab 'Câu Hỏi' để thêm câu hỏi trước khi giao cho lớp.",
                    "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int thoiGian = 45;
            int.TryParse(txtDT_ThoiGian?.Text, out thoiGian);
            if (thoiGian <= 0) thoiGian = 45;

            int lopHocID = Convert.ToInt32(cmbDT_GiaoChoLop.SelectedValue);
            DateTime? hanNop = DateTime.Now.AddDays(7);
            string monHocGiao = cmbDT_MonHoc.SelectedItem?.ToString() ?? cmbDT_MonHoc.Text?.Trim();

            if (MessageBox.Show(
                    $"Giao đề thi cho lớp đã chọn?\nSố câu: {soCauHienCo} | Thời gian: {thoiGian} phút",
                    "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (_deThiDAL.GiaoDeThiChoLop(_selectedDeThiID, lopHocID, thoiGian, hanNop, monHocGiao))
            {
                MessageBox.Show(
                    "Giao đề thi thành công!",
                    "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _selectedDeThiID = 0;
                txtDT_SoCauHoi.Text = "";
                LoadDeThiCombobox();
            }
            else
            {
                MessageBox.Show(
                    "Giao đề thi thất bại! Vui lòng thử lại.",
                    "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------------------------------------------------------------
        // btnDT_XoaDeThi (Tab Quản Lý Đề Thi):
        //   Xóa đề thi mà chưa giao bài thi cho bất kỳ lớp nào (và chưa có học sinh làm bài).
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
            DeThi de = _deThiDAL.LayTheoID(deID);
            string tenDeThi = de?.TenDeThi ?? $"Mã {deID}";

            // 3. Kiểm tra xem đề thi đã giao bài chưa
            if (!_deThiDAL.KiemTraDeThiChuaGiao(deID))
            {
                MessageBox.Show(
                    $"Không thể xóa đề thi \"{tenDeThi}\" vì đề thi này đã được giao cho lớp học hoặc đã có học sinh làm bài!\n\nChỉ được phép xóa đề thi chưa giao bài.",
                    "Không thể xóa",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            // 4. Hỏi xác nhận người dùng
            DialogResult confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa đề thi \"{tenDeThi}\" (Mã đề: {deID}) không?\n\nLưu ý: Tất cả các câu hỏi thuộc đề thi này cũng sẽ bị xóa vĩnh viễn.",
                "Xác nhận xóa đề thi",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (confirm != DialogResult.Yes) return;

            // 5. Thực hiện xóa
            if (_deThiDAL.XoaDeThi(deID, out string thongBao))
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
            if (cmbKQ_DeThi.SelectedValue != null && int.TryParse(cmbKQ_DeThi.SelectedValue.ToString(), out int deID))
                _ketQua = _deThiDAL.LayKetQuaTheoDeThi(deID);
            else
                _ketQua = null;
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
                // Lấy toàn bộ bảng điểm của học sinh đó (tất cả môn)
                DataTable dtBangDiem = _deThiDAL.LayBangDiemHocSinh(hocSinhID);

                ExcelExportHelper.ExportBangDiemHocSinh(
                    dt:           dtBangDiem,
                    tenHocSinh:   tenHocSinh,
                    tenMonHocLoc: null,               // không lọc môn học
                    tuNgay:       null,               // không lọc ngày
                    denNgay:      null,
                    nguoiLapBieu: Session.TenDangNhap
                );
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
                GiaoVien gv = new GiaoVienDAL().LayTheoID(Session.NguoiDungID);
                if (gv != null)
                {
                    txtGVTT_HovaTen.Text = gv.HoTen;
                    lblGVTT_HovaTen.Text = gv.HoTen;
                    txtTTGV_Email.Text = gv.Email;
                    txtTTGV_DienThoai.Text = gv.SoDienThoai;
                    txtTTGV_DiaChi.Text = gv.DiaChi;
                    cmbTTGV_GioiTinh.SelectedItem = gv.GioiTinh;
                }
            }
            catch { }
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

                if (new GiaoVienDAL().CapNhatHoSoCaNhan(Session.TaiKhoanID, gv))
                {
                    MessageBox.Show("Cập nhật thông tin cá nhân thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    Session.HoTen = gv.HoTen;
                    lblGVTT_HovaTen.Text = gv.HoTen;
                    lblGV_LoiChao.Text = $"Xin Chào, {gv.HoTen}";
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
                if (!_tkDAL.DoiMatKhauGiaoVien(Session.TaiKhoanID, cu, moi))
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng hoặc tài khoản đã bị khóa.", "Không thể đổi mật khẩu");
                    return;
                }
                _tkDAL.XoaGhiNho(Session.TenDangNhap);
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
                    LoadLopHocCuaToi();
                    LoadKetQuaThiGanDay();
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
