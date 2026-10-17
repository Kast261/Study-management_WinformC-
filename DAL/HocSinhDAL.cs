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
                TaiKhoanID = r.Table.Columns.Contains("TaiKhoanID") && r["TaiKhoanID"] != DBNull.Value ? Convert.ToInt32(r["TaiKhoanID"]) : 0,
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
