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
    public class KetQuaDAL
    {
        public bool KiemTraDaThi(int hocSinhID, int deThiID)
        {
            string sql = "SELECT COUNT(*) FROM KetQuaThi WHERE HocSinhID = @hsID AND DeThiID = @deID";
            SqlParameter[] pars = {
                new SqlParameter("@hsID", hocSinhID),
                new SqlParameter("@deID", deThiID)
            };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, pars)) > 0;
        }
        public int LuuKetQua(KetQuaThi kq)
        {
            string sql = @"INSERT INTO KetQuaThi (HocSinhID, DeThiID, SoCauDung, Diem, ThoiGianNop, TrangThai) 
                           VALUES (@hsID, @deID, @dung, @diem, GETDATE(), @tt);
                           SELECT SCOPE_IDENTITY();";
            SqlParameter[] pars = {
                new SqlParameter("@hsID", kq.HocSinhID),
                new SqlParameter("@deID", kq.DeThiID),
                new SqlParameter("@dung", kq.SoCauDung),
                new SqlParameter("@diem", kq.Diem),
                new SqlParameter("@tt", kq.TrangThai ?? "Hoàn thành")
            };
            object obj = DatabaseHelper.ExecuteScalar(sql, pars);
            return obj != null ? Convert.ToInt32(obj) : 0;
        }
        public void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung)
        {
            string sql = @"INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) 
                           VALUES (@kqID, @chID, @da, @dung)";
            SqlParameter[] pars = {
                new SqlParameter("@kqID", ketQuaID),
                new SqlParameter("@chID", cauHoiID),
                new SqlParameter("@da", dapAnChon),
                new SqlParameter("@dung", laDung)
            };
            DatabaseHelper.ExecuteNonQuery(sql, pars);
        }
        public DataTable LayLichSuTheoHocSinh(int hocSinhID)
        {
            string sql = @"SELECT k.KetQuaID, d.TenDeThi, d.MonHoc, k.SoCauDung, k.Diem, k.ThoiGianNop 
                           FROM KetQuaThi k
                           JOIN DeThi d ON k.DeThiID = d.DeThiID
                           WHERE k.HocSinhID = @hsID
                           ORDER BY k.ThoiGianNop DESC";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@hsID", hocSinhID) });
        }
    }
}
