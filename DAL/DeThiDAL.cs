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
