using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Npgsql;
namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_DangNhap : ReaLTaiizor.Forms.MaterialForm
    {
        #region Dữ liệu & Trạng thái

        private readonly TaiKhoanDAL _taiKhoanDAL = new TaiKhoanDAL();

        #endregion

        #region 1. Khởi tạo & Sự kiện form

        public frm_DangNhap()
        {
            InitializeComponent();
        }

        private bool IsDesignTime => DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

        private void Frm_DangNhap_Load(object sender, EventArgs e)
        {
            if (IsDesignTime) return;

            if (!DatabaseHelper.KiemTraKetNoi(out string loi))
            {
               
                MessageBox.Show(
                    loi,
                    "Lỗi kết nối PostgreSQL");
            }
            TaiThongTinGhiNho();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (this.Visible && !IsDesignTime)
            {
                TaiThongTinGhiNho();
            }
        }

        #endregion

        #region 2. Xác Thực & Đăng Nhập

        private void Btn_DangNhap_Click(object sender, EventArgs e)
        {
            string username = txt_TaiKhoan.Text.Trim();
            string password = txt_MatKhau.Text;

            // 1. Validate Input
            if (string.IsNullOrEmpty(username))
            {
                MaterialSnackBar snack = new MaterialSnackBar("Vui lòng nhập tên đăng nhập!", "Đóng", true);
                snack.Show(this);
                txt_TaiKhoan.Focus();
                return;
            }

            if (string.IsNullOrEmpty(password))
            {
                MaterialSnackBar snack = new MaterialSnackBar("Vui lòng nhập mật khẩu!", "Đóng", true);
                snack.Show(this);
                txt_MatKhau.Focus();
                return;
            }

            try
            {
                // 2. Xác thực qua TaiKhoanDAL
                TaiKhoan tk = _taiKhoanDAL.DangNhap(username, password);

                if (tk == null)
                {
                    MessageBox.Show("Sai tài khoản hoặc mật khẩu!", "Đăng nhập thất bại", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (tk.TrangThaiKhoa)
                {
                    MessageBox.Show("Tài khoản của bạn đã bị khóa. Vui lòng liên hệ Quản trị viên!", "Tài khoản bị khóa", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }

                if (!string.Equals(tk.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(tk.VaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(tk.VaiTro, "HocSinh", StringComparison.OrdinalIgnoreCase)
                    && !string.Equals(tk.VaiTro, "HocVien", StringComparison.OrdinalIgnoreCase))
                {
                    MessageBox.Show("Vai trò tài khoản không hợp lệ. Vui lòng liên hệ quản trị viên.", "Không thể đăng nhập");
                    return;
                }

                if (!string.Equals(tk.VaiTro, "Admin", StringComparison.OrdinalIgnoreCase) && tk.NguoiDungID <= 0)
                {
                    MessageBox.Show("Tài khoản chưa liên kết với một hồ sơ duy nhất. Vui lòng liên hệ quản trị viên.", "Không thể đăng nhập");
                    return;
                }

                // 3. TASK-05: Xử lý lưu/xóa "Ghi Nhớ Đăng Nhập"
                if (check_GhiNho.Checked)
                    _taiKhoanDAL.LuuGhiNho(username, password);
                else
                    _taiKhoanDAL.XoaGhiNho(username);

                // 4. Lưu vào Session
                Session.TaiKhoanID = tk.TaiKhoanID;
                Session.TenDangNhap = tk.TenDangNhap;
                Session.VaiTro = tk.VaiTro;
                Session.NguoiDungID = tk.NguoiDungID;
                Session.HoTen = tk.TenDangNhap;

                try
                {
                    if (Session.IsGiaoVien && Session.NguoiDungID > 0)
                    {
                        object gvName = DatabaseHelper.ExecuteScalar("SELECT HoTen FROM GiaoVien WHERE GiaoVienID = @id", new NpgsqlParameter("@id", Session.NguoiDungID));
                        if (gvName != null && gvName != DBNull.Value) Session.HoTen = gvName.ToString();
                    }
                    else if (Session.IsHocSinh && Session.NguoiDungID > 0)
                    {
                        object hsName = DatabaseHelper.ExecuteScalar("SELECT HoTen FROM HocSinh WHERE HocSinhID = @id", new NpgsqlParameter("@id", Session.NguoiDungID));
                        if (hsName != null && hsName != DBNull.Value) Session.HoTen = hsName.ToString();
                    }
                }
                catch { }

                // 5. Điều hướng theo VaiTro
                Form nextForm = null;
                if (Session.IsAdmin)
                    nextForm = new Frm_Admin();
                else if (Session.IsGiaoVien)
                    nextForm = new frm_GiaoVien();
                else
                    nextForm = new HocVien();

                nextForm.FormClosed += (s, args) =>
                {
                    // Nếu màn hình đăng nhập đang hiển thị (do người dùng vừa đăng xuất), không thoát app!
                    if (this.Visible)
                    {
                        return;
                    }

                    bool hasVisible = false;
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f.Visible)
                        {
                            hasVisible = true;
                            break;
                        }
                    }
                    if (!hasVisible)
                    {
                        Application.Exit();
                    }
                };

                this.Hide();
                if (!check_GhiNho.Checked)
                {
                    txt_MatKhau.Clear();
                }
                nextForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng nhập: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region 3. Ghi Nhớ Tài Khoản & Mật Khẩu

        private void TaiThongTinGhiNho()
        {
            var (tenDangNhap, matKhau) = _taiKhoanDAL.DocGhiNho();
            if (!string.IsNullOrEmpty(tenDangNhap))
            {
                txt_TaiKhoan.Text = tenDangNhap;
                check_GhiNho.Checked = true;
                if (!string.IsNullOrEmpty(matKhau))
                {
                    txt_MatKhau.Text = matKhau;
                    txt_MatKhau.UseSystemPasswordChar = true;
                }
                else
                {
                    txt_MatKhau.Focus();
                }
            }
        }

        private void Check_GhiNho_CheckedChanged(object sender, EventArgs e)
        {
            if (check_GhiNho.Checked)
            {
                string currentUsername = txt_TaiKhoan.Text.Trim();
                string savedPass = _taiKhoanDAL.LayMatKhauGhiNho(currentUsername);

                if (!string.IsNullOrEmpty(savedPass))
                {
                    txt_MatKhau.Text = savedPass;
                    txt_MatKhau.UseSystemPasswordChar = true;
                }
                else
                {
                    var (tenDangNhap, matKhau) = _taiKhoanDAL.DocGhiNho();
                    if (string.IsNullOrEmpty(currentUsername) && !string.IsNullOrEmpty(matKhau))
                    {
                        if (string.IsNullOrEmpty(currentUsername))
                        {
                            txt_TaiKhoan.Text = tenDangNhap;
                        }
                        txt_MatKhau.Text = matKhau;
                        txt_MatKhau.UseSystemPasswordChar = true;
                    }
                }
            }
            else
            {
                txt_MatKhau.Clear();
            }
        }

        private void Txt_TaiKhoan_TextChanged(object sender, EventArgs e)
        {
            if (check_GhiNho.Checked)
            {
                string u = txt_TaiKhoan.Text.Trim();
                string pass = _taiKhoanDAL.LayMatKhauGhiNho(u);
                txt_MatKhau.Text = pass ?? string.Empty;
                txt_MatKhau.UseSystemPasswordChar = true;
            }
        }

        #endregion

        #region 4. Hiển Thị Mật Khẩu & Giao Diện

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            txt_MatKhau.UseSystemPasswordChar = !checkBox1.Checked;
        }

        private void Txt_MatKhau_TextChanged(object sender, EventArgs e)
        {
            txt_MatKhau.UseSystemPasswordChar = true;
            checkBox1.Checked = false;
        }

        private void MaterialCard1_Paint(object sender, PaintEventArgs e)
        {
        }

        #endregion

    }
}
