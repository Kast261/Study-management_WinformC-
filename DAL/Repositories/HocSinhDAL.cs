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
        public bool Them(HocSinh hs)
        {
            string sql = @"INSERT INTO HocSinh (HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi) 
                           VALUES (@hoten, @ngaysinh, @gioitinh, @email, @sdt, @diachi)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hoten", hs.HoTen),
                new NpgsqlParameter("@ngaysinh", (object)hs.NgaySinh ?? DBNull.Value),
                new NpgsqlParameter("@gioitinh", (object)hs.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("@email", (object)hs.Email ?? DBNull.Value),
                new NpgsqlParameter("@sdt", (object)hs.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("@diachi", (object)hs.DiaChi ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool Sua(HocSinh hs)
        {
            string sql = @"UPDATE HocSinh SET HoTen=@hoten, NgaySinh=@ngaysinh, GioiTinh=@gioitinh, 
                           Email=@email, SoDienThoai=@sdt, DiaChi=@diachi WHERE HocSinhID=@id";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hoten", hs.HoTen),
                new NpgsqlParameter("@ngaysinh", (object)hs.NgaySinh ?? DBNull.Value),
                new NpgsqlParameter("@gioitinh", (object)hs.GioiTinh ?? DBNull.Value),
                new NpgsqlParameter("@email", (object)hs.Email ?? DBNull.Value),
                new NpgsqlParameter("@sdt", (object)hs.SoDienThoai ?? DBNull.Value),
                new NpgsqlParameter("@diachi", (object)hs.DiaChi ?? DBNull.Value),
                new NpgsqlParameter("@id", hs.HocSinhID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool Xoa(int id)
        {
            string sql = "DELETE FROM HocSinh WHERE HocSinhID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) }) > 0;
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
