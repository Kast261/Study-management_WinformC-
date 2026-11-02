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
                    // Khóa đề để hai lần nộp đồng thời không cùng vượt qua kiểm tra đã thi.
                    using (var check = new NpgsqlCommand(@"
                        SELECT d.DeThiID FROM DeThi d JOIN HocSinh hs ON hs.HocSinhID=@hs
                        JOIN TaiKhoan tk ON tk.TaiKhoanID=@tk
                        WHERE d.DeThiID=@de AND d.TrangThai='Đang mở'
                          AND LOWER(tk.VaiTro) IN ('hocsinh','hocvien')
                          AND COALESCE(tk.TrangThaiKhoa,FALSE)=FALSE
                          AND (tk.NguoiDungID=hs.HocSinhID OR (COALESCE(tk.NguoiDungID,0)=0 AND hs.TaiKhoanID=tk.TaiKhoanID))
                          AND (hs.LopHocID=d.LopHocID OR EXISTS (
                              SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=@hs
                                AND dk.LopHocID=d.LopHocID AND dk.TrangThai='Đã duyệt'))
                        FOR UPDATE OF d FOR SHARE OF tk", conn, transaction))
                    {
                        check.Parameters.AddWithValue("hs", hocSinhID);
                        check.Parameters.AddWithValue("tk", taiKhoanID);
                        check.Parameters.AddWithValue("de", deThiID);
                        if (check.ExecuteScalar() == null)
                            throw new InvalidOperationException("Tài khoản không có quyền nộp bài cho đề thi này.");
                    }
                    using (var check = new NpgsqlCommand("SELECT 1 FROM KetQuaThi WHERE HocSinhID=@hs AND DeThiID=@de", conn, transaction))
                    {
                        check.Parameters.AddWithValue("hs", hocSinhID);
                        check.Parameters.AddWithValue("de", deThiID);
                        if (check.ExecuteScalar() != null)
                            throw new InvalidOperationException("Bài thi đã được nộp trước đó. Vui lòng xem lịch sử thi.");
                    }
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
                        SoCauDung = answers.Count(a => a.Item3), TrangThai = "Hoàn thành"
                    };
                    result.Diem = Math.Round(result.SoCauDung * 10M / answers.Count, 2);
                    using (var insert = new NpgsqlCommand(@"
                        INSERT INTO KetQuaThi (HocSinhID,DeThiID,SoCauDung,Diem,ThoiGianNop,TrangThai)
                        VALUES (@hs,@de,@dung,@diem,CURRENT_TIMESTAMP,'Hoàn thành') RETURNING KetQuaID", conn, transaction))
                    {
                        insert.Parameters.AddWithValue("hs", hocSinhID); insert.Parameters.AddWithValue("de", deThiID);
                        insert.Parameters.AddWithValue("dung", result.SoCauDung); insert.Parameters.AddWithValue("diem", result.Diem);
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
