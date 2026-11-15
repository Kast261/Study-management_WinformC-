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
        public KetQuaThi NopBai(int taiKhoanID, int hocSinhID, int deThiID, IDictionary<int, string> dapAn)
        {
            if (dapAn == null) throw new ArgumentNullException(nameof(dapAn));
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    GiamSatThiDAL.KhoaQuyenNopBai(conn, transaction, taiKhoanID, hocSinhID, deThiID);
                    int soLanRoiManHinh = GiamSatThiDAL.DocSoLan(conn, transaction, hocSinhID, deThiID);
                    var answers = new List<Tuple<int, string, bool>>();
                    using (var questions = new NpgsqlCommand("SELECT CauHoiID, DapAnDung FROM CauHoi WHERE DeThiID=@de ORDER BY ThuTu, CauHoiID", conn, transaction))
                    {
                        questions.Parameters.AddWithValue("de", deThiID);
                        using (var reader = questions.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                int id = reader.GetInt32(0);
                                string chosen;
                                if (!dapAn.TryGetValue(id, out chosen)) chosen = "";
                                chosen = (chosen ?? "").Trim().ToUpperInvariant();
                                if (chosen.Length != 1 || "ABCD".IndexOf(chosen, StringComparison.Ordinal) < 0) chosen = "";
                                bool correct = chosen.Length > 0 && string.Equals(chosen, reader.GetString(1).Trim(), StringComparison.OrdinalIgnoreCase);
                                answers.Add(Tuple.Create(id, chosen, correct));
                            }
                        }
                    }
                    if (answers.Count == 0) throw new InvalidOperationException("Đề thi chưa có câu hỏi.");
                    var result = new KetQuaThi
                    {
                        HocSinhID = hocSinhID, DeThiID = deThiID,
                        SoCauDung = answers.Count(a => a.Item3),
                        SoLanRoiManHinh = soLanRoiManHinh,
                        TrangThai = soLanRoiManHinh >= 3 ? "Dừng do vi phạm" : "Hoàn thành"
                    };
                    result.Diem = Math.Round(result.SoCauDung * 10M / answers.Count, 2);
                    using (var insert = new NpgsqlCommand(@"
                        INSERT INTO KetQuaThi (HocSinhID,DeThiID,SoCauDung,Diem,ThoiGianNop,TrangThai)
                        VALUES (@hs,@de,@dung,@diem,CURRENT_TIMESTAMP,@tt) RETURNING KetQuaID", conn, transaction))
                    {
                        insert.Parameters.AddWithValue("hs", hocSinhID); insert.Parameters.AddWithValue("de", deThiID);
                        insert.Parameters.AddWithValue("dung", result.SoCauDung); insert.Parameters.AddWithValue("diem", result.Diem);
                        insert.Parameters.AddWithValue("tt", result.TrangThai);
                        result.KetQuaID = Convert.ToInt32(insert.ExecuteScalar());
                    }
                    foreach (var answer in answers)
                    {
                        using (var insert = new NpgsqlCommand(@"
                            INSERT INTO ChiTietBaiLam (KetQuaID,CauHoiID,DapAnChon,LaDung)
                            VALUES (@kq,@cau,@chon,@dung)", conn, transaction))
                        {
                            insert.Parameters.AddWithValue("kq", result.KetQuaID); insert.Parameters.AddWithValue("cau", answer.Item1);
                            insert.Parameters.AddWithValue("chon", answer.Item2); insert.Parameters.AddWithValue("dung", answer.Item3);
                            insert.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    return result;
                }
            }
        }

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

        public DataTable LayKetQuaTheoLopDangHoc(int hocSinhID, int lopHocID = 0)
        {
            // Kiểm tra lớp đang học ngay trong truy vấn, không tin riêng giá trị ComboBox.
            return DatabaseHelper.ExecuteQuery(@"
                SELECT k.KetQuaID, d.DeThiID, d.LopHocID, l.TenLop,
                       d.TenDeThi, d.MonHoc, k.SoCauDung, k.Diem, k.ThoiGianNop
                FROM KetQuaThi k
                JOIN DeThi d ON d.DeThiID=k.DeThiID
                JOIN LopHoc l ON l.LopHocID=d.LopHocID
                WHERE k.HocSinhID=@hs AND (@lop=0 OR l.LopHocID=@lop)
                  AND EXISTS (SELECT 1 FROM HocSinh hs WHERE hs.HocSinhID=@hs
                      AND (hs.LopHocID=l.LopHocID OR EXISTS (
                          SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=hs.HocSinhID
                            AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')))
                ORDER BY k.ThoiGianNop DESC, k.KetQuaID DESC",
                new NpgsqlParameter("hs", hocSinhID), new NpgsqlParameter("lop", lopHocID));
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
