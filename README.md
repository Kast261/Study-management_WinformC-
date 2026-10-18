# ỨNG DỤNG QUẢN LÝ HỌC TẬP & THI TRẮC NGHIỆM (C# WINFORMS)

Dự án bài tập lớn môn **Lập trình trực quan (LTTQ)** xây dựng ứng dụng Windows Forms trên nền tảng .NET Framework 4.8 với kiến trúc phân tầng (DAL - Models - Forms) và giao diện Material Design (ReaLTaiizor).

---

## 🛠 Yêu Cầu Môi Trường

- **Hệ điều hành:** Windows 10 / 11
- **IDE:** Visual Studio 2019 / 2022
- **Framework:** .NET Framework 4.8
- **Hệ quản trị CSDL:** Microsoft SQL Server (Bản Developer, Express hoặc LocalDB)

---

## 🚀 Hướng Dẫn Cài Đặt & Chạy Lần Đầu

### Cách 1: Tự động hoàn toàn (Khuyên dùng)
Ứng dụng đã được tích hợp cơ chế **tự động kiểm tra, tạo CSDL và khởi tạo dữ liệu mẫu**:
1. Mở Solution `BLT_Quan_Ly_Hoc_Tap.sln` bằng **Visual Studio**.
2. Kiểm tra chuỗi kết nối trong [App.config](file:///d:/LTTQ/BLT_Quan_Ly_Hoc_Tap/App.config):
   ```xml
   <connectionStrings>
       <add name="ChuoiKetNoi"
            connectionString="Data Source=.;Initial Catalog=BTL_QL_Hoc_Tap;Integrated Security=True"
            providerName="System.Data.SqlClient" />
   </connectionStrings>
   ```
   > 💡 *Mẹo:* Ứng dụng sẽ tự động thử kết nối qua `.` hoặc `(local)` hoặc `.\SQLEXPRESS` hoặc `(localdb)\MSSQLLocalDB` nếu SQL Server trên máy bạn dùng tên instance khác.
3. Nhấn **F5** (hoặc `Start` / `Debug`).
4. Khi khởi chạy lần đầu, ứng dụng sẽ:
   - Tự động tạo cơ sở dữ liệu `BTL_QL_Hoc_Tap` trên SQL Server.
   - Tự động tạo cấu trúc toàn bộ các bảng.
   - Tự động nạp dữ liệu mẫu (Giáo viên, Lớp học, Học sinh, Đề thi, Câu hỏi trắc nghiệm, Kết quả thi).

---

### Cách 2: Khởi tạo thủ công bằng SQL Server Management Studio (SSMS)
Nếu bạn muốn chủ động tạo cơ sở dữ liệu trước:
1. Mở **SQL Server Management Studio (SSMS)** và kết nối vào SQL Server của bạn.
2. Mở file script tổng hợp tại:
   [`Database/Scripts/Full_Setup_Database.sql`](file:///d:/LTTQ/BLT_Quan_Ly_Hoc_Tap/Database/Scripts/Full_Setup_Database.sql)
3. Nhấn **F5 (Execute)** để chạy toàn bộ script.
4. Mở dự án trong Visual Studio và nhấn **F5** để chạy ứng dụng.

---

## 👥 Danh Sách Tài Khoản Mẫu Để Kiểm Thử

Tất cả tài khoản thử nghiệm đều có mật khẩu mặc định là: **`123456`**

| Vai Trò | Tên Đăng Nhập | Mật Khẩu | Quyền Hạn & Tính Năng Thử Nghiệm |
|---|---|---|---|
| **Admin** (Quản trị viên) | `admin` | `123456` | Quản lý người dùng, tài khoản, phân quyền, quản lý khóa học, mở lớp |
| **Giáo Viên** | `giaovien1` | `123456` | Quản lý lớp phụ trách, tạo đề thi trắc nghiệm, xem danh sách bài làm & điểm học sinh |
| **Học Sinh** | `hocsinh1` | `123456` | Xem lớp đã đăng ký, làm bài thi trực tuyến, xem điểm & xem lại chi tiết bài làm |

---

## 📁 Cấu Trúc Thư Mục Dự Án

```
BLT_Quan_Ly_Hoc_Tap/
│
├── App.config              # Cấu hình chuỗi kết nối CSDL (ChuoiKetNoi)
├── Program.cs              # Điểm khởi chạy ứng dụng & nạp Theme Material
│
├── Database/
│   ├── CreateDataBase.cs   # Mã nguồn tự động tạo DB, bảng & seed dữ liệu mẫu
│   ├── DatabaseHelper.cs   # Lớp trợ giúp thực thi SQL (Query, NonQuery, Scalar, Transaction)
│   └── Scripts/            # Các file script SQL phục vụ chia sẻ & backup
│       ├── 01_CreateDatabase.sql      # Script tạo cấu trúc database & bảng
│       ├── 02_SeedData.sql            # Script thêm dữ liệu mẫu
│       └── Full_Setup_Database.sql   # Script gộp 1-click dùng cho SSMS
│
├── DAL/                    # Data Access Layer (Truy vấn CSDL cho từng đối tượng)
├── Models/                 # Lớp mô hình thực thể (DTO)
├── Forms/                  # Giao diện người dùng WinForms Material Design
└── Helpers/                # Các tiện ích hỗ trợ (Mã hóa mật khẩu SHA-256, Tiện ích UI...)
```

---

## ⚠️ Khắc Phục Sự Cố Kết Nối SQL Server Thường Gặp

- **Lỗi `Cannot open database "BTL_QL_Hoc_Tap"` hoặc `Server was not found`:**
  - Mở `services.msc` trên Windows, kiểm tra dịch vụ **SQL Server (MSSQLSERVER)** hoặc **SQL Server (SQLEXPRESS)** đang ở trạng thái `Running`.
  - Nếu máy bạn cài bản SQL Server Express, mở `App.config` và đổi `Data Source=.` thành `Data Source=.\SQLEXPRESS`.
