# 📊 Báo Cáo Tiến Độ & Toàn Bộ Code Gợi Ý Cho Các Task Còn Lại

> **Dự án:** BLT_Quan_Ly_Hoc_Tap  
> **Cơ sở dữ liệu:** SQL Server (`BTL_QL_Hoc_Tap`)  
> **Framework:** .NET Framework 4.8 / Windows Forms / ReaLTaiizor  

---

## 📈 1. Đánh Giá Tiến Độ Hiện Tại

| Thành phần | Số task hoàn thành | Tổng số task | Tỉ lệ (%) | Trạng thái |
|:---|:---:|:---:|:---:|:---|
| **Cơ sở dữ liệu & Cấu hình** | 2 / 2 | 100% | Hoàn thành | `DatabaseHelper.cs`, `CreateDataBase.cs`, `App.config` |
| **Tiện ích (Helpers)** | 2 / 3 | 66.7% | Đang làm | Đã xong `Passwords.cs`, `Session.cs`; còn `Validation.cs` |
| **Mô hình dữ liệu (Models)** | 1 / 8 | 12.5% | Đang làm | Đã xong `TaiKhoan.cs`; còn 7 models |
| **Tầng truy cập dữ liệu (DAL)** | 1 / 6 | 16.7% | Đang làm | Đã xong `TaiKhoanDAL.cs`; còn 5 DALs |
| **Form Đăng Nhập (`frm_DangNhap`)** | 8 / 8 | 100% | Hoàn thành | Đăng nhập, ghi nhớ, điều hướng vai trò, xử lý lỗi |
| **Form Quản Trị (`Frm_Admin`)** | 1 / 6 | 16.7% | Chưa làm | Mới có UI + xử lý đăng xuất; chưa load dữ liệu |
| **Form Giáo Viên (`frm_GiaoVien`)** | 0 / 5 | 0% | Chưa làm | Chỉ có giao diện UI |
| **Form Học Viên (`Frm_HocVien`)** | 0 / 4 | 0% | Chưa làm | Chỉ có giao diện UI |
| **Form Kiểm Tra (`Frm_KiemTra`)** | 0 / 7 | 0% | Chưa làm | Chỉ có giao diện UI |
| **TỔNG THỂ DỰ ÁN** | **15 / 41** | **~36.5%** | **Đang trong giai đoạn viết nghiệp vụ (Business Logic)** |

---

## 🗂️ 2. Code Cho Toàn Bộ Các Model Còn Lại

