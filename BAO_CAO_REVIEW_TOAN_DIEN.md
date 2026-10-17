# 📋 BÁO CÁO REVIEW TOÀN DIỆN DỰ ÁN QUẢN LÝ HỌC TẬP (BLT_Quan_Ly_Hoc_Tap)

> **Ngày review:** 15/10/2026  
> **Dự án:** Ứng dụng Quản lý Học tập & Thi Trắc nghiệm (WinForms C# .NET 4.8 / ReaLTaiizor)  
> **Cơ sở dữ liệu:** Microsoft SQL Server (`BTL_QL_Hoc_Tap`)  
> **Đánh giá tổng quan:** Codebase hiện tại hoàn thành khoảng **35% - 40%**. Giao diện đã dựng khung cơ bản, tuy nhiên có **xung đột cấu trúc dữ liệu nghiêm trọng** giữa SQL Server và DAL, **lỗi logic đảo ngược nút bấm** ở form thi, cùng nhiều chức năng ở các form chính chưa được cài đặt nghiệp vụ (Code-Behind).

---

## 📑 MỤC LỤC
1. [Đánh Giá Nghiệp Vụ & Logic Các Tính Năng](#1-đánh-giá-nghiệp-vụ--logic-các-tính-năng)
2. [Review Chi Tiết Từng Form & Thành Phần Toolbox](#2-review-chi-tiết-từng-form--thành-phần-toolbox)
3. [Review Cơ Sở Dữ Liệu & Tối Ưu Truy Vấn (Database Queries)](#3-review-cơ-sở-dữ-liệu--tối-ưu-truy-vấn-database-queries)
4. [Hướng Dẫn Sửa Đổi Cụ Thể (Kèm Code Mẫu)](#4-hướng-dẫn-sửa-đổi-cụ-thể-kèm-code-mẫu)
5. [Lộ Trình Hoàn Thiện Dự Án (Checklist)](#5-lộ-trình-hoàn-thiện-dự-án-checklist)

---

## 1. ĐÁNH GIÁ NGHIỆP VỤ & LOGIC CÁC TÍNH NĂNG

### 1.1. Những điểm đã làm tốt
- **Kiến trúc phân tầng tương đối rõ ràng**: Đã có sự phân chia thư mục: `Database/`, `DAL/`, `Models/`, `Helpers/`, `Forms/`.
- **Bảo mật mật khẩu**: Đã triển khai hàm băm SHA-256 (`Passwords.cs`) và lưu trữ thông tin phiên người dùng (`Session.cs`).
- **Giao diện hiện đại**: Ứng dụng thư viện `ReaLTaiizor` (`MaterialSkin`) mang lại giao diện phẳng, đẹp mắt.
- **Form Đăng nhập (`frm_DangNhap`)**: Đã có kiểm tra rỗng, mã hóa mật khẩu, ghi nhớ đăng nhập ra file (`remember.dat`) và phân luồng form theo vai trò (`VaiTro`).

---

### 1.2. Các lỗi logic & Nghiệp vụ chưa hoàn thiện

| Thành phần | Mức độ nghiêm trọng | Hiện trạng & Lỗi Logic |
| :--- | :---: | :--- |
| **`Frm_KiemTra`** | 🔴 **CRITICAL** | **Gán ngược sự kiện nút bấm**: Nút *"Câu Trước"* bị gán sự kiện nộp bài `XacNhanNopBai()`, còn nút *"Nộp Bài"* lại gán sự kiện chuyển về câu trước `ChuyenCau(_currentIndex - 1)`. |
| **`Frm_KiemTra`** | 🔴 **CRITICAL** | Khi lưu bài thi, gọi chèn vào bảng `ChiTietBaiLam`, nhưng bảng này **hoàn toàn chưa có trong Database** dẫn tới văng lỗi Crash App. |
| **`Frm_KiemTra`** | 🟡 **MEDIUM** | Chưa bắt sự kiện `FormClosing`: Thí sinh bấm nút [X] của cửa sổ sẽ bị thoát bài mà không cảnh báo hoặc không kịp lưu bài. |
| **Xung đột DB - DAL** | 🔴 **CRITICAL** | Cột trong `CreateDataBase.cs` khác hoàn toàn tên cột trong các file `DAL/` (ví dụ `TenDe` vs `TenDeThi`, `DienThoai` vs `SoDienThoai`). Nhiều bảng thiếu hẳn cột khiến các câu query báo lỗi `Invalid column name`. |
| **Đăng xuất (Logout)** | 🟠 **HIGH** | Tại `Frm_Admin`, `frm_GiaoVien`, `Frm_HocVien`: Kiểm tra `SelectedTab.Text == "Đăng Xuất"` nhưng text trong file Designer bị lỗi font thành `"Dang Xu?t"` $\rightarrow$ Sự kiện đăng xuất **không bao giờ chạy**! Khi click vào chỉ thấy tab trống. Hơn nữa, code gọi `Application.Exit()` thay vì dọn dẹp Session và đưa về `frm_DangNhap`. |
| **`Frm_Admin`** | 🟠 **HIGH** | Mới chỉ có code CRUD bảng Giáo Viên. Các tab Học Viên, Khóa Học & Lớp, Quản Lý Tài Khoản hoàn toàn chưa có code (0%). Query Dashboard bị sai tên cột nên bị khối `catch {}` nuốt lỗi làm biểu đồ trắng trơn. |
| **`frm_GiaoVien`** | 🟠 **HIGH** | Chưa có logic nghiệp vụ (0%): Các hàm `LoadLopHocCuaToi()`, tạo đề thi, thêm câu hỏi, duyệt học viên chỉ là các hàm rỗng mang tính placeholder. |
| **`Frm_HocVien`** | 🟠 **HIGH** | Chưa có logic nghiệp vụ (0%): Không tải danh sách lớp, không tải lịch sử điểm, không có nút để học sinh chọn và mở `Frm_KiemTra`. Vẫn còn sót code debug `MessageBox.Show("Su kien da duoc kich hoat");`. |

---

## 2. REVIEW CHI TIẾT TỪNG FORM & THÀNH PHẦN TOOLBOX

### 2.1. Vấn đề chung về Toolbox và Giao diện
1. **Lỗi mã hóa Font Tiếng Việt (Mojibake)**:
   - Các file `.Designer.cs` bị lỗi font chữ Tiếng Việt khi tạo: `L?p H?c`, `Bi Thi`, `K?t Qu?`, `Thng Tin`, `Dang Xu?t`, `Cu Tru?c `.
   - **Khắc phục**: Mở file `.Designer.cs`, lưu lại dưới định dạng **UTF-8 with BOM**, hoặc cấu hình lại thuộc tính `Text` trên giao diện thiết kế Visual Studio Properties.
2. **Đặt tên Control (Naming Conventions)**:
   - Hiện tại đa số control đang mang tên mặc định của Toolbox: `materialTextBoxEdit10`, `materialButton8`, `materialComboBox15`, `foreverButton1`,...
   - **Hậu quả**: Khi viết code nghiệp vụ rất dễ nhầm lẫn (điển hình là lỗi gán nhầm sự kiện nút ở `Frm_KiemTra`).
   - **Quy tắc sửa đổi**:
     - Nút bấm: `btnThem`, `btnSua`, `btnXoa`, `btnLamMoi`, `btnNopBai`, `btnNext`, `btnPrev`.
     - Nhập liệu: `txtHoTen`, `txtEmail`, `txtSDT`, `dtpNgaySinh`, `cboKhoaHoc`.
     - Hiển thị bảng: `dgvGiaoVien`, `dgvHocVien`, `dgvLopHoc`, `dgvKetQua`.

---

### 2.2. Thành phần Toolbox còn thiếu & Cần bổ sung cho từng Form

#### A. Form Đăng Nhập (`frm_DangNhap`)
- **Đã có**: TextBox Tài khoản, Mật khẩu, Checkbox Ghi nhớ, Checkbox ẩn/hiện mật khẩu, Nút đăng nhập.
- **Cần bổ sung/sửa**:
  - `ErrorProvider`: Gắn vào form để hiển thị cảnh báo viền đỏ cạnh textbox khi người dùng quên nhập hoặc nhập sai định dạng thay vì dùng `MaterialSnackBar` hoặc `MessageBox` liên tục.
  - Xử lý khi mở form mới:
    ```csharp
    this.Hide();
    nextForm.FormClosed += (s, args) => this.Close(); // Giúp tắt sạch ứng dụng khi người dùng đóng cửa sổ chính
    nextForm.Show();
    ```

#### B. Form Quản Trị (`Frm_Admin`)
- **Tab Dashboard**:
  - Cần sửa query điểm trung bình: Thay `d.TenDeThi` thành `d.TenDe`.
  - Bổ sung `ComboBox` chọn khoảng thời gian (Học kỳ / Năm học) để lọc biểu đồ thống kê.
- **Tab Giáo Viên & Học Viên**:
  - **Thiếu Toolbox**: Đang dùng TextBox để nhập Ngày sinh $\rightarrow$ Cần thay bằng `DateTimePicker` (định dạng `dd/MM/yyyy`) để tránh lỗi sai kiểu dữ liệu ngày tháng.
  - **Thiếu Toolbox**: `ComboBox` chọn Giới tính ("Nam", "Nữ", "Khác").
  - Cần thêm cột `TaiKhoanID` hoặc nút "Cấp tài khoản" tự động cho học sinh/giáo viên.
- **Tab Khóa Học & Lớp Học**:
  - **Thiếu Toolbox**: `ComboBox` chọn Khóa học (`cboKhoaHoc`) và `ComboBox` chọn Giáo viên phụ trách (`cboGiaoVien`).
  - `DateTimePicker` cho Ngày bắt đầu và Ngày kết thúc lớp học.
  - `NumericUpDown` cho Sĩ số tối đa.
- **Tab Tài Khoản**:
  - `ComboBox` chọn Vai trò (`Admin`, `GiaoVien`, `HocSinh`).
  - Nút "Khóa / Mở Khóa Tài Khoản" và nút "Đặt lại mật khẩu (Reset Password)".

#### C. Form Giáo Viên (`frm_GiaoVien`)
- **Tab Ngân Hàng Câu Hỏi**:
  - **Thiếu Toolbox**: `ComboBox` chọn Đề thi mà câu hỏi thuộc về.
  - Ô nhập nội dung câu hỏi hiện tại là Textbox 1 dòng $\rightarrow$ Cần đổi thành `MaterialMultiLineTextBox` hoặc `RichTextBox` để nhập được câu hỏi dài.
  - `ComboBox` chọn Đáp án đúng: Dropdown gồm 4 lựa chọn `A`, `B`, `C`, `D` để tránh người dùng nhập chữ thường hoặc ký tự linh tinh.
- **Tab Tạo Đề Thi**:
  - **Thiếu Toolbox**: `NumericUpDown` chọn Thời gian thi (phút) và `DateTimePicker` chọn Hạn nộp bài.
  - `ComboBox` chọn Lớp học được phép làm đề thi này.
- **Tab Xem Kết Quả**:
  - **Thiếu Toolbox**: `ComboBox` chọn Lớp học và `ComboBox` chọn Đề thi để lọc danh sách kết quả học sinh.
  - `Button` "Xuất Excel" (Export Bảng Điểm) phục vụ báo cáo.

#### D. Form Học Viên (`Frm_HocVien`)
- **Tab Bài Thi**:
  - Hiện tại chỉ có `materialComboBox1` mà không có danh sách chi tiết và không có nút kích hoạt thi.
  - **Cần thay thế/bổ sung**: 1 `DataGridView` hiển thị danh sách các đề thi được giao:
    - Các cột: Mã đề, Tên bài thi, Môn học, Thời gian (phút), Hạn nộp, Trạng thái (Chưa thi / Đã hoàn thành).
    - 1 nút `MaterialButton` "Bắt Đầu Thi" $\rightarrow$ Lấy ID đề được chọn trên DataGridView rồi gọi `BatDauThi(deThiID)`.
- **Tab Thông Tin Cá Nhân**:
  - Bổ sung chức năng Đổi mật khẩu: TextBox Mật khẩu cũ, Mật khẩu mới, Xác nhận mật khẩu mới và nút "Cập nhật".

#### E. Form Kiểm Tra (`Frm_KiemTra`)
- **Sửa lỗi tráo nút bấm ngay lập tức**:
  ```csharp
  // ĐÚNG:
  materialButton1.Click += (s, ev) => ChuyenCau(_currentIndex - 1); // materialButton1 là nút "Câu Trước"
  foreverButton2.Click += (s, ev) => ChuyenCau(_currentIndex + 1);  // foreverButton2 là nút "Câu Tiếp"
  foreverButton1.Click += (s, ev) => XacNhanNopBai();              // foreverButton1 là nút "Nộp Bài"
  ```
- **Bổ sung thành phần cực kỳ quan trọng: Question Palette (Bảng nút chọn nhanh câu hỏi)**:
  - Thêm 1 `FlowLayoutPanel` (đặt tên `pnlDanhSachCau`).
  - Khi load đề thi có 20 câu, sinh tự động 20 nút nhỏ (1, 2, 3... 20).
  - Khi học sinh đã trả lời câu nào, nút tương ứng chuyển sang màu xanh lá (`Color.MediumSeaGreen`). Câu chưa trả lời màu xám.
  - Học sinh có thể click vào nút số 15 để nhảy thẳng đến câu 15 mà không phải Next 15 lần.
- **Bắt sự kiện đóng form**:
  ```csharp
  protected override void OnFormClosing(FormClosingEventArgs e)
  {
      if (_thoiGianConLai > 0 && !_daNopBai)
      {
          var res = MessageBox.Show("Bài thi đang diễn ra! Bạn có chắc muốn thoát và hủy kết quả?", 
                                    "Cảnh báo", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
          if (res == DialogResult.No)
          {
              e.Cancel = true;
              return;
          }
      }
      _timer?.Stop();
      base.OnFormClosing(e);
  }
  ```

---

## 3. REVIEW CƠ SỞ DỮ LIỆU & TỐI ƯU TRUY VẤN (DATABASE QUERIES)

### 3.1. Các điểm bất đồng bộ cấu trúc (Schema Discrepancy)
Hiện tại file `Database/CreateDataBase.cs` tạo database với một bộ cấu trúc, nhưng các file `DAL/*.cs` lại query với các tên cột và bảng hoàn toàn khác:

| Bảng | Cấu trúc trong `CreateDataBase.cs` | Cấu trúc DAL đang gọi truy vấn | Lỗi phát sinh lúc chạy |
| :--- | :--- | :--- | :--- |
| **`DeThi`** | `TenDe`, `MoTa`, `ThoiGian`, `NgayTao`, `NgayHetHan` | `TenDeThi`, `MonHoc`, `SoCau`, `ThoiGian`, `HanNop`, `TrangThai` | **Crash**: `Invalid column name 'TenDeThi'` |
| **`GiaoVien`** | `DienThoai`, thiếu `ChuyenMon`, thiếu `TaiKhoanID` | `SoDienThoai`, `ChuyenMon`, `TaiKhoanID` | **Crash**: `Invalid column name 'ChuyenMon'` |
| **`HocSinh`** | `DienThoai`, thiếu `GioiTinh`, thiếu `TaiKhoanID` | `SoDienThoai`, `GioiTinh`, `TaiKhoanID` | **Crash**: `Invalid column name 'GioiTinh'` |
| **`LopHoc`** | Thiếu `LichHoc`, `SiSoToiDa`, `TrangThai` | `LichHoc`, `SiSoToiDa`, `TrangThai` | **Crash**: `Invalid column name 'LichHoc'` |
| **`KetQuaThi`**| `NgayThi`, thiếu `ThoiGianNop`, `TrangThai` | `ThoiGianNop`, `TrangThai` | **Crash**: `Invalid column name 'ThoiGianNop'` |
| **`DangKyLop`**| **Chưa có bảng** | `SELECT ... FROM DangKyLop` | **Crash**: `Invalid object name 'DangKyLop'` |
| **`ChiTietBaiLam`**| **Chưa có bảng** | `INSERT INTO ChiTietBaiLam...` | **Crash**: `Invalid object name 'ChiTietBaiLam'` |

---

### 3.2. Đánh giá chất lượng Query & Đề xuất tối ưu hóa

1. **Lạm dụng `SELECT *`**:
   - Trong `DeThiDAL.cs`, `GiaoVienDAL.cs`, `HocSinhDAL.cs`, `TaiKhoanDAL.cs` đều dùng `SELECT * FROM ...`.
   - **Tác hại**: Gây lãng phí I/O mạng, không tận dụng được Covering Index, và dễ bị lỗi mapping nếu thứ tự hoặc số lượng cột thay đổi.
   - **Tối ưu**: Luôn liệt kê rõ ràng các cột cần lấy: `SELECT LopHocID, TenLop, GiaoVienID, KhoaHocID FROM LopHoc`.
2. **Thiếu Quản Lý Giao Tác (SqlTransaction)**:
   - Các hành động mang tính nguyên tử (Atomic) như:
     - Nộp bài thi: Cần `INSERT INTO KetQuaThi` và sau đó lặp `INSERT INTO ChiTietBaiLam` $N$ lần.
     - Tạo đề thi: Cần `INSERT INTO DeThi` và sau đó lặp `INSERT INTO CauHoi`.
   - **Rủi ro**: Nếu câu số 5 bị lỗi mạng hoặc sai dữ liệu, các câu 1-4 đã lưu nhưng bài thi bị hỏng nửa chừng mà không được Rollback.
3. **Cải tiến `DatabaseHelper` để tái sử dụng tối đa**:
   - Dùng từ khóa `params SqlParameter[]` để khi gọi hàm không cần viết `new SqlParameter[] { new SqlParameter(...) }` mà có thể truyền trực tiếp nhiều parameter.
   - Bổ sung hàm thực thi có Transaction để tái sử dụng toàn app.
   - Bổ sung hàm Generic chuyển `SqlDataReader` $\rightarrow$ `List<T>` để không phải viết code duyệt `DataTable.Rows` lặp đi lặp lại ở từng DAL.

---

## 4. HƯỚNG DẪN SỬA ĐỔI CỤ THỂ (KÈM CODE MẪU)

### 4.1. Cập nhật lại Script Database đồng bộ (`CreateDataBase.cs`)
Chỉnh sửa lại câu lệnh SQL tạo bảng trong `Database/CreateDataBase.cs` để khớp 100% với các Models và DAL:

```csharp
// Thay thế nội dung createTablesSql trong CreateDataBase.cs:
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
    TaiKhoanID INT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    ChuyenMon NVARCHAR(100),
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255),
    NgaySinh DATE
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
    LichHoc NVARCHAR(100),
    SiSoToiDa INT DEFAULT 40,
    TrangThai NVARCHAR(50) DEFAULT N'Đang mở',
    NgayBatDau DATE,
    NgayKetThuc DATE
);

IF OBJECT_ID('HocSinh', 'U') IS NULL
CREATE TABLE HocSinh (
    HocSinhID INT IDENTITY(1,1) PRIMARY KEY,
    TaiKhoanID INT NULL,
    HoTen NVARCHAR(100) NOT NULL,
    NgaySinh DATE,
    GioiTinh NVARCHAR(10),
    Email NVARCHAR(100),
    SoDienThoai NVARCHAR(20),
    DiaChi NVARCHAR(255)
);

IF OBJECT_ID('DangKyLop', 'U') IS NULL
CREATE TABLE DangKyLop (
    DangKyID INT IDENTITY(1,1) PRIMARY KEY,
    LopHocID INT NOT NULL,
    HocSinhID INT NOT NULL,
    NgayDangKy DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Đã duyệt'
);

IF OBJECT_ID('DeThi', 'U') IS NULL
CREATE TABLE DeThi (
    DeThiID INT IDENTITY(1,1) PRIMARY KEY,
    LopHocID INT,
    GiaoVienID INT,
    TenDeThi NVARCHAR(150) NOT NULL,
    MonHoc NVARCHAR(100),
    SoCau INT DEFAULT 0,
    ThoiGian INT NOT NULL, -- Số phút
    HanNop DATETIME NULL,
    TrangThai NVARCHAR(50) DEFAULT N'Đang mở'
);

IF OBJECT_ID('CauHoi', 'U') IS NULL
CREATE TABLE CauHoi (
    CauHoiID INT IDENTITY(1,1) PRIMARY KEY,
    DeThiID INT NOT NULL,
    NoiDung NVARCHAR(MAX) NOT NULL,
    DapAnA NVARCHAR(255),
    DapAnB NVARCHAR(255),
    DapAnC NVARCHAR(255),
    DapAnD NVARCHAR(255),
    DapAnDung NVARCHAR(5) NOT NULL, -- 'A', 'B', 'C', hoặc 'D'
    ThuTu INT DEFAULT 1
);

IF OBJECT_ID('KetQuaThi', 'U') IS NULL
CREATE TABLE KetQuaThi (
    KetQuaID INT IDENTITY(1,1) PRIMARY KEY,
    HocSinhID INT NOT NULL,
    DeThiID INT NOT NULL,
    SoCauDung INT DEFAULT 0,
    Diem FLOAT DEFAULT 0,
    ThoiGianNop DATETIME DEFAULT GETDATE(),
    TrangThai NVARCHAR(50) DEFAULT N'Hoàn thành'
);

IF OBJECT_ID('ChiTietBaiLam', 'U') IS NULL
CREATE TABLE ChiTietBaiLam (
    ChiTietID INT IDENTITY(1,1) PRIMARY KEY,
    KetQuaID INT NOT NULL,
    CauHoiID INT NOT NULL,
    DapAnChon NVARCHAR(5),
    LaDung BIT DEFAULT 0
);";
```

---

### 4.2. Nâng cấp `DatabaseHelper.cs` chuẩn tái sử dụng & hỗ trợ Transaction
Giúp giảm 50% lượng code thừa khi viết các lớp DAL và đảm bảo an toàn dữ liệu:

```csharp
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["ChuoiKetNoi"].ConnectionString;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        public static bool KiemTraKetNoi()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        // Tái sử dụng linh hoạt: params SqlParameter[]
        public static DataTable ExecuteQuery(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteScalar();
                }
            }
        }

        // TÁI SỬ DỤNG CHO CÁC NGHIỆP VỤ NHIỀU BƯỚC CẦN TRANSACTION
        public static bool ExecuteTransaction(Action<SqlCommand> action)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            action(cmd);
                        }
                        trans.Commit();
                        return true;
                    }
                    catch
                    {
                        trans.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}
```

---

### 4.3. Viết lại hàm Nộp bài trong `KetQuaDAL.cs` dùng Transaction
Đảm bảo kết quả và chi tiết câu hỏi được lưu trọn vẹn, không sinh rác dữ liệu:

```csharp
public bool LuuBaiThiHoanChinh(KetQuaThi kq, List<Tuple<int, string, bool>> chiTiets)
{
    return DatabaseHelper.ExecuteTransaction(cmd =>
    {
        // 1. Lưu Kết quả thi và lấy KetQuaID
        cmd.CommandText = @"INSERT INTO KetQuaThi (HocSinhID, DeThiID, SoCauDung, Diem, ThoiGianNop, TrangThai)
                            VALUES (@hsID, @deID, @dung, @diem, GETDATE(), @tt);
                            SELECT SCOPE_IDENTITY();";
        cmd.Parameters.Clear();
        cmd.Parameters.AddWithValue("@hsID", kq.HocSinhID);
        cmd.Parameters.AddWithValue("@deID", kq.DeThiID);
        cmd.Parameters.AddWithValue("@dung", kq.SoCauDung);
        cmd.Parameters.AddWithValue("@diem", kq.Diem);
        cmd.Parameters.AddWithValue("@tt", kq.TrangThai ?? "Hoàn thành");

        int ketQuaID = Convert.ToInt32(cmd.ExecuteScalar());

        // 2. Lưu từng câu trả lời vào ChiTietBaiLam
        cmd.CommandText = @"INSERT INTO ChiTietBaiLam (KetQuaID, CauHoiID, DapAnChon, LaDung)
                            VALUES (@kqID, @chID, @da, @dung)";
        foreach (var item in chiTiets)
        {
            cmd.Parameters.Clear();
            cmd.Parameters.AddWithValue("@kqID", ketQuaID);
            cmd.Parameters.AddWithValue("@chID", item.Item1); // CauHoiID
            cmd.Parameters.AddWithValue("@da", item.Item2);   // DapAnChon
            cmd.Parameters.AddWithValue("@dung", item.Item3); // LaDung
            cmd.ExecuteNonQuery();
        }
    });
}
```

---

### 4.4. Chuẩn hóa hàm Đăng xuất dùng chung cho cả 3 Form
Thay thế đoạn code `Application.Exit()` tại tab Đăng Xuất bằng hàm chuẩn:

```csharp
private void XuLyDangXuat()
{
    DialogResult result = MessageBox.Show(
        "Bạn có chắc chắn muốn đăng xuất?",
        "Xác nhận",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question);

    if (result == DialogResult.Yes)
    {
        Session.Clear();
        this.Hide();
        frm_DangNhap formLogin = new frm_DangNhap();
        formLogin.ShowDialog();
        this.Close();
    }
}
```

---

## 5. LỘ TRÌNH HOÀN THIỆN DỰ ÁN (CHECKLIST)

Để bài tập lớn đạt điểm tối đa và vận hành mượt mà, bạn nên thực hiện theo thứ tự sau:

- [ ] **Giai đoạn 1: Chuẩn hóa DB & Data Access (Ưu tiên số 1)**
  - [ ] Đồng bộ script `CreateDataBase.cs` với danh sách bảng và cột chuẩn ở mục 4.1.
  - [ ] Nâng cấp `DatabaseHelper.cs` hỗ trợ `params` và `Transaction`.
  - [ ] Rà soát lại tất cả các câu query trong `DAL/`: Bỏ `SELECT *`, khớp chính xác tên cột.
- [ ] **Giai đoạn 2: Sửa lỗi nghiêm trọng Form Thi (`Frm_KiemTra`)**
  - [ ] Đổi lại sự kiện 3 nút bấm (Nộp bài, Câu trước, Câu tiếp).
  - [ ] Bổ sung Question Palette (lưới chọn nhanh câu hỏi).
  - [ ] Bắt sự kiện `FormClosing` chống đóng form đột ngột.
- [ ] **Giai đoạn 3: Hoàn thiện Form Admin (`Frm_Admin`)**
  - [ ] Sửa query biểu đồ Dashboard (`TenDeThi` $\rightarrow$ `TenDe`).
  - [ ] Lập trình CRUD cho Tab Học Viên (dùng `HocSinhDAL`).
  - [ ] Lập trình CRUD cho Tab Khóa Học & Lớp Học (dùng `LopHocDAL`).
  - [ ] Lập trình Quản lý Tài Khoản: Đổi mật khẩu, Khóa/Mở khóa tài khoản.
- [ ] **Giai đoạn 4: Hoàn thiện Form Giáo Viên (`frm_GiaoVien`)**
  - [ ] Load danh sách lớp phụ trách vào DataGridView.
  - [ ] Lập trình chức năng Tạo đề thi và Thêm câu hỏi trắc nghiệm.
  - [ ] Lập trình chức năng Xem bảng điểm học sinh theo từng đề.
- [ ] **Giai đoạn 5: Hoàn thiện Form Học Viên (`Frm_HocVien`)**
  - [ ] Xóa bỏ câu lệnh `MessageBox.Show("Su kien da duoc kich hoat")`.
  - [ ] Hiển thị danh sách lớp đã đăng ký.
  - [ ] Hiển thị danh sách đề thi kèm nút bấm "Vào thi" mở `Frm_KiemTra`.
  - [ ] Hiển thị bảng lịch sử điểm thi.
- [ ] **Giai đoạn 6: Hoàn thiện Giao diện & Trải nghiệm (Polishing)**
  - [ ] Đổi tên toàn bộ control từ mặc định (`materialButton1`,...) sang tên chuẩn (`btnThem`,...).
  - [ ] Sửa triệt để lỗi mã hóa Tiếng Việt trong các file `.Designer.cs`.
