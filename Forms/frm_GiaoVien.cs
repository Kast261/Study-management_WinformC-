using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_GiaoVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KhoaHocDAL _khDAL = new KhoaHocDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();

        private int _selectedLopHocID = 0;
        private int _selectedCauHoiID = 0;
        private int _selectedDeThiID   = 0;  // ID đề thi đang chọn trên tab Đề Thi
        private bool _isSyncingMonHoc = false; // Cờ chống lặp vô tận khi đồng bộ môn học giữa 2 tab

        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
            this.FormClosed += Frm_GiaoVien_FormClosed;
            SetupEventHandlers();
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

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            try
            {
                lblGV_LoiChao.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";

                InitDropdowns();
                SetupEventHandlers();

                // Load dữ liệu ban đầu cho các tab
                LoadLopHocCuaToi();
                LoadDanhSachLopQuanLy();
                cboDuyet_ChonLop_SelectedIndexChanged(null, null);
                LoadDeThiCombobox();
                LoadThongTinCaNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trang giáo viên: " + ex.Message, "Thông báo");
            }
        }

        #region 1. Khởi tạo & Đăng ký sự kiện
        private void InitDropdowns()
        {
            // Dropdown đáp án đúng của câu hỏi
            cboCH_DapAnDung.Items.Clear();
            cboCH_DapAnDung.Items.AddRange(new object[] { "A", "B", "C", "D" });
            cboCH_DapAnDung.SelectedIndex = 0;

            // Giới tính profile
            cboHV_Profile_GioiTinh.Items.Clear();
            cboHV_Profile_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cboHV_Profile_GioiTinh.SelectedIndex = 0;

            // Khóa học cho form Lớp
            DataTable dtKH = _khDAL.LayTatCa();
            cmbGV_KhoaHoc.DataSource = dtKH;
            cmbGV_KhoaHoc.DisplayMember = "TenKhoaHoc";
            cmbGV_KhoaHoc.ValueMember = "KhoaHocID";

            // Nạp môn học (khóa học) mà giáo viên đang dạy cho cả Tab Câu hỏi (cmbGV_MonHoc) và Tab Bài thi (cboDe_MonHoc)
            LoadMonHocGiaoVien();
        }

        /// <summary>
        /// Nạp danh sách các môn học / khóa học mà giáo viên hiện tại đang dạy
        /// vào cả cmbGV_MonHoc (Tab Câu hỏi) và cboDe_MonHoc (Tab Bài thi)
        /// </summary>
        private void LoadMonHocGiaoVien()
        {
            try
            {
                DataTable dtMonHoc = _khDAL.LayKhoaHocTheoGiaoVien(Session.NguoiDungID);

                string selectedCH = cmbGV_MonHoc.SelectedItem?.ToString();
                string selectedDe = cboDe_MonHoc.SelectedItem?.ToString();

                _isSyncingMonHoc = true;
                cmbGV_MonHoc.Items.Clear();
                cboDe_MonHoc.Items.Clear();

                if (dtMonHoc != null && dtMonHoc.Rows.Count > 0)
                {
                    foreach (DataRow r in dtMonHoc.Rows)
                    {
                        string tenKH = r["TenKhoaHoc"]?.ToString();
                        if (!string.IsNullOrWhiteSpace(tenKH))
                        {
                            if (!cmbGV_MonHoc.Items.Contains(tenKH))
                                cmbGV_MonHoc.Items.Add(tenKH);
                            if (!cboDe_MonHoc.Items.Contains(tenKH))
                                cboDe_MonHoc.Items.Add(tenKH);
                        }
                    }
                }

                if (cmbGV_MonHoc.Items.Count > 0)
                {
                    if (!string.IsNullOrEmpty(selectedCH) && cmbGV_MonHoc.Items.Contains(selectedCH))
                        cmbGV_MonHoc.SelectedItem = selectedCH;
                    else
                        cmbGV_MonHoc.SelectedIndex = 0;
                }

                if (cboDe_MonHoc.Items.Count > 0)
                {
                    if (!string.IsNullOrEmpty(selectedDe) && cboDe_MonHoc.Items.Contains(selectedDe))
                        cboDe_MonHoc.SelectedItem = selectedDe;
                    else
                        cboDe_MonHoc.SelectedIndex = 0;
                }

                _isSyncingMonHoc = false;
                FilterLopTheoMonHoc(cboDe_MonHoc.SelectedItem?.ToString());
            }
            catch (Exception ex)
            {
                _isSyncingMonHoc = false;
                System.Diagnostics.Debug.WriteLine("Lỗi nạp môn học giáo viên: " + ex.Message);
            }
        }

        /// <summary>
        /// Lọc danh sách lớp học ở cboDe_ChonLopHoc theo môn học đang chọn
        /// </summary>
        private void FilterLopTheoMonHoc(string tenMonHoc)
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                if (dtLop == null || dtLop.Rows.Count == 0)
                {
                    cboDe_ChonLopHoc.DataSource = null;
                    return;
                }

                if (string.IsNullOrWhiteSpace(tenMonHoc))
                {
                    cboDe_ChonLopHoc.DataSource = dtLop.Copy();
                    cboDe_ChonLopHoc.DisplayMember = "TenLop";
                    cboDe_ChonLopHoc.ValueMember = "LopHocID";
                    return;
                }

                DataView dv = dtLop.DefaultView;
                dv.RowFilter = $"TenKhoaHoc = '{tenMonHoc.Replace("'", "''")}'";
                DataTable dtFiltered = dv.ToTable();

                if (dtFiltered.Rows.Count > 0)
                {
                    cboDe_ChonLopHoc.DataSource = dtFiltered;
                }
                else
                {
                    cboDe_ChonLopHoc.DataSource = dtLop.Copy();
                }
                cboDe_ChonLopHoc.DisplayMember = "TenLop";
                cboDe_ChonLopHoc.ValueMember = "LopHocID";
            }
            catch { }
        }

        private void cmbGV_MonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSyncingMonHoc) return;
            try
            {
                _isSyncingMonHoc = true;
                string monHoc = cmbGV_MonHoc.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(monHoc) && cboDe_MonHoc.Items.Contains(monHoc))
                {
                    cboDe_MonHoc.SelectedItem = monHoc;
                    FilterLopTheoMonHoc(monHoc);
                }
            }
            finally
            {
                _isSyncingMonHoc = false;
            }
        }

        private void cboDe_MonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isSyncingMonHoc) return;
            try
            {
                _isSyncingMonHoc = true;
                string monHoc = cboDe_MonHoc.SelectedItem?.ToString();
                if (!string.IsNullOrEmpty(monHoc))
                {
                    if (cmbGV_MonHoc.Items.Contains(monHoc))
                        cmbGV_MonHoc.SelectedItem = monHoc;
                    FilterLopTheoMonHoc(monHoc);
                }
            }
            finally
            {
                _isSyncingMonHoc = false;
            }
        }

        private bool _isEventHandlersSetup = false;
        private void SetupEventHandlers()
        {
            if (_isEventHandlersSetup) return;
            _isEventHandlersSetup = true;

            // Tab Lớp học
            dgvGV_LopCuaToi.AutoGenerateColumns = false;
            dataGridViewTextBoxColumn7.DataPropertyName = "TenLop";
            dataGridViewTextBoxColumn8.DataPropertyName = "SiSo";
            dataGridViewTextBoxColumn9.DataPropertyName = "LichHoc";
            btnGV_TaoLop.Click += btnGV_TaoLop_Click;
            btnGV_SuaLop.Click += btnGV_SuaLop_Click;
            btnGV_DongDangKi.Click += btnGV_DongDangKi_Click;
            dtGV_LopHoc.CellClick += dtGV_LopHoc_CellClick;

            // Tab Duyệt đăng ký
            cboDuyet_ChonLop.SelectedIndexChanged += cboDuyet_ChonLop_SelectedIndexChanged;
            btnDuyet_ChapNhan.Click += btnDuyet_ChapNhan_Click;
            btnDuyet_TuChoi.Click += btnDuyet_TuChoi_Click;

            // Tab Câu hỏi
            CauHinhLuoiCauHoi();
            cmbGV_ChonDeThi.SelectedIndexChanged += cmbGV_ChonDeThi_SelectedIndexChanged;
            cmbGV_MonHoc.SelectedIndexChanged += cmbGV_MonHoc_SelectedIndexChanged;
            btnCH_Them.Click += btnCH_Them_Click;
            btnCH_Sua.Click += btnCH_Sua_Click;
            btnCH_Xoa.Click += btnCH_Xoa_Click;
            dgvDanhSachCauHoi.CellClick += dgvDanhSachCauHoi_CellClick;

            // Tab Câu hỏi – Tạo đề thi mới
            btnCH_TaoDeThi.Click += btnCH_TaoDeThi_Click;

            // Tab Đề thi – Giao đề thi đã tạo cho lớp & Xóa đề thi
            cboDe_MonHoc.SelectedIndexChanged += cboDe_MonHoc_SelectedIndexChanged;
            btnDe_TaoDeThi.Click        += btnDe_TaoDeThi_Click;
            btnDe_XoaDeThi.Click        += btnDe_XoaDeThi_Click;
            dgvDanhSachDeThi.CellClick  += dgvDanhSachDeThi_CellClick;
            dgvDanhSachDeThi.CellDoubleClick += dgvDanhSachDeThi_CellDoubleClick;

            // Tab Kết quả thi
            cboKQ_ChonDeThi.SelectedIndexChanged += cboKQ_ChonDeThi_SelectedIndexChanged;
            btnKQ_XuatExcel.Click += btnKQ_XuatExcel_Click;

            // Tab Thông tin cá nhân & Mật khẩu
            btnGV_CapNhatThongTin.Click += btnGV_CapNhatThongTin_Click;
            btnGV_DoiMatKhau.Click += btnGV_DoiMatKhau_Click;
        }

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
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
            if (tabText.Contains("Lớp") || materialTabControl1.SelectedTab == pageGV_LopHoc)
            {
                LoadDanhSachLopQuanLy();
            }
            else if (tabText.Contains("Duyệt") || tabText.Contains("Đăng Kí") || tabText.Contains("Đăng ký") || materialTabControl1.SelectedTab == pageGV_DuyetHocVien)
            {
                LoadDanhSachLopQuanLy();
                cboDuyet_ChonLop_SelectedIndexChanged(null, null);
            }
            else if (tabText.Contains("Ngân Hàng") || tabText.Contains("Câu Hỏi") || materialTabControl1.SelectedTab == pageGV_NganHangCauHoi)
            {
                LoadMonHocGiaoVien();
                LoadDeThiCombobox();
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
            }
            else if (tabText.Contains("Bài Thi") || tabText.Contains("Đề Thi") || materialTabControl1.SelectedTab == pageGV_QuanLyDeThi)
            {
                LoadMonHocGiaoVien();
                LoadDanhSachLopQuanLy();
                LoadDeThiCombobox();
            }
            else if (tabText.Contains("Kết Quả") || materialTabControl1.SelectedTab == pageGV_KetQuaThi)
            {
                LoadDeThiCombobox();
                cboKQ_ChonDeThi_SelectedIndexChanged(null, null);
            }
            else if (tabText.Contains("Thông Tin") || materialTabControl1.SelectedTab == pageGV_ThongTin)
            {
                LoadThongTinCaNhan();
            }
        }
        #endregion

        #region 2. Tab Trang Chủ & Quản Lý Lớp Học
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
                dtGV_LopHoc.AutoGenerateColumns = false;
                Column1.DataPropertyName = "TenLop";
                Column2.DataPropertyName = "TenKhoaHoc";
                Column3.DataPropertyName = "SiSo";
                Column4.DataPropertyName = "TrangThai";
                Column8.DataPropertyName = "NgayBatDau";
                Column9.DataPropertyName = "NgayKetThuc";
                Column10.DataPropertyName = "LichHoc";

                Column8.DefaultCellStyle.Format = "dd/MM/yyyy";
                Column9.DefaultCellStyle.Format = "dd/MM/yyyy";
                Column3.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Column4.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Column8.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                Column9.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

                dtGV_LopHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dtGV_LopHoc.DataSource = dt;

                // Load danh sách lớp vào combobox duyệt & giao bài thi
                DataTable dtDuyet = dt.Copy();
                DataRow drAll = dtDuyet.NewRow();
                drAll["LopHocID"] = 0;
                drAll["TenLop"] = "-- Tất cả các lớp --";
                dtDuyet.Rows.InsertAt(drAll, 0);

                cboDuyet_ChonLop.DataSource = dtDuyet;
                cboDuyet_ChonLop.DisplayMember = "TenLop";
                cboDuyet_ChonLop.ValueMember = "LopHocID";

                // Cập nhật lớp học cho form Đề thi theo môn học hiện tại
                FilterLopTheoMonHoc(cboDe_MonHoc.SelectedItem?.ToString());
            }
            catch { }
        }

        private void dtGV_LopHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dtGV_LopHoc.Rows.Count)
            {
                var row = dtGV_LopHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedLopHocID = Convert.ToInt32(drv["LopHocID"]);
                    txtGV_TenLop.Text = drv["TenLop"]?.ToString();
                    txtGV_LichHoc.Text = drv["LichHoc"]?.ToString();

                    if (drv["NgayBatDau"] != DBNull.Value)
                    {
                        DateTime dtStart = Convert.ToDateTime(drv["NgayBatDau"]);
                        txtGV_NgayBatDau.Text = dtStart.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        txtGV_NgayBatDau.Text = "";
                    }

                    if (drv["NgayKetThuc"] != DBNull.Value)
                    {
                        DateTime dtEnd = Convert.ToDateTime(drv["NgayKetThuc"]);
                        txtGV_NgayKetThuc.Text = dtEnd.ToString("yyyy-MM-dd");
                    }
                    else
                    {
                        txtGV_NgayKetThuc.Text = "";
                    }

                    if (drv.Row.Table.Columns.Contains("SiSoToiDa") && drv["SiSoToiDa"] != DBNull.Value)
                    {
                        txtGV_SiSo.Text = drv["SiSoToiDa"].ToString();
                    }
                    else
                    {
                        txtGV_SiSo.Text = "40";
                    }

                    if (drv.Row.Table.Columns.Contains("KhoaHocID") && drv["KhoaHocID"] != DBNull.Value)
                    {
                        try { cmbGV_KhoaHoc.SelectedValue = drv["KhoaHocID"]; } catch { }
                    }
                }
            }
        }

        private void btnGV_TaoLop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGV_TenLop.Text))
            {
                MessageBox.Show("Vui lòng nhập tên lớp học!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_TenLop.Focus();
                return;
            }

            if (!Validation.IsValidSiSo(txtGV_SiSo.Text, out int siSo, out string errSiSo))
            {
                MessageBox.Show(errSiSo, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_SiSo.Focus();
                return;
            }

            int khID = cmbGV_KhoaHoc.SelectedValue != null ? Convert.ToInt32(cmbGV_KhoaHoc.SelectedValue) : 1;

            DateTime? nbd = null;
            if (DateTime.TryParse(txtGV_NgayBatDau.Text.Trim(), out DateTime dtStart)) nbd = dtStart;
            DateTime? nkt = null;
            if (DateTime.TryParse(txtGV_NgayKetThuc.Text.Trim(), out DateTime dtEnd)) nkt = dtEnd;

            LopHoc lop = new LopHoc
            {
                TenLop = txtGV_TenLop.Text.Trim(),
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

        private void btnGV_SuaLop_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGV_TenLop.Text))
            {
                MessageBox.Show("Tên lớp không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_TenLop.Focus();
                return;
            }

            if (!Validation.IsValidSiSo(txtGV_SiSo.Text, out int siSo, out string errSiSo))
            {
                MessageBox.Show(errSiSo, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_SiSo.Focus();
                return;
            }

            int khID = cmbGV_KhoaHoc.SelectedValue != null ? Convert.ToInt32(cmbGV_KhoaHoc.SelectedValue) : 1;

            DateTime? nbd = null;
            if (DateTime.TryParse(txtGV_NgayBatDau.Text.Trim(), out DateTime dtStart)) nbd = dtStart;
            DateTime? nkt = null;
            if (DateTime.TryParse(txtGV_NgayKetThuc.Text.Trim(), out DateTime dtEnd)) nkt = dtEnd;

            LopHoc lop = new LopHoc
            {
                LopHocID = _selectedLopHocID,
                TenLop = txtGV_TenLop.Text.Trim(),
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
        }

        private void btnGV_DongDangKi_Click(object sender, EventArgs e)
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
        #endregion

        #region 3. Tab Duyệt Học Viên Đăng Ký
        private void cboDuyet_ChonLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                int lopID = 0;
                if (cboDuyet_ChonLop.SelectedValue is int idVal)
                {
                    lopID = idVal;
                }
                else if (cboDuyet_ChonLop.SelectedValue is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    int.TryParse(drv["LopHocID"]?.ToString(), out lopID);
                }
                else if (cboDuyet_ChonLop.SelectedValue != null)
                {
                    int.TryParse(cboDuyet_ChonLop.SelectedValue.ToString(), out lopID);
                }

                DataTable dt = _lopDAL.LayDangKyChoDuyet(Session.NguoiDungID, lopID);

                // Không tự tạo thêm cột
                dgvDangKyChoDuyet.AutoGenerateColumns = false;

                // Ánh xạ cột Designer với cột trong DataTable
                if (dgvDangKyChoDuyet.Columns.Count >= 6)
                {
                    dgvDangKyChoDuyet.Columns[0].DataPropertyName = "HoTen";
                    dgvDangKyChoDuyet.Columns[1].DataPropertyName = "Email";
                    dgvDangKyChoDuyet.Columns[2].DataPropertyName = "SoDienThoai";
                    dgvDangKyChoDuyet.Columns[3].DataPropertyName = "TenLop";
                    dgvDangKyChoDuyet.Columns[4].DataPropertyName = "SiSo";
                    dgvDangKyChoDuyet.Columns[5].DataPropertyName = "NgayDangKy";

                    dgvDangKyChoDuyet.Columns[4].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvDangKyChoDuyet.Columns[5].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                    dgvDangKyChoDuyet.Columns[5].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
                }

                dgvDangKyChoDuyet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                dgvDangKyChoDuyet.DataSource = null;
                dgvDangKyChoDuyet.DataSource = dt;
            }
            catch { }
        }

        private void btnDuyet_ChapNhan_Click(object sender, EventArgs e)
        {
            if (dgvDangKyChoDuyet.CurrentRow != null && dgvDangKyChoDuyet.CurrentRow.DataBoundItem is DataRowView drv)
            {
                int dkID = Convert.ToInt32(drv["DangKyID"]);
                int lopID = Convert.ToInt32(drv["LopHocID"]);
                int hsID = Convert.ToInt32(drv["HocSinhID"]);

                if (_lopDAL.DuyetDangKy(dkID, lopID, hsID))
                {
                    MessageBox.Show("Đã duyệt học viên vào lớp thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cboDuyet_ChonLop_SelectedIndexChanged(null, null);
                    LoadDanhSachLopQuanLy();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn học viên cần duyệt từ danh sách!", "Thông báo");
            }
        }

        private void btnDuyet_TuChoi_Click(object sender, EventArgs e)
        {
            if (dgvDangKyChoDuyet.CurrentRow != null && dgvDangKyChoDuyet.CurrentRow.DataBoundItem is DataRowView drv)
            {
                int dkID = Convert.ToInt32(drv["DangKyID"]);
                if (MessageBox.Show("Từ chối đơn đăng ký này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_lopDAL.TuChoiDangKy(dkID))
                    {
                        MessageBox.Show("Đã từ chối đơn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cboDuyet_ChonLop_SelectedIndexChanged(null, null);
                    }
                }
            }
        }
        #endregion

        #region 4. Tab Ngân Hàng Câu Hỏi
        private void LoadDeThiCombobox()
        {
            try
            {
                int currentDeID = 0;
                if (cmbGV_ChonDeThi.SelectedValue != null)
                {
                    int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out currentDeID);
                }
                if (currentDeID <= 0 && _selectedDeThiID > 0)
                {
                    currentDeID = _selectedDeThiID;
                }

                DataTable dt = _deThiDAL.LayDeThiTheoGiaoVien(Session.NguoiDungID);

                // Cài đặt DisplayMember & ValueMember TRƯỚC khi gán DataSource
                cmbGV_ChonDeThi.DisplayMember = "TenDeThi";
                cmbGV_ChonDeThi.ValueMember = "DeThiID";
                cmbGV_ChonDeThi.DataSource = dt.Copy();

                cboKQ_ChonDeThi.DisplayMember = "TenDeThi";
                cboKQ_ChonDeThi.ValueMember = "DeThiID";
                cboKQ_ChonDeThi.DataSource = dt.Copy();

                // Bảng quản lý đề thi
                dgvDanhSachDeThi.AutoGenerateColumns = false;
                if (dgvDanhSachDeThi.Columns.Count >= 5)
                {
                    dgvDanhSachDeThi.Columns[0].DataPropertyName = "TenDeThi";
                    dgvDanhSachDeThi.Columns[1].DataPropertyName = "MonHoc";
                    dgvDanhSachDeThi.Columns[2].DataPropertyName = "ThoiGian";
                    dgvDanhSachDeThi.Columns[3].DataPropertyName = "TenLop";
                    dgvDanhSachDeThi.Columns[4].DataPropertyName = "SoCau";
                }
                dgvDanhSachDeThi.DataSource = null;
                dgvDanhSachDeThi.DataSource = dt;

                // Khôi phục lại đề thi đang chọn nếu có
                if (currentDeID > 0)
                {
                    try { cmbGV_ChonDeThi.SelectedValue = currentDeID; } catch { }
                    _selectedDeThiID = currentDeID;
                }
                else if (dt.Rows.Count > 0)
                {
                    _selectedDeThiID = Convert.ToInt32(dt.Rows[0]["DeThiID"]);
                }

                // Cập nhật số câu hiển thị
                if (_selectedDeThiID > 0 && dt != null)
                {
                    for (int i = 0; i < dgvDanhSachDeThi.Rows.Count; i++)
                    {
                        if (dgvDanhSachDeThi.Rows[i].DataBoundItem is DataRowView rowView)
                        {
                            string colName = rowView.Row.Table.Columns.Contains("DeThiID") ? "DeThiID" : "dethiid";
                            if (rowView.Row.Table.Columns.Contains(colName) &&
                                rowView[colName] != DBNull.Value &&
                                Convert.ToInt32(rowView[colName]) == _selectedDeThiID)
                            {
                                dgvDanhSachDeThi.ClearSelection();
                                dgvDanhSachDeThi.Rows[i].Selected = true;
                                dgvDanhSachDeThi.FirstDisplayedScrollingRowIndex = i;
                                txtDe_SoCau.Text = _deThiDAL.DemSoCauHoiHienTai(_selectedDeThiID).ToString();

                                string monHoc = rowView.Row.Table.Columns.Contains("MonHoc") ? rowView["MonHoc"]?.ToString() : "";
                                if (!string.IsNullOrEmpty(monHoc))
                                {
                                    _isSyncingMonHoc = true;
                                    if (cboDe_MonHoc.Items.Contains(monHoc)) cboDe_MonHoc.SelectedItem = monHoc;
                                    else cboDe_MonHoc.Text = monHoc;

                                    if (cmbGV_MonHoc.Items.Contains(monHoc)) cmbGV_MonHoc.SelectedItem = monHoc;
                                    else cmbGV_MonHoc.Text = monHoc;
                                    _isSyncingMonHoc = false;

                                    FilterLopTheoMonHoc(monHoc);
                                }

                                if (rowView.Row.Table.Columns.Contains("ThoiGian") && rowView["ThoiGian"] != DBNull.Value)
                                    txtDe_ThoiGian.Text = rowView["ThoiGian"].ToString();

                                if (rowView.Row.Table.Columns.Contains("TenDeThi") && txtGV_DeThi != null)
                                    txtGV_DeThi.Text = rowView["TenDeThi"]?.ToString() ?? "";

                                if (rowView.Row.Table.Columns.Contains("LopHocID") && rowView["LopHocID"] != DBNull.Value)
                                {
                                    int lopID = Convert.ToInt32(rowView["LopHocID"]);
                                    if (lopID > 0)
                                    {
                                        try { cboDe_ChonLopHoc.SelectedValue = lopID; } catch { }
                                    }
                                }
                                break;
                            }
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

        private void CauHinhLuoiCauHoi()
        {
            dgvDanhSachCauHoi.AutoGenerateColumns = false;
            dgvDanhSachCauHoi.Columns.Clear();

            // 1. Cột Thứ tự (STT)
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colThuTu",
                HeaderText = "STT",
                DataPropertyName = "ThuTu",
                Width = 60,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });

            // 2. Cột Nội dung câu hỏi
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNoiDung",
                HeaderText = "Nội Dung Câu Hỏi",
                DataPropertyName = "NoiDung",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 35
            });

            // 3. Cột Đáp án A
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDapAnA",
                HeaderText = "Đáp Án A",
                DataPropertyName = "DapAnA",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            });

            // 4. Cột Đáp án B
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDapAnB",
                HeaderText = "Đáp Án B",
                DataPropertyName = "DapAnB",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            });

            // 5. Cột Đáp án C
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDapAnC",
                HeaderText = "Đáp Án C",
                DataPropertyName = "DapAnC",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            });

            // 6. Cột Đáp án D
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDapAnD",
                HeaderText = "Đáp Án D",
                DataPropertyName = "DapAnD",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 15
            });

            // 7. Cột Đáp án Đúng
            dgvDanhSachCauHoi.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDapAnDung",
                HeaderText = "Đ.Án Đúng",
                DataPropertyName = "DapAnDung",
                Width = 90,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleCenter,
                    Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(35, 168, 109)
                }
            });
        }

        private void cmbGV_ChonDeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGV_ChonDeThi.SelectedValue != null && int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out int deID) && deID > 0)
            {
                _selectedDeThiID = deID;
                var listCH = _deThiDAL.LayDanhSachCauHoi(deID);
                dgvDanhSachCauHoi.DataSource = listCH;
                txtDe_SoCau.Text = _deThiDAL.DemSoCauHoiHienTai(deID).ToString();

                DeThi de = _deThiDAL.LayTheoID(deID);
                if (de != null)
                {
                    txtDe_ThoiGian.Text = de.ThoiGian.ToString();
                    if (!string.IsNullOrEmpty(de.MonHoc))
                    {
                        _isSyncingMonHoc = true;
                        if (cboDe_MonHoc.Items.Contains(de.MonHoc)) cboDe_MonHoc.SelectedItem = de.MonHoc;
                        else cboDe_MonHoc.Text = de.MonHoc;

                        if (cmbGV_MonHoc.Items.Contains(de.MonHoc)) cmbGV_MonHoc.SelectedItem = de.MonHoc;
                        else cmbGV_MonHoc.Text = de.MonHoc;
                        _isSyncingMonHoc = false;

                        FilterLopTheoMonHoc(de.MonHoc);
                    }

                    if (de.LopHocID > 0)
                    {
                        try { cboDe_ChonLopHoc.SelectedValue = de.LopHocID; } catch { }
                    }

                    if (txtGV_DeThi != null && !string.IsNullOrEmpty(de.TenDeThi))
                    {
                        txtGV_DeThi.Text = de.TenDeThi;
                    }
                }

                SyncSelectedRowInDgvDanhSachDeThi(deID);
            }
            else
            {
                dgvDanhSachCauHoi.DataSource = null;
            }
        }

        private void SyncSelectedRowInDgvDanhSachDeThi(int deID)
        {
            try
            {
                if (dgvDanhSachDeThi == null || dgvDanhSachDeThi.Rows.Count == 0) return;
                for (int i = 0; i < dgvDanhSachDeThi.Rows.Count; i++)
                {
                    if (dgvDanhSachDeThi.Rows[i].DataBoundItem is DataRowView drv)
                    {
                        string colName = drv.Row.Table.Columns.Contains("DeThiID") ? "DeThiID" : "dethiid";
                        if (drv.Row.Table.Columns.Contains(colName) && drv[colName] != DBNull.Value && Convert.ToInt32(drv[colName]) == deID)
                        {
                            dgvDanhSachDeThi.ClearSelection();
                            dgvDanhSachDeThi.Rows[i].Selected = true;
                            dgvDanhSachDeThi.FirstDisplayedScrollingRowIndex = i;
                            break;
                        }
                    }
                }
            }
            catch { }
        }

        private void dgvDanhSachCauHoi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvDanhSachCauHoi.Rows.Count)
            {
                CauHoi q = dgvDanhSachCauHoi.Rows[e.RowIndex].DataBoundItem as CauHoi;
                if (q != null)
                {
                    _selectedCauHoiID = q.CauHoiID;
                    txtCH_NoiDung.Text = q.NoiDung;
                    txtCH_DapAnA.Text = q.DapAnA;
                    txtCH_DapAnB.Text = q.DapAnB;
                    txtCH_DapAnC.Text = q.DapAnC;
                    txtCH_DapAnD.Text = q.DapAnD;
                    cboCH_DapAnDung.SelectedItem = q.DapAnDung;
                }
            }
        }

        private void btnCH_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtCH_NoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung câu hỏi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCH_NoiDung.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCH_DapAnA.Text) ||
                string.IsNullOrWhiteSpace(txtCH_DapAnB.Text) ||
                string.IsNullOrWhiteSpace(txtCH_DapAnC.Text) ||
                string.IsNullOrWhiteSpace(txtCH_DapAnD.Text))
            {
                MessageBox.Show("Vui lòng nhập đầy đủ cả 4 phương án đáp án (A, B, C, D)!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenDeNhap = txtGV_DeThi?.Text?.Trim();
            int deID = 0;

            // Nếu người dùng có nhập tên đề vào ô txtGV_DeThi
            if (!string.IsNullOrEmpty(tenDeNhap))
            {
                // Kiểm tra xem đề thi này đã có trong danh sách chưa
                if (cmbGV_ChonDeThi.DataSource is DataTable dtDe)
                {
                    foreach (DataRow r in dtDe.Rows)
                    {
                        if (r["TenDeThi"]?.ToString()?.Trim().Equals(tenDeNhap, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            deID = Convert.ToInt32(r["DeThiID"]);
                            cmbGV_ChonDeThi.SelectedValue = deID;
                            _selectedDeThiID = deID;
                            break;
                        }
                    }
                }

                // Nếu chưa có đề thi này, tự động tạo mới luôn
                if (deID <= 0)
                {
                    string monHoc = cmbGV_MonHoc.SelectedItem?.ToString() ?? cmbGV_MonHoc.Text?.Trim();
                    if (string.IsNullOrWhiteSpace(monHoc))
                        monHoc = cboDe_MonHoc.SelectedItem?.ToString() ?? "Lập trình C# WinForms";

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
                        try { cmbGV_ChonDeThi.SelectedValue = deID; _selectedDeThiID = deID; } catch { }
                    }
                }
            }

            // Nếu không gõ ô txtGV_DeThi thì lấy từ ComboBox
            if (deID <= 0 && cmbGV_ChonDeThi.SelectedValue != null)
            {
                int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out deID);
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
                NoiDung = txtCH_NoiDung.Text.Trim(),
                DapAnA = txtCH_DapAnA.Text.Trim(),
                DapAnB = txtCH_DapAnB.Text.Trim(),
                DapAnC = txtCH_DapAnC.Text.Trim(),
                DapAnD = txtCH_DapAnD.Text.Trim(),
                DapAnDung = cboCH_DapAnDung.SelectedItem?.ToString() ?? "A",
                ThuTu = soCauHienCo + 1
            };

            if (_deThiDAL.ThemCauHoi(ch))
            {
                _deThiDAL.CapNhatSoCau(deID);
                MessageBox.Show($"Thêm câu hỏi thứ {ch.ThuTu} thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                
                // Đồng bộ cập nhật lại danh sách đề thi ở cả 2 tab
                LoadDeThiCombobox();
                try { cmbGV_ChonDeThi.SelectedValue = deID; } catch { }
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                LamMoiFormCauHoi();
            }
        }

        private void btnCH_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedCauHoiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần sửa!", "Thông báo");
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCH_NoiDung.Text))
            {
                MessageBox.Show("Nội dung câu hỏi không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCH_NoiDung.Focus();
                return;
            }

            CauHoi ch = new CauHoi
            {
                CauHoiID = _selectedCauHoiID,
                NoiDung = txtCH_NoiDung.Text.Trim(),
                DapAnA = txtCH_DapAnA.Text.Trim(),
                DapAnB = txtCH_DapAnB.Text.Trim(),
                DapAnC = txtCH_DapAnC.Text.Trim(),
                DapAnD = txtCH_DapAnD.Text.Trim(),
                DapAnDung = cboCH_DapAnDung.SelectedItem?.ToString() ?? "A"
            };

            if (_deThiDAL.SuaCauHoi(ch))
            {
                MessageBox.Show("Cập nhật câu hỏi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                LamMoiFormCauHoi();
            }
        }

        private void btnCH_Xoa_Click(object sender, EventArgs e)
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
                    if (_deThiDAL.XoaCauHoi(_selectedCauHoiID))
                    {
                        if (cmbGV_ChonDeThi.SelectedValue != null && int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out int deID))
                        {
                            _deThiDAL.CapNhatSoCau(deID);
                        }
                        MessageBox.Show("Đã xóa câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDeThiCombobox();
                        cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                        LamMoiFormCauHoi();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Không thể xóa", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        private void LamMoiFormCauHoi()
        {
            _selectedCauHoiID = 0;
            txtCH_NoiDung.Clear();
            txtCH_DapAnA.Clear();
            txtCH_DapAnB.Clear();
            txtCH_DapAnC.Clear();
            txtCH_DapAnD.Clear();
            cboCH_DapAnDung.SelectedIndex = 0;
        }
        #endregion

        #region 5. Tab Quản Lý & Giao Đề Thi

        // -----------------------------------------------------------------------
        // btnCH_TaoDeThi (Tab Ngân Hàng Câu Hỏi):
        //   Tạo đề thi mới với tên nhập ở txtGV_DeThi.
        //   Sau khi tạo, chuyển combobox sang đề mới và reset form câu hỏi.
        // -----------------------------------------------------------------------
        private void btnCH_TaoDeThi_Click(object sender, EventArgs e)
        {
            string tenDe = txtGV_DeThi?.Text?.Trim();
            if (string.IsNullOrWhiteSpace(tenDe))
            {
                MessageBox.Show("Vui lòng nhập tên đề thi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_DeThi?.Focus();
                return;
            }

            string monHoc = cmbGV_MonHoc.SelectedItem?.ToString() ?? cmbGV_MonHoc.Text?.Trim();
            if (string.IsNullOrWhiteSpace(monHoc))
            {
                monHoc = cboDe_MonHoc.SelectedItem?.ToString() ?? "Lập trình C# WinForms";
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
                    cmbGV_ChonDeThi.SelectedValue = newID;
                    _selectedDeThiID = newID;
                } 
                catch { }

                // Reset form câu hỏi để bắt đầu nhập câu mới
                LamMoiFormCauHoi();

                // Nạp lưới câu hỏi (lúc mới tạo là 0 câu)
                dgvDanhSachCauHoi.DataSource = _deThiDAL.LayDanhSachCauHoi(newID);

                // Xóa tên đề thi đã nhập
                if (txtGV_DeThi != null) txtGV_DeThi.Clear();
            }
            else
            {
                MessageBox.Show("Tạo đề thi thất bại! Vui lòng thử lại.", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // -----------------------------------------------------------------------
        // dgvDanhSachDeThi_CellClick (Tab Quản Lý Đề Thi):
        //   Khi chọn một đề thi trong bảng, hiển thị số câu hiện có vào txtDe_SoCau.
        // -----------------------------------------------------------------------
        private void dgvDanhSachDeThi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvDanhSachDeThi.Rows.Count) return;

            var row = dgvDanhSachDeThi.Rows[e.RowIndex];
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
                    try { cmbGV_ChonDeThi.SelectedValue = deID; } catch { }

                    // Hiển thị số câu hiện có của đề thi đang chọn
                    int soCauHienCo = _deThiDAL.DemSoCauHoiHienTai(_selectedDeThiID);
                    txtDe_SoCau.Text = soCauHienCo.ToString();

                    // Cập nhật Môn học cho cả 2 tab
                    string monHoc = drv.Row.Table.Columns.Contains("MonHoc") ? drv["MonHoc"]?.ToString() : "";
                    if (!string.IsNullOrEmpty(monHoc))
                    {
                        _isSyncingMonHoc = true;
                        if (cboDe_MonHoc.Items.Contains(monHoc)) cboDe_MonHoc.SelectedItem = monHoc;
                        else cboDe_MonHoc.Text = monHoc;

                        if (cmbGV_MonHoc.Items.Contains(monHoc)) cmbGV_MonHoc.SelectedItem = monHoc;
                        else cmbGV_MonHoc.Text = monHoc;
                        _isSyncingMonHoc = false;

                        FilterLopTheoMonHoc(monHoc);
                    }

                    // Cập nhật Thời gian
                    if (drv.Row.Table.Columns.Contains("ThoiGian") && drv["ThoiGian"] != DBNull.Value)
                        txtDe_ThoiGian.Text = drv["ThoiGian"].ToString();

                    // Cập nhật Tên đề thi lên Tab Câu hỏi
                    string tenDe = drv.Row.Table.Columns.Contains("TenDeThi") ? drv["TenDeThi"]?.ToString() : "";
                    if (!string.IsNullOrEmpty(tenDe) && txtGV_DeThi != null)
                        txtGV_DeThi.Text = tenDe;

                    // Cập nhật lớp học nếu đã giao
                    if (drv.Row.Table.Columns.Contains("LopHocID") && drv["LopHocID"] != DBNull.Value)
                    {
                        int lopID = Convert.ToInt32(drv["LopHocID"]);
                        if (lopID > 0)
                        {
                            try { cboDe_ChonLopHoc.SelectedValue = lopID; } catch { }
                        }
                    }

                    // Tải câu hỏi lên dgvDanhSachCauHoi
                    dgvDanhSachCauHoi.DataSource = _deThiDAL.LayDanhSachCauHoi(deID);
                }
            }
        }

        private void dgvDanhSachDeThi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvDanhSachDeThi.Rows.Count) return;

            var row = dgvDanhSachDeThi.Rows[e.RowIndex];
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
                    try { cmbGV_ChonDeThi.SelectedValue = deID; } catch { }

                    // Chuyển sang Tab Câu Hỏi để giáo viên xem/soạn câu hỏi cho đề này
                    if (pageGV_NganHangCauHoi != null && materialTabControl1 != null)
                    {
                        materialTabControl1.SelectedTab = pageGV_NganHangCauHoi;
                    }
                }
            }
        }

        // -----------------------------------------------------------------------
        // btnDe_TaoDeThi (Tab Quản Lý Đề Thi):
        //   Giao đề thi đang chọn trong bảng cho lớp học được chọn.
        //   txtDe_SoCau hiển thị sẵn số câu, chỉ đọc để giáo viên xem.
        // -----------------------------------------------------------------------
        private void btnDe_TaoDeThi_Click(object sender, EventArgs e)
        {
            // 1. Tự động lấy DeThiID từ ComboBox nếu chưa click chọn dòng trong bảng
            if (_selectedDeThiID <= 0 && cmbGV_ChonDeThi.SelectedValue != null)
            {
                int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out _selectedDeThiID);
            }

            // 2. Nếu vẫn <= 0, lấy từ dòng hiện tại của dgvDanhSachDeThi
            if (_selectedDeThiID <= 0 && dgvDanhSachDeThi.CurrentRow != null && dgvDanhSachDeThi.CurrentRow.DataBoundItem is DataRowView drv)
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

            if (cboDe_ChonLopHoc.SelectedValue == null)
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
            int.TryParse(txtDe_ThoiGian?.Text, out thoiGian);
            if (thoiGian <= 0) thoiGian = 45;

            int lopHocID = Convert.ToInt32(cboDe_ChonLopHoc.SelectedValue);
            DateTime? hanNop = DateTime.Now.AddDays(7);
            string monHocGiao = cboDe_MonHoc.SelectedItem?.ToString() ?? cboDe_MonHoc.Text?.Trim();

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
                txtDe_SoCau.Text = "";
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
        // btnDe_XoaDeThi (Tab Quản Lý Đề Thi):
        //   Xóa đề thi mà chưa giao bài thi cho bất kỳ lớp nào (và chưa có học sinh làm bài).
        // -----------------------------------------------------------------------
        private void btnDe_XoaDeThi_Click(object sender, EventArgs e)
        {
            // 1. Xác định ID đề thi cần xóa
            int deID = _selectedDeThiID;
            if (deID <= 0 && cmbGV_ChonDeThi.SelectedValue != null)
            {
                int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out deID);
            }

            if (deID <= 0 && dgvDanhSachDeThi.CurrentRow != null && dgvDanhSachDeThi.CurrentRow.DataBoundItem is DataRowView drv)
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
                txtDe_SoCau.Clear();
                txtDe_ThoiGian.Clear();
                if (txtGV_DeThi != null) txtGV_DeThi.Clear();
                dgvDanhSachCauHoi.DataSource = null;

                LoadDeThiCombobox();
            }
            else
            {
                MessageBox.Show(thongBao, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 6. Tab Kết Quả Thi & Xuất File Excel / CSV
        private void cboKQ_ChonDeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKQ_ChonDeThi.SelectedValue != null && int.TryParse(cboKQ_ChonDeThi.SelectedValue.ToString(), out int deID))
            {
                DataTable dt = _deThiDAL.LayKetQuaTheoDeThi(deID);
                dgvKetQuaHocSinh.AutoGenerateColumns = false;
                dataGridViewTextBoxColumn25.DataPropertyName = "HoTen";
                dataGridViewTextBoxColumn26.DataPropertyName = "TenDeThi";
                dataGridViewTextBoxColumn27.DataPropertyName = "SoCauDung";
                dataGridViewTextBoxColumn28.DataPropertyName = "Diem";
                dataGridViewTextBoxColumn29.DataPropertyName = "ThoiGianNop";
                dgvKetQuaHocSinh.DataSource = dt;
            }
        }

        private void btnKQ_XuatExcel_Click(object sender, EventArgs e)
        {
            if (dgvKetQuaHocSinh.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv|Excel File (*.xls)|*.xls",
                FileName = $"KetQuaThi_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    // Tiêu đề cột
                    for (int i = 0; i < dgvKetQuaHocSinh.Columns.Count; i++)
                    {
                        sb.Append($"\"{dgvKetQuaHocSinh.Columns[i].HeaderText}\"");
                        if (i < dgvKetQuaHocSinh.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();

                    // Dữ liệu từng dòng
                    foreach (DataGridViewRow row in dgvKetQuaHocSinh.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            for (int i = 0; i < dgvKetQuaHocSinh.Columns.Count; i++)
                            {
                                sb.Append($"\"{row.Cells[i].Value?.ToString()}\"");
                                if (i < dgvKetQuaHocSinh.Columns.Count - 1) sb.Append(",");
                            }
                            sb.AppendLine();
                        }
                    }

                    // Ghi ra file với chuẩn UTF-8 BOM để Excel không lỗi font tiếng Việt
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file kết quả thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
                    txtGV_Profile_HoTen.Text = gv.HoTen;
                    txtGV_Profile_Email.Text = gv.Email;
                    txtGV_Profile_SDT.Text = gv.SoDienThoai;
                    txtGV_Profile_DiaChi.Text = gv.DiaChi;
                    cboHV_Profile_GioiTinh.SelectedItem = gv.GioiTinh;
                }
            }
            catch { }
        }

        private void btnGV_CapNhatThongTin_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtGV_Profile_HoTen.Text))
                {
                    MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGV_Profile_HoTen.Focus();
                    return;
                }

                if (!Validation.IsValidEmail(txtGV_Profile_Email.Text, out string errEmail))
                {
                    MessageBox.Show(errEmail, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGV_Profile_Email.Focus();
                    return;
                }

                if (!Validation.IsValidPhone(txtGV_Profile_SDT.Text, out string errPhone))
                {
                    MessageBox.Show(errPhone, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtGV_Profile_SDT.Focus();
                    return;
                }

                GiaoVien gv = new GiaoVien
                {
                    GiaoVienID = Session.NguoiDungID,
                    HoTen = txtGV_Profile_HoTen.Text.Trim(),
                    Email = txtGV_Profile_Email.Text.Trim(),
                    SoDienThoai = txtGV_Profile_SDT.Text.Trim(),
                    DiaChi = txtGV_Profile_DiaChi.Text.Trim(),
                    GioiTinh = cboHV_Profile_GioiTinh.SelectedItem?.ToString()
                };

                if (new GiaoVienDAL().Sua(gv))
                {
                    MessageBox.Show("Cập nhật thông tin cá nhân thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblGV_LoiChao.Text = $"Xin Chào, {gv.HoTen}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnGV_DoiMatKhau_Click(object sender, EventArgs e)
        {
            string cu = txtGV_MatKhauCu.Text.Trim();
            string moi = txtGV_MatKhauMoi.Text.Trim();
            string xacNhan = txtGV_XacNhanMatKhau.Text.Trim();

            if (string.IsNullOrEmpty(cu) || string.IsNullOrEmpty(moi))
            {
                MessageBox.Show("Vui lòng điền đầy đủ mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (moi != xacNhan)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra mật khẩu cũ
            var tk = _tkDAL.DangNhap(Session.TenDangNhap, cu);
            if (tk == null)
            {
                MessageBox.Show("Mật khẩu hiện tại không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_tkDAL.DoiMatKhau(Session.TaiKhoanID, moi))
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtGV_MatKhauCu.Clear();
                txtGV_MatKhauMoi.Clear();
                txtGV_XacNhanMatKhau.Clear();
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
