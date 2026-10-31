using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
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

            string sql = @"SELECT TaiKhoanID, TenDangNhap, MatKhau, VaiTro,
                                  NguoiDungID, TrangThaiKhoa
                           FROM TaiKhoan WHERE TenDangNhap = @user";
            NpgsqlParameter[] pars = { new NpgsqlParameter("@user", tenDangNhap.Trim()) };

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
                        object gvObj = DatabaseHelper.ExecuteScalar(sqlGV, new NpgsqlParameter[] { new NpgsqlParameter("@id", taiKhoanID) });
                        if (gvObj != null && gvObj != DBNull.Value) nguoiDungID = Convert.ToInt32(gvObj);
                    }
                    else if (string.Equals(vaiTro, "HocSinh", StringComparison.OrdinalIgnoreCase))
                    {
                        string sqlHS = "SELECT HocSinhID FROM HocSinh WHERE TaiKhoanID = @id";
                        object hsObj = DatabaseHelper.ExecuteScalar(sqlHS, new NpgsqlParameter[] { new NpgsqlParameter("@id", taiKhoanID) });
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

        public void LuuGhiNho(string tenDangNhap, string matKhau = "")
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap))
                {
                    if (File.Exists(_rememberFilePath)) File.Delete(_rememberFilePath);
                }
                else
                {
                    var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    if (File.Exists(_rememberFilePath))
                    {
                        var lines = File.ReadAllLines(_rememberFilePath);
                        foreach (var line in lines)
                        {
                            if (string.IsNullOrWhiteSpace(line)) continue;
                            var parts = line.Split(new char[] { '\t' }, 2);
                            if (parts.Length == 2)
                            {
                                dict[parts[0].Trim()] = GiaiMaMatKhau(parts[1].Trim());
                            }
                            else if (parts.Length == 1 && !dict.ContainsKey(parts[0].Trim()))
                            {
                                dict[parts[0].Trim()] = "";
                            }
                        }
                    }

                    dict[tenDangNhap.Trim()] = matKhau ?? "";

                    var outLines = new List<string>();
                    outLines.Add($"{tenDangNhap.Trim()}\t{MaHoaMatKhau(matKhau ?? "")}");
                    foreach (var kvp in dict)
                    {
                        if (!string.Equals(kvp.Key, tenDangNhap.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            outLines.Add($"{kvp.Key}\t{MaHoaMatKhau(kvp.Value)}");
                        }
                    }

                    File.WriteAllLines(_rememberFilePath, outLines);
                }
            }
            catch { /* Bỏ qua lỗi ghi file nếu không có quyền */ }
        }

        public (string TenDangNhap, string MatKhau) DocGhiNho()
        {
            try
            {
                if (File.Exists(_rememberFilePath))
                {
                    var lines = File.ReadAllLines(_rememberFilePath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(new char[] { '\t' }, 2);
                        if (parts.Length == 2)
                        {
                            return (parts[0].Trim(), GiaiMaMatKhau(parts[1].Trim()));
                        }
                        else if (parts.Length == 1)
                        {
                            return (parts[0].Trim(), "");
                        }
                    }
                }
            }
            catch { }
            return (string.Empty, string.Empty);
        }

        public string LayMatKhauGhiNho(string tenDangNhap)
        {
            if (string.IsNullOrWhiteSpace(tenDangNhap)) return string.Empty;
            try
            {
                if (File.Exists(_rememberFilePath))
                {
                    var lines = File.ReadAllLines(_rememberFilePath);
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(new char[] { '\t' }, 2);
                        if (parts.Length >= 1 && string.Equals(parts[0].Trim(), tenDangNhap.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            return parts.Length == 2 ? GiaiMaMatKhau(parts[1].Trim()) : "";
                        }
                    }
                }
            }
            catch { }
            return string.Empty;
        }

        public void XoaGhiNho(string tenDangNhap = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenDangNhap))
                {
                    if (File.Exists(_rememberFilePath)) File.Delete(_rememberFilePath);
                }
                else if (File.Exists(_rememberFilePath))
                {
                    var lines = File.ReadAllLines(_rememberFilePath);
                    var remaining = new List<string>();
                    foreach (var line in lines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(new char[] { '\t' }, 2);
                        if (!string.Equals(parts[0].Trim(), tenDangNhap.Trim(), StringComparison.OrdinalIgnoreCase))
                        {
                            remaining.Add(line);
                        }
                    }
                    if (remaining.Count == 0)
                        File.Delete(_rememberFilePath);
                    else
                        File.WriteAllLines(_rememberFilePath, remaining);
                }
            }
            catch { }
        }

        private static string MaHoaMatKhau(string plain)
        {
            if (string.IsNullOrEmpty(plain)) return "";
            try
            {
                byte[] bytes = Encoding.UTF8.GetBytes(plain);
                return Convert.ToBase64String(bytes);
            }
            catch
            {
                return plain;
            }
        }

        private static string GiaiMaMatKhau(string cipher)
        {
            if (string.IsNullOrEmpty(cipher)) return "";
            try
            {
                byte[] bytes = Convert.FromBase64String(cipher);
                return Encoding.UTF8.GetString(bytes);
            }
            catch
            {
                return cipher;
            }
        }

        public List<TaiKhoan> GetAll()
        {
            var list = new List<TaiKhoan>();
            string sql = @"SELECT TaiKhoanID, TenDangNhap, MatKhau, VaiTro,
                                  NguoiDungID, TrangThaiKhoa
                           FROM TaiKhoan";
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
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@mk", hashMoi),
                new NpgsqlParameter("@id", taiKhoanID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool KhoaHoacMoKhoaTaiKhoan(int taiKhoanID, bool khoa)
        {
            // Cập nhật cả TrangThaiKhoa hoặc TrangThai tuỳ schema
            string sql = "UPDATE TaiKhoan SET TrangThaiKhoa = @khoa WHERE TaiKhoanID = @id";

            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@khoa", khoa),
                new NpgsqlParameter("@id", taiKhoanID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool XoaTaiKhoan(int taiKhoanID)
        {
            string sql = "DELETE FROM TaiKhoan WHERE TaiKhoanID = @id";
            NpgsqlParameter[] pars = { new NpgsqlParameter("@id", taiKhoanID) };
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
            COALESCE(tk.TrangThaiKhoa, FALSE) AS TrangThaiKhoa,
            CASE 
                WHEN tk.VaiTro = 'GiaoVien' THEN COALESCE(gv.HoTen, '(Chưa liên kết)')
                WHEN tk.VaiTro = 'HocSinh'  THEN COALESCE(hs.HoTen, '(Chưa liên kết)')
                WHEN tk.VaiTro = 'Admin'    THEN 'Quản trị viên hệ thống'
                ELSE '(Chưa xác định)'
            END AS TenNguoiDung,
            CASE 
                WHEN COALESCE(tk.TrangThaiKhoa, FALSE) = TRUE THEN 'Đã khóa'
                ELSE 'Hoạt động'
            END AS TrangThaiHienThi
        FROM TaiKhoan tk
        LEFT JOIN GiaoVien gv ON tk.VaiTro = 'GiaoVien' AND (tk.NguoiDungID = gv.GiaoVienID OR gv.TaiKhoanID = tk.TaiKhoanID)
        LEFT JOIN HocSinh hs  ON tk.VaiTro = 'HocSinh'  AND (tk.NguoiDungID = hs.HocSinhID OR hs.TaiKhoanID = tk.TaiKhoanID)
        ORDER BY tk.TaiKhoanID DESC";

            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable TimKiemTheoTenSinhVien(string tenSinhVien)
        {
            if (string.IsNullOrWhiteSpace(tenSinhVien))
            {
                return LayTatCaCoTenNguoiDung();
            }

            string sql = @"
        SELECT 
            tk.TaiKhoanID,
            tk.TenDangNhap,
            tk.VaiTro,
            tk.NguoiDungID,
            COALESCE(tk.TrangThaiKhoa, FALSE) AS TrangThaiKhoa,
            CASE 
                WHEN tk.VaiTro = 'GiaoVien' THEN COALESCE(gv.HoTen, '(Chưa liên kết)')
                WHEN tk.VaiTro = 'HocSinh'  THEN COALESCE(hs.HoTen, '(Chưa liên kết)')
                WHEN tk.VaiTro = 'Admin'    THEN 'Quản trị viên hệ thống'
                ELSE '(Chưa xác định)'
            END AS TenNguoiDung,
            CASE 
                WHEN COALESCE(tk.TrangThaiKhoa, FALSE) = TRUE THEN 'Đã khóa'
                ELSE 'Hoạt động'
            END AS TrangThaiHienThi
        FROM TaiKhoan tk
        LEFT JOIN GiaoVien gv ON tk.VaiTro = 'GiaoVien' AND (tk.NguoiDungID = gv.GiaoVienID OR gv.TaiKhoanID = tk.TaiKhoanID)
        LEFT JOIN HocSinh hs  ON tk.VaiTro = 'HocSinh'  AND (tk.NguoiDungID = hs.HocSinhID OR hs.TaiKhoanID = tk.TaiKhoanID)
        WHERE (tk.VaiTro = 'HocSinh' AND (hs.HoTen ILIKE @kw OR tk.TenDangNhap ILIKE @kw))
        ORDER BY tk.TaiKhoanID DESC";

            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] {
                new NpgsqlParameter("@kw", "%" + tenSinhVien.Trim() + "%")
            });
        }
        public bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)
        {
            // Kiểm tra tên đăng nhập đã tồn tại chưa
            string checkSql = "SELECT COUNT(*) FROM TaiKhoan WHERE LOWER(TenDangNhap) = LOWER(@user)";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter[] {
        new NpgsqlParameter("@user", tenDangNhap.Trim())
    }));
            if (count > 0) return false;

            string hashPass = Passwords.Hash(matKhauPlain.Trim());
            string sql = @"
        INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
        VALUES (@user, @mk, @vaiTro, @nguoiDungID, FALSE)
        RETURNING TaiKhoanID;";

            NpgsqlParameter[] pars = {
        new NpgsqlParameter("@user", tenDangNhap.Trim()),
        new NpgsqlParameter("@mk", hashPass),
        new NpgsqlParameter("@vaiTro", vaiTro),
        new NpgsqlParameter("@nguoiDungID", nguoiDungID)
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
                        new NpgsqlParameter[] {
                    new NpgsqlParameter("@tkID", newTaiKhoanID),
                    new NpgsqlParameter("@gvID", nguoiDungID)
                        });
                }
                else if (vaiTro == "HocSinh" && nguoiDungID > 0)
                {
                    DatabaseHelper.ExecuteNonQuery(
                        "UPDATE HocSinh SET TaiKhoanID = @tkID WHERE HocSinhID = @hsID",
                        new NpgsqlParameter[] {
                    new NpgsqlParameter("@tkID", newTaiKhoanID),
                    new NpgsqlParameter("@hsID", nguoiDungID)
                        });
                }
            }
            catch { /* Nếu bảng chưa có cột TaiKhoanID thì bỏ qua lỗi */ }

            return true;
        }
    }
}
