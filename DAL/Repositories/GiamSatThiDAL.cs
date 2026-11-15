using System;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Npgsql;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class GiamSatThiDAL
    {
        internal static bool CoBang(NpgsqlConnection conn, NpgsqlTransaction transaction = null)
        {
            using (var cmd = new NpgsqlCommand("SELECT to_regclass('study_giam_sat_thi') IS NOT NULL", conn, transaction))
                return Convert.ToBoolean(cmd.ExecuteScalar());
        }

        internal static int DocSoLan(NpgsqlConnection conn, NpgsqlTransaction transaction, int hocSinhID, int deThiID)
        {
            if (!CoBang(conn, transaction)) return 0; // Kết quả cũ trước khi bật giám sát.
            using (var cmd = new NpgsqlCommand("SELECT SoLan FROM study_giam_sat_thi WHERE HocSinhID=@hs AND DeThiID=@de", conn, transaction))
            {
                cmd.Parameters.AddWithValue("hs", hocSinhID);cmd.Parameters.AddWithValue("de", deThiID);
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }

        public int BatDau(int taiKhoanID, int hocSinhID, int deThiID)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    // Chỉ cần quyền CREATE lần đầu; có thể cài script 03 bằng tài khoản quản trị.
                    if (!CoBang(conn, transaction))
                    {
                        using (var gate = new NpgsqlCommand("SELECT pg_advisory_xact_lock(hashtext(current_schema()),hashtext('study_exam_focus_schema'))", conn, transaction))
                            gate.ExecuteNonQuery();
                        if (!CoBang(conn, transaction))
                            using (var create = new NpgsqlCommand(@"CREATE TABLE IF NOT EXISTS study_giam_sat_thi (
    HocSinhID integer NOT NULL,
    DeThiID integer NOT NULL,
    SoLan integer NOT NULL DEFAULT 0 CHECK (SoLan BETWEEN 0 AND 3),
    LanCuoi timestamp with time zone,
    PRIMARY KEY (HocSinhID, DeThiID)
);", conn, transaction)) create.ExecuteNonQuery();
                    }
                    KhoaQuyenNopBai(conn, transaction, taiKhoanID, hocSinhID, deThiID);
                    using (var insert = new NpgsqlCommand(@"
                        INSERT INTO study_giam_sat_thi (HocSinhID,DeThiID) VALUES (@hs,@de)
                        ON CONFLICT (HocSinhID,DeThiID) DO NOTHING", conn, transaction))
                    {
                        insert.Parameters.AddWithValue("hs", hocSinhID);insert.Parameters.AddWithValue("de", deThiID);
                        insert.ExecuteNonQuery();
                    }
                    int count = DocSoLan(conn, transaction, hocSinhID, deThiID);
                    transaction.Commit();return count;
                }
            }
        }

        public int GhiNhan(int taiKhoanID, int hocSinhID, int deThiID, int soLan)
        {
            if (soLan < 1 || soLan > 3) throw new ArgumentOutOfRangeException(nameof(soLan));
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    KhoaQuyenNopBai(conn, transaction, taiKhoanID, hocSinhID, deThiID);
                    using (var update = new NpgsqlCommand(@"
                        UPDATE study_giam_sat_thi SET
                            LanCuoi=CASE WHEN SoLan<@count THEN CURRENT_TIMESTAMP ELSE LanCuoi END,
                            SoLan=GREATEST(SoLan,@count)
                        WHERE HocSinhID=@hs AND DeThiID=@de RETURNING SoLan", conn, transaction))
                    {
                        update.Parameters.AddWithValue("count", soLan);update.Parameters.AddWithValue("hs", hocSinhID);
                        update.Parameters.AddWithValue("de", deThiID);
                        object value = update.ExecuteScalar();
                        if (value == null) throw new InvalidOperationException("Chưa khởi tạo giám sát bài thi.");
                        int count = Convert.ToInt32(value);
                        transaction.Commit();return count;
                    }
                }
            }
        }

        internal static void KhoaQuyenNopBai(NpgsqlConnection conn, NpgsqlTransaction transaction,
            int taiKhoanID, int hocSinhID, int deThiID)
        {
            // Chung khóa đề với nộp bài, ngăn báo cáo muộn làm thay đổi kết quả đã chấm.
            using (var check = new NpgsqlCommand(@"
                SELECT d.DeThiID FROM DeThi d JOIN HocSinh hs ON hs.HocSinhID=@hs
                JOIN TaiKhoan tk ON tk.TaiKhoanID=@tk
                WHERE NOT COALESCE((to_jsonb(hs)->>'daxoa')::boolean,FALSE) AND d.DeThiID=@de AND d.TrangThai='Đang mở'
                  AND LOWER(BTRIM(tk.VaiTro)) IN ('hocsinh','hocvien')
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

        }
    }
}
