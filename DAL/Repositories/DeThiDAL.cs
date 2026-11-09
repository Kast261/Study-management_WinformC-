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
            return LayDeThiChoHocSinh(hocSinhID, false);
        }

        // Trang chủ chỉ hiển thị những bài học viên còn có thể bắt đầu làm.
        public DataTable LayBaiThiDangMoChoHocSinh(int hocSinhID)
        {
            return LayDeThiChoHocSinh(hocSinhID, true);
        }

        private DataTable LayDeThiChoHocSinh(int hocSinhID, bool chiBaiCoTheLam)
        {
            string sql = @"
        SELECT 
            d.DeThiID AS ""DeThiID"", 
            d.TenDeThi AS ""TenDeThi"", 
            d.MonHoc AS ""MonHoc"", 
            (SELECT COUNT(*)
                FROM CauHoi c
                WHERE c.DeThiID = d.DeThiID) AS ""SoCau"", 
            d.ThoiGian AS ""ThoiGian"", 
            d.HanNop AS ""HanNop"", 
            l.TenLop AS ""TenLop"",
            CASE 
                WHEN kq.KetQuaID IS NOT NULL THEN 'Đã làm'
                WHEN d.TrangThai = 'Đã đóng' THEN 'Đã đóng'
                WHEN d.HanNop IS NOT NULL AND d.HanNop <= CURRENT_TIMESTAMP THEN 'Hết hạn'
                ELSE 'Chưa làm'
            END AS ""TrangThai"",
            kq.Diem AS ""Diem""
        FROM DeThi d
        JOIN LopHoc l ON d.LopHocID = l.LopHocID
        LEFT JOIN KetQuaThi kq ON d.DeThiID = kq.DeThiID AND kq.HocSinhID = @hsID
        -- Xét thành viên hiện tại, không so sánh ngày duyệt với ngày giao đề.
        WHERE d.TrangThai IN ('Đang mở', 'Đã đóng') AND l.LopHocID IN (
            SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID AND LopHocID IS NOT NULL
            UNION
            SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND TrangThai = 'Đã duyệt'
        )
        AND (NOT @chiBaiCoTheLam OR (
            d.TrangThai = 'Đang mở' AND kq.KetQuaID IS NULL AND d.ThoiGian > 0
            AND (d.HanNop IS NULL OR d.HanNop > CURRENT_TIMESTAMP)
            AND EXISTS (SELECT 1 FROM CauHoi c WHERE c.DeThiID = d.DeThiID)
        ))
        ORDER BY 
            CASE 
                WHEN kq.KetQuaID IS NOT NULL THEN 1 -- Đã làm xếp cuối
                WHEN d.TrangThai = 'Đã đóng' THEN 2
                WHEN d.HanNop IS NOT NULL AND d.HanNop <= CURRENT_TIMESTAMP THEN 2 -- Hết hạn xếp thứ 2 từ dưới lên
                ELSE 0 -- Chưa làm xếp lên đầu
            END ASC,
            d.HanNop ASC, d.DeThiID ASC";

            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] {
                new NpgsqlParameter("@hsID", hocSinhID),
                new NpgsqlParameter("@chiBaiCoTheLam", chiBaiCoTheLam)
            });
        }

        public DataTable LayDeThiTheoLop(int lopID)
        {
            string sql = @"SELECT DeThiID, TenDeThi, MoTa, ThoiGian, MonHoc, SoCau,
                                  GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai
                           FROM DeThi WHERE LopHocID = @lopID AND COALESCE(TrangThai,'') <> 'Đã xóa'";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopID) });
        }

        public DataTable LayDeThiTheoGiaoVien(int gvID, bool baoGomDaXoa = false)
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
                             AND (@baoGomDaXoa OR COALESCE(d.TrangThai,'') <> 'Đã xóa')
                           ORDER BY d.DeThiID DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter("@gvID", gvID),
                new NpgsqlParameter("@baoGomDaXoa", baoGomDaXoa));
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
                LopHocID = r["LopHocID"] == DBNull.Value ? 0 : Convert.ToInt32(r["LopHocID"]),
                GiaoVienID = r["GiaoVienID"] == DBNull.Value ? 0 : Convert.ToInt32(r["GiaoVienID"]),
                MonHoc = r["MonHoc"]?.ToString(),
                SoCau = Convert.ToInt32(r["SoCau"]),
                ThoiGian = Convert.ToInt32(r["ThoiGian"]),
                HanNop = r["HanNop"] != DBNull.Value ? Convert.ToDateTime(r["HanNop"]) : (DateTime?)null,
                TrangThai = r["TrangThai"]?.ToString()
            };
        }

        public bool CoTheBatDauThi(int hocSinhID, int deThiID)
        {
            return Convert.ToBoolean(DatabaseHelper.ExecuteScalar(@"
                SELECT EXISTS (
                    SELECT 1 FROM DeThi d JOIN HocSinh hs ON hs.HocSinhID=@hs
                    WHERE d.DeThiID=@de AND d.TrangThai='Đang mở' AND d.ThoiGian>0
                      AND (d.HanNop IS NULL OR d.HanNop>CURRENT_TIMESTAMP)
                      AND (hs.LopHocID=d.LopHocID OR EXISTS (
                          SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=hs.HocSinhID
                            AND dk.LopHocID=d.LopHocID AND dk.TrangThai='Đã duyệt'))
                      AND EXISTS (SELECT 1 FROM CauHoi c WHERE c.DeThiID=d.DeThiID)
                      AND NOT EXISTS (SELECT 1 FROM KetQuaThi k WHERE k.HocSinhID=@hs AND k.DeThiID=d.DeThiID))",
                new NpgsqlParameter("hs", hocSinhID), new NpgsqlParameter("de", deThiID)));
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
        /// Tạo một lượt giao độc lập, sao chép câu hỏi và giữ nguyên đề/bài làm cũ.
        /// Mỗi lần gọi thành công trả về một DeThiID mới, kể cả cùng đề và cùng lớp.
        /// </summary>
        public int TaoLanGiaoDeThi(int taiKhoanID, int giaoVienID, int deThiID, int lopHocID,
            int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            if (taiKhoanID <= 0 || giaoVienID <= 0 || deThiID <= 0 || lopHocID <= 0 || thoiGian <= 0)
                throw new ArgumentException("Vui lòng chọn đề thi, lớp học và thời gian làm bài hợp lệ.");
            if (hanNop.HasValue && hanNop.Value <= DateTime.Now)
                throw new ArgumentException("Hạn nộp phải ở trong tương lai.");
            if (monHoc != null)
            {
                monHoc = monHoc.Trim();
                if (monHoc.Length == 0 || monHoc.Length > 100)
                    throw new ArgumentException("Môn học phải có từ 1 đến 100 ký tự.");
            }

            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    KiemTraQuyenGiaoVien(conn, transaction, taiKhoanID, giaoVienID);

                    // Cùng khóa với các thao tác sửa/nhập câu hỏi để bộ đề được sao chép trọn vẹn.
                    using (var source = new NpgsqlCommand(@"
                        SELECT DeThiID FROM DeThi
                        WHERE DeThiID=@de AND GiaoVienID=@gv
                          AND COALESCE(TrangThai,'') <> 'Đã xóa' FOR UPDATE", conn, transaction))
                    {
                        source.Parameters.AddWithValue("de", deThiID);
                        source.Parameters.AddWithValue("gv", giaoVienID);
                        object value = source.ExecuteScalar();
                        if (value == null)
                            throw new InvalidOperationException("Không tìm thấy đề thi thuộc giáo viên đang đăng nhập.");
                    }

                    using (var target = new NpgsqlCommand(@"
                        SELECT l.LopHocID FROM LopHoc l
                        WHERE l.LopHocID=@lop AND l.GiaoVienID=@gv
                        FOR SHARE OF l", conn, transaction))
                    {
                        target.Parameters.AddWithValue("lop", lopHocID);
                        target.Parameters.AddWithValue("gv", giaoVienID);
                        if (target.ExecuteScalar() == null)
                            throw new InvalidOperationException("Lớp nhận đề không tồn tại hoặc không còn do bạn phụ trách. Hãy tải lại danh sách lớp.");
                    }

                    int lanGiaoID;
                    using (var insert = new NpgsqlCommand(@"
                        INSERT INTO DeThi (TenDeThi,MoTa,ThoiGian,MonHoc,SoCau,GiaoVienID,LopHocID,HanNop,TrangThai)
                        SELECT TenDeThi,MoTa,@tg,COALESCE(@mon,MonHoc),0,GiaoVienID,@lop,@han,'Đang mở'
                        FROM DeThi WHERE DeThiID=@de RETURNING DeThiID", conn, transaction))
                    {
                        insert.Parameters.AddWithValue("de", deThiID);
                        insert.Parameters.AddWithValue("lop", lopHocID);
                        insert.Parameters.AddWithValue("tg", thoiGian);
                        insert.Parameters.Add(new NpgsqlParameter("mon", NpgsqlTypes.NpgsqlDbType.Varchar)
                            { Value = (object)monHoc ?? DBNull.Value });
                        insert.Parameters.Add(new NpgsqlParameter("han", NpgsqlTypes.NpgsqlDbType.Timestamp)
                            { Value = (object)hanNop ?? DBNull.Value });
                        lanGiaoID = Convert.ToInt32(insert.ExecuteScalar());
                    }
                    using (var copy = new NpgsqlCommand(@"
                        INSERT INTO CauHoi (DeThiID,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,ThuTu)
                        SELECT @moi,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,ThuTu
                        FROM CauHoi WHERE DeThiID=@de ORDER BY ThuTu,CauHoiID", conn, transaction))
                    {
                        copy.Parameters.AddWithValue("moi", lanGiaoID);
                        copy.Parameters.AddWithValue("de", deThiID);
                        if (copy.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException("Đề thi chưa có câu hỏi. Hãy thêm câu hỏi trước khi giao.");
                    }
                    CapNhatSoCau(conn, transaction, lanGiaoID);
                    transaction.Commit();
                    return lanGiaoID;
                }
            }
        }

        /// <summary>
        /// Cập nhật lần giao hiện có trước khi có bài làm. Dùng TaoLanGiaoDeThi để giao thêm lần mới.
        /// </summary>
        public bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            if (deThiID <= 0 || lopHocID <= 0 || thoiGian <= 0 || (hanNop.HasValue && hanNop.Value <= DateTime.Now)) return false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var check = new NpgsqlCommand(@"
                        SELECT d.DeThiID FROM DeThi d JOIN LopHoc l ON l.LopHocID=@lop AND l.GiaoVienID=d.GiaoVienID
                        WHERE d.DeThiID=@id AND (COALESCE(d.LopHocID,0)=0 OR d.LopHocID=@lop)
                          AND COALESCE(d.TrangThai,'') <> 'Đã xóa'
                        FOR UPDATE OF d", conn, transaction))
                    {
                        check.Parameters.AddWithValue("id", deThiID); check.Parameters.AddWithValue("lop", lopHocID);
                        if (check.ExecuteScalar() == null) return false;
                    }
                    using (var update = new NpgsqlCommand(@"
                        UPDATE DeThi SET LopHocID=@lop, ThoiGian=@tg, HanNop=@han,
                            SoCau=(SELECT COUNT(*) FROM CauHoi WHERE DeThiID=@id), TrangThai='Đang mở',
                            MonHoc=COALESCE(NULLIF(@mon,''),MonHoc)
                        WHERE DeThiID=@id AND EXISTS (SELECT 1 FROM CauHoi WHERE DeThiID=@id)
                          AND NOT EXISTS (SELECT 1 FROM KetQuaThi WHERE DeThiID=@id)", conn, transaction))
                    {
                        update.Parameters.AddWithValue("id", deThiID); update.Parameters.AddWithValue("lop", lopHocID);
                        update.Parameters.AddWithValue("tg", thoiGian);
                        update.Parameters.Add(new NpgsqlParameter("han", NpgsqlTypes.NpgsqlDbType.Timestamp) { Value=(object)hanNop ?? DBNull.Value });
                        update.Parameters.AddWithValue("mon", (object)monHoc ?? DBNull.Value);
                        if (update.ExecuteNonQuery() != 1) return false;
                    }
                    transaction.Commit(); return true;
                }
            }
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

                int rows;
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        KhoaDeThiChuaGiao(conn, transaction, deThiID, 0);
                        using (var cmd = new NpgsqlCommand("DELETE FROM CauHoi WHERE DeThiID=@id; DELETE FROM DeThi WHERE DeThiID=@id", conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("id", deThiID);
                            rows = cmd.ExecuteNonQuery();
                        }
                        transaction.Commit();
                    }
                }

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

        private static void KiemTraQuyenGiaoVien(NpgsqlConnection conn, NpgsqlTransaction transaction,
            int taiKhoanID, int giaoVienID)
        {
            using (var auth = new NpgsqlCommand(@"
                SELECT gv.GiaoVienID FROM GiaoVien gv
                JOIN TaiKhoan tk ON tk.TaiKhoanID=@tk
                WHERE gv.GiaoVienID=@gv AND LOWER(tk.VaiTro)='giaovien'
                  AND COALESCE(tk.TrangThaiKhoa,FALSE)=FALSE
                  AND (tk.NguoiDungID=gv.GiaoVienID OR
                      (COALESCE(tk.NguoiDungID,0)=0 AND gv.TaiKhoanID=tk.TaiKhoanID))
                FOR SHARE OF tk", conn, transaction))
            {
                auth.Parameters.AddWithValue("tk", taiKhoanID);
                auth.Parameters.AddWithValue("gv", giaoVienID);
                if (auth.ExecuteScalar() == null)
                    throw new InvalidOperationException("Tài khoản không có quyền thao tác với đề thi của giáo viên này.");
            }
        }

        /// <summary>
        /// Ẩn đề/lượt giao do giáo viên sở hữu, giữ câu hỏi và toàn bộ lịch sử bài làm.
        /// </summary>
        public bool XoaDeThi(int deThiID, int taiKhoanID, int giaoVienID, out string thongBao)
        {
            try
            {
                using (var conn = DatabaseHelper.GetConnection())
                {
                    conn.Open();
                    using (var transaction = conn.BeginTransaction())
                    {
                        KiemTraQuyenGiaoVien(conn, transaction, taiKhoanID, giaoVienID);
                        string tenDeThi;
                        // Cùng khóa với NopBai để không phát sinh bài làm trong khi đang xóa.
                        using (var check = new NpgsqlCommand(@"
                            SELECT TenDeThi FROM DeThi WHERE DeThiID=@de AND GiaoVienID=@gv FOR UPDATE", conn, transaction))
                        {
                            check.Parameters.AddWithValue("de", deThiID);
                            check.Parameters.AddWithValue("gv", giaoVienID);
                            object value = check.ExecuteScalar();
                            if (value == null)
                            {
                                thongBao = "Đề thi không tồn tại hoặc không thuộc giáo viên đang đăng nhập.";
                                return false;
                            }
                            tenDeThi = Convert.ToString(value);
                        }
                        using (var delete = new NpgsqlCommand(@"
                            UPDATE DeThi SET TrangThai='Đã xóa' WHERE DeThiID=@de", conn, transaction))
                        {
                            delete.Parameters.AddWithValue("de", deThiID);
                            delete.ExecuteNonQuery();
                        }
                        transaction.Commit();
                        thongBao = $"Đã ẩn đề thi \"{tenDeThi}\" (mã {deThiID}) khỏi danh sách giao bài. Kết quả và bài làm của học sinh được giữ nguyên.";
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                thongBao = "Không thể xóa đề thi: " + ex.Message;
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

        private static void KhoaDeThiChuaGiao(NpgsqlConnection conn, NpgsqlTransaction transaction, int deThiID, int giaoVienID)
        {
            using (var cmd = new NpgsqlCommand(@"
                SELECT GiaoVienID, COALESCE(LopHocID,0), COALESCE(TrangThai,'Chưa giao')
                FROM DeThi WHERE DeThiID=@id FOR UPDATE", conn, transaction))
            {
                cmd.Parameters.AddWithValue("id", deThiID);
                using (var reader = cmd.ExecuteReader())
                {
                    if (!reader.Read() || (giaoVienID > 0 && reader.GetInt32(0) != giaoVienID))
                        throw new InvalidOperationException("Không tìm thấy đề thi thuộc giáo viên đang đăng nhập.");
                    if (reader.GetInt32(1) > 0 || !string.Equals(reader.GetString(2), "Chưa giao", StringComparison.OrdinalIgnoreCase))
                        throw new InvalidOperationException("Không thể thay đổi câu hỏi của đề thi đã giao. Hãy tạo đề mới.");
                }
            }
            using (var cmd = new NpgsqlCommand("SELECT 1 FROM KetQuaThi WHERE DeThiID=@id LIMIT 1", conn, transaction))
            {
                cmd.Parameters.AddWithValue("id", deThiID);
                if (cmd.ExecuteScalar() != null) throw new InvalidOperationException("Đề thi đã có bài làm, không thể thay đổi câu hỏi.");
            }
        }

        private static void KiemTraNoiDungCauHoi(CauHoi ch)
        {
            if (ch == null || string.IsNullOrWhiteSpace(ch.NoiDung)
                || string.IsNullOrWhiteSpace(ch.DapAnA) || string.IsNullOrWhiteSpace(ch.DapAnB)
                || string.IsNullOrWhiteSpace(ch.DapAnC) || string.IsNullOrWhiteSpace(ch.DapAnD))
                throw new ArgumentException("Vui lòng nhập nội dung và đầy đủ bốn phương án đáp án.");
            ch.DapAnDung = (ch.DapAnDung ?? "").Trim().ToUpperInvariant();
            if (ch.DapAnDung.Length != 1 || "ABCD".IndexOf(ch.DapAnDung, StringComparison.Ordinal) < 0)
                throw new ArgumentException("Đáp án đúng phải là A, B, C hoặc D.");
        }

        private static void CapNhatSoCau(NpgsqlConnection conn, NpgsqlTransaction transaction, int deThiID)
        {
            using (var cmd = new NpgsqlCommand("UPDATE DeThi SET SoCau=(SELECT COUNT(*) FROM CauHoi WHERE DeThiID=@id) WHERE DeThiID=@id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("id", deThiID); cmd.ExecuteNonQuery();
            }
        }

        public bool ThemCauHoi(CauHoi ch, int giaoVienID = 0)
        {
            KiemTraNoiDungCauHoi(ch);
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    KhoaDeThiChuaGiao(conn, transaction, ch.DeThiID, giaoVienID);
                    using (var cmd = new NpgsqlCommand(@"
                        INSERT INTO CauHoi (DeThiID,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,ThuTu)
                        VALUES (@de,@nd,@a,@b,@c,@d,@dung,
                            (SELECT GREATEST(COALESCE(MAX(ThuTu),0),0)+1 FROM CauHoi WHERE DeThiID=@de))
                        RETURNING ThuTu", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("de", ch.DeThiID); cmd.Parameters.AddWithValue("nd", ch.NoiDung.Trim());
                        cmd.Parameters.AddWithValue("a", ch.DapAnA.Trim()); cmd.Parameters.AddWithValue("b", ch.DapAnB.Trim());
                        cmd.Parameters.AddWithValue("c", ch.DapAnC.Trim()); cmd.Parameters.AddWithValue("d", ch.DapAnD.Trim());
                        cmd.Parameters.AddWithValue("dung", ch.DapAnDung);
                        ch.ThuTu = Convert.ToInt32(cmd.ExecuteScalar());
                    }
                    CapNhatSoCau(conn, transaction, ch.DeThiID);
                    transaction.Commit(); return true;
                }
            }
        }

        private static int LayDeCuaCauHoi(NpgsqlConnection conn, NpgsqlTransaction transaction, int cauHoiID)
        {
            using (var cmd = new NpgsqlCommand("SELECT DeThiID FROM CauHoi WHERE CauHoiID=@id", conn, transaction))
            {
                cmd.Parameters.AddWithValue("id", cauHoiID);
                object result = cmd.ExecuteScalar();
                if (result == null || result == DBNull.Value) throw new InvalidOperationException("Câu hỏi không còn tồn tại.");
                return Convert.ToInt32(result);
            }
        }

        public bool XoaCauHoi(int cauHoiID, int giaoVienID = 0)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    int deID = LayDeCuaCauHoi(conn, transaction, cauHoiID);
                    KhoaDeThiChuaGiao(conn, transaction, deID, giaoVienID);
                    using (var cmd = new NpgsqlCommand("DELETE FROM CauHoi WHERE CauHoiID=@id", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("id", cauHoiID);
                        if (cmd.ExecuteNonQuery() != 1) return false;
                    }
                    CapNhatSoCau(conn, transaction, deID);
                    transaction.Commit(); return true;
                }
            }
        }

        public bool SuaCauHoi(CauHoi ch, int giaoVienID = 0)
        {
            KiemTraNoiDungCauHoi(ch);
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    int deID = LayDeCuaCauHoi(conn, transaction, ch.CauHoiID);
                    KhoaDeThiChuaGiao(conn, transaction, deID, giaoVienID);
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE CauHoi SET NoiDung=@nd,DapAnA=@a,DapAnB=@b,DapAnC=@c,DapAnD=@d,DapAnDung=@dung
                        WHERE CauHoiID=@id", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("id", ch.CauHoiID); cmd.Parameters.AddWithValue("nd", ch.NoiDung.Trim());
                        cmd.Parameters.AddWithValue("a", ch.DapAnA.Trim()); cmd.Parameters.AddWithValue("b", ch.DapAnB.Trim());
                        cmd.Parameters.AddWithValue("c", ch.DapAnC.Trim()); cmd.Parameters.AddWithValue("d", ch.DapAnD.Trim());
                        cmd.Parameters.AddWithValue("dung", ch.DapAnDung);
                        if (cmd.ExecuteNonQuery() != 1) return false;
                    }
                    transaction.Commit(); return true;
                }
            }
        }

        public DataTable LayTongHopKetQuaGanDay(int giaoVienID)
        {
            // Tổng hợp trực tiếp từ bài nộp để số câu hỏi/đăng ký lớp không nhân bản kết quả.
            // Giữ cả đề đã đóng và bài nộp của học viên đã chuyển lớp.
            return DatabaseHelper.ExecuteQuery(@"
                SELECT d.DeThiID AS ""DeThiID"", d.TenDeThi AS ""TenDeThi"",
                       COUNT(k.KetQuaID) AS ""DaNop"",
                       ROUND(AVG(k.Diem)::numeric, 2) AS ""DiemTrungBinh"",
                       MAX(k.ThoiGianNop) AS ""LanNopGanNhat""
                FROM DeThi d
                JOIN KetQuaThi k ON k.DeThiID=d.DeThiID
                WHERE d.GiaoVienID=@gv
                GROUP BY d.DeThiID, d.TenDeThi
                ORDER BY MAX(k.ThoiGianNop) DESC NULLS LAST, d.DeThiID DESC",
                new NpgsqlParameter("gv", giaoVienID));
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
