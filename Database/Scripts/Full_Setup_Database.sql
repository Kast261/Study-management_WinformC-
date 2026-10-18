-- =====================================================================================
-- SCRIPT TỔNG HỢP: TẠO DATABASE + BẢNG + DỮ LIỆU MẪU (1 CLICK EXECUTE)
-- DỰ ÁN: BLT_Quan_Ly_Hoc_Tap
-- HƯỚNG DẪN: Mở file này trong SQL Server Management Studio (SSMS) và nhấn F5 (Execute)
-- =====================================================================================

USE master;
GO

-- 1. Tạo Database nếu chưa tồn tại
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'BTL_QL_Hoc_Tap')
BEGIN
    CREATE DATABASE [BTL_QL_Hoc_Tap];
    PRINT N'-> Đã tạo database [BTL_QL_Hoc_Tap] thành công.';
END
ELSE
BEGIN
    PRINT N'-> Database [BTL_QL_Hoc_Tap] đã sẵn sàng.';
END
GO

USE [BTL_QL_Hoc_Tap];
GO

-- 2. Tạo các bảng
IF OBJECT_ID('TaiKhoan', 'U') IS NULL
CREATE TABLE TaiKhoan (
    TaiKhoanID INT IDENTITY(1,1) PRIMARY KEY,
    TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
    MatKhau NVARCHAR(255) NOT NULL,
    VaiTro NVARCHAR(20) NOT NULL,
    NguoiDungID INT DEFAULT 0,
    TrangThaiKhoa BIT DEFAULT 0
);

IF OBJECT_ID('GiaoVien', 'U') IS NULL
CREATE TABLE GiaoVien (
    GiaoVienID INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255),
    NgaySinh DATE,
    ChuyenMon NVARCHAR(100),
    GioiTinh NVARCHAR(10),
    TaiKhoanID INT NULL
);

IF OBJECT_ID('KhoaHoc', 'U') IS NULL
CREATE TABLE KhoaHoc (
    KhoaHocID INT IDENTITY(1,1) PRIMARY KEY,
    TenKhoaHoc NVARCHAR(100) NOT NULL,
    MoTa NVARCHAR(255)
);

IF OBJECT_ID('LopHoc', 'U') IS NULL
CREATE TABLE LopHoc (
    LopHocID INT IDENTITY(1,1) PRIMARY KEY,
    TenLop NVARCHAR(100) NOT NULL,
    GiaoVienID INT NULL,
    KhoaHocID INT NULL,
    NgayBatDau DATE,
    NgayKetThuc DATE,
    SiSoToiDa INT DEFAULT 40,
    LichHoc NVARCHAR(100),
    TrangThai NVARCHAR(50) DEFAULT N'Đang mở'
);

IF OBJECT_ID('HocSinh', 'U') IS NULL
CREATE TABLE HocSinh (
    HocSinhID INT IDENTITY(1,1) PRIMARY KEY,
    HoTen NVARCHAR(100) NOT NULL,
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255),
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    LopHocID INT NULL,
    TaiKhoanID INT NULL
);

IF OBJECT_ID('DangKyLop', 'U') IS NULL
CREATE TABLE DangKyLop (
    DangKyID INT IDENTITY(1,1) PRIMARY KEY,
    LopHocID INT,
    HocSinhID INT,
    TrangThai NVARCHAR(50) DEFAULT N'Đã duyệt',
    NgayDangKy DATETIME DEFAULT GETDATE()
);

IF OBJECT_ID('DeThi', 'U') IS NULL
CREATE TABLE DeThi (
    DeThiID INT IDENTITY(1,1) PRIMARY KEY,
    TenDeThi NVARCHAR(150) NOT NULL,
    MoTa NVARCHAR(255),
    ThoiGian INT NOT NULL,
    MonHoc NVARCHAR(100),
    SoCau INT DEFAULT 20,
    GiaoVienID INT,
    LopHocID INT,
    NgayTao DATETIME DEFAULT GETDATE(),
    HanNop DATETIME NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Đang mở'
);

IF OBJECT_ID('CauHoi', 'U') IS NULL
CREATE TABLE CauHoi (
    CauHoiID INT IDENTITY(1,1) PRIMARY KEY,
    NoiDung NVARCHAR(MAX) NOT NULL,
    DapAnA NVARCHAR(255),
    DapAnB NVARCHAR(255),
    DapAnC NVARCHAR(255),
    DapAnD NVARCHAR(255),
    DapAnDung NVARCHAR(5) NOT NULL,
    DeThiID INT,
    ThuTu INT DEFAULT 1
);

IF OBJECT_ID('KetQuaThi', 'U') IS NULL
CREATE TABLE KetQuaThi (
    KetQuaID INT IDENTITY(1,1) PRIMARY KEY,
    HocSinhID INT,
    DeThiID INT,
    Diem FLOAT,
    SoCauDung INT,
    ThoiGianNop DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Hoàn thành'
);

IF OBJECT_ID('ChiTietBaiLam', 'U') IS NULL
CREATE TABLE ChiTietBaiLam (
    ChiTietID INT IDENTITY(1,1) PRIMARY KEY,
    KetQuaID INT,
    CauHoiID INT,
    DapAnChon NVARCHAR(10),
    LaDung BIT
);
GO

-- 3. Khởi tạo dữ liệu mẫu
-- Mật khẩu: 123456 (SHA-256: 8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92)
IF NOT EXISTS (SELECT 1 FROM GiaoVien WHERE Email = 'gv1@school.edu.vn')
    INSERT INTO GiaoVien (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, ChuyenMon, GioiTinh, TaiKhoanID) 
    VALUES (N'Nguyễn Văn Thầy', 'gv1@school.edu.vn', '0901234567', N'Hà Nội', '1988-05-15', N'Công nghệ thông tin', N'Nam', 2);

