-- ===================================================================
-- SCRIPT 02: DỮ LIỆU MẪU ĐỂ CHẠY THỬ NGHIỆM ĐẦY ĐỦ CÁC TÍNH NĂNG
-- Dự án: BLT_Quan_Ly_Hoc_Tap
-- Mật khẩu mặc định của tất cả tài khoản: 123456
-- Mã băm SHA-256 của '123456': 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92
-- ===================================================================

USE [BTL_QL_Hoc_Tap];
GO

-- 1. Thêm Giáo Viên mẫu
IF NOT EXISTS (SELECT 1 FROM GiaoVien WHERE Email = 'gv1@school.edu.vn')
BEGIN
    INSERT INTO GiaoVien (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, ChuyenMon, GioiTinh, TaiKhoanID) 
    VALUES (N'Nguyễn Văn Thầy', 'gv1@school.edu.vn', '0901234567', N'Hà Nội', '1988-05-15', N'Công nghệ thông tin', N'Nam', 2);
    PRINT N'-> Đã thêm Giáo viên mẫu.';
END
GO

-- 2. Thêm Khóa Học mẫu
IF NOT EXISTS (SELECT 1 FROM KhoaHoc WHERE TenKhoaHoc = N'Lập trình C# WinForms')
BEGIN
    INSERT INTO KhoaHoc (TenKhoaHoc, MoTa) 
    VALUES (N'Lập trình C# WinForms', N'Khóa học lập trình trực quan C# WinForms từ cơ bản đến nâng cao');
    PRINT N'-> Đã thêm Khóa học mẫu.';
END
GO

-- 3. Thêm Lớp Học mẫu
IF NOT EXISTS (SELECT 1 FROM LopHoc WHERE TenLop = N'CNTT-K65')
BEGIN
    DECLARE @gvID INT = (SELECT TOP 1 GiaoVienID FROM GiaoVien WHERE Email = 'gv1@school.edu.vn');
    DECLARE @khID INT = (SELECT TOP 1 KhoaHocID FROM KhoaHoc WHERE TenKhoaHoc = N'Lập trình C# WinForms');

    INSERT INTO LopHoc (TenLop, GiaoVienID, KhoaHocID, NgayBatDau, NgayKetThuc, SiSoToiDa, LichHoc, TrangThai)
    VALUES (N'CNTT-K65', ISNULL(@gvID, 1), ISNULL(@khID, 1), '2026-01-01', '2026-06-01', 40, N'Thứ 2, 4, 6 (18:00 - 20:00)', N'Đang mở');
    PRINT N'-> Đã thêm Lớp học mẫu.';
END
GO

-- 4. Thêm Học Sinh mẫu
IF NOT EXISTS (SELECT 1 FROM HocSinh WHERE Email = 'hs1@student.edu.vn')
BEGIN
    DECLARE @lopID INT = (SELECT TOP 1 LopHocID FROM LopHoc WHERE TenLop = N'CNTT-K65');

    INSERT INTO HocSinh (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, GioiTinh, LopHocID, TaiKhoanID)
    VALUES (N'Trần Thị Trò', 'hs1@student.edu.vn', '0987654321', N'Hà Nội', '2006-09-18', N'Nữ', ISNULL(@lopID, 1), 3);
    PRINT N'-> Đã thêm Học sinh mẫu.';
END
GO

-- 5. Thêm Đăng Ký Lớp mẫu
IF NOT EXISTS (SELECT 1 FROM DangKyLop WHERE LopHocID = 1 AND HocSinhID = 1)
BEGIN
    INSERT INTO DangKyLop (LopHocID, HocSinhID, TrangThai, NgayDangKy) 
    VALUES (1, 1, N'Đã duyệt', GETDATE());
    PRINT N'-> Đã thêm Đăng ký lớp mẫu.';
END
GO

-- 6. Thêm Tài Khoản mẫu (Admin, Giáo Viên, Học Sinh)
-- Mật khẩu: 123456
IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'admin')
BEGIN
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Admin', 0, 0);
    PRINT N'-> Đã thêm Tài khoản Admin.';
END

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'giaovien1')
BEGIN
    DECLARE @gvID2 INT = (SELECT TOP 1 GiaoVienID FROM GiaoVien WHERE Email = 'gv1@school.edu.vn');
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('giaovien1', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'GiaoVien', ISNULL(@gvID2, 1), 0);
    PRINT N'-> Đã thêm Tài khoản Giáo viên.';
