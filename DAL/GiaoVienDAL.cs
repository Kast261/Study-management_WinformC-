using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class GiaoVienDAL
    {
        public DataTable LayTatCa()
        {
            string sql = "SELECT GiaoVienID, HoTen, NgaySinh, GioiTinh, ChuyenMon, Email, SoDienThoai, DiaChi FROM GiaoVien";
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
            SqlParameter[] pars = {
                new SqlParameter("@hoten", gv.HoTen),
                new SqlParameter("@ngaysinh", (object)gv.NgaySinh ?? DBNull.Value),
                new SqlParameter("@gioitinh", (object)gv.GioiTinh ?? DBNull.Value),
                new SqlParameter("@chuyenmon", (object)gv.ChuyenMon ?? DBNull.Value),
                new SqlParameter("@email", (object)gv.Email ?? DBNull.Value),
                new SqlParameter("@sdt", (object)gv.SoDienThoai ?? DBNull.Value),
                new SqlParameter("@diachi", (object)gv.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool Sua(GiaoVien gv)
        {
            string sql = @"UPDATE GiaoVien SET HoTen=@hoten, NgaySinh=@ngaysinh, GioiTinh=@gioitinh, ChuyenMon=@chuyenmon, Email=@email, 
                           SoDienThoai=@sdt, DiaChi=@diachi WHERE GiaoVienID=@id";
            SqlParameter[] pars = {
                new SqlParameter("@hoten", gv.HoTen),
                new SqlParameter("@ngaysinh", (object)gv.NgaySinh ?? DBNull.Value),
                new SqlParameter("@gioitinh", (object)gv.GioiTinh ?? DBNull.Value),
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
            string sql = @"SELECT GiaoVienID, HoTen, NgaySinh, GioiTinh, ChuyenMon, Email, SoDienThoai, DiaChi 
                           FROM GiaoVien WHERE HoTen LIKE @kw OR Email LIKE @kw OR SoDienThoai LIKE @kw OR ChuyenMon LIKE @kw";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@kw", "%" + tuKhoa + "%") });
        }
    }
}
