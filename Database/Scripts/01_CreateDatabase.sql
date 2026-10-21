-- ===================================================================
-- SCRIPT 01: KHỞI TẠO CƠ SỞ DỮ LIỆU & BẢNG CHO DỰ ÁN QUẢN LÝ HỌC TẬP
-- Dự án: BLT_Quan_Ly_Hoc_Tap
-- ===================================================================

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
    PRINT N'-> Database [BTL_QL_Hoc_Tap] đã tồn tại.';
END
GO

USE [BTL_QL_Hoc_Tap];
GO

-- 2. Bảng Tài Khoản (TaiKhoan)
IF OBJECT_ID('TaiKhoan', 'U') IS NULL
BEGIN
    CREATE TABLE TaiKhoan (
        TaiKhoanID INT IDENTITY(1,1) PRIMARY KEY,
        TenDangNhap NVARCHAR(50) NOT NULL UNIQUE,
        MatKhau NVARCHAR(255) NOT NULL,
        VaiTro NVARCHAR(20) NOT NULL, -- Admin, GiaoVien, HocSinh
        NguoiDungID INT DEFAULT 0,
        TrangThaiKhoa BIT DEFAULT 0
    );
    PRINT N'-> Đã tạo bảng [TaiKhoan].';
END
GO

-- 3. Bảng Giáo Viên (GiaoVien)
IF OBJECT_ID('GiaoVien', 'U') IS NULL
BEGIN
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
    PRINT N'-> Đã tạo bảng [GiaoVien].';
END
GO

-- 4. Bảng Khóa Học (KhoaHoc)
IF OBJECT_ID('KhoaHoc', 'U') IS NULL
BEGIN
    CREATE TABLE KhoaHoc (
        KhoaHocID INT IDENTITY(1,1) PRIMARY KEY,
        TenKhoaHoc NVARCHAR(100) NOT NULL,
        MoTa NVARCHAR(255)
    );
    PRINT N'-> Đã tạo bảng [KhoaHoc].';
END
GO

-- 5. Bảng Lớp Học (LopHoc)
IF OBJECT_ID('LopHoc', 'U') IS NULL
BEGIN
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
    PRINT N'-> Đã tạo bảng [LopHoc].';
END
GO

-- 6. Bảng Học Sinh (HocSinh)
IF OBJECT_ID('HocSinh', 'U') IS NULL
BEGIN
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
    PRINT N'-> Đã tạo bảng [HocSinh].';
END
GO

-- 7. Bảng Đăng Ký Lớp (DangKyLop)
IF OBJECT_ID('DangKyLop', 'U') IS NULL
BEGIN
    CREATE TABLE DangKyLop (
        DangKyID INT IDENTITY(1,1) PRIMARY KEY,
        LopHocID INT,
        HocSinhID INT,
        TrangThai NVARCHAR(50) DEFAULT N'Đã duyệt',
        NgayDangKy DATETIME DEFAULT GETDATE()
    );
    PRINT N'-> Đã tạo bảng [DangKyLop].';
END
GO

-- 8. Bảng Đề Thi (DeThi)
IF OBJECT_ID('DeThi', 'U') IS NULL
BEGIN
    CREATE TABLE DeThi (
        DeThiID INT IDENTITY(1,1) PRIMARY KEY,
        TenDeThi NVARCHAR(150) NOT NULL,
        MoTa NVARCHAR(255),
        ThoiGian INT NOT NULL, -- Số phút
        MonHoc NVARCHAR(100),
        SoCau INT DEFAULT 20,
        GiaoVienID INT,
        LopHocID INT,
        NgayTao DATETIME DEFAULT GETDATE(),
        HanNop DATETIME NULL,
        TrangThai NVARCHAR(50) DEFAULT N'Đang mở'
    );
    PRINT N'-> Đã tạo bảng [DeThi].';
END
GO

-- 9. Bảng Câu Hỏi (CauHoi)
IF OBJECT_ID('CauHoi', 'U') IS NULL
BEGIN
    CREATE TABLE CauHoi (
        CauHoiID INT IDENTITY(1,1) PRIMARY KEY,
        NoiDung NVARCHAR(MAX) NOT NULL,
        DapAnA NVARCHAR(255),
        DapAnB NVARCHAR(255),
        DapAnC NVARCHAR(255),
        DapAnD NVARCHAR(255),
        DapAnDung NVARCHAR(5) NOT NULL, -- 'A', 'B', 'C', hoặc 'D'
        DeThiID INT,
        ThuTu INT DEFAULT 1
    );
    PRINT N'-> Đã tạo bảng [CauHoi].';
END
GO

-- 10. Bảng Kết Quả Thi (KetQuaThi)
IF OBJECT_ID('KetQuaThi', 'U') IS NULL
BEGIN
    CREATE TABLE KetQuaThi (
        KetQuaID INT IDENTITY(1,1) PRIMARY KEY,
        HocSinhID INT,
        DeThiID INT,
        Diem FLOAT,
        SoCauDung INT,
        ThoiGianNop DATETIME DEFAULT GETDATE(),
        TrangThai NVARCHAR(50) DEFAULT N'Hoàn thành'
    );
    PRINT N'-> Đã tạo bảng [KetQuaThi].';
END
GO

-- 11. Bảng Chi Tiết Bài Làm (ChiTietBaiLam)
IF OBJECT_ID('ChiTietBaiLam', 'U') IS NULL
BEGIN
    CREATE TABLE ChiTietBaiLam (
        ChiTietID INT IDENTITY(1,1) PRIMARY KEY,
        KetQuaID INT,
        CauHoiID INT,
        DapAnChon NVARCHAR(10),
        LaDung BIT
    );
    PRINT N'-> Đã tạo bảng [ChiTietBaiLam].';
END
GO

PRINT N'======================================================';
PRINT N'-> HOÀN TẤT KHỞI TẠO CẤU TRÚC DATABASE [BTL_QL_Hoc_Tap]';
PRINT N'======================================================';
