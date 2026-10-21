using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Data;
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

        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
            this.FormClosed += Frm_GiaoVien_FormClosed;
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

            // Môn học / Khóa học cho form Đề thi
            cboDe_MonHoc.Items.Clear();
            if (dtKH != null && dtKH.Rows.Count > 0)
            {
                foreach (DataRow r in dtKH.Rows)
                {
                    string tenKH = r["TenKhoaHoc"]?.ToString();
                    if (!string.IsNullOrEmpty(tenKH) && !cboDe_MonHoc.Items.Contains(tenKH))
                        cboDe_MonHoc.Items.Add(tenKH);
                }
            }
            if (cboDe_MonHoc.Items.Count == 0)
            {
                cboDe_MonHoc.Items.AddRange(new object[] { "Lập trình C# WinForms", "Cơ sở dữ liệu", "Tin học đại cương" });
            }
            cboDe_MonHoc.SelectedIndex = 0;
        }

        private void SetupEventHandlers()
        {
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
            cmbGV_ChonDeThi.SelectedIndexChanged += cmbGV_ChonDeThi_SelectedIndexChanged;
            btnCH_Them.Click += btnCH_Them_Click;
            btnCH_Sua.Click += btnCH_Sua_Click;
            btnCH_Xoa.Click += btnCH_Xoa_Click;
            dgvDanhSachCauHoi.CellClick += dgvDanhSachCauHoi_CellClick;

            // Tab Đề thi
            btnDe_TaoDeThi.Click += btnDe_TaoDeThi_Click;

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
            else if (tabText.Contains("Duyệt") || materialTabControl1.SelectedTab == pageGV_DuyetHocVien)
            {
                LoadDanhSachLopQuanLy();
                cboDuyet_ChonLop_SelectedIndexChanged(null, null);
            }
            else if (tabText.Contains("Ngân Hàng") || tabText.Contains("Câu Hỏi") || materialTabControl1.SelectedTab == pageGV_NganHangCauHoi)
            {
                LoadDeThiCombobox();
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
            }
            else if (tabText.Contains("Đề Thi") || materialTabControl1.SelectedTab == pageGV_QuanLyDeThi)
            {
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
                dtGV_LopHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
                dtGV_LopHoc.DataSource = dt;

                // Load danh sách lớp vào combobox duyệt & giao bài thi
                cboDuyet_ChonLop.DataSource = dt.Copy();
                cboDuyet_ChonLop.DisplayMember = "TenLop";
                cboDuyet_ChonLop.ValueMember = "LopHocID";

                cboDe_ChonLopHoc.DataSource = dt.Copy();
                cboDe_ChonLopHoc.DisplayMember = "TenLop";
                cboDe_ChonLopHoc.ValueMember = "LopHocID";
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
                    txtGV_SiSo.Text = "40";
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

            LopHoc lop = new LopHoc
            {
                TenLop = txtGV_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                LichHoc = txtGV_LichHoc.Text.Trim(),
                SiSoToiDa = siSo,
                TrangThai = "Đang mở"
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

            LopHoc lop = new LopHoc
            {
                LopHocID = _selectedLopHocID,
                TenLop = txtGV_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                SiSoToiDa = siSo
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
            if (cboDuyet_ChonLop.SelectedValue != null &&
                int.TryParse(cboDuyet_ChonLop.SelectedValue.ToString(),
                             out int lopID))
            {
                DataTable dt = _lopDAL.LayDangKyTheoLop(lopID);

                // Không tự tạo thêm cột
                dgvDangKyChoDuyet.AutoGenerateColumns = false;

                // Ánh xạ cột Designer với cột trong DataTable
                dgvDangKyChoDuyet.Columns[0].DataPropertyName = "HoTen";
                dgvDangKyChoDuyet.Columns[1].DataPropertyName = "Email";
                dgvDangKyChoDuyet.Columns[2].DataPropertyName = "SoDienThoai";
                dgvDangKyChoDuyet.Columns[3].DataPropertyName = "TenLop";
                dgvDangKyChoDuyet.Columns[4].DataPropertyName = "SiSo";
                dgvDangKyChoDuyet.Columns[5].DataPropertyName = "NgayDangKy";

                dgvDangKyChoDuyet.AutoSizeColumnsMode =
                    DataGridViewAutoSizeColumnsMode.None;

                dgvDangKyChoDuyet.DataSource = null;
                dgvDangKyChoDuyet.DataSource = dt;

            }
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
                DataTable dt = _deThiDAL.LayDeThiTheoGiaoVien(Session.NguoiDungID);

                // Gán vào Tab Câu hỏi
                cmbGV_ChonDeThi.DataSource = dt.Copy();
                cmbGV_ChonDeThi.DisplayMember = "TenDeThi";
                cmbGV_ChonDeThi.ValueMember = "DeThiID";

                // Gán vào Tab Kết quả
                cboKQ_ChonDeThi.DataSource = dt.Copy();
                cboKQ_ChonDeThi.DisplayMember = "TenDeThi";
                cboKQ_ChonDeThi.ValueMember = "DeThiID";

                // Bảng quản lý đề thi
                dgvDanhSachDeThi.AutoGenerateColumns = false;

                // Ánh xạ 5 cột giao diện với dữ liệu SQL
                dgvDanhSachDeThi.Columns[0].DataPropertyName = "TenDeThi";
                dgvDanhSachDeThi.Columns[1].DataPropertyName = "MonHoc";
                dgvDanhSachDeThi.Columns[2].DataPropertyName = "ThoiGian";
                dgvDanhSachDeThi.Columns[3].DataPropertyName = "TenLop";
                dgvDanhSachDeThi.Columns[4].DataPropertyName = "SoCau";

                dgvDanhSachDeThi.DataSource = dt;
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

        private void cmbGV_ChonDeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGV_ChonDeThi.SelectedValue != null && int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out int deID))
            {
                var listCH = _deThiDAL.LayDanhSachCauHoi(deID);

                dgvDanhSachCauHoi.AutoGenerateColumns = false;

                // Ánh xạ các cột đã có trong Designer
                dataGridViewTextBoxColumn19.DataPropertyName = "DeThiID";
                dataGridViewTextBoxColumn20.DataPropertyName = "NoiDung";
                dataGridViewTextBoxColumn21.DataPropertyName = "DapAnDung";
                dataGridViewTextBoxColumn22.DataPropertyName = "ThuTu";

                dgvDanhSachCauHoi.DataSource = listCH;
            }
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
            if (cmbGV_ChonDeThi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đề thi cần thêm câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtCH_NoiDung.Text))
            {
                MessageBox.Show("Vui lòng nhập nội dung câu hỏi!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtCH_NoiDung.Focus();
                return;
            }

            int deID = Convert.ToInt32(cmbGV_ChonDeThi.SelectedValue);
            CauHoi ch = new CauHoi
            {
                DeThiID = deID,
                NoiDung = txtCH_NoiDung.Text.Trim(),
                DapAnA = txtCH_DapAnA.Text.Trim(),
                DapAnB = txtCH_DapAnB.Text.Trim(),
                DapAnC = txtCH_DapAnC.Text.Trim(),
                DapAnD = txtCH_DapAnD.Text.Trim(),
                DapAnDung = cboCH_DapAnDung.SelectedItem?.ToString() ?? "A",
                ThuTu = 1
            };

            if (_deThiDAL.ThemCauHoi(ch))
            {
                MessageBox.Show("Thêm câu hỏi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
                        MessageBox.Show("Đã xóa câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
        private void btnDe_TaoDeThi_Click(object sender, EventArgs e)
        {
            if (cboDe_ChonLopHoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn lớp học để giao bài!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int thoiGian = 15;
            int.TryParse(txtDe_ThoiGian.Text, out thoiGian);
            int soCau = 20;
            int.TryParse(txtDe_SoCau.Text, out soCau);

            DeThi de = new DeThi
            {
                TenDeThi = txtGV_DeThi != null && !string.IsNullOrWhiteSpace(txtGV_DeThi.Text) ? txtGV_DeThi.Text.Trim() : "Bài kiểm tra trắc nghiệm",
                MonHoc = cboDe_MonHoc.SelectedItem?.ToString() ?? "Tin học đại cương",
                LopHocID = Convert.ToInt32(cboDe_ChonLopHoc.SelectedValue),
                GiaoVienID = Session.NguoiDungID,
                ThoiGian = thoiGian > 0 ? thoiGian : 15,
                SoCau = soCau > 0 ? soCau : 20,
                HanNop = DateTime.Now.AddDays(7),
                TrangThai = "Đang mở"
            };

            if (_deThiDAL.ThemDeThi(de))
            {
                MessageBox.Show("Tạo và giao đề thi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDeThiCombobox();
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
