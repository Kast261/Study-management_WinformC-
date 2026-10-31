using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class DeThiDAL
    {
        // 1. Hàm mới thêm: Lấy danh sách đề thi thuộc các lớp học viên đã tham gia / đã duyệt
        public DataTable LayDeThiChoHocSinh(int hocSinhID)
        {
            string sql = @"
        SELECT 
            d.DeThiID AS ""DeThiID"", 
            d.TenDeThi AS ""TenDeThi"", 
            d.MonHoc AS ""MonHoc"", 
            COALESCE((SELECT COUNT(*) FROM CauHoi c WHERE c.DeThiID = d.DeThiID), d.SoCau, 0) AS ""SoCau"", 
            d.ThoiGian AS ""ThoiGian"", 
            d.HanNop AS ""HanNop"", 
            l.TenLop AS ""TenLop"",
            CASE 
                WHEN kq.KetQuaID IS NOT NULL THEN 'Đã làm'
                WHEN d.HanNop IS NOT NULL AND d.HanNop < CURRENT_TIMESTAMP THEN 'Hết hạn'
                ELSE 'Chưa làm'
            END AS ""TrangThai"",
            kq.Diem AS ""Diem""
        FROM DeThi d
        JOIN LopHoc l ON d.LopHocID = l.LopHocID
        LEFT JOIN KetQuaThi kq ON d.DeThiID = kq.DeThiID AND kq.HocSinhID = @hsID
        WHERE l.LopHocID IN (
            SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID AND LopHocID IS NOT NULL
            UNION
            SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND TrangThai = 'Đã duyệt'
        )
        ORDER BY 
            CASE 
                WHEN kq.KetQuaID IS NOT NULL THEN 1 -- Đã làm xếp cuối
                WHEN d.HanNop IS NOT NULL AND d.HanNop < CURRENT_TIMESTAMP THEN 2 -- Hết hạn xếp thứ 2 từ dưới lên
                ELSE 0 -- Chưa làm xếp lên đầu
            END ASC,
            d.HanNop ASC";

            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hocSinhID) });
        }

        public DataTable LayDeThiTheoLop(int lopID)
        {
            string sql = @"SELECT DeThiID, TenDeThi, MoTa, ThoiGian, MonHoc, SoCau,
                                  GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai
                           FROM DeThi WHERE LopHocID = @lopID";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopID) });
        }

        public DataTable LayDeThiTheoGiaoVien(int gvID)
        {
            string sql = @"SELECT d.DeThiID AS ""DeThiID"", 
                                  d.TenDeThi AS ""TenDeThi"", 
                                  d.MoTa AS ""MoTa"", 
                                  d.ThoiGian AS ""ThoiGian"", 
                                  d.MonHoc AS ""MonHoc"",
                                  COALESCE((SELECT COUNT(*) FROM CauHoi c WHERE c.DeThiID = d.DeThiID), d.SoCau, 0) AS ""SoCau"",
                                  d.GiaoVienID AS ""GiaoVienID"", 
                                  d.LopHocID AS ""LopHocID"", 
                                  d.NgayTao AS ""NgayTao"", 
                                  d.HanNop AS ""HanNop"", 
                                  d.TrangThai AS ""TrangThai"",
                                  l.TenLop AS ""TenLop""
                           FROM DeThi d
                           LEFT JOIN LopHoc l ON l.LopHocID = d.LopHocID
                           WHERE d.GiaoVienID = @gvID
                           ORDER BY d.DeThiID DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@gvID", gvID) });
        }

        public DeThi LayTheoID(int deThiID)
        {
            string sql = @"SELECT DeThiID, TenDeThi, MoTa, ThoiGian, MonHoc, SoCau,
                                  GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai
                           FROM DeThi WHERE DeThiID = @id";
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
            string sql = @"SELECT CauHoiID, DeThiID, NoiDung, DapAnA, DapAnB,
                                  DapAnC, DapAnD, DapAnDung, ThuTu
                           FROM CauHoi WHERE DeThiID = @id ORDER BY ThuTu ASC";
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

        /// <summary>
        /// Tạo đề thi mới (không giao cho lớp ngay) và trả về DeThiID vừa tạo.
        /// LopHocID được truyền vào 0 hoặc NULL để đánh dấu chưa giao.
        /// </summary>
        public int TaoDeThiMoiLayID(DeThi de)
        {
            string sql = @"INSERT INTO DeThi (GiaoVienID, TenDeThi, MonHoc, SoCau, ThoiGian, TrangThai)
                           VALUES (@gvID, @ten, @mon, @socau, @tg, @tt)
                           RETURNING DeThiID";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@gvID", de.GiaoVienID),
                new NpgsqlParameter("@ten", de.TenDeThi),
                new NpgsqlParameter("@mon", de.MonHoc ?? ""),
                new NpgsqlParameter("@socau", de.SoCau > 0 ? de.SoCau : 0),
                new NpgsqlParameter("@tg", de.ThoiGian > 0 ? de.ThoiGian : 45),
                new NpgsqlParameter("@tt", de.TrangThai ?? "Chưa giao")
            };
            object result = DatabaseHelper.ExecuteScalar(sql, pars);
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : -1;
        }

        /// <summary>
        /// Giao đề thi đã có cho lớp học (cập nhật LopHocID, ThoiGian, HanNop, TrangThai, MonHoc).
        /// </summary>
        public bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            string sql = @"UPDATE DeThi
                           SET LopHocID = @lopID,
                               ThoiGian  = @tg,
                               HanNop    = @han,
                               SoCau     = (SELECT COUNT(*) FROM CauHoi WHERE DeThiID = @id),
                               TrangThai = 'Đang mở',
                               MonHoc    = COALESCE(NULLIF(@mon, ''), MonHoc)
                           WHERE DeThiID = @id";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@lopID", lopHocID),
                new NpgsqlParameter("@tg",    thoiGian),
                new NpgsqlParameter("@han",   (object)hanNop ?? DBNull.Value),
                new NpgsqlParameter("@mon",   (object)monHoc ?? DBNull.Value),
                new NpgsqlParameter("@id",    deThiID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        /// <summary>
        /// Kiểm tra xem đề thi đã được giao bài cho lớp học hay chưa (hoặc đã có học sinh làm bài).
        /// Trả về true nếu đề thi CHƯA GIAO bài và có thể xóa an toàn.
        /// </summary>
        public bool KiemTraDeThiChuaGiao(int deThiID)
        {
            string sql = @"
                SELECT 
                    COALESCE(d.LopHocID, 0) AS LopHocID,
                    d.TrangThai,
                    (SELECT COUNT(*) FROM KetQuaThi k WHERE k.DeThiID = d.DeThiID) AS SoKetQua
                FROM DeThi d
                WHERE d.DeThiID = @id";
            DataTable dt = DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter("@id", deThiID));
            if (dt == null || dt.Rows.Count == 0) return false;

            DataRow r = dt.Rows[0];
            int lopHocID = Convert.ToInt32(r["LopHocID"]);
            string trangThai = r["TrangThai"]?.ToString() ?? "";
            int soKetQua = Convert.ToInt32(r["SoKetQua"]);

            // Đề thi chưa giao: chưa gán lớp (LopHocID <= 0), chưa có kết quả thi và trạng thái không phải Đang mở/Đã giao
            bool daGiao = lopHocID > 0 || soKetQua > 0;
            if (!string.IsNullOrEmpty(trangThai) && !trangThai.Equals("Chưa giao", StringComparison.OrdinalIgnoreCase))
            {
                daGiao = true;
            }
            return !daGiao;
        }

        /// <summary>
        /// Xóa đề thi mà chưa giao bài thi.
        /// Nếu đề thi đã được giao cho lớp hoặc đã có kết quả thi thì chặn xóa.
        /// </summary>
        public bool XoaDeThi(int deThiID, out string thongBao)
        {
            try
            {
                string checkSql = @"
                    SELECT 
                        d.TenDeThi,
                        COALESCE(d.LopHocID, 0) AS LopHocID,
                        d.TrangThai,
                        (SELECT COUNT(*) FROM KetQuaThi k WHERE k.DeThiID = d.DeThiID) AS SoKetQua
                    FROM DeThi d
                    WHERE d.DeThiID = @id";

                DataTable dt = DatabaseHelper.ExecuteQuery(checkSql, new NpgsqlParameter("@id", deThiID));
                if (dt == null || dt.Rows.Count == 0)
                {
                    thongBao = "Đề thi không tồn tại hoặc đã bị xóa trước đó!";
                    return false;
                }

                DataRow r = dt.Rows[0];
                string tenDeThi = r["TenDeThi"]?.ToString() ?? "";
                int lopHocID = Convert.ToInt32(r["LopHocID"]);
                string trangThai = r["TrangThai"]?.ToString() ?? "";
                int soKetQua = Convert.ToInt32(r["SoKetQua"]);

                // Nếu đã giao cho lớp hoặc đã có học sinh làm bài thì KHÔNG cho xóa
                if (lopHocID > 0 || soKetQua > 0 || (!string.IsNullOrEmpty(trangThai) && !trangThai.Equals("Chưa giao", StringComparison.OrdinalIgnoreCase)))
                {
                    thongBao = $"Đề thi \"{tenDeThi}\" đã được giao bài cho lớp học hoặc đã có học sinh làm bài! Không thể xóa đề thi này.";
                    return false;
                }

                // 1. Xóa các câu hỏi trong đề thi
                string sqlXoaCauHoi = "DELETE FROM CauHoi WHERE DeThiID = @id";
                DatabaseHelper.ExecuteNonQuery(sqlXoaCauHoi, new NpgsqlParameter("@id", deThiID));

                // 2. Xóa đề thi
                string sqlXoaDeThi = "DELETE FROM DeThi WHERE DeThiID = @id";
                int rows = DatabaseHelper.ExecuteNonQuery(sqlXoaDeThi, new NpgsqlParameter("@id", deThiID));

                if (rows > 0)
                {
                    thongBao = $"Xóa đề thi \"{tenDeThi}\" thành công!";
                    return true;
                }
                else
                {
                    thongBao = "Không thể xóa đề thi! Vui lòng thử lại.";
                    return false;
                }
            }
            catch (Exception ex)
            {
                thongBao = "Lỗi khi xóa đề thi: " + ex.Message;
                return false;
            }
        }

        public bool CapNhatSoCau(int deThiID)
        {
            string sql = @"UPDATE DeThi 
                           SET SoCau = (SELECT COUNT(*) FROM CauHoi WHERE DeThiID = @id)
                           WHERE DeThiID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter("@id", deThiID)) > 0;
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
            string checkSql = "SELECT COUNT(*) FROM ChiTietBaiLam WHERE CauHoiID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter("@id", cauHoiID)));
            if (count > 0)
            {
                throw new Exception("Không thể xóa câu hỏi này vì đã có học sinh làm bài!");
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
            string sql = @"SELECT hs.HocSinhID, hs.HoTen, d.TenDeThi, k.SoCauDung, k.Diem, k.ThoiGianNop
                           FROM KetQuaThi k
                           JOIN HocSinh hs ON hs.HocSinhID = k.HocSinhID
                           JOIN DeThi d ON d.DeThiID = k.DeThiID
                           WHERE k.DeThiID = @id
                           ORDER BY k.ThoiGianNop DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter("@id", maDeThi));
        }

        /// <summary>
        /// Lấy toàn bộ bảng điểm của một học sinh (tất cả đề thi đã làm),
        /// trả về kết quả có MonHoc để nhóm khi xuất báo cáo.
        /// </summary>
        public DataTable LayBangDiemHocSinh(int hocSinhID)
        {
            string sql = @"SELECT d.MonHoc, d.TenDeThi, k.ThoiGianNop, k.Diem
                           FROM KetQuaThi k
                           JOIN DeThi d ON d.DeThiID = k.DeThiID
                           WHERE k.HocSinhID = @hsID
                           ORDER BY d.MonHoc ASC, k.ThoiGianNop ASC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter("@hsID", hocSinhID));
        }
        public int DemSoCauHoiHienTai(int deThiID)
        {
            string sql = "SELECT COUNT(*) FROM CauHoi WHERE DeThiID = @id";
            object result = DatabaseHelper.ExecuteScalar(sql, new NpgsqlParameter("@id", deThiID));
            return result != null && result != DBNull.Value ? Convert.ToInt32(result) : 0;
        }
    }
}