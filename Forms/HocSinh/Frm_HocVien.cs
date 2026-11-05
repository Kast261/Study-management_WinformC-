using BLT_Quan_Ly_Hoc_Tap.DAL;
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

        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private readonly HocSinhDAL _hocSinhDAL = new HocSinhDAL();
        private readonly TaiKhoanDAL _taiKhoanDAL = new TaiKhoanDAL();
        private int _selectedLopDangKyID = 0;
        private int _selectedLopDangHocID = 0;
        private int _selectedDeThiID = 0;
        // Lưu trữ danh sách đề thi gốc để hỗ trợ việc lọc bằng ComboBox
        private DataTable _dtDeThi;
        private DataTable _dtBaiThiDangMo;
        private bool _dangTaiLopLocKetQua;
        private bool _daTaiHocVien;

        #endregion

        #region 1. Khởi tạo & Đăng ký sự kiện

        public HocVien()
        {
            InitializeComponent();
            // Gán cột Designer trước khi phân trang bind nguồn rỗng, tránh mất các cột.
            ConfigureDataGrids();
            DataGridViewPager.AttachAll(this, KhiDoiTrang);
            foreverButton1.Click += btnHV_LamBaiNgay_Click;
            btnHV_VaoLopHoc.Click += btnHV_LamBaiNgay_Click;
            dgvHV_DeThiSapToi.CellDoubleClick += dgvHV_DeThiSapToi_CellDoubleClick;
            foreverButton1.Enabled = btnHV_VaoLopHoc.Enabled = false;
            materialLabel2.Text = "Đang tải danh sách bài thi...";
            materialLabel9.Text = "Lớp Đang Học";
            tabHocVien.SelectedIndexChanged += TabHocVien_SelectedIndexChanged;
            dgvLopDangKi.CellClick += dgvLopDangKi_CellClick;
            dgvLopThamGia.CellClick += dgvLopThamGia_CellClick;
            dgvHVDeThi.CellClick += dgvHVDeThi_CellClick;
            dgvHVDeThi.SelectionChanged += dgvHVDeThi_SelectionChanged;
            dgvHVKetQua.SelectionChanged += dgvHVKetQua_SelectionChanged;
            dgvHVKetQua.CellDoubleClick += dgvHVKetQua_CellDoubleClick;
            btnHV_DangKi.Click += btnDangKyLop_Click;
            btnHV_HuyDK.Click += btnHV_HuyDK_Click;
            materialComboBox1.SelectedIndexChanged += materialComboBox1_SelectedIndexChanged;
            btnHVDT_LamBai.Click += btnHVDT_LamBai_Click;
            btnHVKQ_XuatFile.Click += btnHVKQ_XuatFile_Click;
            btnHVKQ_BaoCao.Click += btnHVKQ_BaoCao_Click;
            KhoiTaoLocKetQua();
            KhoiTaoHoSoCaNhan();
            dgvHVDeThi_SelectionChanged(null, EventArgs.Empty);
            dgvHVKetQua_SelectionChanged(null, EventArgs.Empty);
            lblHV_DangChon.Text = "Đang chọn: chưa chọn lớp";
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            this.DrawerTabControl = tabHocVien;
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

        private void ConfigureDataGrids()
        {
            foreach (DataGridView grid in new[] { dgvLopDangKi, dgvLopThamGia, dgvHVDeThi, dgvHVKetQua, dgvHV_LopDangHoc, dgvHV_DeThiSapToi })
            {
                grid.AutoGenerateColumns = false;
                grid.ReadOnly = true;
                grid.MultiSelect = false;
                grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            }
            // Cột của các bảng mới trong Designer, gán trước khi AttachAll bind trang rỗng.
            maGV.DataPropertyName = "TenLop";
            Column10.DataPropertyName = "LichHoc";
            HoTen.DataPropertyName = "TenKhoaHoc";
            dataGridViewTextBoxColumn1.DataPropertyName = "TenGiaoVien";
            dataGridViewTextBoxColumn2.DataPropertyName = "SiSo";
            dataGridViewTextBoxColumn3.DataPropertyName = "TrangThai";

            dataGridViewTextBoxColumn6.DataPropertyName = "TenLop";
            dataGridViewTextBoxColumn7.DataPropertyName = "LichHoc";
            dataGridViewTextBoxColumn8.DataPropertyName = "TenKhoaHoc";
            dataGridViewTextBoxColumn9.DataPropertyName = "TenGiaoVien";
            dataGridViewTextBoxColumn10.DataPropertyName = "NgayBatDau";
            dataGridViewTextBoxColumn11.DataPropertyName = "NgayKetThuc";
            dataGridViewTextBoxColumn12.DataPropertyName = "TrangThaiDangKy";
            dataGridViewTextBoxColumn10.DefaultCellStyle.Format = dataGridViewTextBoxColumn11.DefaultCellStyle.Format = "dd/MM/yyyy";
            dataGridViewTextBoxColumn10.DefaultCellStyle.NullValue = dataGridViewTextBoxColumn11.DefaultCellStyle.NullValue = "";

            dataGridViewTextBoxColumn4.DataPropertyName = "TenDeThi";
            dataGridViewTextBoxColumn5.DataPropertyName = "MonHoc";
            Column1.DataPropertyName = "SoCau";
            Column2.DataPropertyName = "ThoiGian";
            Column3.DataPropertyName = "HanNop";
            Column3.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            Column3.DefaultCellStyle.NullValue = "Không giới hạn";
            Column4.DataPropertyName = "TrangThai";
            dataGridViewTextBoxColumn13.DataPropertyName = "Diem";
            dataGridViewTextBoxColumn13.DefaultCellStyle.Format = "0.00";

            dataGridViewTextBoxColumn14.DataPropertyName = "TenDeThi";
            dataGridViewTextBoxColumn15.DataPropertyName = "MonHoc";
            dataGridViewTextBoxColumn16.DataPropertyName = "ThoiGianNop";
            dataGridViewTextBoxColumn17.DataPropertyName = "Diem";
            dataGridViewTextBoxColumn16.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            dataGridViewTextBoxColumn17.DefaultCellStyle.Format = "0.00";
            dgvHVKetQua.ColumnHeadersVisible = true;

            Lop.DataPropertyName = "TenLop";
            GiaoVien.DataPropertyName = "TenGiaoVien";
            LichHoc.DataPropertyName = "LichHoc";
            BT_DeThi.DataPropertyName = "TenDeThi";
            BT_HanNop.DataPropertyName = "HanNop";
            BT_HanNop.DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            BT_HanNop.DefaultCellStyle.NullValue = "Không giới hạn";
            BT_SoCau.DataPropertyName = "SoCau";
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
        }

        #endregion

        #region 2. Tab Trang Chủ

        private void LoadLopHoc()
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoHocSinh(Session.NguoiDungID);
                if (dgvHV_LopDangHoc != null) dgvHV_LopDangHoc.DataSource = dtLop;
            }
            catch { }
        }

        private void btnHV_LamBaiNgay_Click(object sender, EventArgs e)
        {
            if (_dtBaiThiDangMo == null || _dtBaiThiDangMo.Rows.Count == 0) return;
            var selected = dgvHV_DeThiSapToi.CurrentRow?.DataBoundItem as DataRowView;
            DataRow row = selected?.Row ?? _dtBaiThiDangMo.Rows[0];
            BatDauThi(Convert.ToInt32(row["DeThiID"]));
        }

        private void dgvHV_DeThiSapToi_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var selected = dgvHV_DeThiSapToi.Rows[e.RowIndex].DataBoundItem as DataRowView;
            if (selected != null) BatDauThi(Convert.ToInt32(selected["DeThiID"]));
        }

        #endregion

        #region 3. Tab Lớp Học & Đăng Ký

        private void LoadTabLopHocCuaToi()
        {
            try
            {
                DataTable dtLopMo = _lopDAL.LayLopChuaDangKy(Session.NguoiDungID);
                if (dgvLopDangKi != null) dgvLopDangKi.DataSource = dtLopMo;

                DataTable dtLopDangHoc = _lopDAL.LayLopHocVienDangHocVaChoDuyet(Session.NguoiDungID);
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
            if (_selectedLopDangKyID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học muốn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn đăng ký lớp học này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.DangKyLop(_selectedLopDangKyID, Session.NguoiDungID, out string thongBao))
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
        }

        private void btnHV_HuyDK_Click(object sender, EventArgs e)
        {
            if (_selectedLopDangHocID <= 0)
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

            if (MessageBox.Show("Bạn có chắc chắn muốn hủy đăng ký lớp học này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.HuyDangKyLop(_selectedLopDangHocID, Session.NguoiDungID, out string thongBao))
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
        }

        #endregion

        #region 4. Tab Bài Thi & Làm Bài

        private void LoadDanhSachDeThi()
        {
            try
            {
                DataTable exams = _deThiDAL.LayDeThiChoHocSinh(Session.NguoiDungID);
                DataTable openExams = _deThiDAL.LayBaiThiDangMoChoHocSinh(Session.NguoiDungID);
                _selectedDeThiID = 0;
                _dtDeThi = exams;
                _dtBaiThiDangMo = openExams;
                dgvHVDeThi.DataSource = _dtDeThi;
                lblHV_TongBaiThi.Text = $"Tổng bài thi: {_dtDeThi.Rows.Count:N0}";
                // Nguồn riêng để lọc môn học ở tab Bài Thi không làm mất bài trên Trang chủ.
                dgvHV_DeThiSapToi.DataSource = _dtBaiThiDangMo;
                bool coBaiThi = _dtBaiThiDangMo.Rows.Count > 0;
                foreverButton1.Enabled = btnHV_VaoLopHoc.Enabled = coBaiThi;
                materialLabel2.Text = coBaiThi
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
                    materialComboBox1.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                _selectedDeThiID = 0;
                _dtDeThi = _dtBaiThiDangMo = null;
                dgvHVDeThi.DataSource = null;
                dgvHV_DeThiSapToi.DataSource = null;
                foreverButton1.Enabled = btnHV_VaoLopHoc.Enabled = false;
                materialLabel2.Text = "Không thể tải danh sách bài thi.";
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
                if (!_deThiDAL.CoTheBatDauThi(Session.NguoiDungID, deThiID))
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

        private void KhoiTaoLocKetQua()
        {
            cmbKQHV_BaiThi.Hint = "Lọc kết quả theo lớp đang học";
            cmbKQHV_BaiThi.SelectedIndexChanged += cmbKQHV_BaiThi_SelectedIndexChanged;
            lblHVKQ_TongBaiThi.Text = "Chọn lớp đang học để xem kết quả bài thi đã nộp.";
            btnHVKQ_XuatFile.Enabled = false;
            btnHVKQ_BaoCao.Enabled = false;
        }

        private void LoadLichSuThi()
        {
            int selectedClass = cmbKQHV_BaiThi.SelectedValue is int value ? value : 0;
            _dangTaiLopLocKetQua = true;
            try
            {
                using (DataTable classes = _lopDAL.LayTheoHocSinh(Session.NguoiDungID))
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
                    cmbKQHV_BaiThi.DisplayMember = "TenLop";
                    cmbKQHV_BaiThi.ValueMember = "LopHocID";
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
                DataTable results = _ketQuaDAL.LayKetQuaTheoLopDangHoc(Session.NguoiDungID, lopID);
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

                DataTable dtChiTiet = _ketQuaDAL.LayChiTietBaiLamTheoKetQua(ketQuaID);
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
                using (DataTable results = _ketQuaDAL.LayKetQuaTheoLopDangHoc(Session.NguoiDungID, lopID))
                    ExcelExportHelper.ExportBangDiemHocSinh(results, Session.HoTen ?? Session.TenDangNhap,
                        null, null, null, Session.TenDangNhap);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Không thể xuất báo cáo kết quả: " + ex.Message, "Lỗi");
            }
        }

        #endregion

        #region 6. Tab Thông Tin Cá Nhân & Đổi Mật Khẩu

        private void KhoiTaoHoSoCaNhan()
        {
            cmbTTHV_GioiTinh.Items.Clear();
            cmbTTHV_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cmbTTHV_GioiTinh.SelectedIndex = -1;
            txtTTHV_HovaTen.MaxLength = 100;
            txtTTHV_Email.MaxLength = 100;
            txtTTHV_SDT.MaxLength = 10;
            txtTTHV_DiaChi.MaxLength = 255;
            txtTTHV_NgaySinh.MaxLength = 10;
            txtTTHV_NgaySinh.Clear();
            foreach (var input in new[] { txtTTHV_MatKhauHienTai, txtTTHV_MKMoi, txtTTHV_XacNhanMK })
                input.UseSystemPasswordChar = true;
            materialLabel54.Text = "HV";
            lblKQHV_Diem.Text = "Chưa chọn bài thi";
            lblGVTT_HovaTen.Text = string.Empty;
            txtTTHV_MatKhauHienTai.Clear();
            txtTTHV_MKMoi.Clear();
            txtTTHV_XacNhanMK.Clear();
            btnTTHV_LuuThayDoi.Enabled = false;
            btnTTHV_DoiMK.Enabled = false;
        }

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
                HocSinh hs = _hocSinhDAL.LayTheoTaiKhoan(Session.TaiKhoanID);
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

            Session.HoTen = hs.HoTen;
            lblGVTT_HovaTen.Text = hs.HoTen ?? "";
            lblHV_LoiChao.Text = "Xin chào, " + hs.HoTen;
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
                if (!_hocSinhDAL.CapNhatHoSoCaNhan(Session.TaiKhoanID, hs))
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
                if (!_taiKhoanDAL.DoiMatKhauCaNhan(Session.TaiKhoanID, matKhauCu, matKhauMoi))
                {
                    MessageBox.Show("Mật khẩu hiện tại không đúng hoặc tài khoản đã thay đổi. Vui lòng kiểm tra lại.", "Không thể đổi mật khẩu", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                _taiKhoanDAL.XoaGhiNho(Session.TenDangNhap);
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

        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!_daTaiHocVien) return;
            if (tabHocVien.SelectedTab == pageHV_TrangChu)
            {
                LoadLopHoc();
                LoadDanhSachDeThi();
                return;
            }

            if (tabHocVien.SelectedTab == pageHV_BaiThi)
            {
                LoadDanhSachDeThi();
                return;
            }

            if (tabHocVien.SelectedTab == pageHV_KetQua)
            {
                LoadLichSuThi();
                return;
            }

            if (tabHocVien.SelectedTab == pageHV_LopHoc)
            {
                LoadTabLopHocCuaToi();
                return;
            }

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
