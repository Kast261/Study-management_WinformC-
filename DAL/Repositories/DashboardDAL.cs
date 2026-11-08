using System.Data;
using Npgsql;
using BLT_Quan_Ly_Hoc_Tap.Database;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class DashboardDAL
    {
        public DataTable LayTongQuanGiaoVien(int giaoVienID)
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT (SELECT COUNT(*) FROM LopHoc WHERE GiaoVienID=@gv) AS SoLopHoc,
                       (SELECT COUNT(DISTINCT h.HocSinhID)
                        FROM HocSinh h JOIN LopHoc l ON h.LopHocID=l.LopHocID OR EXISTS (
                            SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=h.HocSinhID
                              AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')
                        WHERE l.GiaoVienID=@gv) AS SoHocVien,
                       (SELECT COUNT(*) FROM DangKyLop dk
                        JOIN LopHoc l ON l.LopHocID=dk.LopHocID
                        JOIN HocSinh h ON h.HocSinhID=dk.HocSinhID
                        WHERE l.GiaoVienID=@gv AND (dk.TrangThai='Chờ duyệt' OR dk.TrangThai IS NULL)) AS SoChoDuyet,
                       (SELECT COUNT(*) FROM DeThi d JOIN LopHoc l ON l.LopHocID=d.LopHocID
                        WHERE d.GiaoVienID=@gv AND d.TrangThai='Đang mở' AND d.ThoiGian>0
                          AND (d.HanNop IS NULL OR d.HanNop>CURRENT_TIMESTAMP)
                          AND EXISTS (SELECT 1 FROM CauHoi c WHERE c.DeThiID=d.DeThiID)) AS SoBaiThiDangMo",
                new NpgsqlParameter("@gv", giaoVienID));
        }

        public DataTable LayTongQuan()
        {
            return DatabaseHelper.ExecuteQuery(@"
                SELECT (SELECT COUNT(*) FROM HocSinh) AS SoHocVien,
                       (SELECT COUNT(*) FROM GiaoVien) AS SoGiaoVien,
                       (SELECT COUNT(*) FROM KhoaHoc) AS SoKhoaHoc,
                       (SELECT COUNT(*) FROM LopHoc) AS SoLopHoc");
        }

        public DataTable LayHocVienTheoKhoaHoc()
        {
            // HocSinh.LopHocID là lớp hiện tại; không tính đăng ký đang chờ duyệt.
            return DatabaseHelper.ExecuteQuery(@"
                SELECT k.KhoaHocID, k.TenKhoaHoc, COUNT(DISTINCT h.HocSinhID) AS SoHocVien
                FROM KhoaHoc k
                LEFT JOIN LopHoc l ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN HocSinh h ON h.LopHocID = l.LopHocID OR EXISTS (
                    SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=h.HocSinhID
                      AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')
                GROUP BY k.KhoaHocID, k.TenKhoaHoc
                ORDER BY SoHocVien DESC, k.TenKhoaHoc, k.KhoaHocID
                LIMIT 6");
        }

        public DataTable LayLopMoiNhat()
        {
            // Chưa có NgayTao trong schema: dùng ID tăng dần, không dùng NgayBatDau.
            return DatabaseHelper.ExecuteQuery(@"
                SELECT l.LopHocID, l.TenLop,
                       COALESCE(g.HoTen, 'Chưa phân công') AS TenGiaoVien,
                       COUNT(DISTINCT h.HocSinhID) AS SoHocVien,
                       COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa
                FROM LopHoc l
                LEFT JOIN GiaoVien g ON g.GiaoVienID = l.GiaoVienID
                LEFT JOIN HocSinh h ON h.LopHocID = l.LopHocID OR EXISTS (
                    SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=h.HocSinhID
                      AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt')
                GROUP BY l.LopHocID, l.TenLop, g.HoTen, l.SiSoToiDa
                ORDER BY l.LopHocID DESC
                LIMIT 5");
        }
    }
}
