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
    public class LopHocDAL
    {
        public DataTable LayTatCa()
        {
            string sql = @"
                SELECT 
                    l.LopHocID, 
                    l.TenLop, 
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID) AS VARCHAR(10)) + '/' + CAST(ISNULL(l.SiSoToiDa, 40) AS VARCHAR(10)) AS SiSo,
                    ISNULL(k.TenKhoaHoc, N'Chưa phân loại') AS TenKhoaHoc, 
                    ISNULL(g.HoTen, N'Chưa phân công') AS TenGiaoVien, 
                    l.KhoaHocID, 
                    l.GiaoVienID, 
                    ISNULL(l.SiSoToiDa, 40) AS SiSoToiDa,
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
                    CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID) AS VARCHAR(10)) + '/' + CAST(ISNULL(l.SiSoToiDa, 40) AS VARCHAR(10)) AS SiSo,
                    k.TenKhoaHoc, 
                    l.LichHoc,
                    l.TrangThai
                FROM LopHoc l
                LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
                WHERE l.GiaoVienID = @gvID";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@gvID", gvID) });
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
                    ISNULL(l.TrangThai, N'Đang mở') AS TrangThai
                FROM LopHoc l
                LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
                WHERE l.LopHocID IN (
                    SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID
                    UNION
                    SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID
                )";
            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@hsID", hsID) });
        }

        public bool ThemLop(LopHoc lop)
        {
            string sql = @"INSERT INTO LopHoc (KhoaHocID, GiaoVienID, TenLop, LichHoc, SiSoToiDa, TrangThai) 
                           VALUES (@khID, @gvID, @ten, @lich, @siso, @tt)";
            SqlParameter[] pars = {
                new SqlParameter("@khID", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value),
                new SqlParameter("@gvID", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value),
                new SqlParameter("@ten", lop.TenLop),
                new SqlParameter("@lich", lop.LichHoc ?? ""),
                new SqlParameter("@siso", lop.SiSoToiDa > 0 ? lop.SiSoToiDa : 40),
                new SqlParameter("@tt", lop.TrangThai ?? "Đang mở")
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
            SqlParameter[] pars = {
                new SqlParameter("@khID", lop.KhoaHocID > 0 ? (object)lop.KhoaHocID : DBNull.Value),
                new SqlParameter("@gvID", lop.GiaoVienID > 0 ? (object)lop.GiaoVienID : DBNull.Value),
                new SqlParameter("@ten", lop.TenLop),
                new SqlParameter("@siso", lop.SiSoToiDa > 0 ? lop.SiSoToiDa : 40),
                new SqlParameter("@id", lop.LopHocID)
            };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }

        public bool XoaLop(int lopID)
        {
            // Kiểm tra xem có học sinh nào đang trong lớp không
            string checkSql = "SELECT COUNT(*) FROM HocSinh WHERE LopHocID = @id";
            int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new SqlParameter[] { new SqlParameter("@id", lopID) }));
            if (count > 0)
            {
                throw new Exception("Không thể xóa lớp vì đang có học sinh thuộc lớp này!");
            }

            string sql = "DELETE FROM LopHoc WHERE LopHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", lopID) }) > 0;
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
        WHERE dk.LopHocID = @lopID AND dk.TrangThai = N'Chờ duyệt'
        ORDER BY dk.NgayDangKy ASC";

            return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@lopID", lopHocID) });
        }
        public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID)
        {
            string sql = @"
        UPDATE DangKyLop SET TrangThai = N'Đã duyệt' WHERE DangKyID = @dkID;
        UPDATE HocSinh SET LopHocID = @lopID WHERE HocSinhID = @hsID;";

            SqlParameter[] pars = {
        new SqlParameter("@dkID", dangKyID),
        new SqlParameter("@lopID", lopHocID),
        new SqlParameter("@hsID", hocSinhID)
                };
            return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
        }
        public bool TuChoiDangKy(int dangKyID)
        {
            string sql = "UPDATE DangKyLop SET TrangThai = N'Từ chối' WHERE DangKyID = @dkID";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@dkID", dangKyID) }) > 0;
        }
        public bool DongDangKyLop(int lopHocID)
        {
            string sql = "UPDATE LopHoc SET TrangThai = N'Đã đóng' WHERE LopHocID = @id";
            return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", lopHocID) }) > 0;
        }
    }
}