END

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'hocsinh1')
BEGIN
    DECLARE @hsID2 INT = (SELECT TOP 1 HocSinhID FROM HocSinh WHERE Email = 'hs1@student.edu.vn');
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('hocsinh1', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'HocSinh', ISNULL(@hsID2, 1), 0);
    PRINT N'-> Đã thêm Tài khoản Học sinh.';
END
GO

-- 7. Thêm Đề Thi mẫu
IF NOT EXISTS (SELECT 1 FROM DeThi WHERE TenDeThi = N'Kiểm tra giữa kỳ C# WinForms')
BEGIN
    INSERT INTO DeThi (TenDeThi, MoTa, ThoiGian, MonHoc, SoCau, GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai)
    VALUES (N'Kiểm tra giữa kỳ C# WinForms', N'Bài thi trắc nghiệm kiến thức C# & Windows Forms', 15, N'Lập trình C# WinForms', 5, 1, 1, GETDATE(), DATEADD(day, 30, GETDATE()), N'Đang mở');
    PRINT N'-> Đã thêm Đề thi mẫu.';
END
GO

-- 8. Thêm Câu Hỏi mẫu
IF NOT EXISTS (SELECT 1 FROM CauHoi WHERE DeThiID = 1)
BEGIN
    DECLARE @deID INT = (SELECT TOP 1 DeThiID FROM DeThi WHERE TenDeThi = N'Kiểm tra giữa kỳ C# WinForms');
    SET @deID = ISNULL(@deID, 1);

    INSERT INTO CauHoi (NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, DeThiID, ThuTu) VALUES
    (N'Trong C#, ký hiệu nào được sử dụng để khai báo một lớp kế thừa từ một lớp cha?', N':', N'inherits', N'extends', N'implements', 'A', @deID, 1),
    (N'Trong Windows Forms, sự kiện mặc định phát sinh khi nhấp đúp vào Button trong cửa sổ thiết kế là gì?', N'DoubleClick', N'Click', N'MouseClick', N'Hover', 'B', @deID, 2),
    (N'Control nào thường được sử dụng nhiều nhất để hiển thị và chỉnh sửa bảng dữ liệu trong WinForms?', N'ListBox', N'ListView', N'DataGridView', N'TableLayout', 'C', @deID, 3),
    (N'Trong kiến trúc phân lớp (3-Tier), tầng nào đảm nhiệm việc giao tiếp trực tiếp với cơ sở dữ liệu?', N'GUI (Presentation)', N'BLL (Business Logic)', N'DAL (Data Access Layer)', N'DTO / Model', 'C', @deID, 4),
    (N'Câu lệnh SQL nào dùng để trích xuất dữ liệu từ một hoặc nhiều bảng?', N'INSERT', N'UPDATE', N'DELETE', N'SELECT', 'D', @deID, 5);
    PRINT N'-> Đã thêm 5 câu hỏi trắc nghiệm mẫu.';
END
GO

-- 9. Thêm Kết Quả Thi mẫu
IF NOT EXISTS (SELECT 1 FROM KetQuaThi WHERE HocSinhID = 1 AND DeThiID = 1)
BEGIN
    INSERT INTO KetQuaThi (HocSinhID, DeThiID, Diem, SoCauDung, ThoiGianNop, TrangThai)
    VALUES (1, 1, 10.0, 5, GETDATE(), N'Hoàn thành');
    PRINT N'-> Đã thêm Kết quả thi mẫu.';
END
GO

-- 10. Thêm Chi Tiết Bài Làm mẫu
IF NOT EXISTS (SELECT 1 FROM ChiTietBaiLam WHERE KetQuaID = 1)
BEGIN
    INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) VALUES
    (1, 1, 'A', 1),
    (1, 2, 'B', 1),
    (1, 3, 'C', 1),
    (1, 4, 'C', 1),
    (1, 5, 'D', 1);
    PRINT N'-> Đã thêm Chi tiết bài làm mẫu.';
END
GO

PRINT N'======================================================';
PRINT N'-> HOÀN TẤT THÊM DỮ LIỆU MẪU CHO DỰ ÁN [BTL_QL_Hoc_Tap]';
PRINT N'-> TÀI KHOẢN MẶC ĐỊNH:';
PRINT N'   1. Admin     : admin      / 123456';
PRINT N'   2. Giáo viên : giaovien1  / 123456';
PRINT N'   3. Học sinh  : hocsinh1   / 123456';
PRINT N'======================================================';
