using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Forms;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();

        private int _selectedLopDangKyID = 0;
        private int _selectedLopDangHocID = 0;
        private int _selectedDeThiID = 0;

        // Lưu trữ danh sách đề thi gốc để hỗ trợ việc lọc bằng ComboBox
        private DataTable _dtDeThi;

        public HocVien()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            this.DrawerTabControl = tabHocVien;
        }

        private void HocVien_Load(object sender, EventArgs e)
        {
            try
            {
                tabHocVien.SelectedIndexChanged += TabHocVien_SelectedIndexChanged;

                ConfigureDataGrids();
                LoadThongTinHocVien();
                LoadLopHoc();
                LoadDanhSachDeThi();
                LoadLichSuThi();
                LoadTabLopHocCuaToi();

                // Sự kiện click chọn dòng trong các bảng
                dtHV_LopHoc.CellClick += dtHV_LopHoc_CellClick;
                dtHV_LopDangHoc.CellClick += dtHV_LopDangHoc_CellClick;
                if (dgvHV_DanhSachDeThi != null)
                {
                    dgvHV_DanhSachDeThi.CellClick += dgvHV_DanhSachDeThi_CellClick;
                }

                // Sự kiện nút đăng ký / hủy lớp
                if (foreverButton6 != null) foreverButton6.Click += btnDangKyLop_Click;
                if (btnHV_DangKy != null) btnHV_DangKy.Click += btnDangKyLop_Click;
                if (btnHV_HuyDangKi != null) btnHV_HuyDangKi.Click += btnHV_HuyDangKi_Click;

                // Sự kiện lưu hồ sơ cá nhân
                if (btnHV_LuuProfile != null) btnHV_LuuProfile.Click += btnHV_LuuProfile_Click;

                // Sự kiện lọc môn học và các nút Bài Thi
                if (cboHV_LocMonHoc != null) cboHV_LocMonHoc.SelectedIndexChanged += cboHV_LocMonHoc_SelectedIndexChanged;
                if (btnHV_LamMoiDeThi != null) btnHV_LamMoiDeThi.Click += btnHV_LamMoiDeThi_Click;
                if (btnHV_BatDauLamBai != null) btnHV_BatDauLamBai.Click += btnHV_BatDauLamBai_Click;

                // Xuất Excel Bài thi
                if (btnHV_XemLaiBaiThi != null) btnHV_XemLaiBaiThi.Click += btnHV_XemLaiBaiThi_Click;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin học viên: " + ex.Message, "Thông báo");
            }
        }

        private void ConfigureDataGrids()
        {
            // 1. Cấu hình bảng: dtHV_LopDangHoc (Fill dữ liệu chi tiết lớp học)
            if (dtHV_LopDangHoc != null)
            {
                dtHV_LopDangHoc.AutoGenerateColumns = false;
                dtHV_LopDangHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                if (dtHV_LopDangHoc.Columns.Contains("Column16")) dtHV_LopDangHoc.Columns["Column16"].DataPropertyName = "TenLop";
                if (dtHV_LopDangHoc.Columns.Contains("Column10")) dtHV_LopDangHoc.Columns["Column10"].DataPropertyName = "LichHoc";
                if (dtHV_LopDangHoc.Columns.Contains("Column17")) dtHV_LopDangHoc.Columns["Column17"].DataPropertyName = "TenKhoaHoc";
                if (dtHV_LopDangHoc.Columns.Contains("Column18")) dtHV_LopDangHoc.Columns["Column18"].DataPropertyName = "TenGiaoVien";

                if (dtHV_LopDangHoc.Columns.Contains("colNgayBatDau")) dtHV_LopDangHoc.Columns["colNgayBatDau"].DataPropertyName = "NgayBatDau";
                if (dtHV_LopDangHoc.Columns.Contains("colNgayKetThuc")) dtHV_LopDangHoc.Columns["colNgayKetThuc"].DataPropertyName = "NgayKetThuc";

                if (dtHV_LopDangHoc.Columns.Contains("Column21")) dtHV_LopDangHoc.Columns["Column21"].DataPropertyName = "TrangThaiDangKy";
            }

            // 2. Cấu hình bảng: dgvHV_DanhSachDeThi (Fill dữ liệu Đề Thi)
            // 2. Cấu hình bảng: dgvHV_DanhSachDeThi (Fill dữ liệu Đề Thi)
            if (dgvHV_DanhSachDeThi != null)
            {
                dgvHV_DanhSachDeThi.AutoGenerateColumns = false;
                dgvHV_DanhSachDeThi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

                // Quét qua các cột và gán dữ liệu dựa theo Tên Tiêu Đề Cột (HeaderText) chuẩn xác
                foreach (DataGridViewColumn col in dgvHV_DanhSachDeThi.Columns)
                {
                    if (col.HeaderText.Contains("Đề Thi")) col.DataPropertyName = "TenDeThi";
                    else if (col.HeaderText.Contains("Môn")) col.DataPropertyName = "MonHoc";
                    else if (col.HeaderText.Contains("Số Câu")) col.DataPropertyName = "SoCau";
                    else if (col.HeaderText.Contains("Thời Gian")) col.DataPropertyName = "ThoiGian";
                    else if (col.HeaderText.Contains("Hạn Nộp")) col.DataPropertyName = "HanNop";
                    else if (col.HeaderText.Contains("Trạng Thái")) col.DataPropertyName = "TrangThai";
                    else if (col.HeaderText.Contains("Điểm")) col.DataPropertyName = "Diem";
                }
            }

            // Bảng Tìm Lớp
            if (dtHV_LopHoc != null)
            {
                dtHV_LopHoc.AutoGenerateColumns = false;
                dtHV_LopHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (dtHV_LopHoc.Columns.Contains("dataGridViewTextBoxColumn1")) dtHV_LopHoc.Columns["dataGridViewTextBoxColumn1"].DataPropertyName = "TenLop";
                if (dtHV_LopHoc.Columns.Contains("dataGridViewTextBoxColumn2")) dtHV_LopHoc.Columns["dataGridViewTextBoxColumn2"].DataPropertyName = "TenKhoaHoc";
                if (dtHV_LopHoc.Columns.Contains("dataGridViewTextBoxColumn3")) dtHV_LopHoc.Columns["dataGridViewTextBoxColumn3"].DataPropertyName = "TenGiaoVien";
                if (dtHV_LopHoc.Columns.Contains("dataGridViewTextBoxColumn4")) dtHV_LopHoc.Columns["dataGridViewTextBoxColumn4"].DataPropertyName = "SiSo";
                if (dtHV_LopHoc.Columns.Contains("dataGridViewTextBoxColumn5")) dtHV_LopHoc.Columns["dataGridViewTextBoxColumn5"].DataPropertyName = "TrangThai";
            }

            // Bảng Lịch Sử Thi
            if (dgvHV_LichSuThi != null)
            {
                dgvHV_LichSuThi.AutoGenerateColumns = false;
                dgvHV_LichSuThi.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                if (dgvHV_LichSuThi.Columns.Contains("Column22")) dgvHV_LichSuThi.Columns["Column22"].DataPropertyName = "TenDeThi";
                if (dgvHV_LichSuThi.Columns.Contains("Column23")) dgvHV_LichSuThi.Columns["Column23"].DataPropertyName = "MonHoc";
                if (dgvHV_LichSuThi.Columns.Contains("Column24")) dgvHV_LichSuThi.Columns["Column24"].DataPropertyName = "ThoiGianNop";
                if (dgvHV_LichSuThi.Columns.Contains("Column25")) dgvHV_LichSuThi.Columns["Column25"].DataPropertyName = "Diem";
            }

            // Bảng Lớp Đang Học ở Trang chủ
            if (dgvHV_LopDangHoc != null)
            {
                dgvHV_LopDangHoc.AutoGenerateColumns = false;
                dgvHV_LopDangHoc.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                Lop.DataPropertyName = "TenLop";
                GiaoVien.DataPropertyName = "TenGiaoVien";
                LichHoc.DataPropertyName = "LichHoc";
            }
        }

        private void LoadDanhSachDeThi()
        {
            try
            {
                _dtDeThi = _deThiDAL.LayDeThiChoHocSinh(Session.NguoiDungID);
                dgvHV_DanhSachDeThi.DataSource = _dtDeThi;

                if (cboHV_LocMonHoc != null)
                {
                    cboHV_LocMonHoc.Items.Clear();
                    cboHV_LocMonHoc.Items.Add("-- Tất cả môn học --");

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
                            cboHV_LocMonHoc.Items.Add(mon);
                        }
                    }
                    cboHV_LocMonHoc.SelectedIndex = 0;
                }
            }
            catch { }
        }

        private void cboHV_LocMonHoc_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_dtDeThi == null) return;

            string selectedMonHoc = cboHV_LocMonHoc.SelectedItem?.ToString();

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

        private void btnHV_LamMoiDeThi_Click(object sender, EventArgs e)
        {
            LoadDanhSachDeThi();
        }

        private void dgvHV_DanhSachDeThi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvHV_DanhSachDeThi.Rows.Count)
            {
                var row = dgvHV_DanhSachDeThi.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("DeThiID"))
                {
                    _selectedDeThiID = Convert.ToInt32(drv["DeThiID"]);
                }
                else if (dgvHV_DanhSachDeThi.Columns.Contains("DeThiID") && row.Cells["DeThiID"].Value != null)
                {
                    _selectedDeThiID = Convert.ToInt32(row.Cells["DeThiID"].Value);
                }
            }
        }

        private void btnHV_BatDauLamBai_Click(object sender, EventArgs e)
        {
            if (_selectedDeThiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn 1 đề thi từ danh sách bên trên trước khi bắt đầu!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra trực tiếp Trạng Thái của dòng đang chọn
            if (dgvHV_DanhSachDeThi.CurrentRow != null && dgvHV_DanhSachDeThi.CurrentRow.DataBoundItem is DataRowView drv)
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
            if (_ketQuaDAL.KiemTraDaThi(Session.NguoiDungID, deThiID))
            {
                MessageBox.Show("Bạn đã hoàn thành bài thi này rồi, không thể thi lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Frm_KiemTra frm = new Frm_KiemTra(deThiID);
            frm.ShowDialog();

            LoadLichSuThi();
            LoadDanhSachDeThi();
        }

        private void LoadThongTinHocVien()
        {
            try
            {
                if (Session.NguoiDungID > 0)
                {
                    HocSinh hs = new HocSinhDAL().LayTheoID(Session.NguoiDungID);
                    if (hs != null)
                    {
                        txtHV_Profile_HoTen.Text = hs.HoTen;
                        txtHV_Profile_Email.Text = hs.Email;
                        txtHV_Profile_SDT.Text = hs.SoDienThoai;
                        txtHV_Profile_DiaChi.Text = hs.DiaChi;
                        if (hs.NgaySinh.HasValue)
                            txtHV_Profile_NgaySinh.Text = hs.NgaySinh.Value.ToString("dd/MM/yyyy");
                        cboHV_Profile_GioiTinh.SelectedItem = hs.GioiTinh;
                    }
                }
            }
            catch { }
        }

        private void LoadLopHoc()
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoHocSinh(Session.NguoiDungID);
                if (dgvHV_LopDangHoc != null) dgvHV_LopDangHoc.DataSource = dtLop;
            }
            catch { }
        }

        private void LoadTabLopHocCuaToi()
        {
            try
            {
                DataTable dtLopMo = _lopDAL.LayLopChuaDangKy(Session.NguoiDungID);
                if (dtHV_LopHoc != null) dtHV_LopHoc.DataSource = dtLopMo;

                DataTable dtLopDangHoc = _lopDAL.LayLopHocVienDangHocVaChoDuyet(Session.NguoiDungID);
                if (dtHV_LopDangHoc != null) dtHV_LopDangHoc.DataSource = dtLopDangHoc;

                _selectedLopDangKyID = 0;
                _selectedLopDangHocID = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải danh sách lớp học: " + ex.Message, "Thông báo");
            }
        }

        private void dtHV_LopHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dtHV_LopHoc.Rows.Count)
            {
                var row = dtHV_LopHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    _selectedLopDangKyID = Convert.ToInt32(drv["LopHocID"]);
                }
            }
        }

        private void dtHV_LopDangHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dtHV_LopDangHoc.Rows.Count)
            {
                var row = dtHV_LopDangHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("LopHocID"))
                {
                    _selectedLopDangHocID = Convert.ToInt32(drv["LopHocID"]);
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

        private void btnHV_HuyDangKi_Click(object sender, EventArgs e)
        {
            if (_selectedLopDangHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học muốn hủy đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // --- LOGIC MỚI THÊM: Chặn hủy đăng ký nếu lớp "Đã duyệt" ---
            if (dtHV_LopDangHoc.CurrentRow != null && dtHV_LopDangHoc.CurrentRow.DataBoundItem is DataRowView drv)
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

        private void btnHV_LuuProfile_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtHV_Profile_HoTen.Text))
            {
                MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime? ngaySinh = null;
            if (!string.IsNullOrWhiteSpace(txtHV_Profile_NgaySinh.Text) &&
                DateTime.TryParse(txtHV_Profile_NgaySinh.Text.Trim(), out DateTime dt))
            {
                ngaySinh = dt;
            }

            HocSinh hs = new HocSinh
            {
                HocSinhID = Session.NguoiDungID,
                HoTen = txtHV_Profile_HoTen.Text.Trim(),
                Email = txtHV_Profile_Email.Text.Trim(),
                SoDienThoai = txtHV_Profile_SDT.Text.Trim(),
                DiaChi = txtHV_Profile_DiaChi.Text.Trim(),
                GioiTinh = cboHV_Profile_GioiTinh.SelectedItem?.ToString(),
                NgaySinh = ngaySinh
            };

            if (new HocSinhDAL().Sua(hs))
            {
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                Session.HoTen = hs.HoTen;
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLichSuThi()
        {
            try
            {
                DataTable dtKQ = _ketQuaDAL.LayLichSuTheoHocSinh(Session.NguoiDungID);
                if (dgvHV_LichSuThi != null) dgvHV_LichSuThi.DataSource = dtKQ;
            }
            catch { }
        }

        private void btnHV_XemLaiBaiThi_Click(object sender, EventArgs e)
        {
            if (dgvHV_LichSuThi.CurrentRow == null)
            {
                MessageBox.Show("Vui lòng chọn bài thi đã làm trong bảng lịch sử!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var row = dgvHV_LichSuThi.CurrentRow;
                int ketQuaID = 0;

                if (row.DataBoundItem is DataRowView drv && drv.Row.Table.Columns.Contains("KetQuaID"))
                {
                    ketQuaID = Convert.ToInt32(drv["KetQuaID"]);
                }
                else if (dgvHV_LichSuThi.Columns.Contains("KetQuaID") && row.Cells["KetQuaID"].Value != null)
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

                XuatChiTietBaiLamRaExcel(dtChiTiet);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi xuất file Excel: " + ex.Message, "Lỗi");
            }
        }

        private void XuatChiTietBaiLamRaExcel(DataTable dt)
        {
            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = $"ChiTietBaiLam_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("\uFEFF"); // Ghi UTF-8 BOM để hiển thị tiếng Việt trên Excel
                sb.AppendLine("STT;Câu Hỏi;Đáp Án A;Đáp Án B;Đáp Án C;Đáp Án D;Đáp Án Đúng;Đáp Án Đã Chọn;Kết Quả");

                foreach (DataRow r in dt.Rows)
                {
                    string stt = r["ThuTu"]?.ToString();
                    string cauHoi = $"\"{r["CauHoi"]?.ToString().Replace("\"", "\"\"")}\"";
                    string a = $"\"{r["DapAnA"]?.ToString().Replace("\"", "\"\"")}\"";
                    string b = $"\"{r["DapAnB"]?.ToString().Replace("\"", "\"\"")}\"";
                    string c = $"\"{r["DapAnC"]?.ToString().Replace("\"", "\"\"")}\"";
                    string d = $"\"{r["DapAnD"]?.ToString().Replace("\"", "\"\"")}\"";
                    string dung = r["DapAnDung"]?.ToString();
                    string chon = r["DapAnChon"]?.ToString();
                    bool laDung = Convert.ToBoolean(r["LaDung"]);
                    string kqText = laDung ? "ĐÚNG" : "SAI";

                    sb.AppendLine($"{stt};{cauHoi};{a};{b};{c};{d};{dung};{chon};{kqText}");
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Xuất file kết quả bài thi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
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
    }
}