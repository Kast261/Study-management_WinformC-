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
            string sql = @"INSERT INTO GiaoVien (HoTen, NgaySinh, GioiTinh, ChuyenMon, Email, SoDienThoai, DiaChi) 
                           VALUES (@hoten, @ngaysinh, @gioitinh, @chuyenmon, @email, @sdt, @diachi)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hoten", gv.HoTen),
                new NpgsqlParameter("@ngaysinh", (object)gv.NgaySinh ?? DBNull.Value),
                new NpgsqlParameter("@gioitinh", (object)gv.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("@chuyenmon", (object)gv.ChuyenMon ?? DBNull.Value),
                new NpgsqlParameter("@email", (object)gv.Email ?? DBNull.Value),
                new NpgsqlParameter("@sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("@diachi", (object)gv.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool Sua(GiaoVien gv)
        {
            string sql = @"UPDATE GiaoVien SET HoTen=@hoten, NgaySinh=@ngaysinh, GioiTinh=@gioitinh, ChuyenMon=@chuyenmon, Email=@email, 
                           SoDienThoai=@sdt, DiaChi=@diachi WHERE GiaoVienID=@id";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hoten", gv.HoTen),
                new NpgsqlParameter("@ngaysinh", (object)gv.NgaySinh ?? DBNull.Value),
                new NpgsqlParameter("@gioitinh", (object)gv.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("@chuyenmon", (object)gv.ChuyenMon ?? DBNull.Value),
                new NpgsqlParameter("@email", (object)gv.Email ?? DBNull.Value),
                new NpgsqlParameter("@sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("@diachi", (object)gv.DiaChi ?? DBNull.Value),
                new NpgsqlParameter("@id", gv.GiaoVienID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Xoa(int id)
        {
            string sql = "DELETE FROM GiaoVien WHERE GiaoVienID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) }) > 0;
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
