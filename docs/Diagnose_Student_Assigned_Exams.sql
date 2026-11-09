-- Chỉ đọc dữ liệu; không lấy mật khẩu và không thay đổi database.
-- Chạy trong SQL Editor của database mà ứng dụng đang sử dụng.
WITH student AS (
    SELECT tk.TaiKhoanID, tk.NguoiDungID, hs.HocSinhID,
           hs.LopHocID AS LopChinhID
    FROM TaiKhoan tk
    LEFT JOIN HocSinh hs ON hs.HocSinhID = NULLIF(tk.NguoiDungID, 0)
        OR (COALESCE(tk.NguoiDungID, 0) = 0 AND hs.TaiKhoanID = tk.TaiKhoanID)
    WHERE LOWER(BTRIM(tk.TenDangNhap)) = LOWER('HungHocVien')
)
SELECT s.TaiKhoanID, s.NguoiDungID, s.HocSinhID, s.LopChinhID,
       l.LopHocID, l.TenLop, l.GiaoVienID,
       (SELECT STRING_AGG(tk.TenDangNhap, ', ')
        FROM TaiKhoan tk
        WHERE LOWER(BTRIM(tk.VaiTro)) = 'giaovien'
          AND (tk.NguoiDungID = l.GiaoVienID OR
              (COALESCE(tk.NguoiDungID,0)=0 AND EXISTS
               (SELECT 1 FROM GiaoVien gv WHERE gv.GiaoVienID=l.GiaoVienID
                    AND gv.TaiKhoanID=tk.TaiKhoanID)))) AS TaiKhoanGiaoVien,
       (SELECT STRING_AGG(COALESCE(dk.TrangThai,'NULL'), ', ')
        FROM DangKyLop dk WHERE dk.HocSinhID=s.HocSinhID
          AND dk.LopHocID=l.LopHocID) AS TrangThaiDangKy,
       d.DeThiID, d.TenDeThi, QUOTE_NULLABLE(d.TrangThai) AS TrangThaiDeThi,
       (SELECT COUNT(*) FROM CauHoi c WHERE c.DeThiID=d.DeThiID) AS SoCauDaLuu,
       d.ThoiGian, d.HanNop,
       CASE
           WHEN s.HocSinhID IS NULL THEN 'Tài khoản chưa liên kết đúng hồ sơ học viên'
           WHEN NOT (COALESCE(s.LopChinhID=l.LopHocID,FALSE) OR EXISTS
                (SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID=s.HocSinhID
                 AND dk.LopHocID=l.LopHocID AND dk.TrangThai='Đã duyệt'))
                THEN 'Học viên chưa thuộc mã lớp này: kiểm tra lớp trùng tên'
           WHEN d.DeThiID IS NULL THEN 'Chưa có lượt đề thi được lưu cho mã lớp này'
           WHEN d.TrangThai NOT IN ('Đang mở','Đã đóng') OR d.TrangThai IS NULL
                THEN 'Trạng thái đề chưa nằm trong danh sách đã giao'
           WHEN d.TrangThai='Đã đóng' THEN 'Hiện trong tab Bài thi; đã đóng'
           WHEN d.HanNop<=CURRENT_TIMESTAMP THEN 'Hiện trong tab Bài thi; đã hết hạn'
           WHEN d.ThoiGian<=0 OR NOT EXISTS
                (SELECT 1 FROM CauHoi c WHERE c.DeThiID=d.DeThiID)
                THEN 'Hiện trong tab Bài thi; chưa đủ điều kiện bắt đầu làm'
           ELSE 'Đủ điều kiện hiện bài thi'
       END AS ChanDoan
FROM student s
CROSS JOIN LopHoc l
LEFT JOIN DeThi d ON d.LopHocID=l.LopHocID
WHERE LOWER(BTRIM(l.TenLop))=LOWER('Toán Lớp 2')
ORDER BY l.LopHocID, d.DeThiID;
