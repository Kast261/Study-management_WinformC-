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
    public class LopHocDAL
    {
        public DataTable LayTatCa()
        {
            string sql = @"
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dks WHERE dks.HocSinhID=h.HocSinhID AND dks.LopHocID=l.LopHocID AND dks.TrangThai='Đã duyệt')) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    COALESCE(k.TenKhoaHoc, 'Chưa phân loại') AS TenKhoaHoc, 
                    COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien, 
                    l.KhoaHocID, 
                    l.GiaoVienID, 
                    COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa,
                    l.LichHoc,
                    l.TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID";
            return DatabaseHelper.ExecuteQuery(sql);
        }

        public DataTable LayTheoGiaoVien(int gvID)
        {
            string sql = @"
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    l.KhoaHocID,
                    COALESCE(k.TenKhoaHoc, N'Chưa phân loại') AS TenKhoaHoc,
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dks WHERE dks.HocSinhID=h.HocSinhID AND dks.LopHocID=l.LopHocID AND dks.TrangThai='Đã duyệt')) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa,
                    l.NgayBatDau,
                    l.NgayKetThuc,
                    l.LichHoc,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                WHERE l.GiaoVienID = @gvID
                ORDER BY l.LopHocID DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@gvID", gvID) });
        }

        public DataTable LayTheoHocSinh(int hsID)
        {
            // Lấy lớp mà học sinh đang học (thông qua HocSinh.LopHocID hoặc DangKyLop)
            string sql = @"
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    g.HoTen AS TenGiaoVien, 
                    l.LichHoc, 
                    l.NgayBatDau,
                    l.NgayKetThuc,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE l.LopHocID IN (
                    SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID
                    UNION
                    SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND TrangThai = 'Đã duyệt'
                )";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hsID) });
        }

        public bool ThemLop(LopHoc lop)
        {
            string sql = @"INSERT INTO LopHoc (KhoaHocID, GiaoVienID, TenLop, LichHoc, SiSoToiDa, TrangThai, NgayBatDau, NgayKetThuc) 
                           VALUES (@khID, @gvID, @ten, @lich, @siso, @tt, @nbd, @nkt)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@khID", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value),
                new NpgsqlParameter("@gvID", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value),
                new NpgsqlParameter("@ten", lop.TenLop),
                new NpgsqlParameter("@lich", lop.LichHoc ?? ""),
                new NpgsqlParameter("@siso", lop.SiSoToiDa > 0 ? lop.SiSoToiDa : 40),
                new NpgsqlParameter("@tt", lop.TrangThai ?? "Đang mở"),
                new NpgsqlParameter("@nbd", lop.NgayBatDau.HasValue ? (object)lop.NgayBatDau.Value : DBNull.Value),
                new NpgsqlParameter("@nkt", lop.NgayKetThuc.HasValue ? (object)lop.NgayKetThuc.Value : DBNull.Value)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool SuaLop(LopHoc lop)
        {
            return CapNhatLop(lop, false);
        }

        private bool CapNhatLop(LopHoc lop, bool giuLichVaNgay)
        {
            if (lop == null || string.IsNullOrWhiteSpace(lop.TenLop) || lop.SiSoToiDa <= 0) return false;
            if (!giuLichVaNgay && lop.NgayBatDau.HasValue && lop.NgayKetThuc.HasValue
                && lop.NgayKetThuc.Value < lop.NgayBatDau.Value) return false;
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var locked = new NpgsqlCommand("SELECT LopHocID FROM LopHoc WHERE LopHocID=@id FOR UPDATE", conn, transaction))
                    {
                        locked.Parameters.AddWithValue("id", lop.LopHocID);
                        if (locked.ExecuteScalar() == null) return false;
                    }
                    if (DemHocVien(conn, transaction, lop.LopHocID) > lop.SiSoToiDa) return false;
                    string sql = "UPDATE LopHoc SET KhoaHocID=@kh, GiaoVienID=@gv, TenLop=@ten, SiSoToiDa=@siso"
                        + (giuLichVaNgay ? "" : ", LichHoc=@lich, NgayBatDau=@nbd, NgayKetThuc=@nkt")
                        + " WHERE LopHocID=@id";
                    using (var update = new NpgsqlCommand(sql, conn, transaction))
                    {
                        update.Parameters.AddWithValue("kh", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value);
                        update.Parameters.AddWithValue("gv", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value);
                        update.Parameters.AddWithValue("ten", lop.TenLop.Trim()); update.Parameters.AddWithValue("siso", lop.SiSoToiDa);
                        update.Parameters.AddWithValue("id", lop.LopHocID);
                        if (!giuLichVaNgay)
                        {
                            update.Parameters.AddWithValue("lich", lop.LichHoc ?? "");
                            update.Parameters.Add(new NpgsqlParameter("nbd", NpgsqlTypes.NpgsqlDbType.Date) {Value=(object)lop.NgayBatDau ?? DBNull.Value});
                            update.Parameters.Add(new NpgsqlParameter("nkt", NpgsqlTypes.NpgsqlDbType.Date) {Value=(object)lop.NgayKetThuc ?? DBNull.Value});
                        }
                        if (update.ExecuteNonQuery() != 1) return false;
                    }
                    transaction.Commit(); return true;
                }
            }
        }

        public bool XoaLop(int lopID)
        {
            // Kiểm tra xem có học sinh nào đang trong lớp không
            string checkSql = @"SELECT (SELECT COUNT(*) FROM HocSinh WHERE LopHocID=@id)
                + (SELECT COUNT(*) FROM DangKyLop WHERE LopHocID=@id)
                + (SELECT COUNT(*) FROM DeThi WHERE LopHocID=@id)";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter[] { new NpgsqlParameter("@id", lopID) }));
            if (count > 0)
            {
                throw new Exception("Không thể xóa lớp đã có học sinh, đăng ký hoặc đề thi. Hãy đóng đăng ký để giữ lịch sử.");
            }

            string sql = "DELETE FROM LopHoc WHERE LopHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", lopID) }) > 0;
        }

        public DataTable LayDangKyTheoLop(int lopHocID)
        {
            string sql = @"
        SELECT 
            dk.DangKyID,
            dk.LopHocID,
            dk.HocSinhID,
            hs.HoTen,
            hs.Email,
            hs.SoDienThoai,
            l.TenLop,
            CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dks WHERE dks.HocSinhID=h.HocSinhID AND dks.LopHocID=l.LopHocID AND dks.TrangThai='Đã duyệt')) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
            dk.NgayDangKy,
            dk.TrangThai
        FROM DangKyLop dk
        JOIN HocSinh hs ON dk.HocSinhID = hs.HocSinhID
        JOIN LopHoc l ON dk.LopHocID = l.LopHocID
        WHERE dk.LopHocID = @lopID AND (dk.TrangThai = 'Chờ duyệt' OR dk.TrangThai IS NULL)
        ORDER BY dk.NgayDangKy ASC";

            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopHocID) });
        }

        public DataTable LayDangKyChoDuyet(int gvID, int lopHocID = 0)
        {
            string sql = @"
        SELECT 
            dk.DangKyID,
            dk.LopHocID,
            dk.HocSinhID,
            hs.HoTen,
            hs.Email,
            hs.SoDienThoai,
            l.TenLop,
            CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dks WHERE dks.HocSinhID=h.HocSinhID AND dks.LopHocID=l.LopHocID AND dks.TrangThai='Đã duyệt')) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
            dk.NgayDangKy,
            COALESCE(dk.TrangThai, 'Chờ duyệt') AS TrangThai
        FROM DangKyLop dk
        JOIN HocSinh hs ON dk.HocSinhID = hs.HocSinhID
        JOIN LopHoc l ON dk.LopHocID = l.LopHocID
        WHERE l.GiaoVienID = @gvID AND (dk.TrangThai = 'Chờ duyệt' OR dk.TrangThai IS NULL)";

            if (lopHocID > 0)
            {
                sql += " AND dk.LopHocID = @lopID ORDER BY dk.NgayDangKy ASC";
                return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { 
                    new NpgsqlParameter("@gvID", gvID),
                    new NpgsqlParameter("@lopID", lopHocID) 
                });
            }
            else
            {
                sql += " ORDER BY dk.NgayDangKy ASC";
                return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { 
                    new NpgsqlParameter("@gvID", gvID) 
                });
            }
        }
        private static long DemHocVien(NpgsqlConnection conn, NpgsqlTransaction transaction, int lopHocID)
        {
            using (var cmd = new NpgsqlCommand(@"
                SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID=@lop OR EXISTS (
                    SELECT 1 FROM DangKyLop dk WHERE dk.LopHocID=@lop
                      AND dk.HocSinhID=h.HocSinhID AND dk.TrangThai='Đã duyệt')", conn, transaction))
            {
                cmd.Parameters.AddWithValue("lop", lopHocID);
                return Convert.ToInt64(cmd.ExecuteScalar());
            }
        }

        private static DataTable DocBang(NpgsqlConnection conn, NpgsqlTransaction transaction, string sql, params NpgsqlParameter[] parameters)
        {
            using (var cmd = new NpgsqlCommand(sql, conn, transaction))
            {
                cmd.Parameters.AddRange(parameters);
                using (var adapter = new NpgsqlDataAdapter(cmd))
                {
                    var table = new DataTable(); adapter.Fill(table); return table;
                }
            }
        }

        public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID, int giaoVienID = 0)
        {
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var lop = DocBang(conn, transaction, @"
                        SELECT COALESCE(SiSoToiDa,40) AS SucChua FROM LopHoc
                        WHERE LopHocID=@lop AND COALESCE(TrangThai,'Đang mở')='Đang mở'
                          AND (@gv=0 OR GiaoVienID=@gv) FOR UPDATE",
                        new NpgsqlParameter("lop", lopHocID), new NpgsqlParameter("gv", giaoVienID)))
                    {
                        if (lop.Rows.Count != 1) return false;
                        using (var hs = DocBang(conn, transaction, "SELECT HocSinhID FROM HocSinh WHERE HocSinhID=@hs FOR UPDATE", new NpgsqlParameter("hs", hocSinhID)))
                            if (hs.Rows.Count != 1) return false;
                        if (DemHocVien(conn, transaction, lopHocID) >= Convert.ToInt32(lop.Rows[0]["SucChua"])) return false;
                    }
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE DangKyLop SET TrangThai='Đã duyệt'
                        WHERE DangKyID=@dk AND LopHocID=@lop AND HocSinhID=@hs
                          AND (TrangThai='Chờ duyệt' OR TrangThai IS NULL)", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("dk", dangKyID); cmd.Parameters.AddWithValue("lop", lopHocID);
                        cmd.Parameters.AddWithValue("hs", hocSinhID);
                        if (cmd.ExecuteNonQuery() != 1) return false;
                    }
                    // Đăng ký đã duyệt ở trên lưu tư cách thành viên của lớp mới.
                    // Giữ lớp chính cũ để không làm mất lớp/bài thi khi học thêm lớp khác.
                    using (var cmd = new NpgsqlCommand(@"
                        UPDATE HocSinh SET LopHocID=COALESCE(NULLIF(LopHocID,0),@lop)
                        WHERE HocSinhID=@hs", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("lop", lopHocID); cmd.Parameters.AddWithValue("hs", hocSinhID);
                        if (cmd.ExecuteNonQuery() != 1) return false;
                    }
                    transaction.Commit(); return true;
                }
            }
        }

        public bool TuChoiDangKy(int dangKyID, int giaoVienID = 0)
        {
            return DatabaseHelper.ExecuteNonQuery(@"
                UPDATE DangKyLop dk SET TrangThai='Từ chối'
                WHERE dk.DangKyID=@dk AND (dk.TrangThai='Chờ duyệt' OR dk.TrangThai IS NULL)
                  AND EXISTS (SELECT 1 FROM LopHoc l WHERE l.LopHocID=dk.LopHocID AND (@gv=0 OR l.GiaoVienID=@gv))",
                new NpgsqlParameter("dk", dangKyID), new NpgsqlParameter("gv", giaoVienID)) == 1;
        }
        public bool DongDangKyLop(int lopHocID, int giaoVienID = 0)
        {
            if (lopHocID <= 0 || giaoVienID < 0) return false;
            string sql = @"UPDATE LopHoc SET TrangThai = 'Đã đóng'
                           WHERE LopHocID = @id AND (@gv=0 OR GiaoVienID=@gv)
                             AND COALESCE(TrangThai,'Đang mở')='Đang mở'";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter("id", lopHocID),
                new NpgsqlParameter("gv", giaoVienID)) == 1;
        }

        /// <summary>
        /// Lấy các lớp học & khóa học đang mở mà học viên này CHƯA đăng ký
        /// </summary>
        public DataTable LayLopChuaDangKy(int hsID)
        {
            string sql = @"
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    COALESCE(k.TenKhoaHoc, 'Chưa phân loại') AS TenKhoaHoc, 
                    COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien, 
                    COALESCE(l.LichHoc, 'Chưa có') AS LichHoc,
                    l.NgayBatDau,
                    l.NgayKetThuc,
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID=l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=h.HocSinhID AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa,
                    (SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID=l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=h.HocSinhID AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')) AS SiSoHienTai,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE COALESCE(l.TrangThai, 'Đang mở') = 'Đang mở'
                  AND l.LopHocID NOT IN (
                      SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND LopHocID IS NOT NULL AND TrangThai IN ('Đã duyệt', 'Chờ duyệt')
                      UNION
                      SELECT COALESCE(LopHocID, 0) FROM HocSinh WHERE HocSinhID = @hsID
                  )
                ORDER BY l.LopHocID DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hsID) });
        }

        /// <summary>
        /// Lấy các lớp và khóa học hiện tại đang học hoặc đang chờ duyệt (cho DataGrid thứ hai)
        /// </summary>
     public DataTable LayLopHocVienDangHocVaChoDuyet(int hsID)
        {
            string sql = @"
        SELECT 
            l.LopHocID, 
            l.TenLop, 
            COALESCE(k.TenKhoaHoc, 'Chưa phân loại') AS TenKhoaHoc, 
            COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien, 
            COALESCE(l.LichHoc, 'Chưa có') AS LichHoc, 
            l.NgayBatDau,
            l.NgayKetThuc,

            CAST((
                SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID=l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dk2 WHERE dk2.HocSinhID=h.HocSinhID AND dk2.LopHocID=l.LopHocID AND dk2.TrangThai='Đã duyệt')
            ) AS varchar(10))
            || '/' ||
            CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,

            dk.TrangThai AS TrangThaiDangKy,
            dk.DangKyID,
            COALESCE(l.TrangThai, 'Đang mở') AS TrangThaiLop

        FROM DangKyLop dk
        JOIN LopHoc l ON dk.LopHocID = l.LopHocID
        LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
        LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID

        WHERE dk.HocSinhID = @hsID
          AND dk.TrangThai IN ('Đã duyệt', 'Chờ duyệt', 'Từ chối')

        UNION

        SELECT 
            l.LopHocID, 
            l.TenLop, 
            COALESCE(k.TenKhoaHoc, 'Chưa phân loại') AS TenKhoaHoc, 
            COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien, 
            COALESCE(l.LichHoc, 'Chưa có') AS LichHoc, 
            l.NgayBatDau,
            l.NgayKetThuc,

            CAST((
                SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID=l.LopHocID OR EXISTS (SELECT 1 FROM DangKyLop dk2 WHERE dk2.HocSinhID=h.HocSinhID AND dk2.LopHocID=l.LopHocID AND dk2.TrangThai='Đã duyệt')
            ) AS varchar(10))
            || '/' ||
            CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,

            'Đang học' AS TrangThaiDangKy,
            0 AS DangKyID,
            COALESCE(l.TrangThai, 'Đang mở') AS TrangThaiLop

        FROM HocSinh hs
        JOIN LopHoc l ON hs.LopHocID = l.LopHocID
        LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
        LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID

        WHERE hs.HocSinhID = @hsID
          AND hs.LopHocID IS NOT NULL
          AND l.LopHocID NOT IN (
              SELECT LopHocID
              FROM DangKyLop
              WHERE HocSinhID = @hsID AND LopHocID IS NOT NULL
                AND TrangThai IN ('Đã duyệt', 'Chờ duyệt', 'Từ chối')
          )

        ORDER BY LopHocID DESC";

            return DatabaseHelper.ExecuteQuery(
                sql,
                new NpgsqlParameter[]
                {
            new NpgsqlParameter("@hsID", hsID)
                });
        }

        /// <summary>
        /// Học viên đăng ký vào một lớp học (chờ giáo viên duyệt)
        /// </summary>
        public bool DangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            thongBao = "Lớp không còn mở, đã đủ sĩ số hoặc bạn đã đăng ký lớp này.";
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var lop = DocBang(conn, transaction, @"
                        SELECT COALESCE(SiSoToiDa,40) AS SucChua FROM LopHoc
                        WHERE LopHocID=@lop AND COALESCE(TrangThai,'Đang mở')='Đang mở' FOR UPDATE",
                        new NpgsqlParameter("lop", lopHocID)))
                    {
                        if (lop.Rows.Count != 1) return false;
                        if (DemHocVien(conn, transaction, lopHocID) >= Convert.ToInt32(lop.Rows[0]["SucChua"])) return false;
                    }
                    using (var hs = DocBang(conn, transaction, "SELECT LopHocID FROM HocSinh WHERE HocSinhID=@hs FOR UPDATE", new NpgsqlParameter("hs", hocSinhID)))
                    {
                        if (hs.Rows.Count != 1 || (hs.Rows[0][0] != DBNull.Value && Convert.ToInt32(hs.Rows[0][0]) == lopHocID)) return false;
                    }
                    using (var registrations = DocBang(conn, transaction, @"
                        SELECT DangKyID, TrangThai FROM DangKyLop WHERE LopHocID=@lop AND HocSinhID=@hs ORDER BY DangKyID DESC",
                        new NpgsqlParameter("lop", lopHocID), new NpgsqlParameter("hs", hocSinhID)))
                    {
                        foreach (DataRow row in registrations.Rows)
                            if (row["TrangThai"] == DBNull.Value || row["TrangThai"].ToString() == "Chờ duyệt" || row["TrangThai"].ToString() == "Đã duyệt") return false;
                        string sql = registrations.Rows.Count == 0
                            ? "INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai,NgayDangKy) VALUES (@lop,@hs,'Chờ duyệt',CURRENT_TIMESTAMP)"
                            : "UPDATE DangKyLop SET TrangThai='Chờ duyệt',NgayDangKy=CURRENT_TIMESTAMP WHERE DangKyID=@dk";
                        using (var cmd = new NpgsqlCommand(sql, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("lop", lopHocID); cmd.Parameters.AddWithValue("hs", hocSinhID);
                            if (registrations.Rows.Count > 0) cmd.Parameters.AddWithValue("dk", Convert.ToInt32(registrations.Rows[0]["DangKyID"]));
                            if (cmd.ExecuteNonQuery() != 1) return false;
                        }
                    }
                    transaction.Commit();
                    thongBao = "Đăng ký thành công! Đơn đăng ký đang chờ giáo viên phê duyệt.";
                    return true;
                }
            }
        }

        public bool HuyDangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            thongBao = "Không thể hủy: lớp đã đóng, bạn chưa đăng ký hoặc đã được duyệt vào lớp.";
            using (var conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (var transaction = conn.BeginTransaction())
                {
                    using (var lop = DocBang(conn, transaction, "SELECT LopHocID FROM LopHoc WHERE LopHocID=@lop AND COALESCE(TrangThai,'Đang mở')='Đang mở' FOR UPDATE", new NpgsqlParameter("lop", lopHocID)))
                        if (lop.Rows.Count != 1) return false;
                    using (var hs = DocBang(conn, transaction, "SELECT LopHocID FROM HocSinh WHERE HocSinhID=@hs FOR UPDATE", new NpgsqlParameter("hs", hocSinhID)))
                        if (hs.Rows.Count != 1 || (hs.Rows[0][0] != DBNull.Value && Convert.ToInt32(hs.Rows[0][0]) == lopHocID)) return false;
                    using (var approved = DocBang(conn, transaction, "SELECT 1 FROM DangKyLop WHERE LopHocID=@lop AND HocSinhID=@hs AND TrangThai='Đã duyệt'",
                        new NpgsqlParameter("lop", lopHocID), new NpgsqlParameter("hs", hocSinhID)))
                        if (approved.Rows.Count > 0) return false;
                    using (var cmd = new NpgsqlCommand("DELETE FROM DangKyLop WHERE LopHocID=@lop AND HocSinhID=@hs", conn, transaction))
                    {
                        cmd.Parameters.AddWithValue("lop", lopHocID); cmd.Parameters.AddWithValue("hs", hocSinhID);
                        if (cmd.ExecuteNonQuery() == 0) return false;
                    }
                    transaction.Commit(); thongBao = "Hủy đăng ký lớp học thành công!"; return true;
                }
            }
        }

        public bool SuaThongTinLopCoBan(LopHoc lop)
        {
            // Form Admin không nhập lịch/ngày: không ghi đè các giá trị đó.
            return CapNhatLop(lop, true);
        }
    }
}
