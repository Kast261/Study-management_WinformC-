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
    public class KhoaHocDAL
    {
        public DataTable LayTatCa()
        {
            string sql = "SELECT KhoaHocID, TenKhoaHoc, MoTa FROM KhoaHoc";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable LayKhoaHocTheoGiaoVien(int gvID)
        {
            string sql = @"
                SELECT DISTINCT k.KhoaHocID, k.TenKhoaHoc, k.MoTa 
                FROM KhoaHoc k
                WHERE k.KhoaHocID IN (SELECT l.KhoaHocID FROM LopHoc l WHERE l.GiaoVienID = @gvID AND l.KhoaHocID IS NOT NULL)
                   OR LOWER(k.TenKhoaHoc) = (SELECT LOWER(COALESCE(g.ChuyenMon, '')) FROM GiaoVien g WHERE g.GiaoVienID = @gvID)
                ORDER BY k.TenKhoaHoc";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@gvID", gvID) });
            if (dt == null || dt.Rows.Count == 0)
            {
                return LayTatCa();
            }
            return dt;
        }

        public KhoaHoc LayTheoID(int id)
        {
            string sql = "SELECT KhoaHocID, TenKhoaHoc, MoTa FROM KhoaHoc WHERE KhoaHocID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) });
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
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@ten", kh.TenKhoaHoc),
                new NpgsqlParameter("@mota", (object)kh.MoTa ?? DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool Sua(KhoaHoc kh)
        {
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            // Explicit CALL works with Npgsql 6; do not use its StoredProcedure
            // command mode, which invokes PostgreSQL functions in that version.
            try
            {
                using (DataTable result = DatabaseHelper.ExecuteQuery(
                    "CALL study_sua_khoa_hoc(@id, @ten, @mota, FALSE)",
                    new NpgsqlParameter("id", kh.KhoaHocID),
                    new NpgsqlParameter("ten", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)kh.TenKhoaHoc ?? DBNull.Value },
                    new NpgsqlParameter("mota", NpgsqlTypes.NpgsqlDbType.Varchar) { Value = (object)kh.MoTa ?? DBNull.Value }))
                {
                    return result.Rows.Count == 1 && Convert.ToBoolean(result.Rows[0]["p_updated"]);
                }
            }
            catch (PostgresException ex) when (ex.SqlState == "42883")
            {
                throw new InvalidOperationException(
                    "Chưa cài thủ tục PostgreSQL. Chạy Database/PostgreSQL/02_Procedures.sql trên database ứng dụng.", ex);
            }
        }

        public bool Xoa(int id)
        {
            // Kiểm tra xem có lớp học nào đang thuộc khóa học này không
            string checkSql = "SELECT COUNT(*) FROM LopHoc WHERE KhoaHocID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) }));
            if (count > 0)
            {
                throw new Exception("Không thể xóa khóa học này vì đang có lớp học thuộc khóa học!");
            }

            string sql = "DELETE FROM KhoaHoc WHERE KhoaHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", id) }) > 0;
        }
    }
}
