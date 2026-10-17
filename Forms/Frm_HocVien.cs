using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Forms;
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
using static ReaLTaiizor.Controls.HopeTabPage;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();

        private int _selectedLopDangKyID = 0;
        private int _selectedLopDangHocID = 0;

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
                LoadThongTinHocVien();
                LoadLopHoc();
                LoadLichSuThi();
                LoadTabLopHocCuaToi();

                // Gán sự kiện chọn dòng trên DataGridView lớp học
                dtHV_LopHoc.CellClick += dtHV_LopHoc_CellClick;
                dtHV_LopDangHoc.CellClick += dtHV_LopDangHoc_CellClick;

                // Gán sự kiện các nút đăng ký / hủy đăng ký
                foreverButton6.Click += btnDangKyLop_Click;
                if (btnHV_DangKy != null)
                {
                    btnHV_DangKy.Click += btnDangKyLop_Click;
                }
                btnHV_HuyDangKi.Click += btnHV_HuyDangKi_Click;

                // Gán sự kiện lưu hồ sơ cá nhân
                btnHV_LuuProfile.Click += btnHV_LuuProfile_Click;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin học viên: " + ex.Message, "Thông báo");
            }
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
                dgvHV_LopDangHoc.DataSource = dtLop;
            }
            catch { }
        }

        private void LoadTabLopHocCuaToi()
        {
            try
            {
                // Danh sách lớp mở cho đăng ký
                DataTable dtLopMo = _lopDAL.LayLopChuaDangKy(Session.NguoiDungID);
                dtHV_LopHoc.DataSource = dtLopMo;

                // Danh sách lớp đang học hoặc chờ duyệt
                DataTable dtLopDangHoc = _lopDAL.LayLopHocVienDangHocVaChoDuyet(Session.NguoiDungID);
                dtHV_LopDangHoc.DataSource = dtLopDangHoc;

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
                else if (dtHV_LopHoc.Columns.Contains("LopHocID") && row.Cells["LopHocID"].Value != null)
                {
                    _selectedLopDangKyID = Convert.ToInt32(row.Cells["LopHocID"].Value);
                }
                else if (row.Cells[0].Value != null && int.TryParse(row.Cells[0].Value.ToString(), out int id))
                {
                    _selectedLopDangKyID = id;
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
                else if (dtHV_LopDangHoc.Columns.Contains("LopHocID") && row.Cells["LopHocID"].Value != null)
                {
                    _selectedLopDangHocID = Convert.ToInt32(row.Cells["LopHocID"].Value);
                }
                else if (row.Cells[0].Value != null && int.TryParse(row.Cells[0].Value.ToString(), out int id))
                {
                    _selectedLopDangHocID = id;
                }
            }
        }

        private void btnDangKyLop_Click(object sender, EventArgs e)
        {
            if (_selectedLopDangKyID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học muốn đăng ký từ danh sách bên trên!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn đăng ký lớp học này?\nĐơn sẽ được gửi chờ giáo viên phê duyệt.",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.DangKyLop(_selectedLopDangKyID, Session.NguoiDungID, out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTabLopHocCuaToi();
                    LoadLopHoc();
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
                MessageBox.Show("Vui lòng chọn lớp học muốn hủy đăng ký từ danh sách bên dưới!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Bạn có chắc chắn muốn hủy đăng ký lớp học này?",
                "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.HuyDangKyLop(_selectedLopDangHocID, Session.NguoiDungID, out string thongBao))
                {
                    MessageBox.Show(thongBao, "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadTabLopHocCuaToi();
                    LoadLopHoc();
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
                MessageBox.Show("Họ tên không được để trống!", "Cảnh báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHV_Profile_HoTen.Focus();
                return;
            }

            if (!Validation.IsValidEmail(txtHV_Profile_Email.Text, out string errEmail))
            {
                MessageBox.Show(errEmail, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHV_Profile_Email.Focus();
                return;
            }

            if (!Validation.IsValidPhone(txtHV_Profile_SDT.Text, out string errPhone))
            {
                MessageBox.Show(errPhone, "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtHV_Profile_SDT.Focus();
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
                MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                Session.HoTen = hs.HoTen;
                if (lblHV_LoiChao != null)
                {
                    lblHV_LoiChao.Text = $"Xin chào, {hs.HoTen}!";
                }
            }
            else
            {
                MessageBox.Show("Cập nhật thông tin thất bại!", "Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadLichSuThi()
        {
            try
            {
                DataTable dtKQ = _ketQuaDAL.LayLichSuTheoHocSinh(Session.NguoiDungID);
                dgvHV_LichSuThi.DataSource = dtKQ;
            }
            catch { }
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
        }

        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabHocVien.SelectedTab == pageHV_LopHoc)
            {
                LoadTabLopHocCuaToi();
                return;
            }

            if (tabHocVien.SelectedTab != null &&
                tabHocVien.SelectedTab.Text.Trim().Contains("Đăng Xuất"))
            {
                DialogResult result = MessageBox.Show(
                    "Bạn có chắc chắn muốn đăng xuất?",
                    "Xác nhận",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

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
                    this.Close();
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
                if (f.Visible && f != this)
                    return;
            }
            Application.Exit();
        }
    }
}
