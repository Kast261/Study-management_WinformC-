using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_Admin : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly GiaoVienDAL _gvDAL = new GiaoVienDAL();
        private readonly HocSinhDAL _hsDAL = new HocSinhDAL();
        private readonly KhoaHocDAL _khDAL = new KhoaHocDAL();
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();

        private int _selectedGiaoVienID = 0;
        private int _selectedHocVienID = 0;
        private int _selectedKhoaHocID = 0;
        private int _selectedLopHocID = 0;

        public Frm_Admin()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;

            this.FormClosed += Frm_Admin_FormClosed;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            InitDropdownOptions();
            SetupEventHandlers();
            SetupTaiKhoanEvents();

            LoadDashboard();
            LoadDanhSachGiaoVien();
            LoadDanhSachHocVien();
            LoadDanhSachKhoaHoc();
            LoadDanhSachLopHoc();
            LoadDanhSachTaiKhoan();
            LoadDropdownData();

            LamMoiFormGV();
            LamMoiFormHV();
            LamMoiFormKH();
            LamMoiFormLop();
            LamMoiFormTK();
        }

        #region Khởi tạo & Đăng ký sự kiện
        private void InitDropdownOptions()
        {
            // Giới tính Giáo Viên
            txtGV_GioiTinh.Items.Clear();
            txtGV_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            txtGV_GioiTinh.SelectedIndex = 0;

            // Giới tính Học Viên
            cboHV_GioiTinh.Items.Clear();
            cboHV_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cboHV_GioiTinh.SelectedIndex = 0;

            // Vai trò Tài Khoản
            if (cmbTK_VaiTro != null)
            {
                cmbTK_VaiTro.Items.Clear();
                cmbTK_VaiTro.Items.AddRange(new object[] { "Admin", "GiaoVien", "HocSinh" });
                cmbTK_VaiTro.SelectedIndex = 2; // Mặc định HocSinh
            }
        }

        private void SetupEventHandlers()
        {
            // --- Giáo Viên ---
            btnGV_Them.Click += btnGV_Them_Click;
            btnGV_Sua.Click += btnGV_Sua_Click;
            btnGV_Xoa.Click += btnGV_Xoa_Click;
            btnGV_LamMoi.Click += btnGV_LamMoi_Click;
            txtGV_TimKiem.TextChanged += txtGV_TimKiem_TextChanged;
            dgvGiaoVien.CellClick += dgvGiaoVien_CellClick;

            // --- Học Viên ---
            btnHV_Them.Click += btnHV_Them_Click;
            btnHV_Sua.Click += btnHV_Sua_Click;
            btnHV_Xoa.Click += btnHV_Xoa_Click;
            btnHV_LamMoi.Click += btnHV_LamMoi_Click;
            txtHV_TimKiem.TextChanged += txtHV_TimKiem_TextChanged;
            dgvHocVien.CellClick += dgvHocVien_CellClick;

            // --- Khóa Học ---
            btnKH_Them.Click += btnKH_Them_Click;
            btnKH_Sua.Click += btnKH_Sua_Click;
            btnKH_Xoa.Click += btnKH_Xoa_Click;
            dgvKhoaHoc.CellClick += dgvKhoaHoc_CellClick;

            // --- Lớp Học ---
            btnLop_Them.Click += btnLop_Them_Click;
            btnLop_Sua.Click += btnLop_Sua_Click;
            btnLop_Xoa.Click += btnLop_Xoa_Click;
            dgvLopHoc.CellClick += dgvLopHoc_CellClick;
        }

        private void LoadDropdownData()
        {
            try
            {
                // Load danh sách khóa học vào ComboBox Lớp Học
                DataTable dtKH = _khDAL.LayTatCa();
                cboLop_KhoaHoc.DataSource = dtKH;
                cboLop_KhoaHoc.DisplayMember = "TenKhoaHoc";
                cboLop_KhoaHoc.ValueMember = "KhoaHocID";

                // Load danh sách giáo viên vào ComboBox Lớp Học (materialComboBox2)
                DataTable dtGV = _gvDAL.LayTatCa();
                materialComboBox2.DataSource = dtGV;
                materialComboBox2.DisplayMember = "HoTen";
                materialComboBox2.ValueMember = "GiaoVienID";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh mục khóa học / giáo viên: " + ex.Message, "Thông báo");
            }
        }
        #endregion

        #region Dashboard & Thống kê
        private void LoadDashboard()
        {
            try
            {
                int soHV = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
                int soGV = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
                int soKH = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KhoaHoc"));
                int soLop = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc"));

                materialLabel1.Text = $"Học Viên: {soHV}";
                materialLabel2.Text = $"Giáo Viên: {soGV}";
                materialLabel3.Text = $"Khóa Học: {soKH}";
                materialLabel4.Text = $"Lớp Học: {soLop}";

                chart1.Series.Clear();
                Series series = new Series("Điểm trung bình") { ChartType = SeriesChartType.Column };
                DataTable dtDiem = DatabaseHelper.ExecuteQuery(@"
                    SELECT TOP 5 d.TenDeThi, AVG(k.Diem) AS DiemTB 
                    FROM KetQuaThi k JOIN DeThi d ON k.DeThiID = d.DeThiID 
                    GROUP BY d.TenDeThi");

                foreach (DataRow r in dtDiem.Rows)
                {
                    series.Points.AddXY(r["TenDeThi"].ToString(), Convert.ToDouble(r["DiemTB"]));
                }
                chart1.Series.Add(series);
            }
            catch { }
        }
        #endregion

        #region Quản lý Giáo Viên
        private void LoadDanhSachGiaoVien()
        {
            try
            {
                dgvGiaoVien.AutoGenerateColumns = false;
                Column1.DataPropertyName = "GiaoVienID";
                Column2.DataPropertyName = "HoTen";
                Column3.DataPropertyName = "ChuyenMon";
                Column4.DataPropertyName = "Email";
                Column5.DataPropertyName = "SoDienThoai";

                dgvGiaoVien.DataSource = _gvDAL.LayTatCa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách giáo viên: " + ex.Message, "Lỗi");
            }
        }

        private void LamMoiFormGV()
        {
            _selectedGiaoVienID = 0;
            txtGV_HoTen.Clear();
            txtGV_NgaySinh.Clear();
            txtGV_ChuyenMon.Clear();
            txtGV_Email.Clear();
            txtGV_SoDienThoai.Clear();
            txtGV_DiaChi.Clear();
            txtGV_TimKiem.Clear();
            if (txtGV_GioiTinh.Items.Count > 0) txtGV_GioiTinh.SelectedIndex = 0;
            txtGV_HoTen.Focus();
        }

        private void dgvGiaoVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvGiaoVien.Rows.Count)
            {
                DataGridViewRow row = dgvGiaoVien.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedGiaoVienID = Convert.ToInt32(drv["GiaoVienID"]);
                    txtGV_HoTen.Text = drv["HoTen"]?.ToString();
                    txtGV_ChuyenMon.Text = drv["ChuyenMon"]?.ToString();
                    txtGV_Email.Text = drv["Email"]?.ToString();
                    txtGV_SoDienThoai.Text = drv["SoDienThoai"]?.ToString();
                    txtGV_DiaChi.Text = drv["DiaChi"]?.ToString();

                    if (drv["NgaySinh"] != DBNull.Value)
                        txtGV_NgaySinh.Text = Convert.ToDateTime(drv["NgaySinh"]).ToString("dd/MM/yyyy");
                    else
                        txtGV_NgaySinh.Clear();

                    string gt = drv["GioiTinh"]?.ToString();
                    if (!string.IsNullOrEmpty(gt) && txtGV_GioiTinh.Items.Contains(gt))
                        txtGV_GioiTinh.SelectedItem = gt;
                }
            }
        }

        private void btnGV_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGV_HoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên giáo viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_HoTen.Focus();
                return;
            }

            GiaoVien gv = new GiaoVien
            {
                HoTen = txtGV_HoTen.Text.Trim(),
                NgaySinh = ParseDate(txtGV_NgaySinh.Text),
                GioiTinh = txtGV_GioiTinh.SelectedItem?.ToString(),
                ChuyenMon = txtGV_ChuyenMon.Text.Trim(),
                Email = txtGV_Email.Text.Trim(),
                SoDienThoai = txtGV_SoDienThoai.Text.Trim(),
                DiaChi = txtGV_DiaChi.Text.Trim()
            };

            if (_gvDAL.Them(gv))
            {
                MessageBox.Show("Thêm giáo viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachGiaoVien();
                LoadDropdownData();
                LoadDashboard();
                LamMoiFormGV();
            }
        }

        private void btnGV_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedGiaoVienID <= 0)
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtGV_HoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtGV_HoTen.Focus();
                return;
            }

            GiaoVien gv = new GiaoVien
            {
                GiaoVienID = _selectedGiaoVienID,
                HoTen = txtGV_HoTen.Text.Trim(),
                NgaySinh = ParseDate(txtGV_NgaySinh.Text),
                GioiTinh = txtGV_GioiTinh.SelectedItem?.ToString(),
                ChuyenMon = txtGV_ChuyenMon.Text.Trim(),
                Email = txtGV_Email.Text.Trim(),
                SoDienThoai = txtGV_SoDienThoai.Text.Trim(),
                DiaChi = txtGV_DiaChi.Text.Trim()
            };

            if (_gvDAL.Sua(gv))
            {
                MessageBox.Show("Cập nhật thông tin giáo viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachGiaoVien();
                LoadDropdownData();
                LamMoiFormGV();
            }
        }

        private void btnGV_Xoa_Click(object sender, EventArgs e)
        {
            if (_selectedGiaoVienID <= 0)
            {
                MessageBox.Show("Vui lòng chọn giáo viên cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa giáo viên này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_gvDAL.Xoa(_selectedGiaoVienID))
                    {
                        MessageBox.Show("Đã xóa giáo viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDanhSachGiaoVien();
                        LoadDropdownData();
                        LoadDashboard();
                        LamMoiFormGV();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa giáo viên: " + ex.Message, "Lỗi xóa dữ liệu", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnGV_LamMoi_Click(object sender, EventArgs e)
        {
            LamMoiFormGV();
            LoadDanhSachGiaoVien();
        }

        private void txtGV_TimKiem_TextChanged(object sender, EventArgs e)
        {
            string kw = txtGV_TimKiem.Text.Trim();
            if (string.IsNullOrEmpty(kw) || kw == "Tên cần tìm")
            {
                LoadDanhSachGiaoVien();
            }
            else
            {
                dgvGiaoVien.DataSource = _gvDAL.TimKiem(kw);
            }
        }
        #endregion

        #region Quản lý Học Viên
        private void LoadDanhSachHocVien()
        {
            try
            {
                dgvHocVien.AutoGenerateColumns = false;
                dataGridViewTextBoxColumn1.DataPropertyName = "HocSinhID";
                dataGridViewTextBoxColumn2.DataPropertyName = "HoTen";
                dataGridViewTextBoxColumn3.DataPropertyName = "GioiTinh";
                dataGridViewTextBoxColumn4.DataPropertyName = "Email";
                dataGridViewTextBoxColumn5.DataPropertyName = "SoDienThoai";

                dgvHocVien.DataSource = _hsDAL.LayTatCa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách học viên: " + ex.Message, "Lỗi");
            }
        }

        private void LamMoiFormHV()
        {
            _selectedHocVienID = 0;
            txtHV_HoTen.Clear();
            txtHV_NgaySinh.Clear();
            txtHV_Email.Clear();
            txtHV_SoDienThoai.Clear();
            txtHV_DiaChi.Clear();
            txtHV_TimKiem.Clear();
            if (cboHV_GioiTinh.Items.Count > 0) cboHV_GioiTinh.SelectedIndex = 0;
            txtHV_HoTen.Focus();
        }

        private void dgvHocVien_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvHocVien.Rows.Count)
            {
                DataGridViewRow row = dgvHocVien.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedHocVienID = Convert.ToInt32(drv["HocSinhID"]);
                    txtHV_HoTen.Text = drv["HoTen"]?.ToString();
                    txtHV_Email.Text = drv["Email"]?.ToString();
                    txtHV_SoDienThoai.Text = drv["SoDienThoai"]?.ToString();
                    txtHV_DiaChi.Text = drv["DiaChi"]?.ToString();

                    if (drv["NgaySinh"] != DBNull.Value)
                        txtHV_NgaySinh.Text = Convert.ToDateTime(drv["NgaySinh"]).ToString("dd/MM/yyyy");
                    else
                        txtHV_NgaySinh.Clear();

                    string gt = drv["GioiTinh"]?.ToString();
                    if (!string.IsNullOrEmpty(gt) && cboHV_GioiTinh.Items.Contains(gt))
                        cboHV_GioiTinh.SelectedItem = gt;
                }
            }
        }

        private void btnHV_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHV_HoTen.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên học viên!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHV_HoTen.Focus();
                return;
            }

            HocSinh hs = new HocSinh
            {
                HoTen = txtHV_HoTen.Text.Trim(),
                NgaySinh = ParseDate(txtHV_NgaySinh.Text),
                GioiTinh = cboHV_GioiTinh.SelectedItem?.ToString(),
                Email = txtHV_Email.Text.Trim(),
                SoDienThoai = txtHV_SoDienThoai.Text.Trim(),
                DiaChi = txtHV_DiaChi.Text.Trim()
            };

            if (_hsDAL.Them(hs))
            {
                MessageBox.Show("Thêm học viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachHocVien();
                LoadDashboard();
                LoadDanhSachLopHoc(); // Cập nhật sĩ số các lớp
                LamMoiFormHV();
            }
        }

        private void btnHV_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedHocVienID <= 0)
            {
                MessageBox.Show("Vui lòng chọn học viên cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtHV_HoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHV_HoTen.Focus();
                return;
            }

            HocSinh hs = new HocSinh
            {
                HocSinhID = _selectedHocVienID,
                HoTen = txtHV_HoTen.Text.Trim(),
                NgaySinh = ParseDate(txtHV_NgaySinh.Text),
                GioiTinh = cboHV_GioiTinh.SelectedItem?.ToString(),
                Email = txtHV_Email.Text.Trim(),
                SoDienThoai = txtHV_SoDienThoai.Text.Trim(),
                DiaChi = txtHV_DiaChi.Text.Trim()
            };

            if (_hsDAL.Sua(hs))
            {
                MessageBox.Show("Cập nhật thông tin học viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachHocVien();
                LamMoiFormHV();
            }
        }

        private void btnHV_Xoa_Click(object sender, EventArgs e)
        {
            if (_selectedHocVienID <= 0)
            {
                MessageBox.Show("Vui lòng chọn học viên cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa học viên này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_hsDAL.Xoa(_selectedHocVienID))
                    {
                        MessageBox.Show("Đã xóa học viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDanhSachHocVien();
                        LoadDashboard();
                        LoadDanhSachLopHoc(); // Cập nhật lại sĩ số
                        LamMoiFormHV();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa học viên: " + ex.Message, "Lỗi xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnHV_LamMoi_Click(object sender, EventArgs e)
        {
            LamMoiFormHV();
            LoadDanhSachHocVien();
        }

        private void txtHV_TimKiem_TextChanged(object sender, EventArgs e)
        {
            string kw = txtHV_TimKiem.Text.Trim();
            if (string.IsNullOrEmpty(kw) || kw == "Tên cần tìm")
            {
                LoadDanhSachHocVien();
            }
            else
            {
                dgvHocVien.DataSource = _hsDAL.TimKiem(kw);
            }
        }
        #endregion

        #region Quản lý Khóa Học
        private void LoadDanhSachKhoaHoc()
        {
            try
            {
                dgvKhoaHoc.AutoGenerateColumns = false;
                Column6.DataPropertyName = "TenKhoaHoc";
                Column7.DataPropertyName = "MoTa";

                dgvKhoaHoc.DataSource = _khDAL.LayTatCa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách khóa học: " + ex.Message, "Lỗi");
            }
        }

        private void LamMoiFormKH()
        {
            _selectedKhoaHocID = 0;
            txtKH_TenKhoaHoc.Clear();
            txtKH_MoTa.Clear();
            txtKH_TenKhoaHoc.Focus();
        }

        private void dgvKhoaHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvKhoaHoc.Rows.Count)
            {
                DataGridViewRow row = dgvKhoaHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedKhoaHocID = Convert.ToInt32(drv["KhoaHocID"]);
                    txtKH_TenKhoaHoc.Text = drv["TenKhoaHoc"]?.ToString();
                    txtKH_MoTa.Text = drv["MoTa"]?.ToString();
                }
            }
        }

        private void btnKH_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtKH_TenKhoaHoc.Text))
            {
                MessageBox.Show("Vui lòng nhập tên khóa học!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKH_TenKhoaHoc.Focus();
                return;
            }

            KhoaHoc kh = new KhoaHoc
            {
                TenKhoaHoc = txtKH_TenKhoaHoc.Text.Trim(),
                MoTa = txtKH_MoTa.Text.Trim()
            };

            if (_khDAL.Them(kh))
            {
                MessageBox.Show("Thêm khóa học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachKhoaHoc();
                LoadDropdownData();
                LoadDashboard();
                LamMoiFormKH();
            }
        }

        private void btnKH_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedKhoaHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn khóa học cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtKH_TenKhoaHoc.Text))
            {
                MessageBox.Show("Tên khóa học không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtKH_TenKhoaHoc.Focus();
                return;
            }

            KhoaHoc kh = new KhoaHoc
            {
                KhoaHocID = _selectedKhoaHocID,
                TenKhoaHoc = txtKH_TenKhoaHoc.Text.Trim(),
                MoTa = txtKH_MoTa.Text.Trim()
            };

            if (_khDAL.Sua(kh))
            {
                MessageBox.Show("Cập nhật khóa học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachKhoaHoc();
                LoadDanhSachLopHoc();
                LoadDropdownData();
            }
        }

        private void btnKH_Xoa_Click(object sender, EventArgs e)
        {
            if (_selectedKhoaHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn khóa học cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa khóa học này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_khDAL.Xoa(_selectedKhoaHocID))
                    {
                        MessageBox.Show("Đã xóa khóa học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDanhSachKhoaHoc();
                        LoadDropdownData();
                        LoadDashboard();
                        LamMoiFormKH();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa khóa học: " + ex.Message, "Lỗi xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Quản lý Lớp Học
        private void LoadDanhSachLopHoc()
        {
            try
            {
                dgvLopHoc.AutoGenerateColumns = false;
                Column8.DataPropertyName = "TenLop";
                Column13.DataPropertyName = "SiSo";
                Column14.DataPropertyName = "TenKhoaHoc";
                Column15.DataPropertyName = "TenGiaoVien";

                dgvLopHoc.DataSource = _lopDAL.LayTatCa();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp học: " + ex.Message, "Lỗi");
            }
        }

        private void LamMoiFormLop()
        {
            _selectedLopHocID = 0;
            txtLop_TenLop.Clear();
            txtLop_SiSoToiDa.Text = "40";
            if (cboLop_KhoaHoc.Items.Count > 0) cboLop_KhoaHoc.SelectedIndex = 0;
            if (materialComboBox2.Items.Count > 0) materialComboBox2.SelectedIndex = 0;
            txtLop_TenLop.Focus();
        }

        private void dgvLopHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvLopHoc.Rows.Count)
            {
                DataGridViewRow row = dgvLopHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedLopHocID = Convert.ToInt32(drv["LopHocID"]);
                    txtLop_TenLop.Text = drv["TenLop"]?.ToString();
                    txtLop_SiSoToiDa.Text = drv["SiSoToiDa"]?.ToString();

                    if (drv["KhoaHocID"] != DBNull.Value)
                        cboLop_KhoaHoc.SelectedValue = Convert.ToInt32(drv["KhoaHocID"]);

                    if (drv["GiaoVienID"] != DBNull.Value)
                        materialComboBox2.SelectedValue = Convert.ToInt32(drv["GiaoVienID"]);
                }
            }
        }

        private void btnLop_Them_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLop_TenLop.Text))
            {
                MessageBox.Show("Vui lòng nhập tên lớp học!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLop_TenLop.Focus();
                return;
            }

            int siSo = 40;
            int.TryParse(txtLop_SiSoToiDa.Text.Trim(), out siSo);

            int khID = 0;
            if (cboLop_KhoaHoc.SelectedValue != null)
                int.TryParse(cboLop_KhoaHoc.SelectedValue.ToString(), out khID);

            int gvID = 0;
            if (materialComboBox2.SelectedValue != null)
                int.TryParse(materialComboBox2.SelectedValue.ToString(), out gvID);

            LopHoc lop = new LopHoc
            {
                TenLop = txtLop_TenLop.Text.Trim(),
                SiSoToiDa = siSo > 0 ? siSo : 40,
                KhoaHocID = khID,
                GiaoVienID = gvID,
                TrangThai = "Đang mở"
            };

            if (_lopDAL.ThemLop(lop))
            {
                MessageBox.Show("Thêm lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopHoc();
                LoadDashboard();
                LamMoiFormLop();
            }
        }

        private void btnLop_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtLop_TenLop.Text))
            {
                MessageBox.Show("Tên lớp không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLop_TenLop.Focus();
                return;
            }

            int siSo = 40;
            int.TryParse(txtLop_SiSoToiDa.Text.Trim(), out siSo);

            int khID = 0;
            if (cboLop_KhoaHoc.SelectedValue != null)
                int.TryParse(cboLop_KhoaHoc.SelectedValue.ToString(), out khID);

            int gvID = 0;
            if (materialComboBox2.SelectedValue != null)
                int.TryParse(materialComboBox2.SelectedValue.ToString(), out gvID);

            LopHoc lop = new LopHoc
            {
                LopHocID = _selectedLopHocID,
                TenLop = txtLop_TenLop.Text.Trim(),
                SiSoToiDa = siSo > 0 ? siSo : 40,
                KhoaHocID = khID,
                GiaoVienID = gvID
            };

            if (_lopDAL.SuaLop(lop))
            {
                MessageBox.Show("Cập nhật thông tin lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopHoc();
            }
        }

        private void btnLop_Xoa_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần xóa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn xóa lớp học này?", "Xác nhận xóa", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                try
                {
                    if (_lopDAL.XoaLop(_selectedLopHocID))
                    {
                        MessageBox.Show("Đã xóa lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadDanhSachLopHoc();
                        LoadDashboard();
                        LamMoiFormLop();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Không thể xóa lớp: " + ex.Message, "Lỗi xóa", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Quản lý Tài Khoản
        private int _selectedTaiKhoanID = 0;
        private bool _selectedTrangThaiKhoa = false;

        private void LoadDanhSachTaiKhoan()
        {
            try
            {
                if (dgvTaiKhoan == null) return;
                dgvTaiKhoan.AutoGenerateColumns = false;
                Column9.DataPropertyName  = "TenDangNhap";
                Column10.DataPropertyName = "VaiTro";
                Column11.DataPropertyName = "TenNguoiDung";
                Column12.DataPropertyName = "TrangThaiHienThi";
                dgvTaiKhoan.DataSource = _tkDAL.LayTatCaCoTenNguoiDung();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách tài khoản: " + ex.Message, "Lỗi");
            }
        }

        private void LamMoiFormTK()
        {
            _selectedTaiKhoanID = 0;
            _selectedTrangThaiKhoa = false;
            if (txtTK_TaiKhoanDangNhap != null) txtTK_TaiKhoanDangNhap.Clear();
            if (txtTK_MatKhau != null) txtTK_MatKhau.Clear();
            if (cmbTK_VaiTro != null && cmbTK_VaiTro.Items.Count > 0) cmbTK_VaiTro.SelectedIndex = 0;
            UpdateNutKhoaMoKhoa();
        }

        private void UpdateNutKhoaMoKhoa()
        {
            if (btnTK_KhoaMoKhoa == null) return;
            if (_selectedTrangThaiKhoa)
                btnTK_KhoaMoKhoa.Text = "Mở Tài Khoản";
            else
                btnTK_KhoaMoKhoa.Text = "Khóa Tài Khoản";
        }

        private void SetupTaiKhoanEvents()
        {
            if (btnTK_TaoTaiKhoan != null)  btnTK_TaoTaiKhoan.Click  += btnTK_TaoTaiKhoan_Click;
            if (btnTK_KhoaMoKhoa != null)   btnTK_KhoaMoKhoa.Click   += btnTK_KhoaMoKhoa_Click;
            if (btnTK_DatLaiMatKhau != null) btnTK_DatLaiMatKhau.Click += btnTK_DatLaiMatKhau_Click;
            if (dgvTaiKhoan != null)         dgvTaiKhoan.CellClick    += dgvTaiKhoan_CellClick;
            if (cmbTK_VaiTro != null)        cmbTK_VaiTro.SelectedIndexChanged += cmbTK_VaiTro_Changed;
        }

        private void cmbTK_VaiTro_Changed(object sender, EventArgs e)
        {
            LoadDropdownGanCho();
        }

        private void LoadDropdownGanCho()
        {
            if (cmbTK_GanCho == null || cmbTK_VaiTro == null) return;
            string role = cmbTK_VaiTro.SelectedItem?.ToString() ?? "";
            try
            {
                if (role == "GiaoVien")
                {
                    DataTable dt = _gvDAL.LayTatCa();
                    cmbTK_GanCho.DataSource    = dt;
                    cmbTK_GanCho.DisplayMember = "HoTen";
                    cmbTK_GanCho.ValueMember   = "GiaoVienID";
                    cmbTK_GanCho.Visible       = true;
                }
                else if (role == "HocSinh")
                {
                    DataTable dt = _hsDAL.LayTatCa();
                    cmbTK_GanCho.DataSource    = dt;
                    cmbTK_GanCho.DisplayMember = "HoTen";
                    cmbTK_GanCho.ValueMember   = "HocSinhID";
                    cmbTK_GanCho.Visible       = true;
                }
                else
                {
                    cmbTK_GanCho.DataSource = null;
                    cmbTK_GanCho.Visible    = false;
                }
            }
            catch { }
        }

        private void dgvTaiKhoan_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvTaiKhoan.Rows.Count)
            {
                var row = dgvTaiKhoan.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedTaiKhoanID    = Convert.ToInt32(drv["TaiKhoanID"]);
                    _selectedTrangThaiKhoa = drv["TrangThaiKhoa"] != DBNull.Value && Convert.ToBoolean(drv["TrangThaiKhoa"]);
                    UpdateNutKhoaMoKhoa();
                }
            }
        }

        private void btnTK_TaoTaiKhoan_Click(object sender, EventArgs e)
        {
            if (txtTK_TaiKhoanDangNhap == null || string.IsNullOrWhiteSpace(txtTK_TaiKhoanDangNhap.Text))
            {
                MessageBox.Show("Vui lòng nhập tên đăng nhập!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (txtTK_MatKhau == null || string.IsNullOrWhiteSpace(txtTK_MatKhau.Text))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tenDangNhap = txtTK_TaiKhoanDangNhap.Text.Trim();
            string matKhauPlain = txtTK_MatKhau.Text.Trim();
            string vaiTro = cmbTK_VaiTro?.SelectedItem?.ToString() ?? "HocSinh";

            int nguoiDungID = 0;
            if (cmbTK_GanCho != null && cmbTK_GanCho.Visible && cmbTK_GanCho.SelectedValue != null)
                int.TryParse(cmbTK_GanCho.SelectedValue.ToString(), out nguoiDungID);

            try
            {
                if (_tkDAL.ThemTaiKhoan(tenDangNhap, matKhauPlain, vaiTro, nguoiDungID))
                {
                    MessageBox.Show("Tạo tài khoản thành công! Có thể đăng nhập ngay.", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDanhSachTaiKhoan();
                    LamMoiFormTK();
                }
                else
                {
                    MessageBox.Show("Tên đăng nhập đã tồn tại hoặc có lỗi xảy ra!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo tài khoản: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnTK_KhoaMoKhoa_Click(object sender, EventArgs e)
        {
            if (_selectedTaiKhoanID <= 0)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần khóa/mở!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool seKhoa = !_selectedTrangThaiKhoa; // Đảo trạng thái
            string msg  = seKhoa ? "Bạn có chắc chắn muốn KHÓA tài khoản này?" : "Bạn có chắc chắn muốn MỞ tài khoản này?";
            if (MessageBox.Show(msg, "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            if (_tkDAL.KhoaHoacMoKhoaTaiKhoan(_selectedTaiKhoanID, seKhoa))
            {
                string thongBao = seKhoa ? "Đã khóa tài khoản thành công!" : "Đã mở khóa tài khoản thành công!";
                MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                _selectedTrangThaiKhoa = seKhoa;
                UpdateNutKhoaMoKhoa();
                LoadDanhSachTaiKhoan();
            }
        }

        private void btnTK_DatLaiMatKhau_Click(object sender, EventArgs e)
        {
            if (_selectedTaiKhoanID <= 0)
            {
                MessageBox.Show("Vui lòng chọn tài khoản cần đặt lại mật khẩu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Đặt lại mật khẩu về '123456' cho tài khoản này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_tkDAL.DoiMatKhau(_selectedTaiKhoanID, "123456"))
                {
                    MessageBox.Show("Đã đặt lại mật khẩu thành '123456'!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("Đặt lại mật khẩu thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region Tiện ích chung & Điều hướng
        private DateTime? ParseDate(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return null;
            string[] formats = { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd", "dd-MM-yyyy", "MM/dd/yyyy" };
            if (DateTime.TryParseExact(input.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime d))
                return d;
            if (DateTime.TryParse(input.Trim(), out DateTime dt))
                return dt;
            return null;
        }

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialTabControl1.SelectedTab != null &&
                (materialTabControl1.SelectedTab == pageDangXuat || materialTabControl1.SelectedTab.Text.Contains("Đăng Xuất")))
            {
                DialogResult result = MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận đăng xuất", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Hide();
                    Form loginForm = Application.OpenForms["frm_DangNhap"];
                    if (loginForm != null)
                    {
                        loginForm.Show();
                    }
                    else
                    {
                        new frm_DangNhap().Show();
                    }
                    this.Close();
                }
                else
                {
                    materialTabControl1.SelectedIndex = 0;
                }
            }
        }

        private void Frm_Admin_FormClosed(object sender, FormClosedEventArgs e)
        {
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
        #endregion
    }
}