IF NOT EXISTS (SELECT 1 FROM KhoaHoc WHERE TenKhoaHoc = N'Lập trình C# WinForms')
    INSERT INTO KhoaHoc (TenKhoaHoc, MoTa) 
    VALUES (N'Lập trình C# WinForms', N'Khóa học lập trình trực quan C# WinForms từ cơ bản đến nâng cao');

IF NOT EXISTS (SELECT 1 FROM LopHoc WHERE TenLop = N'CNTT-K65')
    INSERT INTO LopHoc (TenLop, GiaoVienID, KhoaHocID, NgayBatDau, NgayKetThuc, SiSoToiDa, LichHoc, TrangThai)
    VALUES (N'CNTT-K65', 1, 1, '2026-01-01', '2026-06-01', 40, N'Thứ 2, 4, 6 (18:00 - 20:00)', N'Đang mở');

IF NOT EXISTS (SELECT 1 FROM HocSinh WHERE Email = 'hs1@student.edu.vn')
    INSERT INTO HocSinh (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, GioiTinh, LopHocID, TaiKhoanID)
    VALUES (N'Trần Thị Trò', 'hs1@student.edu.vn', '0987654321', N'Hà Nội', '2006-09-18', N'Nữ', 1, 3);

IF NOT EXISTS (SELECT 1 FROM DangKyLop WHERE LopHocID = 1 AND HocSinhID = 1)
    INSERT INTO DangKyLop (LopHocID, HocSinhID, TrangThai, NgayDangKy) 
    VALUES (1, 1, N'Đã duyệt', GETDATE());

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'admin')
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('admin', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'Admin', 0, 0);

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'giaovien1')
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('giaovien1', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'GiaoVien', 1, 0);

IF NOT EXISTS (SELECT 1 FROM TaiKhoan WHERE TenDangNhap = 'hocsinh1')
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) 
    VALUES ('hocsinh1', '8d969eef6ecad3c29a3a629280e686cf0c3f5d5a86aff3ca12020c923adc6c92', 'HocSinh', 1, 0);

IF NOT EXISTS (SELECT 1 FROM DeThi WHERE TenDeThi = N'Kiểm tra giữa kỳ C# WinForms')
    INSERT INTO DeThi (TenDeThi, MoTa, ThoiGian, MonHoc, SoCau, GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai)
    VALUES (N'Kiểm tra giữa kỳ C# WinForms', N'Bài thi trắc nghiệm kiến thức C# & Windows Forms', 15, N'Lập trình C# WinForms', 5, 1, 1, GETDATE(), DATEADD(day, 30, GETDATE()), N'Đang mở');

IF NOT EXISTS (SELECT 1 FROM CauHoi WHERE DeThiID = 1)
    INSERT INTO CauHoi (NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, DeThiID, ThuTu) VALUES
    (N'Trong C#, ký hiệu nào được sử dụng để khai báo một lớp kế thừa từ một lớp cha?', N':', N'inherits', N'extends', N'implements', 'A', 1, 1),
    (N'Trong Windows Forms, sự kiện mặc định phát sinh khi nhấp đúp vào Button trong cửa sổ thiết kế là gì?', N'DoubleClick', N'Click', N'MouseClick', N'Hover', 'B', 1, 2),
    (N'Control nào thường được sử dụng nhiều nhất để hiển thị và chỉnh sửa bảng dữ liệu trong WinForms?', N'ListBox', N'ListView', N'DataGridView', N'TableLayout', 'C', 1, 3),
    (N'Trong kiến trúc phân lớp (3-Tier), tầng nào đảm nhiệm việc giao tiếp trực tiếp với cơ sở dữ liệu?', N'GUI (Presentation)', N'BLL (Business Logic)', N'DAL (Data Access Layer)', N'DTO / Model', 'C', 1, 4),
    (N'Câu lệnh SQL nào dùng để trích xuất dữ liệu từ một hoặc nhiều bảng?', N'INSERT', N'UPDATE', N'DELETE', N'SELECT', 'D', 1, 5);

IF NOT EXISTS (SELECT 1 FROM KetQuaThi WHERE HocSinhID = 1 AND DeThiID = 1)
    INSERT INTO KetQuaThi (HocSinhID, DeThiID, Diem, SoCauDung, ThoiGianNop, TrangThai)
    VALUES (1, 1, 10.0, 5, GETDATE(), N'Hoàn thành');

IF NOT EXISTS (SELECT 1 FROM ChiTietBaiLam WHERE KetQuaID = 1)
    INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) VALUES
    (1, 1, 'A', 1),
    (1, 2, 'B', 1),
    (1, 3, 'C', 1),
    (1, 4, 'C', 1),
    (1, 5, 'D', 1);
GO

PRINT N'====================================================================';
PRINT N'-> HOÀN TẤT THIẾT LẬP DATABASE [BTL_QL_Hoc_Tap] VÀ TẠO DỮ LIỆU MẪU';
PRINT N'-> TÀI KHOẢN ĐĂNG NHẬP THỬ NGHIỆM:';
PRINT N'   1. Admin       : admin       /  123456';
PRINT N'   2. Giáo viên   : giaovien1   /  123456';
PRINT N'   3. Học sinh    : hocsinh1    /  123456';
PRINT N'====================================================================';
