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
    public class GiaoVienDAL
    {
        public DataTable LayTatCa()
        {
            string sql = @"
                SELECT 
                    gv.GiaoVienID, 
                    gv.HoTen, 
                    gv.NgaySinh, 
                    gv.GioiTinh, 
                    gv.ChuyenMon, 
                    gv.Email, 
                    gv.SoDienThoai, 
                    gv.DiaChi,
                    (SELECT COUNT(*) FROM LopHoc l WHERE l.GiaoVienID = gv.GiaoVienID) AS SoLop
                FROM GiaoVien gv
                ORDER BY gv.GiaoVienID ASC";
            return DatabaseHelper.ExecuteQuery(sql);
        }
        public GiaoVien LayTheoID(int id)
        {
            string sql = @"SELECT GiaoVienID, HoTen, Email, SoDienThoai, DiaChi,
                                  NgaySinh, ChuyenMon, GioiTinh, TaiKhoanID
                           FROM GiaoVien WHERE GiaoVienID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new GiaoVien
            {
                GiaoVienID = Convert.ToInt32(r["GiaoVienID"]),
                TaiKhoanID = r.Table.Columns.Contains("TaiKhoanID") && r["TaiKhoanID"] != DBNull.Value ? Convert.ToInt32(r["TaiKhoanID"]) : 0,
                HoTen = r["HoTen"].ToString(),
                NgaySinh = r.Table.Columns.Contains("NgaySinh") && r["NgaySinh"] != DBNull.Value ? Convert.ToDateTime(r["NgaySinh"]) : (DateTime?)null,
                GioiTinh = r.Table.Columns.Contains("GioiTinh") ? r["GioiTinh"]?.ToString() : null,
                ChuyenMon = r["ChuyenMon"]?.ToString(),
                Email = r["Email"]?.ToString(),
                SoDienThoai = r["SoDienThoai"]?.ToString(),
                DiaChi = r["DiaChi"]?.ToString()
            };
        }
        public bool Them(GiaoVien gv)
        {
            if (gv == null) throw new ArgumentNullException(nameof(gv));
            int id = ProcedureExecutor.Id(
                "CALL study_them_giao_vien(@hoten, @ngaysinh, @gioitinh, @chuyenmon, @email, @sdt, @diachi, 0)",
                ProcedureExecutor.Text("hoten", gv.HoTen), ProcedureExecutor.Date("ngaysinh", gv.NgaySinh),
                ProcedureExecutor.Text("gioitinh", gv.GioiTinh), ProcedureExecutor.Text("chuyenmon", gv.ChuyenMon),
                ProcedureExecutor.Text("email", gv.Email), ProcedureExecutor.Text("sdt", gv.SoDienThoai),
                ProcedureExecutor.Text("diachi", gv.DiaChi));
            if (id <= 0) return false;
            gv.GiaoVienID = id;
            return true;
        }
        public bool Sua(GiaoVien gv)
        {
            if (gv == null) throw new ArgumentNullException(nameof(gv));
            return ProcedureExecutor.Boolean(
                "CALL study_sua_giao_vien(@id, @hoten, @ngaysinh, @gioitinh, @chuyenmon, @email, @sdt, @diachi, FALSE)",
                "p_updated", new NpgsqlParameter("id", gv.GiaoVienID),
                ProcedureExecutor.Text("hoten", gv.HoTen), ProcedureExecutor.Date("ngaysinh", gv.NgaySinh),
                ProcedureExecutor.Text("gioitinh", gv.GioiTinh), ProcedureExecutor.Text("chuyenmon", gv.ChuyenMon),
                ProcedureExecutor.Text("email", gv.Email), ProcedureExecutor.Text("sdt", gv.SoDienThoai),
                ProcedureExecutor.Text("diachi", gv.DiaChi));
        }

        public bool CapNhatHoSoCaNhan(int taiKhoanID, GiaoVien gv)
        {
            // Màn hình cá nhân không nhập NgaySinh/ChuyenMon: giữ nguyên hai trường đó.
            return DatabaseHelper.ExecuteNonQuery(@"
                UPDATE GiaoVien gv SET HoTen=@ten, Email=@email, SoDienThoai=@sdt,
                    DiaChi=@diachi, GioiTinh=@gioitinh
                WHERE gv.GiaoVienID=@gv AND EXISTS (
                    SELECT 1 FROM TaiKhoan tk WHERE tk.TaiKhoanID=@tk
                      AND LOWER(tk.VaiTro)='giaovien' AND COALESCE(tk.TrangThaiKhoa,FALSE)=FALSE
                      AND (tk.NguoiDungID=gv.GiaoVienID OR (COALESCE(tk.NguoiDungID,0)=0
                           AND gv.TaiKhoanID=tk.TaiKhoanID)))",
                new NpgsqlParameter("ten", gv.HoTen), new NpgsqlParameter("email", (object)gv.Email ?? DBNull.Value),
                new NpgsqlParameter("sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("diachi", (object)gv.DiaChi ?? DBNull.Value),
                new NpgsqlParameter("gioitinh", (object)gv.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("gv", gv.GiaoVienID), new NpgsqlParameter("tk", taiKhoanID)) == 1;
        }

        public bool Xoa(int id)
        {
            return ProcedureExecutor.Boolean("CALL study_xoa_giao_vien(@id, FALSE)", "p_deleted",
                new NpgsqlParameter("id", id));
        }
        public DataTable TimKiem(string tuKhoa)
        {
            string sql = @"
                SELECT 
                    gv.GiaoVienID, 
                    gv.HoTen, 
                    gv.NgaySinh, 
                    gv.GioiTinh, 
                    gv.ChuyenMon, 
                    gv.Email, 
                    gv.SoDienThoai, 
                    gv.DiaChi,
                    (SELECT COUNT(*) FROM LopHoc l WHERE l.GiaoVienID = gv.GiaoVienID) AS SoLop
                FROM GiaoVien gv
                WHERE gv.HoTen ILIKE @kw OR gv.Email ILIKE @kw OR gv.SoDienThoai ILIKE @kw OR gv.ChuyenMon ILIKE @kw
                ORDER BY gv.GiaoVienID ASC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@kw", "%" + tuKhoa + "%") });
        }
    }
}
