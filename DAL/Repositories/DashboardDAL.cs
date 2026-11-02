using System.Data;
using BLT_Quan_Ly_Hoc_Tap.Database;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public class DashboardDAL
    {
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
                SELECT k.KhoaHocID, k.TenKhoaHoc, COUNT(h.HocSinhID) AS SoHocVien
                FROM KhoaHoc k
                LEFT JOIN LopHoc l ON l.KhoaHocID = k.KhoaHocID
                LEFT JOIN HocSinh h ON h.LopHocID = l.LopHocID
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
                       COUNT(h.HocSinhID) AS SoHocVien,
                       COALESCE(l.SiSoToiDa, 40) AS SiSoToiDa
                FROM LopHoc l
                LEFT JOIN GiaoVien g ON g.GiaoVienID = l.GiaoVienID
                LEFT JOIN HocSinh h ON h.LopHocID = l.LopHocID
                GROUP BY l.LopHocID, l.TenLop, g.HoTen, l.SiSoToiDa
                ORDER BY l.LopHocID DESC
                LIMIT 5");
        }
    }
}
