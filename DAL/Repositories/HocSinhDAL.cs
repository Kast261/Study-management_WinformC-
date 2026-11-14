using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Npgsql;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class HocSinhDAL
    {
        public DataTable LayTatCa()
        {
            string sql = @"
                SELECT 
                    hs.HocSinhID, 
                    hs.HoTen, 
                    hs.NgaySinh, 
                    hs.GioiTinh, 
                    hs.Email, 
                    hs.SoDienThoai, 
                    hs.DiaChi,
                    COALESCE(l.TenLop, 'Chưa xếp lớp') AS TenLop
                FROM HocSinh hs
                LEFT JOIN LopHoc l ON hs.LopHocID = l.LopHocID
                ORDER BY hs.HocSinhID ASC";
            return DatabaseHelper.ExecuteQuery(sql);
        }
        public HocSinh LayTheoID(int id)
        {
            string sql = @"SELECT HocSinhID, HoTen, Email, SoDienThoai, DiaChi,
                                  NgaySinh, GioiTinh, LopHocID, TaiKhoanID
                           FROM HocSinh WHERE HocSinhID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new HocSinh
            {
                HocSinhID = Convert.ToInt32(r["HocSinhID"]),
                TaiKhoanID = r.Table.Columns.Contains("TaiKhoanID") && r["TaiKhoanID"] != DBNull.Value ? Convert.ToInt32(r["TaiKhoanID"]) : 0,
                HoTen = r["HoTen"].ToString(),
                NgaySinh = r["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(r["NgaySinh"]) : (DateTime?)null,
                GioiTinh = r["GioiTinh"]?.ToString(),
                Email = r["Email"]?.ToString(),
                SoDienThoai = r["SoDienThoai"]?.ToString(),
                DiaChi = r["DiaChi"]?.ToString()
            };
        }
        public bool Them(HocSinh hs, int lopHocID = 0)
        {
            if (hs == null || lopHocID < 0) return false;
            // The procedure holds the same class lock as registration/approval.
            // One CALL atomically checks capacity and creates the student.
            int id = ProcedureExecutor.Id(
                "CALL study_them_hoc_sinh(@hoten, @ngaysinh, @gioitinh, @email, @sdt, @diachi, @lop, 0)",
                ProcedureExecutor.Text("hoten", hs.HoTen), ProcedureExecutor.Date("ngaysinh", hs.NgaySinh),
                ProcedureExecutor.Text("gioitinh", hs.GioiTinh), ProcedureExecutor.Text("email", hs.Email),
                ProcedureExecutor.Text("sdt", hs.SoDienThoai), ProcedureExecutor.Text("diachi", hs.DiaChi),
                new NpgsqlParameter("lop", lopHocID));
            if (id <= 0) return false;
            hs.HocSinhID = id;
            return true;
        }

        public HocSinh LayTheoTaiKhoan(int taiKhoanID)
        {
            string sql = @"
                SELECT hs.HocSinhID, hs.HoTen, hs.NgaySinh, hs.GioiTinh,
                       hs.Email, hs.SoDienThoai, hs.DiaChi
                FROM HocSinh hs
                JOIN TaiKhoan tk ON
                    hs.HocSinhID = NULLIF(tk.NguoiDungID, 0)
                    OR (COALESCE(tk.NguoiDungID, 0) = 0
                        AND hs.TaiKhoanID = tk.TaiKhoanID)
                WHERE tk.TaiKhoanID = @tkID
                  AND LOWER(tk.VaiTro) IN ('hocsinh', 'hocvien')
                  AND COALESCE(tk.TrangThaiKhoa, FALSE) = FALSE";

            DataTable dt = DatabaseHelper.ExecuteQuery(
                sql, new NpgsqlParameter("@tkID", taiKhoanID));
            if (dt.Rows.Count == 0) return null;
            if (dt.Rows.Count != 1)
                throw new InvalidOperationException("Tài khoản đang liên kết với nhiều hồ sơ học sinh. Vui lòng liên hệ quản trị viên.");

            DataRow r = dt.Rows[0];
            return new HocSinh
            {
                HocSinhID = Convert.ToInt32(r["HocSinhID"]),
                TaiKhoanID = taiKhoanID,
                HoTen = r["HoTen"].ToString(),
                NgaySinh = r["NgaySinh"] == DBNull.Value
                    ? (DateTime?)null : Convert.ToDateTime(r["NgaySinh"]),
                GioiTinh = r["GioiTinh"].ToString(),
                Email = r["Email"].ToString(),
                SoDienThoai = r["SoDienThoai"].ToString(),
                DiaChi = r["DiaChi"].ToString()
            };
        }

        public bool CapNhatHoSoCaNhan(int taiKhoanID, HocSinh hs)
        {
            string sql = @"
                UPDATE HocSinh hs
                SET HoTen = @hoten, NgaySinh = @ngaysinh,
                    GioiTinh = @gioitinh, Email = @email,
                    SoDienThoai = @sdt, DiaChi = @diachi
                WHERE hs.HocSinhID = @hsID
                  AND EXISTS (
                      SELECT 1 FROM TaiKhoan tk
                      WHERE tk.TaiKhoanID = @tkID
                        AND LOWER(tk.VaiTro) IN ('hocsinh', 'hocvien')
                        AND COALESCE(tk.TrangThaiKhoa, FALSE) = FALSE
                        AND (hs.HocSinhID = NULLIF(tk.NguoiDungID, 0)
                             OR (COALESCE(tk.NguoiDungID, 0) = 0
                                 AND hs.TaiKhoanID = tk.TaiKhoanID)))";

            NpgsqlParameter[] pars =
            {
                new NpgsqlParameter("@tkID", taiKhoanID),
                new NpgsqlParameter("@hsID", hs.HocSinhID),
                new NpgsqlParameter("@hoten", hs.HoTen),
                new NpgsqlParameter("@ngaysinh", NpgsqlTypes.NpgsqlDbType.Date)
                {
                    Value = (object)hs.NgaySinh ?? DBNull.Value
                },
                new NpgsqlParameter("@gioitinh", (object)hs.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("@email", (object)hs.Email ?? DBNull.Value),
                new NpgsqlParameter("@sdt", (object)hs.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("@diachi", (object)hs.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) == 1;
        }

        public bool Sua(HocSinh hs)
        {
            if (hs == null) throw new ArgumentNullException(nameof(hs));
            return ProcedureExecutor.Boolean(
                "CALL study_sua_hoc_sinh(@id, @hoten, @ngaysinh, @gioitinh, @email, @sdt, @diachi, FALSE)",
                "p_updated", new NpgsqlParameter("id", hs.HocSinhID),
                ProcedureExecutor.Text("hoten", hs.HoTen), ProcedureExecutor.Date("ngaysinh", hs.NgaySinh),
                ProcedureExecutor.Text("gioitinh", hs.GioiTinh), ProcedureExecutor.Text("email", hs.Email),
                ProcedureExecutor.Text("sdt", hs.SoDienThoai), ProcedureExecutor.Text("diachi", hs.DiaChi));
        }
        public bool Xoa(int id)
        {
            return ProcedureExecutor.Boolean("CALL study_xoa_hoc_sinh(@id, FALSE)", "p_deleted",
                new NpgsqlParameter("id", id));
        }
        public DataTable TimKiem(string tuKhoa)
        {
            string sql = @"
                SELECT 
                    hs.HocSinhID, 
                    hs.HoTen, 
                    hs.NgaySinh, 
                    hs.GioiTinh, 
                    hs.Email, 
                    hs.SoDienThoai, 
                    hs.DiaChi,
                    COALESCE(l.TenLop, 'Chưa xếp lớp') AS TenLop
                FROM HocSinh hs
                LEFT JOIN LopHoc l ON hs.LopHocID = l.LopHocID
                WHERE hs.HoTen ILIKE @kw OR hs.Email ILIKE @kw OR hs.SoDienThoai ILIKE @kw
                ORDER BY hs.HocSinhID ASC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@kw", "%" + tuKhoa + "%") });
        }
    }
}
