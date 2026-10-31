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
    public class KetQuaDAL
    {
        public bool KiemTraDaThi(int hocSinhID, int deThiID)
        {
            string sql = "SELECT COUNT(*) FROM KetQuaThi WHERE HocSinhID = @hsID AND DeThiID = @deID";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hsID", hocSinhID),
                new NpgsqlParameter("@deID", deThiID)
            };
            return Convert.ToInt32(DatabaseHelper.ExecuteScalar(sql, pars)) > 0;
        }
        public int LuuKetQua(KetQuaThi kq)
        {
            string sql = @"INSERT INTO KetQuaThi (HocSinhID, DeThiID, SoCauDung, Diem, ThoiGianNop, TrangThai) 
                           VALUES (@hsID, @deID, @dung, @diem, CURRENT_TIMESTAMP, @tt)
                           RETURNING KetQuaID;";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@hsID", kq.HocSinhID),
                new NpgsqlParameter("@deID", kq.DeThiID),
                new NpgsqlParameter("@dung", kq.SoCauDung),
                new NpgsqlParameter("@diem", kq.Diem),
                new NpgsqlParameter("@tt", kq.TrangThai ?? "Hoàn thành")
            };
            object obj = DatabaseHelper.ExecuteScalar(sql, pars);
            return obj != null ? Convert.ToInt32(obj) : 0;
        }
        public void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung)
        {
            string sql = @"INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) 
                           VALUES (@kqID, @chID, @da, @dung)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@kqID", ketQuaID),
                new NpgsqlParameter("@chID", cauHoiID),
                new NpgsqlParameter("@da", dapAnChon),
                new NpgsqlParameter("@dung", laDung)
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
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hocSinhID) });
        }

        public DataTable LayChiTietBaiLamTheoKetQua(int ketQuaID)
        {
            string sql = @"SELECT ct.ChiTietID, c.ThuTu, c.NoiDung AS CauHoi, 
                                  c.DapAnA, c.DapAnB, c.DapAnC, c.DapAnD, 
                                  c.DapAnDung, ct.DapAnChon, ct.LaDung
                           FROM ChiTietBaiLam ct
                           JOIN CauHoi c ON ct.CauHoiID = c.CauHoiID
                           WHERE ct.KetQuaID = @kqID
                           ORDER BY c.ThuTu ASC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@kqID", ketQuaID) });
        }
    }
}
