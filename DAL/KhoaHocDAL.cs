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
    public class KhoaHocDAL
    {
        public DataTable LayTatCa()
        {
            string sql = "SELECT KhoaHocID, TenKhoaHoc, MoTa FROM KhoaHoc";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public KhoaHoc LayTheoID(int id)
        {
            string sql = "SELECT * FROM KhoaHoc WHERE KhoaHocID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) });
            if (dt.Rows.Count == 0) return null;
            DataRow r = dt.Rows[0];
            return new KhoaHoc
            {
                KhoaHocID = Convert.ToInt32(r["KhoaHocID"]),
                TenKhoaHoc = r["TenKhoaHoc"].ToString(),
                MoTa = r["MoTa"]?.ToString()
            };
        }

        public bool Them(KhoaHoc kh)
        {
            string sql = "INSERT INTO KhoaHoc (TenKhoaHoc, MoTa) VALUES (@ten, @mota)";
            SqlParameter[] pars = {
                new SqlParameter("@ten", kh.TenKhoaHoc),
                new SqlParameter("@mota", (object)kh.MoTa ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Sua(KhoaHoc kh)
        {
            string sql = "UPDATE KhoaHoc SET TenKhoaHoc = @ten, MoTa = @mota WHERE KhoaHocID = @id";
            SqlParameter[] pars = {
                new SqlParameter("@ten", kh.TenKhoaHoc),
                new SqlParameter("@mota", (object)kh.MoTa ?? DBNull.Value),
                new SqlParameter("@id", kh.KhoaHocID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Xoa(int id)
        {
            // Kiểm tra xem có lớp học nào đang thuộc khóa học này không
            string checkSql = "SELECT COUNT(*) FROM LopHoc WHERE KhoaHocID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new SqlParameter[] { new SqlParameter("@id", id) }));
            if (count > 0)
            {
                throw new Exception("Không thể xóa khóa học này vì đang có lớp học thuộc khóa học!");
            }

            string sql = "DELETE FROM KhoaHoc WHERE KhoaHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", id) }) > 0;
        }
    }
}
