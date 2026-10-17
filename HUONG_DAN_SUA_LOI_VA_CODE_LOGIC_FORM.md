# 📘 HƯỚNG DẪN SỬA LỖI ĐĂNG NHẬP & TỔNG HỢP CODE LOGIC CÁC FORM

> **Dự án:** Quản Lý Trung Tâm Học Tập (`BLT_Quan_Ly_Hoc_Tap`)  
> **Nền tảng:** WinForms C# (.NET Framework 4.8) - ReaLTaiizor UI - SQL Server  

---

## 📑 MỤC LỤC
1. [Khảo sát các tên Controls (Toolbox) đã đổi trên Form Admin](#1-khảo-sát-các-tên-controls-toolbox-đã-đổi-trên-form-admin)
2. [Nguyên nhân các lỗi khi Đăng Nhập & Cách khắc phục triệt để](#2-nguyên-nhân-các-lỗi-khi-đăng-nhập--cách-khắc-phục-triệt-để)
3. [Cập nhật Cấu trúc Cơ sở dữ liệu (Database Schema Synchronization)](#3-cập-nhật-cấu-trúc-cơ-sở-dữ-liệu-database-schema-synchronization)
4. [Chi tiết Logic nghiệp vụ đã hoàn thiện trên Form Admin (`Frm_Admin.cs`)](#4-chi-tiết-logic-nghiệp-vụ-đã-hoàn-thiện-trên-form-admin-frm_admincs)
   - [4.1. Quản lý Giáo Viên](#41-quản-lý-giáo-viên)
   - [4.2. Quản lý Học Viên](#42-quản-lý-học-viên)
   - [4.3. Quản lý Khóa Học](#43-quản-lý-khóa-học)
   - [4.4. Quản lý Lớp Học & Tỷ lệ Sĩ số (Hiện tại / Max)](#44-quản-lý-lớp-học--tỷ-lệ-sĩ-số-hiện-tại--max)
5. [Sửa lỗi Form Kiểm Tra / Làm bài thi (`Frm_KiemTra.cs`)](#5-sửa-lỗi-form-kiểm-tra--làm-bài-thi-frm_kiemtracs)
6. [Tài khoản kiểm thử mặc định](#6-tài-khoản-kiểm-thử-mặc-định)

---

## 1. Khảo sát các tên Controls (Toolbox) đã đổi trên Form Admin

Hệ thống controls trên `Frm_Admin` đã được đồng bộ chuẩn định danh:

### 1.1. Tab Quản lý Giáo Viên (`pageGiaoVien`)
| Tên Control | Loại Control | Chức năng |
| :--- | :--- | :--- |
| `txtGV_HoTen` | MaterialTextBoxEdit | Nhập họ và tên giáo viên |
| `txtGV_NgaySinh` | MaterialTextBoxEdit | Nhập ngày sinh (định dạng `dd/MM/yyyy`) |
| `txtGV_GioiTinh` | MaterialComboBox | Chọn giới tính (Nam / Nữ / Khác) |
| `txtGV_ChuyenMon` | MaterialTextBoxEdit | Nhập chuyên ngành/môn giảng dạy |
| `txtGV_Email` | MaterialTextBoxEdit | Nhập địa chỉ Email |
| `txtGV_SoDienThoai` | MaterialTextBoxEdit | Nhập số điện thoại liên lạc |
| `txtGV_DiaChi` | MaterialTextBoxEdit | Nhập địa chỉ cư trú |
| `txtGV_TimKiem` | MaterialTextBoxEdit | Ô nhập tìm kiếm giáo viên theo tên/sđt/email |
| `btnGV_Them` | MaterialButton | Nút Thêm mới giáo viên |
| `btnGV_Sua` | MaterialButton | Nút Cập nhật thông tin giáo viên đã chọn |
| `btnGV_Xoa` | MaterialButton | Nút Xóa giáo viên đã chọn |
| `btnGV_LamMoi` | MaterialButton | Nút Xóa trắng form nhập liệu |
| `dgvGiaoVien` | DataGridView | Bảng danh sách giáo viên hệ thống |

### 1.2. Tab Quản lý Học Viên (`pageHocVien`)
| Tên Control | Loại Control | Chức năng |
| :--- | :--- | :--- |
| `txtHV_HoTen` | MaterialTextBoxEdit | Nhập họ tên học viên |
| `txtHV_NgaySinh` | MaterialTextBoxEdit | Nhập ngày sinh học viên |
| `cboHV_GioiTinh` | MaterialComboBox | Chọn giới tính học viên |
| `txtHV_Email` | MaterialTextBoxEdit | Nhập email học viên |
| `txtHV_SoDienThoai` | MaterialTextBoxEdit | Nhập số điện thoại học viên |
| `txtHV_DiaChi` | MaterialTextBoxEdit | Nhập nơi ở học viên |
| `txtHV_TimKiem` | MaterialTextBoxEdit | Ô tìm kiếm học viên theo tên/email/sđt |
| `btnHV_Them` | MaterialButton | Nút Thêm mới học viên |
| `btnHV_Sua` | MaterialButton | Nút Cập nhật học viên |
| `btnHV_Xoa` | MaterialButton | Nút Xóa học viên |
| `btnHV_LamMoi` | MaterialButton | Nút Làm mới form học viên |
| `dgvHocVien` | DataGridView | Bảng hiển thị danh sách học viên |

### 1.3. Tab Khóa Học & Lớp Học (`pageKhoaHocVaLop`)
| Tên Control | Loại Control | Chức năng |
| :--- | :--- | :--- |
| **Khối Khóa Học:** | | |
| `txtKH_TenKhoaHoc` | MaterialTextBoxEdit | Nhập tên khóa học |
| `txtKH_MoTa` | MaterialTextBoxEdit | Nhập mô tả mục tiêu khóa học |
| `btnKH_Them` | MaterialButton | Nút Thêm khóa học mới |
| `btnKH_Sua` | MaterialButton | Nút Cập nhật khóa học |
| `btnKH_Xoa` | MaterialButton | Nút Xóa khóa học |
| `dgvKhoaHoc` | DataGridView | Bảng danh sách các khóa học |
| **Khối Lớp Học:** | | |
| `txtLop_TenLop` | MaterialTextBoxEdit | Nhập tên lớp học |
| `txtLop_SiSoToiDa` | MaterialTextBoxEdit | Nhập sĩ số tối đa của lớp (mặc định 40) |
| `cboLop_KhoaHoc` | MaterialComboBox | Dropdown chọn Khóa học trực thuộc |
| `materialComboBox2` | MaterialComboBox | Dropdown chọn Giáo viên phụ trách giảng dạy |
| `btnLop_Them` | MaterialButton | Nút Thêm lớp học mới |
| `btnLop_Sua` | MaterialButton | Nút Sửa thông tin lớp học |
| `btnLop_Xoa` | MaterialButton | Nút Xóa lớp học |
| `dgvLopHoc` | DataGridView | Bảng danh sách lớp hiển thị Tên, Sĩ số (Hiện tại/Max), Khóa học, Giáo viên |

---

## 2. Nguyên nhân các lỗi khi Đăng Nhập & Cách khắc phục triệt để

### ⚠️ Nguyên nhân 1: Xung đột Cột Cơ sở dữ liệu giữa DAL và SQL Server (Gây Crash khi vừa mở Form)
- **Khi đăng nhập Admin:** `FrmMain_Load` gọi `LoadDanhSachGiaoVien()` -> gọi `_gvDAL.LayTatCa()`. Câu lệnh SQL yêu cầu cột `ChuyenMon`, `SoDienThoai`. Tuy nhiên, kịch bản tạo bảng CSDL ban đầu chỉ có `DienThoai` và không có `ChuyenMon`. SQL Server ném ngoại lệ `SqlException: Invalid column name 'ChuyenMon'`. Vì không có khối bắt lỗi nên Form Admin bị văng ngay khi chuyển trang.
- **Khi đăng nhập Học Viên:** `HocVien_Load` gọi `LoadLopHoc()`, câu truy vấn `LopHocDAL.LayTheoHocSinh()` truy vấn bảng `DangKyLop` và cột `LichHoc`. Cả hai đối tượng này chưa được tạo trong CSDL, gây ngoại lệ `Invalid object name 'DangKyLop'`. Tiếp theo, `LoadLichSuThi()` truy vấn các cột `TenDeThi`, `MonHoc`, `ThoiGianNop` không trùng với tên trong CSDL.
- **Biểu đồ Thống kê Dashboard:** Truy vấn `AVG(k.Diem)` nhóm theo `d.TenDeThi` nhưng cột trong CSDL là `TenDe`.

### ⚠️ Nguyên nhân 2: Lỗi điều hướng và treo tiến trình ngầm (Zombie Process)
- Trong `frm_DangNhap.cs`, sau khi bấm Đăng Nhập, form gọi `this.Hide()` và `nextForm.Show()`.
- Khi người dùng đóng Form Admin/Giáo viên/Học viên bằng nút **[X]**, vì `frm_DangNhap` vẫn đang chạy ẩn nên ứng dụng **không thoát hoàn toàn**. File `.exe` và kết nối database bị khóa lại ngầm, gây xung đột phiên tiếp theo.
- **Khắc phục:** Đã bổ sung sự kiện `nextForm.FormClosed += (s, args) => Application.Exit();` và cơ chế hiển thị lại form đăng nhập khi người dùng chọn tab **Đăng Xuất**.

### ⚠️ Nguyên nhân 3: Sự kiện Buttons bị mất liên kết trong Designer
- Khi đổi tên control trong Designer, các phương thức xử lý sự kiện như `btnGV_Them_Click`, `btnHV_Them_Click`, `btnLop_Them_Click`, `dgvGiaoVien.CellClick`... bị ngắt liên kết (không được gán vào `Click += ...`).
- **Khắc phục:** Đã gán trực tiếp toàn bộ sự kiện thông qua phương thức `SetupEventHandlers()` ngay trong code-behind của Form, đảm bảo hoạt động 100% không lo Designer bị reset.

---

## 3. Cập nhật Cấu trúc Cơ sở dữ liệu (Database Schema Synchronization)

Đã cập nhật tệp `CreateDataBase.cs` và đồng bộ CSDL SQL Server với kịch bản sau:

```sql
-- 1. Bổ sung các cột cho bảng GiaoVien
IF COL_LENGTH('GiaoVien', 'ChuyenMon') IS NULL ALTER TABLE GiaoVien ADD ChuyenMon NVARCHAR(100);
IF COL_LENGTH('GiaoVien', 'GioiTinh') IS NULL ALTER TABLE GiaoVien ADD GioiTinh NVARCHAR(10);
IF COL_LENGTH('GiaoVien', 'SoDienThoai') IS NULL 
BEGIN
    IF COL_LENGTH('GiaoVien', 'DienThoai') IS NOT NULL
        EXEC sp_rename 'GiaoVien.DienThoai', 'SoDienThoai', 'COLUMN';
    ELSE
        ALTER TABLE GiaoVien ADD SoDienThoai NVARCHAR(20);
END

-- 2. Bổ sung các cột cho bảng HocSinh
IF COL_LENGTH('HocSinh', 'GioiTinh') IS NULL ALTER TABLE HocSinh ADD GioiTinh NVARCHAR(10);
IF COL_LENGTH('HocSinh', 'SoDienThoai') IS NULL 
BEGIN
    IF COL_LENGTH('HocSinh', 'DienThoai') IS NOT NULL
        EXEC sp_rename 'HocSinh.DienThoai', 'SoDienThoai', 'COLUMN';
    ELSE
        ALTER TABLE HocSinh ADD SoDienThoai NVARCHAR(20);
END

-- 3. Bổ sung các cột cho bảng LopHoc
IF COL_LENGTH('LopHoc', 'SiSoToiDa') IS NULL ALTER TABLE LopHoc ADD SiSoToiDa INT DEFAULT 40;
IF COL_LENGTH('LopHoc', 'LichHoc') IS NULL ALTER TABLE LopHoc ADD LichHoc NVARCHAR(100);
IF COL_LENGTH('LopHoc', 'TrangThai') IS NULL ALTER TABLE LopHoc ADD TrangThai NVARCHAR(50) DEFAULT N'Đang mở';

-- 4. Bổ sung bảng DangKyLop
IF OBJECT_ID('DangKyLop', 'U') IS NULL
CREATE TABLE DangKyLop (
    DangKyID INT IDENTITY(1,1) PRIMARY KEY,
    LopHocID INT,
    HocSinhID INT,
    TrangThai NVARCHAR(50) DEFAULT N'Đã duyệt',
    NgayDangKy DATETIME DEFAULT GETDATE()
);

-- 5. Đồng bộ bảng DeThi và KetQuaThi
IF COL_LENGTH('DeThi', 'TenDeThi') IS NULL 
BEGIN
    IF COL_LENGTH('DeThi', 'TenDe') IS NOT NULL
        EXEC sp_rename 'DeThi.TenDe', 'TenDeThi', 'COLUMN';
    ELSE
        ALTER TABLE DeThi ADD TenDeThi NVARCHAR(150);
END
IF COL_LENGTH('DeThi', 'MonHoc') IS NULL ALTER TABLE DeThi ADD MonHoc NVARCHAR(100);
IF COL_LENGTH('DeThi', 'SoCau') IS NULL ALTER TABLE DeThi ADD SoCau INT DEFAULT 20;

IF COL_LENGTH('KetQuaThi', 'ThoiGianNop') IS NULL 
BEGIN
    IF COL_LENGTH('KetQuaThi', 'NgayThi') IS NOT NULL
        EXEC sp_rename 'KetQuaThi.NgayThi', 'ThoiGianNop', 'COLUMN';
    ELSE
        ALTER TABLE KetQuaThi ADD ThoiGianNop DATETIME DEFAULT GETDATE();
END
```

---

## 4. Chi tiết Logic nghiệp vụ đã hoàn thiện trên Form Admin (`Frm_Admin.cs`)

### 4.1. Quản lý Giáo Viên
- **Nạp danh sách:** Nạp trực tiếp từ `GiaoVienDAL.LayTatCa()`, tự động cấu hình `DataPropertyName` cho các cột: Mã GV, Họ Tên, Chuyên Môn, Email, Số Điện Thoại.
- **Thêm mới:** Kiểm tra họ tên bắt buộc, phân tích ngày sinh hợp lệ (`dd/MM/yyyy`), thêm vào CSDL, làm mới form, đồng thời cập nhật ngay Dropdown giáo viên bên tab Lớp Học và số lượng trên Dashboard.
- **Sửa:** Chọn dòng trên DataGridView -> Dữ liệu đổ lên các ô nhập liệu -> Bấm "Sửa" để cập nhật.
- **Xóa:** Hỏi xác nhận trước khi xóa, tự động kiểm tra và xóa an toàn.
- **Làm mới:** Reset toàn bộ textbox và focus vào ô Họ tên.
- **Tìm kiếm:** Bắt sự kiện `TextChanged` trên `txtGV_TimKiem`, tìm kiếm tức thì theo Họ tên, Chuyên môn, Email hoặc Số điện thoại.

### 4.2. Quản lý Học Viên
- **Nạp danh sách:** Hiển thị Mã HV, Họ Tên, Giới Tính, Email, Số Điện Thoại.
- **Thêm/Sửa/Xóa/Làm Mới:** Tương tự như Giáo Viên.
- **Tìm kiếm:** Tìm theo tên, email, số điện thoại tức thì qua `txtHV_TimKiem.TextChanged`.
- **Đồng bộ sĩ số:** Khi thêm/xóa học viên, danh sách Lớp Học được cập nhật lại để tự tính toán lại tỷ lệ sĩ số tức thời.

### 4.3. Quản lý Khóa Học
- **Thao tác:** Thêm, Sửa, Xóa thông tin Tên Khóa Học và Mô Tả.
- **Ràng buộc an toàn:** Nếu khóa học đang có lớp học trực thuộc, hệ thống sẽ cảnh báo và chặn thao tác xóa để bảo toàn dữ liệu.
- **Tự động đồng bộ ComboBox:** Khi thêm/sửa khóa học, danh sách chọn khóa học bên tab Lớp Học được cập nhật ngay lập tức.

### 4.4. Quản lý Lớp Học & Tỷ lệ Sĩ số (Hiện tại / Max)
- **Hiển thị đúng yêu cầu:** Cột `Sĩ Số` trên `dgvLopHoc` được định dạng dưới dạng:  
  $$\text{Sĩ Số} = \frac{\text{Số học sinh thực tế trong lớp}}{\text{Sĩ số tối đa}}$$
  *Ví dụ:* `1/40`
- **Câu lệnh truy vấn tự động đếm:**
  ```sql
  SELECT 
      l.LopHocID, 
      l.TenLop, 
      CAST((SELECT COUNT(*) FROM HocSinh h WHERE h.LopHocID = l.LopHocID) AS VARCHAR(10)) + '/' + CAST(ISNULL(l.SiSoToiDa, 40) AS VARCHAR(10)) AS SiSo,
      ISNULL(k.TenKhoaHoc, N'Chưa phân loại') AS TenKhoaHoc, 
      ISNULL(g.HoTen, N'Chưa phân công') AS TenGiaoVien, 
      l.KhoaHocID, 
      l.GiaoVienID, 
      ISNULL(l.SiSoToiDa, 40) AS SiSoToiDa
  FROM LopHoc l
  LEFT JOIN KhoaHoc k ON l.KhoaHocID = k.KhoaHocID
  LEFT JOIN GiaoVien g ON l.GiaoVienID = g.GiaoVienID
  ```
- **Thao tác:**
  - Thêm lớp với Tên lớp, Sĩ số tối đa, chọn Khóa học và Giáo viên phụ trách từ dropdown.
  - Sửa và Xóa có kiểm tra ràng buộc không cho xóa lớp nếu đang có học viên theo học.

---

## 5. Sửa lỗi Form Kiểm Tra / Làm bài thi (`Frm_KiemTra.cs`)

Trong tệp `Frm_KiemTra.Designer.cs`, các control đã được đổi sang:
- `lblKiemTra_CauHoi` (thay cho `lblNoiDungCauHoi`)
- `lblKiemTra_SoCauHoi` (thay cho nhãn thời gian)
- `materialProgressBar1` (thay cho `hopeProgressBar1`)
- `radKiemTra_DapAnA`, `B`, `C`, `D`
- `btnKiemTra_CauTruoc`, `btnKiemTra_CauSau`, `btnKiemTra_NopBai`

Tệp `Frm_KiemTra.cs` đã được cập nhật đồng bộ 100% với các tên biến mới, sửa lỗi tráo nút điều hướng trước đó, đảm bảo dự án `Build Succeeded 0 Error`.

---

## 6. Tài khoản kiểm thử mặc định

Bạn có thể đăng nhập ngay vào ứng dụng với các tài khoản sau:

| Vai trò | Tên đăng nhập | Mật khẩu | Chuyển đến Form |
| :--- | :--- | :---: | :--- |
| **Quản trị viên (Admin)** | `admin` | `123456` | `Frm_Admin` |
| **Giáo viên** | `giaovien1` | `123456` | `frm_GiaoVien` |
| **Học viên** | `hocsinh1` | `123456` | `HocVien` |

> [!TIP]
> Toàn bộ logic đã được biên dịch thành công (`0 Error(s)`). Bạn có thể bấm **F5** hoặc **Start** trong Visual Studio để chạy thử ngay lập tức!
