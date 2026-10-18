using BLT_Quan_Ly_Hoc_Tap.Helpers;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class CreateDataBase
    {
        public static void Init()
        {
            try
            {
                // 1. Kiểm tra và kết nối vào SQL Server (hỗ trợ tự động fallback nếu cấu hình chưa khớp máy)
                SqlConnectionStringBuilder originalBuilder = new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString);
                string dbName = string.IsNullOrWhiteSpace(originalBuilder.InitialCatalog) ? "BTL_QL_Hoc_Tap" : originalBuilder.InitialCatalog;

                string[] candidateServers = new string[]
                {
                    originalBuilder.DataSource,
                    ".",
                    "(local)",
                    @".\SQLEXPRESS",
                    @"(localdb)\MSSQLLocalDB",
                    "localhost"
                };

                bool connected = false;
                string workingDataSource = originalBuilder.DataSource;

                foreach (string server in candidateServers.Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(server)) continue;
                    try
                    {
                        SqlConnectionStringBuilder testBuilder = new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString)
                        {
                            DataSource = server,
                            InitialCatalog = "master",
                            ConnectTimeout = 3
                        };

                        using (SqlConnection conn = new SqlConnection(testBuilder.ConnectionString))
                        {
                            conn.Open();
                            string checkDbSql = $"IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = N'{dbName}') CREATE DATABASE [{dbName}]";
                            using (SqlCommand cmd = new SqlCommand(checkDbSql, conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }

                        workingDataSource = server;
                        connected = true;
                        break;
                    }
                    catch
                    {
                        // Tiếp tục thử server khả dụng khác
                    }
                }

                if (!connected)
                {
                    System.Windows.Forms.MessageBox.Show(
                        "Không thể kết nối đến SQL Server!\n\n" +
                        "Vui lòng đảm bảo dịch vụ SQL Server (MSSQLSERVER hoặc SQLEXPRESS) đang chạy " +
                        "và kiểm tra lại ConnectionString trong App.config.",
                        "Lỗi Kết Nối Database",
                        System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                    return;
                }

                // Cập nhật ConnectionString đang hoạt động cho toàn bộ ứng dụng
                SqlConnectionStringBuilder finalBuilder = new SqlConnectionStringBuilder(DatabaseHelper.ConnectionString)
                {
                    DataSource = workingDataSource,
                    InitialCatalog = dbName
                };
                DatabaseHelper.ConnectionString = finalBuilder.ConnectionString;

                // 2. Tạo các bảng nếu chưa tồn tại
                string createTablesSql = @"
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
                    GiaoVienID INT,
                    KhoaHocID INT,
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
                    LopHocID INT,
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
                    ThoiGian INT NOT NULL, -- Số phút
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
                    DapAnDung NVARCHAR(5) NOT NULL, -- 'A', 'B', 'C', hoặc 'D'
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

                -- Migration columns if table already existed previously
                IF COL_LENGTH('GiaoVien', 'ChuyenMon') IS NULL ALTER TABLE GiaoVien ADD ChuyenMon NVARCHAR(100);
                IF COL_LENGTH('GiaoVien', 'GioiTinh') IS NULL ALTER TABLE GiaoVien ADD GioiTinh NVARCHAR(10);
                IF COL_LENGTH('GiaoVien', 'TaiKhoanID') IS NULL ALTER TABLE GiaoVien ADD TaiKhoanID INT NULL;
                IF COL_LENGTH('GiaoVien', 'SoDienThoai') IS NULL 
                BEGIN
                    IF COL_LENGTH('GiaoVien', 'DienThoai') IS NOT NULL
                        EXEC sp_rename 'GiaoVien.DienThoai', 'SoDienThoai', 'COLUMN';
                    ELSE
                        ALTER TABLE GiaoVien ADD SoDienThoai NVARCHAR(20);
                END

                IF COL_LENGTH('HocSinh', 'GioiTinh') IS NULL ALTER TABLE HocSinh ADD GioiTinh NVARCHAR(10);
                IF COL_LENGTH('HocSinh', 'TaiKhoanID') IS NULL ALTER TABLE HocSinh ADD TaiKhoanID INT NULL;
                IF COL_LENGTH('HocSinh', 'SoDienThoai') IS NULL 
                BEGIN
                    IF COL_LENGTH('HocSinh', 'DienThoai') IS NOT NULL
                        EXEC sp_rename 'HocSinh.DienThoai', 'SoDienThoai', 'COLUMN';
                    ELSE
                        ALTER TABLE HocSinh ADD SoDienThoai NVARCHAR(20);
                END

                IF COL_LENGTH('LopHoc', 'SiSoToiDa') IS NULL ALTER TABLE LopHoc ADD SiSoToiDa INT DEFAULT 40;
                IF COL_LENGTH('LopHoc', 'LichHoc') IS NULL ALTER TABLE LopHoc ADD LichHoc NVARCHAR(100);
                IF COL_LENGTH('LopHoc', 'TrangThai') IS NULL ALTER TABLE LopHoc ADD TrangThai NVARCHAR(50) DEFAULT N'Đang mở';
                IF COL_LENGTH('LopHoc', 'NgayBatDau') IS NULL ALTER TABLE LopHoc ADD NgayBatDau DATE NULL;
                IF COL_LENGTH('LopHoc', 'NgayKetThuc') IS NULL ALTER TABLE LopHoc ADD NgayKetThuc DATE NULL;

                IF COL_LENGTH('DeThi', 'TenDeThi') IS NULL 
                BEGIN
                    IF COL_LENGTH('DeThi', 'TenDe') IS NOT NULL
                        EXEC sp_rename 'DeThi.TenDe', 'TenDeThi', 'COLUMN';
                    ELSE
                        ALTER TABLE DeThi ADD TenDeThi NVARCHAR(150);
                END
                IF COL_LENGTH('DeThi', 'MonHoc') IS NULL ALTER TABLE DeThi ADD MonHoc NVARCHAR(100);
                IF COL_LENGTH('DeThi', 'SoCau') IS NULL ALTER TABLE DeThi ADD SoCau INT DEFAULT 20;
                IF COL_LENGTH('DeThi', 'HanNop') IS NULL 
                BEGIN
                    IF COL_LENGTH('DeThi', 'NgayHetHan') IS NOT NULL
                        EXEC sp_rename 'DeThi.NgayHetHan', 'HanNop', 'COLUMN';
                    ELSE
                        ALTER TABLE DeThi ADD HanNop DATETIME;
                END
                IF COL_LENGTH('DeThi', 'TrangThai') IS NULL ALTER TABLE DeThi ADD TrangThai NVARCHAR(50) DEFAULT N'Đang mở';

                IF COL_LENGTH('CauHoi', 'ThuTu') IS NULL ALTER TABLE CauHoi ADD ThuTu INT DEFAULT 1;

                IF COL_LENGTH('KetQuaThi', 'ThoiGianNop') IS NULL 
                BEGIN
                    IF COL_LENGTH('KetQuaThi', 'NgayThi') IS NOT NULL
                        EXEC sp_rename 'KetQuaThi.NgayThi', 'ThoiGianNop', 'COLUMN';
                    ELSE
                        ALTER TABLE KetQuaThi ADD ThoiGianNop DATETIME DEFAULT GETDATE();
                END
                IF COL_LENGTH('KetQuaThi', 'TrangThai') IS NULL ALTER TABLE KetQuaThi ADD TrangThai NVARCHAR(50) DEFAULT N'Hoàn thành';
                ";

                DatabaseHelper.ExecuteNonQuery(createTablesSql);

                // 3. Khởi tạo dữ liệu mẫu thông minh theo từng bảng
                SeedData();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Lỗi khởi tạo DB: " + ex.Message, "Thông Báo Lỗi",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
            }
        }

        private static void SeedData()
        {
            string defaultPass = Passwords.Hash("123456");

            // Seed GiaoVien
            int countGV = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
            if (countGV == 0)
            {
                string sqlGV = @"
                    INSERT INTO GiaoVien (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, ChuyenMon, GioiTinh, TaiKhoanID) 
                    VALUES (N'Nguyễn Văn Thầy', 'gv1@school.edu.vn', '0901234567', N'Hà Nội', '1988-05-15', N'Công nghệ thông tin', N'Nam', 2);
                ";
                DatabaseHelper.ExecuteNonQuery(sqlGV);
            }

            // Seed KhoaHoc
            int countKH = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KhoaHoc"));
            if (countKH == 0)
            {
                string sqlKH = @"
                    INSERT INTO KhoaHoc (TenKhoaHoc, MoTa) 
                    VALUES (N'Lập trình C# WinForms', N'Khóa học lập trình trực quan C# WinForms từ cơ bản đến nâng cao');
                ";
                DatabaseHelper.ExecuteNonQuery(sqlKH);
            }

            // Seed LopHoc
            int countLH = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc"));
            if (countLH == 0)
            {
                string sqlLH = @"
                    INSERT INTO LopHoc (TenLop, GiaoVienID, KhoaHocID, NgayBatDau, NgayKetThuc, SiSoToiDa, LichHoc, TrangThai)
                    VALUES (N'CNTT-K65', 1, 1, '2026-01-01', '2026-06-01', 40, N'Thứ 2, 4, 6 (18:00 - 20:00)', N'Đang mở');
                ";
                DatabaseHelper.ExecuteNonQuery(sqlLH);
            }

            // Seed HocSinh
            int countHS = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
            if (countHS == 0)
            {
                string sqlHS = @"
                    INSERT INTO HocSinh (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, GioiTinh, LopHocID, TaiKhoanID)
                    VALUES (N'Trần Thị Trò', 'hs1@student.edu.vn', '0987654321', N'Hà Nội', '2006-09-18', N'Nữ', 1, 3);
                ";
                DatabaseHelper.ExecuteNonQuery(sqlHS);
            }

            // Seed DangKyLop
            int countDK = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DangKyLop"));
            if (countDK == 0)
            {
                string sqlDK = @"
                    INSERT INTO DangKyLop (LopHocID, HocSinhID, TrangThai, NgayDangKy) 
                    VALUES (1, 1, N'Đã duyệt', GETDATE());
                ";
                DatabaseHelper.ExecuteNonQuery(sqlDK);
            }

            // Seed TaiKhoan
            int countTK = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
            if (countTK == 0)
            {
                string sqlTK = $@"
                    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa) VALUES
                    ('admin', '{defaultPass}', 'Admin', 0, 0),
                    ('giaovien1', '{defaultPass}', 'GiaoVien', 1, 0),
                    ('hocsinh1', '{defaultPass}', 'HocSinh', 1, 0);
                ";
                DatabaseHelper.ExecuteNonQuery(sqlTK);
            }

            // Seed DeThi
            int countDT = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            if (countDT == 0)
            {
                string sqlDT = @"
                    INSERT INTO DeThi (TenDeThi, MoTa, ThoiGian, MonHoc, SoCau, GiaoVienID, LopHocID, NgayTao, HanNop, TrangThai)
                    VALUES (N'Kiểm tra giữa kỳ C# WinForms', N'Bài thi trắc nghiệm kiến thức C# & Windows Forms', 15, N'Lập trình C# WinForms', 5, 1, 1, GETDATE(), DATEADD(day, 30, GETDATE()), N'Đang mở');
                ";
                DatabaseHelper.ExecuteNonQuery(sqlDT);
            }

            // Seed CauHoi
            int countCH = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
            if (countCH == 0)
            {
                string sqlCH = @"
                    INSERT INTO CauHoi (NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, DeThiID, ThuTu) VALUES
                    (N'Trong C#, ký hiệu nào được sử dụng để khai báo một lớp kế thừa từ một lớp cha?', N':', N'inherits', N'extends', N'implements', 'A', 1, 1),
                    (N'Trong Windows Forms, sự kiện mặc định phát sinh khi nhấp đúp vào Button trong cửa sổ thiết kế là gì?', N'DoubleClick', N'Click', N'MouseClick', N'Hover', 'B', 1, 2),
                    (N'Control nào thường được sử dụng nhiều nhất để hiển thị và chỉnh sửa bảng dữ liệu trong WinForms?', N'ListBox', N'ListView', N'DataGridView', N'TableLayout', 'C', 1, 3),
                    (N'Trong kiến trúc phân lớp (3-Tier), tầng nào đảm nhiệm việc giao tiếp trực tiếp với cơ sở dữ liệu?', N'GUI (Presentation)', N'BLL (Business Logic)', N'DAL (Data Access Layer)', N'DTO / Model', 'C', 1, 4),
                    (N'Câu lệnh SQL nào dùng để trích xuất dữ liệu từ một hoặc nhiều bảng?', N'INSERT', N'UPDATE', N'DELETE', N'SELECT', 'D', 1, 5);
                ";
                DatabaseHelper.ExecuteNonQuery(sqlCH);
            }

            // Seed KetQuaThi
            int countKQ = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            if (countKQ == 0)
            {
                string sqlKQ = @"
                    INSERT INTO KetQuaThi (HocSinhID, DeThiID, Diem, SoCauDung, ThoiGianNop, TrangThai)
                    VALUES (1, 1, 10.0, 5, GETDATE(), N'Hoàn thành');
                ";
                DatabaseHelper.ExecuteNonQuery(sqlKQ);
            }

            // Seed ChiTietBaiLam
            int countCT = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            if (countCT == 0)
            {
                string sqlCT = @"
                    INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung) VALUES
                    (1, 1, 'A', 1),
                    (1, 2, 'B', 1),
                    (1, 3, 'C', 1),
                    (1, 4, 'C', 1),
                    (1, 5, 'D', 1);
                ";
                DatabaseHelper.ExecuteNonQuery(sqlCT);
            }
        }
    }
}
