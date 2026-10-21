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
    public class DeThiDAL
    {
        public DataTable LayDeThiTheoLop(int lopID)
        {
            string sql = "SELECT * FROM DeThi WHERE LopHocID = @lopID";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopID) });
        }
        public DataTable LayDeThiTheoGiaoVien(int gvID)
        {
            string sql = "SELECT * FROM DeThi WHERE GiaoVienID = @gvID";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@gvID", gvID) });
        }
        public DeThi LayTheoID(int deThiID)
        {
            string sql = "SELECT * FROM DeThi WHERE DeThiID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", deThiID) });
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
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", deThiID) });
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
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@lopID", de.LopHocID),
                new NpgsqlParameter("@gvID", de.GiaoVienID),
                new NpgsqlParameter("@ten", de.TenDeThi),
                new NpgsqlParameter("@mon", de.MonHoc ?? ""),
                new NpgsqlParameter("@socau", de.SoCau),
                new NpgsqlParameter("@tg", de.ThoiGian),
                new NpgsqlParameter("@han", (object)de.HanNop ?? DBNull.Value),
                new NpgsqlParameter("@tt", de.TrangThai ?? "Đang mở")
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool ThemCauHoi(CauHoi ch)
        {
            string sql = @"INSERT INTO CauHoi (DeThiID, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, ThuTu)
                           VALUES (@deID, @nd, @a, @b, @c, @d, @dung, @thutu)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@deID", ch.DeThiID),
                new NpgsqlParameter("@nd", ch.NoiDung),
                new NpgsqlParameter("@a", ch.DapAnA),
                new NpgsqlParameter("@b", ch.DapAnB),
                new NpgsqlParameter("@c", ch.DapAnC),
                new NpgsqlParameter("@d", ch.DapAnD),
                new NpgsqlParameter("@dung", ch.DapAnDung),
                new NpgsqlParameter("@thutu", ch.ThuTu)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool XoaCauHoi(int cauHoiID)
        {
            // Không cho xóa nếu đã có học sinh làm bài với câu hỏi này (giữ nguyên lịch sử điểm)
            string checkSql = "SELECT COUNT(*) FROM ChiTietBaiLam WHERE CauHoiID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter("@id", cauHoiID)));
            if (count > 0)
            {
                throw new Exception("Không thể xóa câu hỏi này vì đã có học sinh làm bài với câu hỏi này!");
            }

            string sql = "DELETE FROM CauHoi WHERE CauHoiID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter("@id", cauHoiID)) > 0;
        }
        public bool SuaCauHoi(CauHoi ch)
        {
            string sql = @"UPDATE CauHoi
                           SET NoiDung = @nd, DapAnA = @a, DapAnB = @b, DapAnC = @c, DapAnD = @d, DapAnDung = @dung
                           WHERE CauHoiID = @id";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@nd", ch.NoiDung),
                new NpgsqlParameter("@a", ch.DapAnA),
                new NpgsqlParameter("@b", ch.DapAnB),
                new NpgsqlParameter("@c", ch.DapAnC),
                new NpgsqlParameter("@d", ch.DapAnD),
                new NpgsqlParameter("@dung", ch.DapAnDung),
                new NpgsqlParameter("@id", ch.CauHoiID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public DataTable LayKetQuaTheoDeThi(int maDeThi)
        {
            string sql = @"SELECT hs.HoTen, d.TenDeThi, k.SoCauDung, k.Diem, k.ThoiGianNop
                           FROM KetQuaThi k
                           JOIN HocSinh hs ON hs.HocSinhID = k.HocSinhID
                           JOIN DeThi d ON d.DeThiID = k.DeThiID
                           WHERE k.DeThiID = @id
                           ORDER BY k.ThoiGianNop DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter("@id", maDeThi));
        }
    }
}
