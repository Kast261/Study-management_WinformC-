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
using System.Data.SqlClient;
namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_DangNhap : ReaLTaiizor.Forms.MaterialForm
    {
        
        private readonly TaiKhoanDAL _taiKhoanDAL = new TaiKhoanDAL();
        public frm_DangNhap()
        {
            InitializeComponent();
            this.AcceptButton = btn_DangNhap;
        }

        private void frm_DangNhap_Load(object sender, EventArgs e)
        {
            if (!DatabaseHelper.KiemTraKetNoi())
            {
                MessageBox.Show(
                    "Không thể kết nối đến cơ sở dữ liệu SQL Server!\nVui lòng kiểm tra lại cấu hình trong DatabaseHelper.",
                    "Lỗi Kết Nối",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                btn_DangNhap.Enabled = false;
                return;
            }
            string savedUser = _taiKhoanDAL.DocGhiNho();
            if (!string.IsNullOrEmpty(savedUser))
            {
                txt_TaiKhoan.Text = savedUser;
                check_GhiNho.Checked = true;
                txt_MatKhau.Focus();
            }
        }

        private void btn_DangNhap_Click(object sender, EventArgs e)
        {
            string username = txt_TaiKhoan.Text.Trim();
            string password = txt_MatKhau.Text.Trim();

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

                // 3. TASK-05: Xử lý lưu/xóa "Ghi Nhớ Đăng Nhập"
                if (check_GhiNho.Checked)
                    _taiKhoanDAL.LuuGhiNho(username);
                else
                    _taiKhoanDAL.LuuGhiNho(string.Empty);

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
                        object gvName = DatabaseHelper.ExecuteScalar("SELECT HoTen FROM GiaoVien WHERE GiaoVienID = @id", new SqlParameter("@id", Session.NguoiDungID));
                        if (gvName != null && gvName != DBNull.Value) Session.HoTen = gvName.ToString();
                    }
                    else if (Session.IsHocSinh && Session.NguoiDungID > 0)
                    {
                        object hsName = DatabaseHelper.ExecuteScalar("SELECT HoTen FROM HocSinh WHERE HocSinhID = @id", new SqlParameter("@id", Session.NguoiDungID));
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
                    bool hasVisible = false;
                    foreach (Form f in Application.OpenForms)
                    {
                        if (f.Visible && f != this)
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
                txt_MatKhau.Clear();
                nextForm.Show();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi đăng nhập: " + ex.Message, "Lỗi hệ thống", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void check_GhiNho_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void materialCard1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            txt_MatKhau.UseSystemPasswordChar = !txt_MatKhau.UseSystemPasswordChar;
            txt_MatKhau.PasswordChar = txt_MatKhau.PasswordChar;
        }
    }
}
