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
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
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
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID) AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    k.TenKhoaHoc, 
                    l.LichHoc,
                    l.TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                WHERE l.GiaoVienID = @gvID";
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
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE l.LopHocID IN (
                    SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID
                    UNION
                    SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID
                )";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hsID) });
        }

        public bool ThemLop(LopHoc lop)
        {
            string sql = @"INSERT INTO LopHoc (KhoaHocID, GiaoVienID, TenLop, LichHoc, SiSoToiDa, TrangThai) 
                           VALUES (@khID, @gvID, @ten, @lich, @siso, @tt)";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@khID", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value),
                new NpgsqlParameter("@gvID", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value),
                new NpgsqlParameter("@ten", lop.TenLop),
                new NpgsqlParameter("@lich", lop.LichHoc ?? ""),
                new NpgsqlParameter("@siso", lop.SiSoToiDa > 0 ? lop.SiSoToiDa : 40),
                new NpgsqlParameter("@tt", lop.TrangThai ?? "Đang mở")
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool SuaLop(LopHoc lop)
        {
            string sql = @"UPDATE LopHoc 
                           SET KhoaHocID = @khID, 
                               GiaoVienID = @gvID, 
                               TenLop = @ten, 
                               SiSoToiDa = @siso 
                           WHERE LopHocID = @id";
            NpgsqlParameter[] pars = {
                new NpgsqlParameter("@khID", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value),
                new NpgsqlParameter("@gvID", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value),
                new NpgsqlParameter("@ten", lop.TenLop),
                new NpgsqlParameter("@siso", lop.SiSoToiDa > 0 ? lop.SiSoToiDa : 40),
                new NpgsqlParameter("@id", lop.LopHocID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool XoaLop(int lopID)
        {
            // Kiểm tra xem có học sinh nào đang trong lớp không
            string checkSql = "SELECT COUNT(*) FROM HocSinh WHERE LopHocID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new NpgsqlParameter[] { new NpgsqlParameter("@id", lopID) }));
            if (count > 0)
            {
                throw new Exception("Không thể xóa lớp vì đang có học sinh thuộc lớp này!");
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
            hs.GioiTinh,
            hs.Email,
            hs.SoDienThoai,
            dk.NgayDangKy,
            dk.TrangThai
        FROM DangKyLop dk
        JOIN HocSinh hs ON dk.HocSinhID = hs.HocSinhID
        WHERE dk.LopHocID = @lopID AND dk.TrangThai = 'Chờ duyệt'
        ORDER BY dk.NgayDangKy ASC";

            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopHocID) });
        }
        public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID)
        {
            string sql = @"
        UPDATE DangKyLop SET TrangThai = 'Đã duyệt' WHERE DangKyID = @dkID;
        UPDATE HocSinh SET LopHocID = @lopID WHERE HocSinhID = @hsID;";

            NpgsqlParameter[] pars = {
        new NpgsqlParameter("@dkID", dangKyID),
        new NpgsqlParameter("@lopID", lopHocID),
        new NpgsqlParameter("@hsID", hocSinhID)
                };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool TuChoiDangKy(int dangKyID)
        {
            string sql = "UPDATE DangKyLop SET TrangThai = 'Từ chối' WHERE DangKyID = @dkID";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@dkID", dangKyID) }) > 0;
        }
        public bool DongDangKyLop(int lopHocID)
        {
            string sql = "UPDATE LopHoc SET TrangThai = 'Đã đóng' WHERE LopHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@id", lopHocID) }) > 0;
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
                    CAST((SELECT COUNT(*) FROM DangKyLop dk WHERE dk.LopHocID = l.LopHocID AND dk.TrangThai = 'Đã duyệt') AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa,
                    (SELECT COUNT(*) FROM DangKyLop dk WHERE dk.LopHocID = l.LopHocID AND dk.TrangThai = 'Đã duyệt') AS SiSoHienTai,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE COALESCE(l.TrangThai, 'Đang mở') = 'Đang mở'
                  AND l.LopHocID NOT IN (
                      SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND TrangThai IN ('Đã duyệt', 'Chờ duyệt')
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
                    CAST((SELECT COUNT(*) FROM DangKyLop dk2 WHERE dk2.LopHocID = l.LopHocID AND dk2.TrangThai = 'Đã duyệt') AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    CASE 
                        WHEN dk.TrangThai = 'Chờ duyệt' THEN 'Chờ duyệt'
                        ELSE 'Đang học'
                    END AS TrangThaiHoc,
                    dk.DangKyID,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThaiLop
                FROM DangKyLop dk
                JOIN LopHoc l ON dk.LopHocID = l.LopHocID
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE dk.HocSinhID = @hsID AND dk.TrangThai IN ('Đã duyệt', 'Chờ duyệt')
                UNION
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    COALESCE(k.TenKhoaHoc, 'Chưa phân loại') AS TenKhoaHoc, 
                    COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien, 
                    COALESCE(l.LichHoc, 'Chưa có') AS LichHoc, 
                    l.NgayBatDau,
                    l.NgayKetThuc,
                    CAST((SELECT COUNT(*) FROM DangKyLop dk2 WHERE dk2.LopHocID = l.LopHocID AND dk2.TrangThai = 'Đã duyệt') AS varchar(10)) || '/' || CAST(COALESCE(l.SiSoToiDa, 40) AS varchar(10)) AS SiSo,
                    'Đang học' AS TrangThaiHoc,
                    0 AS DangKyID,
                    COALESCE(l.TrangThai, 'Đang mở') AS TrangThaiLop
                FROM HocSinh hs
                JOIN LopHoc l ON hs.LopHocID = l.LopHocID
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE hs.HocSinhID = @hsID AND hs.LopHocID IS NOT NULL
                  AND l.LopHocID NOT IN (SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID AND TrangThai IN ('Đã duyệt', 'Chờ duyệt'))
                ORDER BY LopHocID DESC";
            return DatabaseHelper.ExecuteQuery(sql, new NpgsqlParameter[] { new NpgsqlParameter("@hsID", hsID) });
        }

        /// <summary>
        /// Học viên đăng ký vào một lớp học (chờ giáo viên duyệt)
        /// </summary>
        public bool DangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            string checkLopSql = @"
                SELECT 
                    COALESCE(TrangThai, 'Đang mở') AS TrangThai,
                    COALESCE(SiSoToiDa, 40) AS SiSoToiDa,
                    (SELECT COUNT(*) FROM DangKyLop WHERE LopHocID = @lopID AND TrangThai = 'Đã duyệt') AS DaDuyet
                FROM LopHoc 
                WHERE LopHocID = @lopID";

            DataTable dt = DatabaseHelper.ExecuteQuery(checkLopSql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopHocID) });
            if (dt.Rows.Count == 0)
            {
                thongBao = "Lớp học không tồn tại!";
                return false;
            }

            string trangThaiLop = dt.Rows[0]["TrangThai"].ToString();
            int siSoToiDa = Convert.ToInt32(dt.Rows[0]["SiSoToiDa"]);
            int daDuyet = Convert.ToInt32(dt.Rows[0]["DaDuyet"]);

            if (trangThaiLop.Equals("Đã đóng", StringComparison.OrdinalIgnoreCase))
            {
                thongBao = "Lớp học này hiện đã đóng đăng ký, không thể đăng ký thêm!";
                return false;
            }

            if (daDuyet >= siSoToiDa)
            {
                thongBao = $"Lớp học đã đủ sĩ số ({daDuyet}/{siSoToiDa}), không thể đăng ký!";
                return false;
            }

            string checkDkSql = "SELECT DangKyID, TrangThai FROM DangKyLop WHERE LopHocID = @lopID AND HocSinhID = @hsID";
            DataTable dtDk = DatabaseHelper.ExecuteQuery(checkDkSql, new NpgsqlParameter[] {
                new NpgsqlParameter("@lopID", lopHocID),
                new NpgsqlParameter("@hsID", hocSinhID)
            });

            if (dtDk.Rows.Count > 0)
            {
                string ttDk = dtDk.Rows[0]["TrangThai"]?.ToString();
                int dkID = Convert.ToInt32(dtDk.Rows[0]["DangKyID"]);

                if (ttDk == "Chờ duyệt")
                {
                    thongBao = "Bạn đã đăng ký lớp học này rồi và đang chờ giáo viên phê duyệt!";
                    return false;
                }
                if (ttDk == "Đã duyệt")
                {
                    thongBao = "Bạn đã được duyệt vào học lớp này rồi!";
                    return false;
                }

                // Cập nhật lại trạng thái thành Chờ duyệt
                string updateSql = "UPDATE DangKyLop SET TrangThai = 'Chờ duyệt', NgayDangKy = CURRENT_TIMESTAMP WHERE DangKyID = @dkID";
                int rows = DatabaseHelper.ExecuteNonQuery(updateSql, new NpgsqlParameter[] { new NpgsqlParameter("@dkID", dkID) });
                if (rows > 0)
                {
                    thongBao = "Đăng ký thành công! Đơn đăng ký của bạn đang CHỜ DUYỆT bởi giáo viên.";
                    return true;
                }
            }
            else
            {
                string insertSql = @"INSERT INTO DangKyLop (LopHocID, HocSinhID, TrangThai, NgayDangKy) 
                                     VALUES (@lopID, @hsID, 'Chờ duyệt', CURRENT_TIMESTAMP)";
                int rows = DatabaseHelper.ExecuteNonQuery(insertSql, new NpgsqlParameter[] {
                    new NpgsqlParameter("@lopID", lopHocID),
                    new NpgsqlParameter("@hsID", hocSinhID)
                });
                if (rows > 0)
                {
                    thongBao = "Đăng ký thành công! Đơn đăng ký của bạn đang CHỜ DUYỆT bởi giáo viên.";
                    return true;
                }
            }

            thongBao = "Không thể đăng ký lớp học. Vui lòng thử lại sau!";
            return false;
        }

        /// <summary>
        /// Học viên hủy đăng ký lớp học (chỉ khi lớp chưa đóng đăng ký và chưa quá hạn)
        /// </summary>
        public bool HuyDangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            string checkLopSql = @"
                SELECT 
                    COALESCE(TrangThai, 'Đang mở') AS TrangThai,
                    COALESCE(SiSoToiDa, 40) AS SiSoToiDa,
                    (SELECT COUNT(*) FROM DangKyLop WHERE LopHocID = @lopID AND TrangThai = 'Đã duyệt') AS DaDuyet
                FROM LopHoc 
                WHERE LopHocID = @lopID";

            DataTable dt = DatabaseHelper.ExecuteQuery(checkLopSql, new NpgsqlParameter[] { new NpgsqlParameter("@lopID", lopHocID) });
            if (dt.Rows.Count == 0)
            {
                thongBao = "Lớp học không tồn tại!";
                return false;
            }

            string trangThaiLop = dt.Rows[0]["TrangThai"].ToString();
            int siSoToiDa = Convert.ToInt32(dt.Rows[0]["SiSoToiDa"]);
            int daDuyet = Convert.ToInt32(dt.Rows[0]["DaDuyet"]);

            if (trangThaiLop.Equals("Đã đóng", StringComparison.OrdinalIgnoreCase))
            {
                thongBao = "Lớp học đã đóng đăng ký, bạn không thể hủy đăng ký vào thời điểm này!";
                return false;
            }

            string checkDkSql = "SELECT DangKyID, TrangThai FROM DangKyLop WHERE LopHocID = @lopID AND HocSinhID = @hsID";
            DataTable dtDk = DatabaseHelper.ExecuteQuery(checkDkSql, new NpgsqlParameter[] {
                new NpgsqlParameter("@lopID", lopHocID),
                new NpgsqlParameter("@hsID", hocSinhID)
            });

            bool daCoDon = dtDk.Rows.Count > 0;
            string ttDk = daCoDon ? dtDk.Rows[0]["TrangThai"]?.ToString() : "";
            int dkID = daCoDon ? Convert.ToInt32(dtDk.Rows[0]["DangKyID"]) : 0;

            object hsLopObj = DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID", new NpgsqlParameter("@hsID", hocSinhID));
            int hsLopID = (hsLopObj != null && hsLopObj != DBNull.Value) ? Convert.ToInt32(hsLopObj) : 0;

            if (!daCoDon && hsLopID != lopHocID)
            {
                thongBao = "Bạn chưa đăng ký lớp học này!";
                return false;
            }

            if (daCoDon && ttDk == "Đã hủy")
            {
                thongBao = "Bạn đã hủy đăng ký lớp này trước đó rồi!";
                return false;
            }

            // Xóa bản ghi đăng ký
            if (daCoDon)
            {
                DatabaseHelper.ExecuteNonQuery("DELETE FROM DangKyLop WHERE DangKyID = @dkID", new NpgsqlParameter("@dkID", dkID));
            }

            // Nếu học sinh đang được gán lớp thì xóa gán
            if (hsLopID == lopHocID)
            {
                DatabaseHelper.ExecuteNonQuery("UPDATE HocSinh SET LopHocID = NULL WHERE HocSinhID = @hsID", new NpgsqlParameter("@hsID", hocSinhID));
            }

            thongBao = "Hủy đăng ký lớp học thành công!";
            return true;
        }
    }
}
