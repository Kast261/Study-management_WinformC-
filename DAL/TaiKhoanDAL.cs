using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class TaiKhoanDAL
    {
        private readonly string _rememberFilePath = Path.Combine(Application.StartupPath, "remember.dat");

        public TaiKhoan DangNhap(string tenDangNhap, string matKhauNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap) || string.IsNullOrEmpty(matKhauNhap))
                return null;

            string sql = "SELECT * FROM TaiKhoan WHERE TenDangNhap = @user";
            SqlParameter[] pars = { new SqlParameter("@user", tenDangNhap.Trim()) };

            DataTable dt = DatabaseHelper.ExecuteQuery(sql, pars);
            if (dt.Rows.Count == 0) return null;

            DataRow row = dt.Rows[0];
            string hashInDb = row["MatKhau"] != DBNull.Value ? row["MatKhau"].ToString().Trim() : "";

            
            bool isPasswordCorrect = false;
            if (hashInDb == matKhauNhap || Passwords.Verify(matKhauNhap, hashInDb))
            {
                isPasswordCorrect = true;
            }

            if (!isPasswordCorrect)
                return null;

            int taiKhoanID = Convert.ToInt32(row["TaiKhoanID"]);
            string vaiTro = row["VaiTro"] != DBNull.Value ? row["VaiTro"].ToString().Trim() : "";

          
            bool trangThaiKhoa = false;
            if (dt.Columns.Contains("TrangThaiKhoa") && row["TrangThaiKhoa"] != DBNull.Value)
            {
                trangThaiKhoa = Convert.ToBoolean(row["TrangThaiKhoa"]);
            }
            else if (dt.Columns.Contains("TrangThai") && row["TrangThai"] != DBNull.Value)
            {
                trangThaiKhoa = !Convert.ToBoolean(row["TrangThai"]);
            }

           
            int nguoiDungID = 0;
            if (dt.Columns.Contains("NguoiDungID") && row["NguoiDungID"] != DBNull.Value)
            {
                nguoiDungID = Convert.ToInt32(row["NguoiDungID"]);
            }
            else
            {
                
                try
                {
                    if (string.Equals(vaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase))
                    {
                        string sqlGV = "SELECT GiaoVienID FROM GiaoVien WHERE TaiKhoanID = @id";
                        object gvObj = DatabaseHelper.ExecuteScalar(sqlGV, new SqlParameter[] { new SqlParameter("@id", taiKhoanID) });
                        if (gvObj != null && gvObj != DBNull.Value) nguoiDungID = Convert.ToInt32(gvObj);
                    }
                    else if (string.Equals(vaiTro, "HocSinh", StringComparison.OrdinalIgnoreCase))
                    {
                        string sqlHS = "SELECT HocSinhID FROM HocSinh WHERE TaiKhoanID = @id";
                        object hsObj = DatabaseHelper.ExecuteScalar(sqlHS, new SqlParameter[] { new SqlParameter("@id", taiKhoanID) });
                        if (hsObj != null && hsObj != DBNull.Value) nguoiDungID = Convert.ToInt32(hsObj);
                    }
                }
                catch
                {
                    
                }
            }

            return new TaiKhoan
            {
                TaiKhoanID = taiKhoanID,
                TenDangNhap = row["TenDangNhap"] != DBNull.Value ? row["TenDangNhap"].ToString().Trim() : "",
                MatKhau = hashInDb,
                VaiTro = vaiTro,
                NguoiDungID = nguoiDungID,
                TrangThaiKhoa = trangThaiKhoa
            };
        }

        public void LuuGhiNho(string tenDangNhap)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap))
                {
                    if (File.Exists(_rememberFilePath)) File.Delete(_rememberFilePath);
                }
                else
                {
                    File.WriteAllText(_rememberFilePath, tenDangNhap.Trim());
                }
            }
            catch { /* Bỏ qua lỗi ghi file nếu không có quyền */ }
        }

        public string DocGhiNho()
        {
            try
            {
                if (File.Exists(_rememberFilePath))
                    return File.ReadAllText(_rememberFilePath).Trim();
            }
            catch { }
            return string.Empty;
        }

        public List<TaiKhoan> GetAll()
        {
            var list = new List<TaiKhoan>();
            string sql = "SELECT * FROM TaiKhoan";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql);
            foreach (DataRow row in dt.Rows)
            {
                bool trangThaiKhoa = false;
                if (dt.Columns.Contains("TrangThaiKhoa") && row["TrangThaiKhoa"] != DBNull.Value)
                    trangThaiKhoa = Convert.ToBoolean(row["TrangThaiKhoa"]);
                else if (dt.Columns.Contains("TrangThai") && row["TrangThai"] != DBNull.Value)
                    trangThaiKhoa = !Convert.ToBoolean(row["TrangThai"]);

                int nguoiDungID = 0;
                if (dt.Columns.Contains("NguoiDungID") && row["NguoiDungID"] != DBNull.Value)
                    nguoiDungID = Convert.ToInt32(row["NguoiDungID"]);

                list.Add(new TaiKhoan
                {
                    TaiKhoanID = Convert.ToInt32(row["TaiKhoanID"]),
                    TenDangNhap = row["TenDangNhap"].ToString(),
                    VaiTro = row["VaiTro"] != DBNull.Value ? row["VaiTro"].ToString().Trim() : "",
                    NguoiDungID = nguoiDungID,
                    TrangThaiKhoa = trangThaiKhoa
                });
            }
            return list;
        }

        public bool DoiMatKhau(int taiKhoanID, string matKhauMoiPlain)
        {
            string hashMoi = Passwords.Hash(matKhauMoiPlain);
            string sql = "UPDATE TaiKhoan SET MatKhau = @mk WHERE TaiKhoanID = @id";
            SqlParameter[] pars = {
                new SqlParameter("@mk", hashMoi),
                new SqlParameter("@id", taiKhoanID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool KhoaHoacMoKhoaTaiKhoan(int taiKhoanID, bool khoa)
        {
            // Cập nhật cả TrangThaiKhoa hoặc TrangThai tuỳ schema
            string sql = @"
                IF COL_LENGTH('TaiKhoan', 'TrangThaiKhoa') IS NOT NULL
                    UPDATE TaiKhoan SET TrangThaiKhoa = @khoa WHERE TaiKhoanID = @id
                ELSE IF COL_LENGTH('TaiKhoan', 'TrangThai') IS NOT NULL
                    UPDATE TaiKhoan SET TrangThai = @active WHERE TaiKhoanID = @id";

            SqlParameter[] pars = {
                new SqlParameter("@khoa", khoa),
                new SqlParameter("@active", !khoa),
                new SqlParameter("@id", taiKhoanID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool XoaTaiKhoan(int taiKhoanID)
        {
            string sql = "DELETE FROM TaiKhoan WHERE TaiKhoanID = @id";
            SqlParameter[] pars = { new SqlParameter("@id", taiKhoanID) };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public DataTable LayTatCaCoTenNguoiDung()
        {
            string sql = @"
        SELECT 
            tk.TaiKhoanID,
            tk.TenDangNhap,
            tk.VaiTro,
            tk.NguoiDungID,
            ISNULL(tk.TrangThaiKhoa, 0) AS TrangThaiKhoa,
            CASE 
                WHEN tk.VaiTro = 'GiaoVien' THEN ISNULL(gv.HoTen, N'(Chưa liên kết)')
                WHEN tk.VaiTro = 'HocSinh'  THEN ISNULL(hs.HoTen, N'(Chưa liên kết)')
                WHEN tk.VaiTro = 'Admin'    THEN N'Quản trị viên hệ thống'
                ELSE N'(Chưa xác định)'
            END AS TenNguoiDung,
            CASE 
                WHEN ISNULL(tk.TrangThaiKhoa, 0) = 1 THEN N'Đã khóa'
                ELSE N'Hoạt động'
            END AS TrangThaiHienThi
        FROM TaiKhoan tk
        LEFT JOIN GiaoVien gv ON tk.VaiTro = 'GiaoVien' AND tk.NguoiDungID = gv.GiaoVienID
        LEFT JOIN HocSinh hs  ON tk.VaiTro = 'HocSinh'  AND tk.NguoiDungID = hs.HocSinhID
        ORDER BY tk.TaiKhoanID DESC";

            return DatabaseHelper.ExecuteQuery(sql);
        }
        public bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)
        {
            // Kiểm tra tên đăng nhập đã tồn tại chưa
            string checkSql = "SELECT COUNT(*) FROM TaiKhoan WHERE LOWER(TenDangNhap) = LOWER(@user)";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new SqlParameter[] {
        new SqlParameter("@user", tenDangNhap.Trim())
    }));
            if (count > 0) return false;

            string hashPass = Passwords.Hash(matKhauPlain.Trim());
            string sql = @"
        INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
        VALUES (@user, @mk, @vaiTro, @nguoiDungID, 0);
        SELECT SCOPE_IDENTITY();";

            SqlParameter[] pars = {
        new SqlParameter("@user", tenDangNhap.Trim()),
        new SqlParameter("@mk", hashPass),
        new SqlParameter("@vaiTro", vaiTro),
        new SqlParameter("@nguoiDungID", nguoiDungID)
    };

            object result = DatabaseHelper.ExecuteScalar(sql, pars);
            if (result == null || result == DBNull.Value) return false;

            int newTaiKhoanID = Convert.ToInt32(result);

            // Cập nhật TaiKhoanID ngược lại bảng thực thể
            try
            {
                if (vaiTro == "GiaoVien" && nguoiDungID > 0)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE GiaoVien SET TaiKhoanID = @tkID WHERE GiaoVienID = @gvID",
                        new SqlParameter[] {
                    new SqlParameter("@tkID", newTaiKhoanID),
                    new SqlParameter("@gvID", nguoiDungID)
                        });
                }
                else if (vaiTro == "HocSinh" && nguoiDungID > 0)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE HocSinh SET TaiKhoanID = @tkID WHERE HocSinhID = @hsID",
                        new SqlParameter[] {
                    new SqlParameter("@tkID", newTaiKhoanID),
                    new SqlParameter("@hsID", nguoiDungID)
                        });
                }
            }
            catch { /* Nếu bảng chưa có cột TaiKhoanID thì bỏ qua lỗi */ }

            return true;
        }
    }
}