### 2.1. File `Models/GiaoVien.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class GiaoVien
    {
        public int GiaoVienID { get; set; }
        public int TaiKhoanID { get; set; }
        public string HoTen { get; set; }
        public string ChuyenMon { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string AnhDaiDien { get; set; }
    }
}
```

### 2.2. File `Models/HocSinh.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class HocSinh
    {
        public int HocSinhID { get; set; }
        public int TaiKhoanID { get; set; }
        public string HoTen { get; set; }
        public DateTime? NgaySinh { get; set; }
        public string GioiTinh { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string AnhDaiDien { get; set; }
    }
}
```

### 2.3. File `Models/KhoaHoc.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class KhoaHoc
    {
        public int KhoaHocID { get; set; }
        public string TenKhoaHoc { get; set; }
        public string MoTa { get; set; }
        public DateTime? NgayBatDau { get; set; }
        public DateTime? NgayKetThuc { get; set; }
        public string TrangThai { get; set; }
    }
}
```

### 2.4. File `Models/LopHoc.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class LopHoc
    {
        public int LopHocID { get; set; }
        public int KhoaHocID { get; set; }
        public int GiaoVienID { get; set; }
        public string TenLop { get; set; }
        public string LichHoc { get; set; }
        public int SiSoToiDa { get; set; }
        public string TrangThai { get; set; }
        
        // Thuộc tính mở rộng hiển thị
        public string TenGiaoVien { get; set; }
        public string TenKhoaHoc { get; set; }
    }
}
```

### 2.5. File `Models/DeThi.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class DeThi
    {
        public int DeThiID { get; set; }
        public int LopHocID { get; set; }
        public int GiaoVienID { get; set; }
        public string TenDeThi { get; set; }
        public string MonHoc { get; set; }
        public int SoCau { get; set; }
        public int ThoiGian { get; set; } // Số phút
        public DateTime? HanNop { get; set; }
        public string TrangThai { get; set; }
    }
}
```

### 2.6. File `Models/CauHoi.cs`
```csharp
namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class CauHoi
    {
        public int CauHoiID { get; set; }
        public int DeThiID { get; set; }
        public string NoiDung { get; set; }
        public string DapAnA { get; set; }
        public string DapAnB { get; set; }
        public string DapAnC { get; set; }
        public string DapAnD { get; set; }
        public string DapAnDung { get; set; } // 'A', 'B', 'C', 'D'
        public int ThuTu { get; set; }
    }
}
```

### 2.7. File `Models/KetQuaThi.cs`
```csharp
using System;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class KetQuaThi
    {
        public int KetQuaID { get; set; }
        public int HocSinhID { get; set; }
        public int DeThiID { get; set; }
        public int SoCauDung { get; set; }
        public decimal Diem { get; set; }
        public DateTime ThoiGianNop { get; set; }
        public string TrangThai { get; set; }

        // Thuộc tính phụ trợ hiển thị bảng
        public string TenHocSinh { get; set; }
        public string TenDeThi { get; set; }
    }
}
```

---

## 🛠️ 3. Code Helper `Helpers/Validation.cs`

```csharp
using System;
using System.Text.RegularExpressions;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class Validation
    {
        public static bool IsEmpty(string input) => string.IsNullOrWhiteSpace(input);

        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            return Regex.IsMatch(email, @"^[\w-\.]+@([\w-]+\.)+[\w-]{2,4}$");
        }

        public static bool IsValidPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return false;
            return Regex.IsMatch(phone, @"^(0[3|5|7|8|9])+([0-9]{8})$");
        }
    }
}
```

---

## 💾 4. Code Cho Toàn Bộ Các Lớp DAL

### 4.1. File `DAL/GiaoVienDAL.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class GiaoVienDAL
    {
        public DataTable LayTatCa()
        {
            string sql = "SELECT GiaoVienID, HoTen, ChuyenMon, Email, SoDienThoai, DiaChi FROM GiaoVien";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public GiaoVien LayTheoID(int id)
        {
            string sql = "SELECT * FROM GiaoVien WHERE GiaoVienID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new GiaoVien
            {
                GiaoVienID = Convert.ToInt32(r["GiaoVienID"]),
                TaiKhoanID = r["TaiKhoanID"] != DBNull.Value ? Convert.ToInt32(r["TaiKhoanID"]) : 0,
                HoTen = r["HoTen"].ToString(),
                ChuyenMon = r["ChuyenMon"]?.ToString(),
                Email = r["Email"]?.ToString(),
                SoDienThoai = r["SoDienThoai"]?.ToString(),
                DiaChi = r["DiaChi"]?.ToString()
            };
        }

        public bool Them(GiaoVien gv)
        {
            string sql = @"INSERT INTO GiaoVien (HoTen, ChuyenMon, Email, SoDienThoai, DiaChi) 
                           VALUES (@hoten, @chuyenmon, @email, @sdt, @diachi)";
            SqlParameter[] pars = {
                new SqlParameter("@hoten", gv.HoTen),
                new SqlParameter("@chuyenmon", (object)gv.ChuyenMon ?? DBNull.Value),
                new SqlParameter("@email", (object)gv.Email ?? DBNull.Value),
                new SqlParameter("@sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@diachi", (object)gv.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Sua(GiaoVien gv)
        {
            string sql = @"UPDATE GiaoVien SET HoTen=@hoten, ChuyenMon=@chuyenmon, Email=@email, 
                           SoDienThoai=@sdt, DiaChi=@diachi WHERE GiaoVienID=@id";
            SqlParameter[] pars = {
                new SqlParameter("@hoten", gv.HoTen),
                new SqlParameter("@chuyenmon", (object)gv.ChuyenMon ?? DBNull.Value),
                new SqlParameter("@email", (object)gv.Email ?? DBNull.Value),
                new SqlParameter("@sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@diachi", (object)gv.DiaChi ?? DBNull.Value),
                new SqlParameter("@id", gv.GiaoVienID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Xoa(int id)
        {
            string sql = "DELETE FROM GiaoVien WHERE GiaoVienID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) }) > 0;
        }

        public DataTable TimKiem(string tuKhoa)
        {
            string sql = @"SELECT GiaoVienID, HoTen, ChuyenMon, Email, SoDienThoai, DiaChi 
                           FROM GiaoVien WHERE HoTen LIKE @kw OR Email LIKE @kw OR SoDienThoai LIKE @kw";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@kw", "%" + tuKhoa + "%") });
        }
    }
}
```

### 4.2. File `DAL/HocSinhDAL.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class HocSinhDAL
    {
        public DataTable LayTatCa()
        {
            string sql = "SELECT HocSinhID, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi FROM HocSinh";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public HocSinh LayTheoID(int id)
        {
            string sql = "SELECT * FROM HocSinh WHERE HocSinhID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new HocSinh
            {
                HocSinhID = Convert.ToInt32(r["HocSinhID"]),
                TaiKhoanID = r["TaiKhoanID"] != DBNull.Value ? Convert.ToInt32(r["TaiKhoanID"]) : 0,
                HoTen = r["HoTen"].ToString(),
                NgaySinh = r["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(r["NgaySinh"]) : (DateTime?)null,
                GioiTinh = r["GioiTinh"]?.ToString(),
                Email = r["Email"]?.ToString(),
                SoDienThoai = r["SoDienThoai"]?.ToString(),
                DiaChi = r["DiaChi"]?.ToString()
            };
        }

        public bool Them(HocSinh hs)
        {
            string sql = @"INSERT INTO HocSinh (HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi) 
                           VALUES (@hoten, @ngaysinh, @gioitinh, @email, @sdt, @diachi)";
            SqlParameter[] pars = {
                new SqlParameter("@hoten", hs.HoTen),
                new SqlParameter("@ngaysinh", (object)hs.NgaySinh ?? DBNull.Value),
                new SqlParameter("@gioitinh", (object)hs.GioiTinh ?? DBNull.Value),
                new SqlParameter("@email", (object)hs.Email ?? DBNull.Value),
                new SqlParameter("@sdt", (object)hs.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@diachi", (object)hs.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Sua(HocSinh hs)
        {
            string sql = @"UPDATE HocSinh SET HoTen=@hoten, NgaySinh=@ngaysinh, GioiTinh=@gioitinh, 
                           Email=@email, SoDienThoai=@sdt, DiaChi=@diachi WHERE HocSinhID=@id";
            SqlParameter[] pars = {
                new SqlParameter("@hoten", hs.HoTen),
                new SqlParameter("@ngaysinh", (object)hs.NgaySinh ?? DBNull.Value),
                new SqlParameter("@gioitinh", (object)hs.GioiTinh ?? DBNull.Value),
                new SqlParameter("@email", (object)hs.Email ?? DBNull.Value),
                new SqlParameter("@sdt", (object)hs.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@diachi", (object)hs.DiaChi ?? DBNull.Value),
                new SqlParameter("@id", hs.HocSinhID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Xoa(int id)
        {
            string sql = "DELETE FROM HocSinh WHERE HocSinhID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) }) > 0;
        }

        public DataTable TimKiem(string tuKhoa)
        {
            string sql = @"SELECT HocSinhID, HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi 
                           FROM HocSinh WHERE HoTen LIKE @kw OR Email LIKE @kw OR SoDienThoai LIKE @kw";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@kw", "%" + tuKhoa + "%") });
        }
    }
}
```

### 4.3. File `DAL/LopHocDAL.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class LopHocDAL
    {
        public DataTable LayTatCa()
        {
            string sql = @"SELECT l.LopHocID, l.TenLop, k.TenKhoaHoc, g.HoTen AS TenGiaoVien, l.LichHoc, l.SiSoToiDa, l.TrangThai 
                           FROM LopHoc l
                           LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                           LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable LayTheoGiaoVien(int gvID)
        {
            string sql = "SELECT * FROM LopHoc WHERE GiaoVienID = @gvID";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@gvID", gvID) });
        }

        public DataTable LayTheoHocSinh(int hsID)
        {
            string sql = @"SELECT l.LopHocID, l.TenLop, g.HoTen AS TenGiaoVien, l.LichHoc, d.TrangThai 
                           FROM DangKyLop d
                           JOIN LopHoc l ON d.LopHocID = l.LopHocID
                           LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                           WHERE d.HocSinhID = @hsID";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@hsID", hsID) });
        }

        public bool ThemLop(LopHoc lop)
        {
            string sql = @"INSERT INTO LopHoc (KhoaHocID, GiaoVienID, TenLop, LichHoc, SiSoToiDa, TrangThai) 
                           VALUES (@khID, @gvID, @ten, @lich, @siso, @tt)";
            SqlParameter[] pars = {
                new SqlParameter("@khID", lop.KhoaHocID),
                new SqlParameter("@gvID", lop.GiaoVienID),
                new SqlParameter("@ten", lop.TenLop),
                new SqlParameter("@lich", lop.LichHoc ?? ""),
                new SqlParameter("@siso", lop.SiSoToiDa),
                new SqlParameter("@tt", lop.TrangThai ?? "Đang mở")
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
    }
}
```

### 4.4. File `DAL/DeThiDAL.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class DeThiDAL
    {
        public DataTable LayDeThiTheoLop(int lopID)
        {
            string sql = "SELECT * FROM DeThi WHERE LopHocID = @lopID";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@lopID", lopID) });
        }

        public DataTable LayDeThiTheoGiaoVien(int gvID)
        {
            string sql = "SELECT * FROM DeThi WHERE GiaoVienID = @gvID";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@gvID", gvID) });
        }

        public DeThi LayTheoID(int deThiID)
        {
            string sql = "SELECT * FROM DeThi WHERE DeThiID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@id", deThiID) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new DeThi
            {
                DeThiID = Convert.ToInt32(r["DeThiID"]),
                TenDeThi = r["TenDeThi"].ToString(),
                MonHoc = r["MonHoc"]?.ToString(),
                SoCau = Convert.ToInt32(r["SoCau"]),
                ThoiGian = Convert.ToInt32(r["ThoiGian"]),
                HanNop = r["HanNop"] != DBNull.Value ? Convert.ToDateTime(r["HanNop"]) : (DateTime?)null,
                TrangThai = r["TrangThai"]?.ToString()
            };
        }

        public List<CauHoi> LayDanhSachCauHoi(int deThiID)
        {
            var list = new List<CauHoi>();
            string sql = "SELECT * FROM CauHoi WHERE DeThiID = @id ORDER BY ThuTu ASC";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@id", deThiID) });
            foreach (DataRow r in dt.Rows)
            {
                list.Add(new CauHoi
                {
                    CauHoiID = Convert.ToInt32(r["CauHoiID"]),
                    DeThiID = Convert.ToInt32(r["DeThiID"]),
                    NoiDung = r["NoiDung"].ToString(),
                    DapAnA = r["DapAnA"].ToString(),
                    DapAnB = r["DapAnB"].ToString(),
                    DapAnC = r["DapAnC"].ToString(),
                    DapAnD = r["DapAnD"].ToString(),
                    DapAnDung = r["DapAnDung"].ToString().Trim(),
                    ThuTu = r["ThuTu"] != DBNull.Value ? Convert.ToInt32(r["ThuTu"]) : 0
                });
            }
            return list;
        }

        public bool ThemDeThi(DeThi de)
        {
            string sql = @"INSERT INTO DeThi (LopHocID, GiaoVienID, TenDeThi, MonHoc, SoCau, ThoiGian, HanNop, TrangThai)
                           VALUES (@lopID, @gvID, @ten, @mon, @socau, @tg, @han, @tt)";
            SqlParameter[] pars = {
                new SqlParameter("@lopID", de.LopHocID),
                new SqlParameter("@gvID", de.GiaoVienID),
                new SqlParameter("@ten", de.TenDeThi),
                new SqlParameter("@mon", de.MonHoc ?? ""),
                new SqlParameter("@socau", de.SoCau),
                new SqlParameter("@tg", de.ThoiGian),
                new SqlParameter("@han", (object)de.HanNop ?? DBNull.Value),
                new SqlParameter("@tt", de.TrangThai ?? "Đang mở")
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool ThemCauHoi(CauHoi ch)
        {
            string sql = @"INSERT INTO CauHoi (DeThiID, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, ThuTu)
                           VALUES (@deID, @nd, @a, @b, @c, @d, @dung, @thutu)";
            SqlParameter[] pars = {
                new SqlParameter("@deID", ch.DeThiID),
                new SqlParameter("@nd", ch.NoiDung),
                new SqlParameter("@a", ch.DapAnA),
                new SqlParameter("@b", ch.DapAnB),
                new SqlParameter("@c", ch.DapAnC),
                new SqlParameter("@d", ch.DapAnD),
                new SqlParameter("@dung", ch.DapAnDung),
                new SqlParameter("@thutu", ch.ThuTu)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
    }
}
```

### 4.5. File `DAL/KetQuaDAL.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class KetQuaDAL
    {
        public bool KiemTraDaThi(int hocSinhID, int deThiID)
        {
            string sql = "SELECT COUNT(*) FROM KetQuaThi WHERE HocSinhID = @hsID AND DeThiID = @deID";
            SqlParameter[] pars = {
                new SqlParameter("@hsID", hocSinhID),
                new SqlParameter("@deID", deThiID)
            };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, pars)) > 0;
        }

        public int LuuKetQua(KetQuaThi kq)
        {
            string sql = @"INSERT INTO KetQuaThi (HocSinhID, DeThiID, SoCauDung, Diem, ThoiGianNop, TrangThai) 
                           VALUES (@hsID, @deID, @dung, @diem, GETDATE(), @tt);
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] pars = {
                new SqlParameter("@hsID", kq.HocSinhID),
                new SqlParameter("@deID", kq.DeThiID),
                new SqlParameter("@dung", kq.SoCauDung),
                new SqlParameter("@diem", kq.Diem),
                new SqlParameter("@tt", kq.TrangThai ?? "Hoàn thành")
            };
            object obj = DatabaseHelper.ExecuteScalar(sql, pars);
            return obj != null ? Convert.ToInt32(obj) : 0;
        }

        public void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung)
        {
            string sql = @"INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) 
                           VALUES (@kqID, @chID, @da, @dung)";
            SqlParameter[] pars = {
                new SqlParameter("@kqID", ketQuaID),
                new SqlParameter("@chID", cauHoiID),
                new SqlParameter("@da", dapAnChon),
                new SqlParameter("@dung", laDung)
            };
            DatabaseHelper.ExecuteNonQuery(sql, pars);
        }

        public DataTable LayLichSuTheoHocSinh(int hocSinhID)
        {
            string sql = @"SELECT k.KetQuaID, d.TenDeThi, d.MonHoc, k.SoCauDung, k.Diem, k.ThoiGianNop 
                           FROM KetQuaThi k
                           JOIN DeThi d ON k.DeThiID = d.DeThiID
                           WHERE k.HocSinhID = @hsID
                           ORDER BY k.ThoiGianNop DESC";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@hsID", hocSinhID) });
        }
    }
}
```

---

## 🖥️ 5. Code Gợi Ý Cho 4 Form Còn Lại

### 5.1. File `Forms/Frm_Admin.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using System;
using System.Data;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_Admin : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly GiaoVienDAL _gvDAL = new GiaoVienDAL();
        private readonly HocSinhDAL _hsDAL = new HocSinhDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();
        private int _selectedGiaoVienID = 0;

        public Frm_Admin()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            LoadDashboard();
            LoadDanhSachGiaoVien();
        }

        // 1. DASHBOARD
        private void LoadDashboard()
        {
            try
            {
                // Thống kê số lượng
                int soHV = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
                int soGV = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
                int soKH = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KhoaHoc"));
                int soLop = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc"));

                materialLabel1.Text = $"Học Viên: {soHV}";
                materialLabel2.Text = $"Giáo Viên: {soGV}";
                materialLabel3.Text = $"Khóa Học: {soKH}";
                materialLabel4.Text = $"Lớp Học: {soLop}";

                // Biểu đồ thống kê điểm số
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

        // 2. QUẢN LÝ GIÁO VIÊN
        private void LoadDanhSachGiaoVien()
        {
            dataGridView1.DataSource = _gvDAL.LayTatCa();
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dataGridView1.Rows[e.RowIndex];
                _selectedGiaoVienID = Convert.ToInt32(row.Cells[0].Value);
                materialTextBoxEdit10.Text = row.Cells[1].Value?.ToString(); // Họ tên
                materialTextBoxEdit11.Text = row.Cells[2].Value?.ToString(); // Chuyên môn
                materialTextBoxEdit9.Text = row.Cells[3].Value?.ToString();  // Email
                materialTextBoxEdit8.Text = row.Cells[4].Value?.ToString();  // Số điện thoại
                materialTextBoxEdit7.Text = row.Cells[5].Value?.ToString();  // Địa chỉ
            }
        }

        private void materialButton8_Click(object sender, EventArgs e) // Nút Thêm
        {
            if (string.IsNullOrWhiteSpace(materialTextBoxEdit10.Text))
            {
                MessageBox.Show("Vui lòng nhập họ tên giáo viên!");
                return;
            }
            GiaoVien gv = new GiaoVien
            {
                HoTen = materialTextBoxEdit10.Text.Trim(),
                ChuyenMon = materialTextBoxEdit11.Text.Trim(),
                Email = materialTextBoxEdit9.Text.Trim(),
                SoDienThoai = materialTextBoxEdit8.Text.Trim(),
                DiaChi = materialTextBoxEdit7.Text.Trim()
            };
            if (_gvDAL.Them(gv))
            {
                MessageBox.Show("Thêm giáo viên thành công!");
                LoadDanhSachGiaoVien();
                LamMoiFormGV();
            }
        }

        private void materialButton7_Click(object sender, EventArgs e) // Nút Sửa
        {
            if (_selectedGiaoVienID <= 0) return;
            GiaoVien gv = new GiaoVien
            {
                GiaoVienID = _selectedGiaoVienID,
                HoTen = materialTextBoxEdit10.Text.Trim(),
                ChuyenMon = materialTextBoxEdit11.Text.Trim(),
                Email = materialTextBoxEdit9.Text.Trim(),
                SoDienThoai = materialTextBoxEdit8.Text.Trim(),
                DiaChi = materialTextBoxEdit7.Text.Trim()
            };
            if (_gvDAL.Sua(gv))
            {
                MessageBox.Show("Cập nhật thành công!");
                LoadDanhSachGiaoVien();
            }
        }

        private void materialButton6_Click(object sender, EventArgs e) // Nút Xóa
        {
            if (_selectedGiaoVienID <= 0) return;
            if (MessageBox.Show("Bạn có chắc chắn muốn xóa giáo viên này?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                if (_gvDAL.Xoa(_selectedGiaoVienID))
                {
                    MessageBox.Show("Đã xóa giáo viên!");
                    LoadDanhSachGiaoVien();
                    LamMoiFormGV();
                }
            }
        }

        private void materialButton5_Click(object sender, EventArgs e) // Nút Làm mới
        {
            LamMoiFormGV();
        }

        private void LamMoiFormGV()
        {
            _selectedGiaoVienID = 0;
            materialTextBoxEdit10.Clear();
            materialTextBoxEdit11.Clear();
            materialTextBoxEdit9.Clear();
            materialTextBoxEdit8.Clear();
            materialTextBoxEdit7.Clear();
        }

        private void materialTextBoxEdit12_TextChanged(object sender, EventArgs e) // Tìm kiếm GV
        {
            dataGridView1.DataSource = _gvDAL.TimKiem(materialTextBoxEdit12.Text.Trim());
        }

        // ĐĂNG XUẤT
        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialTabControl1.SelectedTab.Text == "Đăng Xuất")
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất không?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Close();
                    new frm_DangNhap().Show();
                }
                else
                {
                    materialTabControl1.SelectedIndex = 0;
                }
            }
        }
    }
}
```

---

### 5.2. File `Forms/frm_GiaoVien.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using ReaLTaiizor.Controls;
using System;
using System.Data;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_GiaoVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();

        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
        }

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            materialLabel31.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";
            LoadLopHocCuaToi();
        }

        private void LoadLopHocCuaToi()
        {
            DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
            // Gán vào DataGridView danh sách lớp của giáo viên
        }

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialTabControl1.SelectedTab.Text.Contains("Đăng Xuất"))
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Close();
                    new frm_DangNhap().Show();
                }
                else
                {
                    materialTabControl1.SelectedIndex = 0;
                }
            }
        }
    }
}
```

---

### 5.3. File `Forms/Frm_HocVien.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using ReaLTaiizor.Controls;
using System;
using System.Data;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();

        public HocVien()
        {
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
        }

        private void HocVien_Load(object sender, EventArgs e)
        {
            LoadThongTinHocVien();
            LoadLopHoc();
            LoadLichSuThi();
        }

        private void LoadThongTinHocVien()
        {
            // Hiển thị tên học viên
        }

        private void LoadLopHoc()
        {
            DataTable dtLop = _lopDAL.LayTheoHocSinh(Session.NguoiDungID);
            // Hiển thị lớp học lên tab Lớp Học
        }

        private void LoadLichSuThi()
        {
            DataTable dtKQ = _ketQuaDAL.LayLichSuTheoHocSinh(Session.NguoiDungID);
            // Gán DataSource vào DataGridView kết quả
        }

        // Sự kiện click nút làm bài thi:
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

        private void materialTabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (materialTabControl1.SelectedTab.Text.Contains("Đăng Xuất"))
            {
                if (MessageBox.Show("Bạn có muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Close();
                    new frm_DangNhap().Show();
                }
                else
                {
                    materialTabControl1.SelectedIndex = 0;
                }
            }
        }
    }
}
```

---

### 5.4. File `Forms/Frm_KiemTra.cs`
```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_KiemTra : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly int _deThiID;
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();

        private DeThi _deThi;
        private List<CauHoi> _cauHois = new List<CauHoi>();
        private Dictionary<int, string> _dapAnChon = new Dictionary<int, string>(); // index -> 'A'/'B'/'C'/'D'
        private int _currentIndex = 0;
        private int _thoiGianConLai; // giây
        private Timer _timer;

        public Frm_KiemTra(int deThiID)
        {
            InitializeComponent();
            _deThiID = deThiID;
        }

        private void KiemTra_Load(object sender, EventArgs e)
        {
            _deThi = _deThiDAL.LayTheoID(_deThiID);
            _cauHois = _deThiDAL.LayDanhSachCauHoi(_deThiID);

            if (_deThi == null || _cauHois.Count == 0)
            {
                MessageBox.Show("Đề thi chưa có câu hỏi nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            this.Text = $"BÀI THI: {_deThi.TenDeThi} - Môn: {_deThi.MonHoc}";
            _thoiGianConLai = _deThi.ThoiGian * 60;

            // Khởi tạo Timer đếm ngược
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // Khởi tạo các sự kiện nút điều hướng
            foreverButton1.Click += (s, ev) => ChuyenCau(_currentIndex - 1); // Câu trước
            foreverButton2.Click += (s, ev) => ChuyenCau(_currentIndex + 1); // Câu sau
            materialButton1.Click += (s, ev) => XacNhanNopBai();             // Nộp bài

            // Sự kiện chọn radio button
            materialRadioButton1.CheckedChanged += Radio_CheckedChanged;
            materialRadioButton2.CheckedChanged += Radio_CheckedChanged;
            materialRadioButton3.CheckedChanged += Radio_CheckedChanged;
            materialRadioButton4.CheckedChanged += Radio_CheckedChanged;

            HienThiCauHoi(_currentIndex);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_thoiGianConLai > 0)
            {
                _thoiGianConLai--;
                int phut = _thoiGianConLai / 60;
                int giay = _thoiGianConLai % 60;
                materialLabel1.Text = $"Thời gian: {phut:D2}:{giay:D2}";
            }
            else
            {
                _timer.Stop();
                MessageBox.Show("Đã hết thời gian làm bài! Hệ thống tự động nộp bài.", "Hết giờ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NopBai();
            }
        }

        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= _cauHois.Count) return;
            _currentIndex = index;
            CauHoi q = _cauHois[index];

            materialLabel2.Text = $"Câu {index + 1}/{_cauHois.Count}: {q.NoiDung}";
            materialRadioButton1.Text = $"A. {q.DapAnA}";
            materialRadioButton2.Text = $"B. {q.DapAnB}";
            materialRadioButton3.Text = $"C. {q.DapAnC}";
            materialRadioButton4.Text = $"D. {q.DapAnD}";

            // Bỏ chọn trước khi set lại trạng thái đã lưu
            materialRadioButton1.Checked = false;
            materialRadioButton2.Checked = false;
            materialRadioButton3.Checked = false;
            materialRadioButton4.Checked = false;

            if (_dapAnChon.ContainsKey(index))
            {
                string da = _dapAnChon[index];
                if (da == "A") materialRadioButton1.Checked = true;
                else if (da == "B") materialRadioButton2.Checked = true;
                else if (da == "C") materialRadioButton3.Checked = true;
                else if (da == "D") materialRadioButton4.Checked = true;
            }

            hopeProgressBar1.Value = (int)(((index + 1) / (float)_cauHois.Count) * 100);
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            if (materialRadioButton1.Checked) _dapAnChon[_currentIndex] = "A";
            else if (materialRadioButton2.Checked) _dapAnChon[_currentIndex] = "B";
            else if (materialRadioButton3.Checked) _dapAnChon[_currentIndex] = "C";
            else if (materialRadioButton4.Checked) _dapAnChon[_currentIndex] = "D";
        }

        private void ChuyenCau(int newIndex)
        {
            if (newIndex >= 0 && newIndex < _cauHois.Count)
            {
                HienThiCauHoi(newIndex);
            }
        }

        private void XacNhanNopBai()
        {
            int soCauDaLam = _dapAnChon.Count;
            string msg = $"Bạn đã làm {soCauDaLam}/{_cauHois.Count} câu.\nBạn có chắc chắn muốn nộp bài?";
            if (MessageBox.Show(msg, "Xác nhận nộp bài", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _timer.Stop();
                NopBai();
            }
        }

        private void NopBai()
        {
            int soCauDung = 0;
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                if (_dapAnChon.ContainsKey(i) && string.Equals(_dapAnChon[i], q.DapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }

            decimal diem = Math.Round(((decimal)soCauDung / _cauHois.Count) * 10, 2);

            KetQuaThi kq = new KetQuaThi
            {
                HocSinhID = Session.NguoiDungID,
                DeThiID = _deThiID,
                SoCauDung = soCauDung,
                Diem = diem,
                ThoiGianNop = DateTime.Now,
                TrangThai = "Hoàn thành"
            };

            int kqID = _ketQuaDAL.LuuKetQua(kq);

            // Lưu chi tiết bài làm
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                string chon = _dapAnChon.ContainsKey(i) ? _dapAnChon[i] : "";
                bool laDung = string.Equals(chon, q.DapAnDung, StringComparison.OrdinalIgnoreCase);
                _ketQuaDAL.LuuChiTietBaiLam(kqID, q.CauHoiID, chon, laDung);
            }

            MessageBox.Show($"Kết quả: {soCauDung}/{_cauHois.Count} câu đúng.\nĐiểm số: {diem}/10", 
                            "Kết quả bài thi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            this.Close();
        }
    }
}
```

---

## 🎯 6. Trình Tự Thực Hiện Khuyến Nghị

1. **Bước 1:** Tạo đầy đủ các file trong thư mục `Models/` (7 models).
2. **Bước 2:** Tạo file `Helpers/Validation.cs`.
3. **Bước 3:** Hoàn thiện 5 file `DAL/` (`GiaoVienDAL`, `HocSinhDAL`, `LopHocDAL`, `DeThiDAL`, `KetQuaDAL`).
4. **Bước 4:** Gắn code cho `Frm_Admin.cs` để test CRUD giáo viên, học viên và xem thống kê dashboard.
5. **Bước 5:** Gắn code cho `Frm_HocVien.cs` và `Frm_KiemTra.cs` để test luồng học viên vào làm bài thi trắc nghiệm.
6. **Bước 6:** Gắn code cho `frm_GiaoVien.cs` để quản lý lớp học và tạo đề thi.
