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
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            int id = ProcedureExecutor.Id("CALL study_them_khoa_hoc(@ten, @mota, 0)",
                ProcedureExecutor.Text("ten", kh.TenKhoaHoc), ProcedureExecutor.Text("mota", kh.MoTa));
            if (id <= 0) return false;
            kh.KhoaHocID = id;
            return true;
        }

        public bool Sua(KhoaHoc kh)
        {
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            return ProcedureExecutor.Boolean("CALL study_sua_khoa_hoc(@id, @ten, @mota, FALSE)", "p_updated",
                new NpgsqlParameter("id", kh.KhoaHocID),
                ProcedureExecutor.Text("ten", kh.TenKhoaHoc), ProcedureExecutor.Text("mota", kh.MoTa));
        }

        public bool Xoa(int id)
        {
            return ProcedureExecutor.Boolean("CALL study_xoa_khoa_hoc(@id, FALSE)", "p_deleted",
                new NpgsqlParameter("id", id));
        }
    }
}
