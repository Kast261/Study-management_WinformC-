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
        public bool ThemCauHoi(CauHoiDTO ch)
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
        public bool XoaCauHoi(int maCauHoi) // Thay đổi kiểu dữ liệu/tham số cho phù hợp với logic của bạn
        {
            // Code thực thi việc xóa câu hỏi trong Database
            return true;
        }
        public bool SuaCauHoi(CauHoiDTO cauHoi) // Truyền tham số tương ứng
        {
            // Code thực thi việc sửa câu hỏi trong Database
            return true;
        }
        public DataTable LayKetQuaTheoDeThi(int maDeThi) // Hoặc kiểu trả về List<KetQua> tùy bạn
        {
            // Code thực thi việc lấy dữ liệu
            return new DataTable();
        }
    }
}
