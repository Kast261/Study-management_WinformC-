# 📘 TÀI LIỆU GỢI Ý & HƯỚNG DẪN CODE TOÀN DIỆN CHO DỰ ÁN

> **Dự án:** Quản Lý Học Tập (`BLT_Quan_Ly_Hoc_Tap`)  
> **Căn cứ kế hoạch:** `TOAN_BO_CODE_CAN_VIET.md`  
> **Framework:** .NET Framework 4.8 / Windows Forms / ReaLTaiizor / SQL Server  
> **Ngày cập nhật:** 2026-10-17

---

## 📑 MỤC LỤC

1. [DATABASE MIGRATION & BỔ SUNG CƠ SỞ DỮ LIỆU](#1-database-migration--bổ-sung-cơ-sở-dữ-liệu)
2. [TẦNG TRUY CẬP DỮ LIỆU (DAL) CẦN BỔ SUNG](#2-tầng-truy-cập-dữ-liệu-dal-cần-bổ-sung)
   - 2.1. Hoàn thiện `DAL/TaiKhoanDAL.cs` (Sửa 2 lỗi build hiện tại)
   - 2.2. Bổ sung `DAL/LopHocDAL.cs` (Duyệt học viên, đóng lớp)
   - 2.3. Bổ sung `DAL/DeThiDAL.cs` (Sửa/Xóa câu hỏi, quản lý bài thi)
   - 2.4. Bổ sung `DAL/KetQuaDAL.cs` (Xem chi tiết câu trả lời)
3. [SỬA LỖI ADMIN FORM (`Forms/Frm_Admin.cs`)](#3-sửa-lỗi-admin-form-formsfrm_admincs)
4. [CODE HOÀN CHỈNH FORM KIỂM TRA (`Forms/Frm_KiemTra.cs`)](#4-code-hoàn-chỉnh-form-kiểm-tra-formsfrm_kiemtracs)
5. [CODE HOÀN CHỈNH FORM GIÁO VIÊN (`Forms/frm_GiaoVien.cs`)](#5-code-hoàn-chỉnh-form-giáo-viên-formsfrm_giaoviencs)
6. [CODE HOÀN CHỈNH FORM HỌC VIÊN (`Forms/Frm_HocVien.cs`)](#6-code-hoàn-chỉnh-form-học-viên-formsfrm_hocviencs)
7. [FORM PHỤ: XEM LẠI CHI TIẾT BÀI THI (`Forms/Frm_XemLaiBaiThi.cs`)](#7-form-phụ-xem-lại-chi-tiết-bài-thi-formsfrm_xemlaibaithics)
8. [THỨ TỰ TRIỂN KHAI VÀ CHECKLIST KIỂM THỬ](#8-thứ-tự-triển-khai-và-checklist-kiểm-thử)

---

## 1. DATABASE MIGRATION & BỔ SUNG CƠ SỞ DỮ LIỆU

### 1.1. Script T-SQL chạy trực tiếp trong SQL Server Management Studio (SSMS)

Mở SSMS, chọn Database `BTL_QL_Hoc_Tap` (hoặc tên Database trong `App.config`) và thực thi đoạn script sau:

```sql
    USE BTL_QL_Hoc_Tap;
    GO

    -- 1. Thêm cột TaiKhoanID vào bảng GiaoVien và HocSinh nếu chưa có
    IF COL_LENGTH('GiaoVien', 'TaiKhoanID') IS NULL
        ALTER TABLE GiaoVien ADD TaiKhoanID INT NULL;
    GO

    IF COL_LENGTH('HocSinh', 'TaiKhoanID') IS NULL
        ALTER TABLE HocSinh ADD TaiKhoanID INT NULL;
    GO

    -- 2. Thêm cột Ngày Bắt Đầu & Kết Thúc vào bảng LopHoc nếu chưa có
    IF COL_LENGTH('LopHoc', 'NgayBatDau') IS NULL
        ALTER TABLE LopHoc ADD NgayBatDau DATE NULL;
    GO

    IF COL_LENGTH('LopHoc', 'NgayKetThuc') IS NULL
        ALTER TABLE LopHoc ADD NgayKetThuc DATE NULL;
    GO

    -- 3. Đảm bảo bảng DangKyLop có cột TrangThai hợp lệ ('Chờ duyệt', 'Đã duyệt', 'Từ chối')
    IF OBJECT_ID('DangKyLop', 'U') IS NULL
    BEGIN
        CREATE TABLE DangKyLop (
            DangKyID INT IDENTITY(1,1) PRIMARY KEY,
            LopHocID INT NOT NULL,
            HocSinhID INT NOT NULL,
            TrangThai NVARCHAR(50) DEFAULT N'Chờ duyệt',
            NgayDangKy DATETIME DEFAULT GETDATE()
        );
    END
    GO
```

### 1.2. Cập nhật code trong `Database/CreateDataBase.cs`

Bổ sung các dòng kiểm tra trên vào phương thức `CreateDataBase.Init()` để ứng dụng tự động kiểm tra và thêm cột khi khởi chạy mà không cần chạy SQL thủ công:

```csharp
// Thêm đoạn này vào bên trong chuỗi createTablesSql trong CreateDataBase.cs
IF COL_LENGTH('GiaoVien', 'TaiKhoanID') IS NULL ALTER TABLE GiaoVien ADD TaiKhoanID INT NULL;
IF COL_LENGTH('HocSinh', 'TaiKhoanID') IS NULL ALTER TABLE HocSinh ADD TaiKhoanID INT NULL;
IF COL_LENGTH('LopHoc', 'NgayBatDau') IS NULL ALTER TABLE LopHoc ADD NgayBatDau DATE NULL;
IF COL_LENGTH('LopHoc', 'NgayKetThuc') IS NULL ALTER TABLE LopHoc ADD NgayKetThuc DATE NULL;
```

---

## 2. TẦNG TRUY CẬP DỮ LIỆU (DAL) CẦN BỔ SUNG

### 2.1. File `DAL/TaiKhoanDAL.cs`

> ⚠️ **Lưu ý:** Hiện tại `Frm_Admin.cs` đang bị lỗi biên dịch (Compile Error) CS1061 do thiếu 2 phương thức:
>
> - `LayTatCaCoTenNguoiDung()`
> - `ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)`

Hãy thêm các phương thức sau vào class `TaiKhoanDAL`:

```csharp
/// <summary>
/// Lấy toàn bộ danh sách tài khoản kèm tên thực tế của Người dùng (Giáo viên hoặc Học sinh)
/// và hiển thị trạng thái Khóa / Hoạt động trực quan cho DataGridView dgvTaiKhoan.
/// </summary>
public DataTable LayTatCaCoTenNguoiDung()
{
    string sql = @"
        SELECT
            tk.TaiKhoanID,
            tk.TenDangNhap,
            tk.VaiTro,
            tk.NguoiDungID,
            ISNULL(tk.TrangThaiKhoa, 0) AS TrangThaiKhoa,
            CASE
                WHEN tk.VaiTro = 'GiaoVien' THEN ISNULL(gv.HoTen, N'(Chưa liên kết)')
                WHEN tk.VaiTro = 'HocSinh'  THEN ISNULL(hs.HoTen, N'(Chưa liên kết)')
                WHEN tk.VaiTro = 'Admin'    THEN N'Quản trị viên hệ thống'
                ELSE N'(Chưa xác định)'
            END AS TenNguoiDung,
            CASE
                WHEN ISNULL(tk.TrangThaiKhoa, 0) = 1 THEN N'Đã khóa'
                ELSE N'Hoạt động'
            END AS TrangThaiHienThi
        FROM TaiKhoan tk
        LEFT JOIN GiaoVien gv ON tk.VaiTro = 'GiaoVien' AND tk.NguoiDungID = gv.GiaoVienID
        LEFT JOIN HocSinh hs  ON tk.VaiTro = 'HocSinh'  AND tk.NguoiDungID = hs.HocSinhID
        ORDER BY tk.TaiKhoanID DESC";

    return DatabaseHelper.ExecuteQuery(sql);
}

/// <summary>
/// Thêm một tài khoản mới kèm băm mật khẩu và liên kết ngược ID vào bảng GiaoVien/HocSinh.
/// </summary>
public bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)
{
    // Kiểm tra tên đăng nhập đã tồn tại chưa
    string checkSql = "SELECT COUNT(*) FROM TaiKhoan WHERE LOWER(TenDangNhap) = LOWER(@user)";
    int count = Convert.ToInt32(DatabaseHelper.ExecuteScalar(checkSql, new SqlParameter[] {
        new SqlParameter("@user", tenDangNhap.Trim())
    }));
    if (count > 0) return false;

    string hashPass = Passwords.Hash(matKhauPlain.Trim());
    string sql = @"
        INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID, TrangThaiKhoa)
        VALUES (@user, @mk, @vaiTro, @nguoiDungID, 0);
        SELECT SCOPE_IDENTITY();";

    SqlParameter[] pars = {
        new SqlParameter("@user", tenDangNhap.Trim()),
        new SqlParameter("@mk", hashPass),
        new SqlParameter("@vaiTro", vaiTro),
        new SqlParameter("@nguoiDungID", nguoiDungID)
    };

    object result = DatabaseHelper.ExecuteScalar(sql, pars);
    if (result == null || result == DBNull.Value) return false;

    int newTaiKhoanID = Convert.ToInt32(result);

    // Cập nhật TaiKhoanID ngược lại bảng thực thể
    try
    {
        if (vaiTro == "GiaoVien" && nguoiDungID > 0)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE GiaoVien SET TaiKhoanID = @tkID WHERE GiaoVienID = @gvID",
                new SqlParameter[] {
                    new SqlParameter("@tkID", newTaiKhoanID),
                    new SqlParameter("@gvID", nguoiDungID)
                });
        }
        else if (vaiTro == "HocSinh" && nguoiDungID > 0)
        {
            DatabaseHelper.ExecuteNonQuery(
                "UPDATE HocSinh SET TaiKhoanID = @tkID WHERE HocSinhID = @hsID",
                new SqlParameter[] {
                    new SqlParameter("@tkID", newTaiKhoanID),
                    new SqlParameter("@hsID", nguoiDungID)
                });
        }
    }
    catch { /* Nếu bảng chưa có cột TaiKhoanID thì bỏ qua lỗi */ }

    return true;
}
```

---

### 2.2. File `DAL/LopHocDAL.cs`

Bổ sung các hàm quản lý Duyệt học viên & Đóng đăng ký lớp:

```csharp
/// <summary>
/// Lấy danh sách học viên đăng ký lớp đang ở trạng thái 'Chờ duyệt' cho một lớp cụ thể.
/// </summary>
public DataTable LayDangKyTheoLop(int lopHocID)
{
    string sql = @"
        SELECT
            dk.DangKyID,
            dk.LopHocID,
            dk.HocSinhID,
            hs.HoTen,
            hs.GioiTinh,
            hs.Email,
            hs.SoDienThoai,
            dk.NgayDangKy,
            dk.TrangThai
        FROM DangKyLop dk
        JOIN HocSinh hs ON dk.HocSinhID = hs.HocSinhID
        WHERE dk.LopHocID = @lopID AND dk.TrangThai = N'Chờ duyệt'
        ORDER BY dk.NgayDangKy ASC";

    return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@lopID", lopHocID) });
}

/// <summary>
/// Duyệt học viên vào lớp: chuyển trạng thái DangKyLop thành 'Đã duyệt' và gán LopHocID cho HocSinh.
/// </summary>
public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID)
{
    string sql = @"
        UPDATE DangKyLop SET TrangThai = N'Đã duyệt' WHERE DangKyID = @dkID;
        UPDATE HocSinh SET LopHocID = @lopID WHERE HocSinhID = @hsID;";

    SqlParameter[] pars = {
        new SqlParameter("@dkID", dangKyID),
        new SqlParameter("@lopID", lopHocID),
        new SqlParameter("@hsID", hocSinhID)
    };
    return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
}

/// <summary>
/// Từ chối học viên đăng ký vào lớp.
/// </summary>
public bool TuChoiDangKy(int dangKyID)
{
    string sql = "UPDATE DangKyLop SET TrangThai = N'Từ chối' WHERE DangKyID = @dkID";
    return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@dkID", dangKyID) }) > 0;
}

/// <summary>
/// Đóng đăng ký lớp học (chuyển TrangThai thành 'Đã đóng').
/// </summary>
public bool DongDangKyLop(int lopHocID)
{
    string sql = "UPDATE LopHoc SET TrangThai = N'Đã đóng' WHERE LopHocID = @id";
    return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", lopHocID) }) > 0;
}
```

---

### 2.3. File `DAL/DeThiDAL.cs`

Bổ sung các phương thức sửa/xóa câu hỏi và lấy kết quả thi của học sinh cho giáo viên:

```csharp
public bool SuaCauHoi(CauHoi ch)
{
    string sql = @"
        UPDATE CauHoi
        SET NoiDung = @nd, DapAnA = @a, DapAnB = @b, DapAnC = @c, DapAnD = @d, DapAnDung = @dung
        WHERE CauHoiID = @id";

    SqlParameter[] pars = {
        new SqlParameter("@nd", ch.NoiDung),
        new SqlParameter("@a", ch.DapAnA),
        new SqlParameter("@b", ch.DapAnB),
        new SqlParameter("@c", ch.DapAnC),
        new SqlParameter("@d", ch.DapAnD),
        new SqlParameter("@dung", ch.DapAnDung),
        new SqlParameter("@id", ch.CauHoiID)
    };
    return DatabaseHelper.ExecuteNonQuery(sql, pars) > 0;
}

public bool XoaCauHoi(int cauHoiID)
{
    string sql = "DELETE FROM CauHoi WHERE CauHoiID = @id";
    return DatabaseHelper.ExecuteNonQuery(sql, new SqlParameter[] { new SqlParameter("@id", cauHoiID) }) > 0;
}

/// <summary>
/// Lấy danh sách kết quả bài thi của học sinh theo mã đề thi (hiển thị lên Tab Kết Quả của Giáo Viên)
/// </summary>
public DataTable LayKetQuaTheoDeThi(int deThiID)
{
    string sql = @"
        SELECT
            hs.HoTen AS TenHocSinh,
            dt.TenDeThi,
            CAST(kq.SoCauDung AS VARCHAR(10)) + '/' + CAST(ISNULL(dt.SoCau, 20) AS VARCHAR(10)) AS SoCauDung,
            kq.Diem,
            kq.ThoiGianNop
        FROM KetQuaThi kq
        JOIN HocSinh hs ON kq.HocSinhID = hs.HocSinhID
        JOIN DeThi dt   ON kq.DeThiID = dt.DeThiID
        WHERE kq.DeThiID = @deID
        ORDER BY kq.ThoiGianNop DESC";

    return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@deID", deThiID) });
}
```

---

### 2.4. File `DAL/KetQuaDAL.cs`

Bổ sung phương thức lấy chi tiết bài làm để phục vụ Popup xem lại bài kiểm tra:

```csharp
/// <summary>
/// Lấy chi tiết câu hỏi, đáp án đã chọn và đáp án đúng cho một bài thi của học sinh.
/// </summary>
public DataTable LayChiTietBaiLamTheoKetQua(int ketQuaID)
{
    string sql = @"
        SELECT
            ch.NoiDung AS CauHoi,
            ch.DapAnA, ch.DapAnB, ch.DapAnC, ch.DapAnD,
            ch.DapAnDung,
            ct.DapAnChon,
            ISNULL(ct.LaDung, 0) AS LaDung
        FROM ChiTietBaiLam ct
        JOIN CauHoi ch ON ct.CauHoiID = ch.CauHoiID
        WHERE ct.KetQuaID = @kqID
        ORDER BY ch.ThuTu ASC, ch.CauHoiID ASC";

    return DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@kqID", ketQuaID) });
}
```

---

## 3. SỬA LỖI ADMIN FORM (`Forms/Frm_Admin.cs`)

### Mục tiêu:

1. Khi click **Sửa Giáo Viên** (`btnGV_Sua_Click`), sau khi cập nhật thành công phải gọi:
   - `LoadDanhSachGiaoVien()`
   - `LamMoiFormGV()`
2. Khi click **Sửa Học Viên** (`btnHV_Sua_Click`), sau khi cập nhật thành công phải gọi:
   - `LoadDanhSachHocVien()`
   - `LamMoiFormHV()`

### Code chuẩn trong `Forms/Frm_Admin.cs`:

```csharp
private void btnGV_Sua_Click(object sender, EventArgs e)
{
    if (_selectedGiaoVienID <= 0)
    {
        MessageBox.Show("Vui lòng chọn giáo viên cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    if (string.IsNullOrWhiteSpace(txtGV_HoTen.Text))
    {
        MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtGV_HoTen.Focus();
        return;
    }

    GiaoVien gv = new GiaoVien
    {
        GiaoVienID = _selectedGiaoVienID,
        HoTen = txtGV_HoTen.Text.Trim(),
        NgaySinh = ParseDate(txtGV_NgaySinh.Text),
        GioiTinh = txtGV_GioiTinh.SelectedItem?.ToString(),
        ChuyenMon = txtGV_ChuyenMon.Text.Trim(),
        Email = txtGV_Email.Text.Trim(),
        SoDienThoai = txtGV_SoDienThoai.Text.Trim(),
        DiaChi = txtGV_DiaChi.Text.Trim()
    };

    if (_gvDAL.Sua(gv))
    {
        MessageBox.Show("Cập nhật thông tin giáo viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadDanhSachGiaoVien(); // Tự động reload DGV
        LoadDropdownData();     // Reload combobox lớp học nếu đổi tên GV
        LamMoiFormGV();         // Reset sạch form nhập
    }
}

private void btnHV_Sua_Click(object sender, EventArgs e)
{
    if (_selectedHocVienID <= 0)
    {
        MessageBox.Show("Vui lòng chọn học viên cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    if (string.IsNullOrWhiteSpace(txtHV_HoTen.Text))
    {
        MessageBox.Show("Họ tên không được để trống!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        txtHV_HoTen.Focus();
        return;
    }

    HocSinh hs = new HocSinh
    {
        HocSinhID = _selectedHocVienID,
        HoTen = txtHV_HoTen.Text.Trim(),
        NgaySinh = ParseDate(txtHV_NgaySinh.Text),
        GioiTinh = cboHV_GioiTinh.SelectedItem?.ToString(),
        Email = txtHV_Email.Text.Trim(),
        SoDienThoai = txtHV_SoDienThoai.Text.Trim(),
        DiaChi = txtHV_DiaChi.Text.Trim()
    };

    if (_hsDAL.Sua(hs))
    {
        MessageBox.Show("Cập nhật thông tin học viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
        LoadDanhSachHocVien(); // Tự động reload DGV
        LamMoiFormHV();         // Reset sạch form nhập
    }
}
```

---

## 4. CODE HOÀN CHỈNH FORM KIỂM TRA (`Forms/Frm_KiemTra.cs`)

### 4 yêu cầu sửa đổi:

1. **Timer**: Đổi sang màu đỏ (`Color.Red`) khi thời gian còn $\le$ 5 phút (300 giây).
2. **ProgressBar**: Chỉ tăng theo tỉ lệ các câu **đã chọn đáp án**, không tăng bừa khi chỉ bấm "Câu sau".
3. **Điều hướng**: Nút "Câu trước" chỉ lùi đến câu 1 (`_currentIndex == 0` thì disable nút).
4. **Nộp bài**: Nếu còn câu chưa điền, hiển thị hộp thoại cảnh báo số câu chưa làm trước khi xác nhận nộp.

### Code hoàn chỉnh file `Forms/Frm_KiemTra.cs`:

```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Manager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class Frm_KiemTra : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly int _deThiID;
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();

        private DeThi _deThi;
        private List<CauHoi> _cauHois = new List<CauHoi>();
        private Dictionary<int, string> _dapAnChon = new Dictionary<int, string>(); // index -> "A"|"B"|"C"|"D"
        private int _currentIndex = 0;
        private int _thoiGianConLai; // tính bằng giây
        private Timer _timer;
        private bool _daNopBai = false;

        public Frm_KiemTra(int deThiID)
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            _deThiID = deThiID;
        }

        private void KiemTra_Load(object sender, EventArgs e)
        {
            _deThi = _deThiDAL.LayTheoID(_deThiID);
            _cauHois = _deThiDAL.LayDanhSachCauHoi(_deThiID);

            if (_deThi == null || _cauHois.Count == 0)
            {
                MessageBox.Show("Đề thi chưa có câu hỏi nào!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
                return;
            }

            this.Text = $"BÀI THI: {_deThi.TenDeThi} - Môn: {_deThi.MonHoc}";
            _thoiGianConLai = (_deThi.ThoiGian > 0 ? _deThi.ThoiGian : 15) * 60;

            // 1. Cấu hình Timer đếm ngược
            _timer = new Timer { Interval = 1000 };
            _timer.Tick += Timer_Tick;
            _timer.Start();

            // 2. Sự kiện nút điều hướng
            btnKiemTra_CauTruoc.Click += (s, ev) => ChuyenCau(_currentIndex - 1);
            btnKiemTra_CauSau.Click += (s, ev) => ChuyenCau(_currentIndex + 1);
            btnKiemTra_NopBai.Click += (s, ev) => XacNhanNopBai();

            // 3. Sự kiện chọn Radio button
            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;

            // Hiển thị câu hỏi đầu tiên
            HienThiCauHoi(0);
            CapNhatTienDo();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (_thoiGianConLai > 0)
            {
                _thoiGianConLai--;
                int phut = _thoiGianConLai / 60;
                int giay = _thoiGianConLai % 60;
                lblKiemTra_SoCauHoi.Text = $"Thời gian: {phut:D2}:{giay:D2}";

                // Yêu cầu: Đổi màu chữ đỏ khi thời gian còn 5 phút (<= 300 giây)
                if (_thoiGianConLai <= 300)
                {
                    lblKiemTra_SoCauHoi.ForeColor = Color.Red;
                }
                else
                {
                    lblKiemTra_SoCauHoi.ForeColor = Color.Black;
                }
            }
            else
            {
                _timer.Stop();
                MessageBox.Show("Đã hết thời gian làm bài! Hệ thống tự động thu bài và chấm điểm.", "Hết giờ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                NopBai();
            }
        }

        private void HienThiCauHoi(int index)
        {
            if (index < 0 || index >= _cauHois.Count) return;
            _currentIndex = index;
            CauHoi q = _cauHois[index];

            lblKiemTra_CauHoi.Text = $"Câu {index + 1}/{_cauHois.Count}: {q.NoiDung}";
            radKiemTra_DapAnA.Text = $"A. {q.DapAnA}";
            radKiemTra_DapAnB.Text = $"B. {q.DapAnB}";
            radKiemTra_DapAnC.Text = $"C. {q.DapAnC}";
            radKiemTra_DapAnD.Text = $"D. {q.DapAnD}";

            // Tạm thời gỡ handler để tránh trigger CheckedChanged khi load
            radKiemTra_DapAnA.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged -= Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged -= Radio_CheckedChanged;

            radKiemTra_DapAnA.Checked = false;
            radKiemTra_DapAnB.Checked = false;
            radKiemTra_DapAnC.Checked = false;
            radKiemTra_DapAnD.Checked = false;

            if (_dapAnChon.ContainsKey(index))
            {
                string da = _dapAnChon[index];
                if (da == "A") radKiemTra_DapAnA.Checked = true;
                else if (da == "B") radKiemTra_DapAnB.Checked = true;
                else if (da == "C") radKiemTra_DapAnC.Checked = true;
                else if (da == "D") radKiemTra_DapAnD.Checked = true;
            }

            radKiemTra_DapAnA.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnB.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnC.CheckedChanged += Radio_CheckedChanged;
            radKiemTra_DapAnD.CheckedChanged += Radio_CheckedChanged;

            // Yêu cầu: Nút câu trước chỉ về đến câu 1 (index 0)
            btnKiemTra_CauTruoc.Enabled = (_currentIndex > 0);
            btnKiemTra_CauSau.Enabled = (_currentIndex < _cauHois.Count - 1);
        }

        private void Radio_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton rb = sender as RadioButton;
            if (rb != null && rb.Checked)
            {
                if (rb == radKiemTra_DapAnA) _dapAnChon[_currentIndex] = "A";
                else if (rb == radKiemTra_DapAnB) _dapAnChon[_currentIndex] = "B";
                else if (rb == radKiemTra_DapAnC) _dapAnChon[_currentIndex] = "C";
                else if (rb == radKiemTra_DapAnD) _dapAnChon[_currentIndex] = "D";

                // Yêu cầu: Tiến độ chỉ tăng khi đã chọn đáp án
                CapNhatTienDo();
            }
        }

        private void CapNhatTienDo()
        {
            if (_cauHois.Count > 0)
            {
                int phanTram = (int)((float)_dapAnChon.Count / _cauHois.Count * 100);
                proKT_TienDo.Value = Math.Min(100, phanTram);
            }
        }

        private void ChuyenCau(int newIndex)
        {
            if (newIndex >= 0 && newIndex < _cauHois.Count)
            {
                HienThiCauHoi(newIndex);
            }
        }

        private void XacNhanNopBai()
        {
            int soCauDaLam = _dapAnChon.Count;
            int tongSoCau = _cauHois.Count;

            string msg;
            // Yêu cầu: Cảnh báo nếu còn câu chưa điền
            if (soCauDaLam < tongSoCau)
            {
                int chuaLam = tongSoCau - soCauDaLam;
                msg = $"CẢNH BÁO: Bạn vẫn còn {chuaLam} câu chưa trả lời!\n" +
                      $"(Đã hoàn thành {soCauDaLam}/{tongSoCau} câu)\n\n" +
                      $"Bạn có thực sự muốn nộp bài sớm không?";
            }
            else
            {
                msg = $"Bạn đã hoàn thành đủ {soCauDaLam}/{tongSoCau} câu hỏi.\nBạn có chắc chắn muốn nộp bài?";
            }

            if (MessageBox.Show(msg, "Xác nhận nộp bài", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _timer.Stop();
                NopBai();
            }
        }

        private void NopBai()
        {
            _daNopBai = true;
            int soCauDung = 0;
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                if (_dapAnChon.ContainsKey(i) && string.Equals(_dapAnChon[i], q.DapAnDung, StringComparison.OrdinalIgnoreCase))
                {
                    soCauDung++;
                }
            }

            decimal diem = Math.Round(((decimal)soCauDung / _cauHois.Count) * 10, 2);

            KetQuaThi kq = new KetQuaThi
            {
                HocSinhID = Session.NguoiDungID,
                DeThiID = _deThiID,
                SoCauDung = soCauDung,
                Diem = diem,
                ThoiGianNop = DateTime.Now,
                TrangThai = "Hoàn thành"
            };

            int kqID = _ketQuaDAL.LuuKetQua(kq);

            // Lưu chi tiết câu đúng sai
            for (int i = 0; i < _cauHois.Count; i++)
            {
                CauHoi q = _cauHois[i];
                string chon = _dapAnChon.ContainsKey(i) ? _dapAnChon[i] : "";
                bool laDung = string.Equals(chon, q.DapAnDung, StringComparison.OrdinalIgnoreCase);
                _ketQuaDAL.LuuChiTietBaiLam(kqID, q.CauHoiID, chon, laDung);
            }

            MessageBox.Show(
                $"BẠN ĐÃ HOÀN THÀNH BÀI THI!\n\n" +
                $"Số câu trả lời đúng: {soCauDung}/{_cauHois.Count}\n" +
                $"Điểm số đạt được: {diem:0.00} / 10.0\n",
                "Kết Quả Chấm Điểm",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);

            this.Close();
        }

        private void Frm_KiemTra_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!_daNopBai)
            {
                DialogResult result = MessageBox.Show(
                    "Bạn đang trong phòng thi. Nếu đóng form bây giờ, bài thi sẽ bị thu tự động với số câu hiện tại.\n\nBạn có muốn thoát?",
                    "Cảnh báo thoát bài thi",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (result == DialogResult.Yes)
                {
                    _timer?.Stop();
                    NopBai();
                }
                else
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
```

---

## 5. CODE HOÀN CHỈNH FORM GIÁO VIÊN (`Forms/frm_GiaoVien.cs`)

File này bao quát toàn bộ 7 tabs nghiệp vụ của Giáo viên:

- **Tab Lớp học:** Quản lý lớp, tạo lớp, sửa lớp, đóng đăng ký.
- **Tab Duyệt học viên:** Xem học viên chờ duyệt, bấm Duyệt / Từ chối.
- **Tab Ngân hàng câu hỏi:** Thêm / Sửa / Xóa câu hỏi trắc nghiệm của đề.
- **Tab Bài thi:** Tạo đề thi mới, phân công lớp thi và đặt hạn nộp.
- **Tab Kết quả:** Xem bảng điểm học viên của bài thi, Xuất Excel/CSV.
- **Tab Thông tin:** Đổi thông tin cá nhân và Đổi mật khẩu.

```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class frm_GiaoVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KhoaHocDAL _khDAL = new KhoaHocDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();

        private int _selectedLopHocID = 0;
        private int _selectedCauHoiID = 0;

        public frm_GiaoVien()
        {
            this.DoubleBuffered = true;
            InitializeComponent();
            this.DrawerTabControl = materialTabControl1;
        }

        private void GiaoVien_Load(object sender, EventArgs e)
        {
            try
            {
                lblGV_LoiChao.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";

                InitDropdowns();
                SetupEventHandlers();

                // Load dữ liệu các tab
                LoadLopHocCuaToi();
                LoadDanhSachLopQuanLy();
                LoadDeThiCombobox();
                LoadThongTinCaNhan();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải trang giáo viên: " + ex.Message, "Thông báo");
            }
        }

        #region 1. Khởi tạo & Đăng ký sự kiện
        private void InitDropdowns()
        {
            // Dropdown đáp án đúng của câu hỏi
            cboCH_DapAnDung.Items.Clear();
            cboCH_DapAnDung.Items.AddRange(new object[] { "A", "B", "C", "D" });
            cboCH_DapAnDung.SelectedIndex = 0;

            // Giới tính profile
            cboHV_Profile_GioiTinh.Items.Clear();
            cboHV_Profile_GioiTinh.Items.AddRange(new object[] { "Nam", "Nữ", "Khác" });
            cboHV_Profile_GioiTinh.SelectedIndex = 0;

            // Khóa học cho form Lớp
            DataTable dtKH = _khDAL.LayTatCa();
            cmbGV_KhoaHoc.DataSource = dtKH;
            cmbGV_KhoaHoc.DisplayMember = "TenKhoaHoc";
            cmbGV_KhoaHoc.ValueMember = "KhoaHocID";
        }

        private void SetupEventHandlers()
        {
            // Tab Lớp học
            btnGV_TaoLop.Click += btnGV_TaoLop_Click;
            btnGV_SuaLop.Click += btnGV_SuaLop_Click;
            btnGV_DongDangKi.Click += btnGV_DongDangKi_Click;
            dtGV_LopHoc.CellClick += dtGV_LopHoc_CellClick;

            // Tab Duyệt đăng ký
            cboDuyet_ChonLop.SelectedIndexChanged += cboDuyet_ChonLop_SelectedIndexChanged;
            btnDuyet_ChapNhan.Click += btnDuyet_ChapNhan_Click;
            btnDuyet_TuChoi.Click += btnDuyet_TuChoi_Click;

            // Tab Câu hỏi
            cmbGV_ChonDeThi.SelectedIndexChanged += cmbGV_ChonDeThi_SelectedIndexChanged;
            btnCH_Them.Click += btnCH_Them_Click;
            btnCH_Sua.Click += btnCH_Sua_Click;
            btnCH_Xoa.Click += btnCH_Xoa_Click;
            dgvDanhSachCauHoi.CellClick += dgvDanhSachCauHoi_CellClick;

            // Tab Đề thi
            btnDe_TaoDeThi.Click += btnDe_TaoDeThi_Click;

            // Tab Kết quả thi
            cboKQ_ChonDeThi.SelectedIndexChanged += cboKQ_ChonDeThi_SelectedIndexChanged;
            btnKQ_XuatExcel.Click += btnKQ_XuatExcel_Click;

            // Tab Thông tin cá nhân & Mật khẩu
            btnGV_CapNhatThongTin.Click += btnGV_CapNhatThongTin_Click;
            btnGV_DoiMatKhau.Click += btnGV_DoiMatKhau_Click;
        }
        #endregion

        #region 2. Tab Trang Chủ & Quản Lý Lớp Học
        private void LoadLopHocCuaToi()
        {
            try
            {
                DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                if (dgvGV_LopCuaToi != null) dgvGV_LopCuaToi.DataSource = dt;
            }
            catch { }
        }

        private void LoadDanhSachLopQuanLy()
        {
            try
            {
                DataTable dt = _lopDAL.LayTheoGiaoVien(Session.NguoiDungID);
                dtGV_LopHoc.AutoGenerateColumns = false;
                Column1.DataPropertyName = "TenLop";
                Column2.DataPropertyName = "TenKhoaHoc";
                Column3.DataPropertyName = "LichHoc";
                Column4.DataPropertyName = "SiSo";
                dtGV_LopHoc.DataSource = dt;

                // Load danh sách lớp vào combobox duyệt & giao bài thi
                cboDuyet_ChonLop.DataSource = dt.Copy();
                cboDuyet_ChonLop.DisplayMember = "TenLop";
                cboDuyet_ChonLop.ValueMember = "LopHocID";

                cboDe_ChonLopHoc.DataSource = dt.Copy();
                cboDe_ChonLopHoc.DisplayMember = "TenLop";
                cboDe_ChonLopHoc.ValueMember = "LopHocID";
            }
            catch { }
        }

        private void dtGV_LopHoc_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dtGV_LopHoc.Rows.Count)
            {
                var row = dtGV_LopHoc.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    _selectedLopHocID = Convert.ToInt32(drv["LopHocID"]);
                    txtGV_TenLop.Text = drv["TenLop"]?.ToString();
                    txtGV_LichHoc.Text = drv["LichHoc"]?.ToString();
                    txtGV_SiSo.Text = "40";
                }
            }
        }

        private void btnGV_TaoLop_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtGV_TenLop.Text))
            {
                MessageBox.Show("Vui lòng nhập tên lớp học!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int siSo = 40;
            int.TryParse(txtGV_SiSo.Text, out siSo);
            int khID = cmbGV_KhoaHoc.SelectedValue != null ? Convert.ToInt32(cmbGV_KhoaHoc.SelectedValue) : 1;

            LopHoc lop = new LopHoc
            {
                TenLop = txtGV_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                LichHoc = txtGV_LichHoc.Text.Trim(),
                SiSoToiDa = siSo > 0 ? siSo : 40,
                TrangThai = "Đang mở"
            };

            if (_lopDAL.ThemLop(lop))
            {
                MessageBox.Show("Tạo lớp học mới thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopQuanLy();
                LoadLopHocCuaToi();
            }
        }

        private void btnGV_SuaLop_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần sửa từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int siSo = 40;
            int.TryParse(txtGV_SiSo.Text, out siSo);
            int khID = cmbGV_KhoaHoc.SelectedValue != null ? Convert.ToInt32(cmbGV_KhoaHoc.SelectedValue) : 1;

            LopHoc lop = new LopHoc
            {
                LopHocID = _selectedLopHocID,
                TenLop = txtGV_TenLop.Text.Trim(),
                KhoaHocID = khID,
                GiaoVienID = Session.NguoiDungID,
                SiSoToiDa = siSo > 0 ? siSo : 40
            };

            if (_lopDAL.SuaLop(lop))
            {
                MessageBox.Show("Cập nhật thông tin lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDanhSachLopQuanLy();
            }
        }

        private void btnGV_DongDangKi_Click(object sender, EventArgs e)
        {
            if (_selectedLopHocID <= 0)
            {
                MessageBox.Show("Vui lòng chọn lớp học cần đóng đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (MessageBox.Show("Đóng đăng ký cho lớp này? Học viên mới sẽ không thể đăng ký vào nữa.", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_lopDAL.DongDangKyLop(_selectedLopHocID))
                {
                    MessageBox.Show("Đã đóng đăng ký lớp học thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadDanhSachLopQuanLy();
                }
            }
        }
        #endregion

        #region 3. Tab Duyệt Học Viên Đăng Ký
        private void cboDuyet_ChonLop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDuyet_ChonLop.SelectedValue != null && int.TryParse(cboDuyet_ChonLop.SelectedValue.ToString(), out int lopID))
            {
                DataTable dt = _lopDAL.LayDangKyTheoLop(lopID);
                dgvDangKyChoDuyet.DataSource = dt;
            }
        }

        private void btnDuyet_ChapNhan_Click(object sender, EventArgs e)
        {
            if (dgvDangKyChoDuyet.CurrentRow != null && dgvDangKyChoDuyet.CurrentRow.DataBoundItem is DataRowView drv)
            {
                int dkID = Convert.ToInt32(drv["DangKyID"]);
                int lopID = Convert.ToInt32(drv["LopHocID"]);
                int hsID = Convert.ToInt32(drv["HocSinhID"]);

                if (_lopDAL.DuyetDangKy(dkID, lopID, hsID))
                {
                    MessageBox.Show("Đã duyệt học viên vào lớp thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cboDuyet_ChonLop_SelectedIndexChanged(null, null);
                    LoadDanhSachLopQuanLy();
                }
            }
            else
            {
                MessageBox.Show("Vui lòng chọn học viên cần duyệt từ danh sách!", "Thông báo");
            }
        }

        private void btnDuyet_TuChoi_Click(object sender, EventArgs e)
        {
            if (dgvDangKyChoDuyet.CurrentRow != null && dgvDangKyChoDuyet.CurrentRow.DataBoundItem is DataRowView drv)
            {
                int dkID = Convert.ToInt32(drv["DangKyID"]);
                if (MessageBox.Show("Từ chối đơn đăng ký này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    if (_lopDAL.TuChoiDangKy(dkID))
                    {
                        MessageBox.Show("Đã từ chối đơn đăng ký!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        cboDuyet_ChonLop_SelectedIndexChanged(null, null);
                    }
                }
            }
        }
        #endregion

        #region 4. Tab Ngân Hàng Câu Hỏi
        private void LoadDeThiCombobox()
        {
            try
            {
                DataTable dt = _deThiDAL.LayDeThiTheoGiaoVien(Session.NguoiDungID);

                // Gán vào Tab Câu hỏi
                cmbGV_ChonDeThi.DataSource = dt.Copy();
                cmbGV_ChonDeThi.DisplayMember = "TenDeThi";
                cmbGV_ChonDeThi.ValueMember = "DeThiID";

                // Gán vào Tab Kết quả
                cboKQ_ChonDeThi.DataSource = dt.Copy();
                cboKQ_ChonDeThi.DisplayMember = "TenDeThi";
                cboKQ_ChonDeThi.ValueMember = "DeThiID";

                // Bảng quản lý đề thi
                dgvDanhSachDeThi.DataSource = dt;
            }
            catch { }
        }

        private void cmbGV_ChonDeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbGV_ChonDeThi.SelectedValue != null && int.TryParse(cmbGV_ChonDeThi.SelectedValue.ToString(), out int deID))
            {
                var listCH = _deThiDAL.LayDanhSachCauHoi(deID);
                dgvDanhSachCauHoi.DataSource = listCH;
            }
        }

        private void dgvDanhSachCauHoi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvDanhSachCauHoi.Rows.Count)
            {
                CauHoi q = dgvDanhSachCauHoi.Rows[e.RowIndex].DataBoundItem as CauHoi;
                if (q != null)
                {
                    _selectedCauHoiID = q.CauHoiID;
                    txtCH_NoiDung.Text = q.NoiDung;
                    txtCH_DapAnA.Text = q.DapAnA;
                    txtCH_DapAnB.Text = q.DapAnB;
                    txtCH_DapAnC.Text = q.DapAnC;
                    txtCH_DapAnD.Text = q.DapAnD;
                    cboCH_DapAnDung.SelectedItem = q.DapAnDung;
                }
            }
        }

        private void btnCH_Them_Click(object sender, EventArgs e)
        {
            if (cmbGV_ChonDeThi.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn đề thi cần thêm câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int deID = Convert.ToInt32(cmbGV_ChonDeThi.SelectedValue);
            CauHoi ch = new CauHoi
            {
                DeThiID = deID,
                NoiDung = txtCH_NoiDung.Text.Trim(),
                DapAnA = txtCH_DapAnA.Text.Trim(),
                DapAnB = txtCH_DapAnB.Text.Trim(),
                DapAnC = txtCH_DapAnC.Text.Trim(),
                DapAnD = txtCH_DapAnD.Text.Trim(),
                DapAnDung = cboCH_DapAnDung.SelectedItem?.ToString() ?? "A",
                ThuTu = 1
            };

            if (_deThiDAL.ThemCauHoi(ch))
            {
                MessageBox.Show("Thêm câu hỏi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                LamMoiFormCauHoi();
            }
        }

        private void btnCH_Sua_Click(object sender, EventArgs e)
        {
            if (_selectedCauHoiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần sửa!", "Thông báo");
                return;
            }

            CauHoi ch = new CauHoi
            {
                CauHoiID = _selectedCauHoiID,
                NoiDung = txtCH_NoiDung.Text.Trim(),
                DapAnA = txtCH_DapAnA.Text.Trim(),
                DapAnB = txtCH_DapAnB.Text.Trim(),
                DapAnC = txtCH_DapAnC.Text.Trim(),
                DapAnD = txtCH_DapAnD.Text.Trim(),
                DapAnDung = cboCH_DapAnDung.SelectedItem?.ToString() ?? "A"
            };

            if (_deThiDAL.SuaCauHoi(ch))
            {
                MessageBox.Show("Cập nhật câu hỏi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                LamMoiFormCauHoi();
            }
        }

        private void btnCH_Xoa_Click(object sender, EventArgs e)
        {
            if (_selectedCauHoiID <= 0)
            {
                MessageBox.Show("Vui lòng chọn câu hỏi cần xóa!", "Thông báo");
                return;
            }

            if (MessageBox.Show("Xóa câu hỏi này?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                if (_deThiDAL.XoaCauHoi(_selectedCauHoiID))
                {
                    MessageBox.Show("Đã xóa câu hỏi!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    cmbGV_ChonDeThi_SelectedIndexChanged(null, null);
                    LamMoiFormCauHoi();
                }
            }
        }

        private void LamMoiFormCauHoi()
        {
            _selectedCauHoiID = 0;
            txtCH_NoiDung.Clear();
            txtCH_DapAnA.Clear();
            txtCH_DapAnB.Clear();
            txtCH_DapAnC.Clear();
            txtCH_DapAnD.Clear();
            cboCH_DapAnDung.SelectedIndex = 0;
        }
        #endregion

        #region 5. Tab Quản Lý & Giao Đề Thi
        private void btnDe_TaoDeThi_Click(object sender, EventArgs e)
        {
            if (cboDe_ChonLopHoc.SelectedValue == null)
            {
                MessageBox.Show("Vui lòng chọn lớp học để giao bài!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int thoiGian = 15;
            int.TryParse(txtDe_ThoiGian.Text, out thoiGian);
            int soCau = 20;
            int.TryParse(txtDe_SoCau.Text, out soCau);

            DeThi de = new DeThi
            {
                TenDeThi = txtGV_DeThi != null && !string.IsNullOrWhiteSpace(txtGV_DeThi.Text) ? txtGV_DeThi.Text.Trim() : "Bài kiểm tra trắc nghiệm",
                MonHoc = cboDe_MonHoc.SelectedItem?.ToString() ?? "Tin học đại cương",
                LopHocID = Convert.ToInt32(cboDe_ChonLopHoc.SelectedValue),
                GiaoVienID = Session.NguoiDungID,
                ThoiGian = thoiGian > 0 ? thoiGian : 15,
                SoCau = soCau > 0 ? soCau : 20,
                HanNop = DateTime.Now.AddDays(7),
                TrangThai = "Đang mở"
            };

            if (_deThiDAL.ThemDeThi(de))
            {
                MessageBox.Show("Tạo và giao đề thi thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadDeThiCombobox();
            }
        }
        #endregion

        #region 6. Tab Kết Quả Thi & Xuất File Excel / CSV
        private void cboKQ_ChonDeThi_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboKQ_ChonDeThi.SelectedValue != null && int.TryParse(cboKQ_ChonDeThi.SelectedValue.ToString(), out int deID))
            {
                DataTable dt = _deThiDAL.LayKetQuaTheoDeThi(deID);
                dgvKetQuaHocSinh.DataSource = dt;
            }
        }

        private void btnKQ_XuatExcel_Click(object sender, EventArgs e)
        {
            if (dgvKetQuaHocSinh.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "CSV File (*.csv)|*.csv|Excel File (*.xls)|*.xls",
                FileName = $"KetQuaThi_{DateTime.Now:yyyyMMdd_HHmm}.csv"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    StringBuilder sb = new StringBuilder();
                    // Tiêu đề cột
                    for (int i = 0; i < dgvKetQuaHocSinh.Columns.Count; i++)
                    {
                        sb.Append($"\"{dgvKetQuaHocSinh.Columns[i].HeaderText}\"");
                        if (i < dgvKetQuaHocSinh.Columns.Count - 1) sb.Append(",");
                    }
                    sb.AppendLine();

                    // Dữ liệu từng dòng
                    foreach (DataGridViewRow row in dgvKetQuaHocSinh.Rows)
                    {
                        if (!row.IsNewRow)
                        {
                            for (int i = 0; i < dgvKetQuaHocSinh.Columns.Count; i++)
                            {
                                sb.Append($"\"{row.Cells[i].Value?.ToString()}\"");
                                if (i < dgvKetQuaHocSinh.Columns.Count - 1) sb.Append(",");
                            }
                            sb.AppendLine();
                        }
                    }

                    // Ghi ra file với chuẩn UTF-8 BOM để Excel không lỗi font tiếng Việt
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Xuất file kết quả thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        #endregion

        #region 7. Tab Thông Tin Cá Nhân & Đổi Mật Khẩu
        private void LoadThongTinCaNhan()
        {
            try
            {
                GiaoVien gv = new GiaoVienDAL().LayTheoID(Session.NguoiDungID);
                if (gv != null)
                {
                    txtGV_Profile_HoTen.Text = gv.HoTen;
                    txtGV_Profile_Email.Text = gv.Email;
                    txtGV_Profile_SDT.Text = gv.SoDienThoai;
                    txtGV_Profile_DiaChi.Text = gv.DiaChi;
                    cboHV_Profile_GioiTinh.SelectedItem = gv.GioiTinh;
                }
            }
            catch { }
        }

        private void btnGV_CapNhatThongTin_Click(object sender, EventArgs e)
        {
            try
            {
                GiaoVien gv = new GiaoVien
                {
                    GiaoVienID = Session.NguoiDungID,
                    HoTen = txtGV_Profile_HoTen.Text.Trim(),
                    Email = txtGV_Profile_Email.Text.Trim(),
                    SoDienThoai = txtGV_Profile_SDT.Text.Trim(),
                    DiaChi = txtGV_Profile_DiaChi.Text.Trim(),
                    GioiTinh = cboHV_Profile_GioiTinh.SelectedItem?.ToString()
                };

                if (new GiaoVienDAL().Sua(gv))
                {
                    MessageBox.Show("Cập nhật thông tin cá nhân thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblGV_LoiChao.Text = $"Xin Chào, {gv.HoTen}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnGV_DoiMatKhau_Click(object sender, EventArgs e)
        {
            string cu = txtGV_MatKhauCu.Text.Trim();
            string moi = txtGV_MatKhauMoi.Text.Trim();
            string xacNhan = txtGV_XacNhanMatKhau.Text.Trim();

            if (string.IsNullOrEmpty(cu) || string.IsNullOrEmpty(moi))
            {
                MessageBox.Show("Vui lòng điền đầy đủ mật khẩu!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (moi != xacNhan)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Kiểm tra mật khẩu cũ
            var tk = _tkDAL.DangNhap(Session.TenDangNhap, cu);
            if (tk == null)
            {
                MessageBox.Show("Mật khẩu hiện tại không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_tkDAL.DoiMatKhau(Session.TaiKhoanID, moi))
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtGV_MatKhauCu.Clear();
                txtGV_MatKhauMoi.Clear();
                txtGV_XacNhanMatKhau.Clear();
            }
        }
        #endregion
    }
}
```

---

## 6. CODE HOÀN CHỈNH FORM HỌC VIÊN (`Forms/Frm_HocVien.cs`)

Form học viên quản lý:

1. Danh sách lớp học đang tham gia.
2. Danh sách bài thi, trạng thái (Đã thi / Chưa thi), nút **Bắt Đầu Làm Bài** mở `Frm_KiemTra`.
3. Lịch sử làm bài, nút **Xem Lại Bài Thi** hiển thị chi tiết câu đúng/sai.
4. Thông tin cá nhân & Đổi mật khẩu.

```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Manager;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public partial class HocVien : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly LopHocDAL _lopDAL = new LopHocDAL();
        private readonly DeThiDAL _deThiDAL = new DeThiDAL();
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private readonly HocSinhDAL _hsDAL = new HocSinhDAL();
        private readonly TaiKhoanDAL _tkDAL = new TaiKhoanDAL();

        public HocVien()
        {
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            this.DrawerTabControl = tabHocVien;
        }

        private void HocVien_Load(object sender, EventArgs e)
        {
            try
            {
                lblHV_LoiChao.Text = $"Xin Chào, {Session.HoTen ?? Session.TenDangNhap}";
                tabHocVien.SelectedIndexChanged += TabHocVien_SelectedIndexChanged;

                SetupEventHandlers();
                LoadThongTinHocVien();
                LoadLopHoc();
                LoadDanhSachBaiThi();
                LoadLichSuThi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải thông tin học viên: " + ex.Message, "Thông báo");
            }
        }

        private void SetupEventHandlers()
        {
            // Nút làm bài thi
            btnHV_BatDauLamBai.Click += btnHV_BatDauLamBai_Click;
            btnHV_LamMoiDeThi.Click += (s, e) => LoadDanhSachBaiThi();

            // Nút xem lại bài thi
            btnHV_XemLaiBaiThi.Click += btnHV_XemLaiBaiThi_Click;

            // Profile & Đổi mật khẩu
            btnHV_LuuProfile.Click += btnHV_LuuProfile_Click;
            btnHV_DoiMatKhau.Click += btnHV_DoiMatKhau_Click;
        }

        #region 1. Quản Lý Lớp Học
        private void LoadLopHoc()
        {
            try
            {
                DataTable dtLop = _lopDAL.LayTheoHocSinh(Session.NguoiDungID);
                dgvHV_LopDangHoc.DataSource = dtLop;
                dtHV_LopHoc.DataSource = dtLop.Copy();
            }
            catch { }
        }
        #endregion

        #region 2. Quản Lý Bài Thi & Làm Bài
        private void LoadDanhSachBaiThi()
        {
            try
            {
                // Lấy tất cả bài thi thuộc lớp mà học sinh đang học kèm trạng thái Đã thi / Chưa thi
                string sql = @"
                    SELECT
                        d.DeThiID,
                        d.TenDeThi,
                        d.MonHoc,
                        d.SoCau,
                        d.ThoiGian,
                        d.HanNop,
                        CASE
                            WHEN kq.KetQuaID IS NOT NULL THEN N'Đã hoàn thành'
                            ELSE N'Chưa làm'
                        END AS TrangThaiLamBai,
                        ISNULL(kq.Diem, 0) AS DiemSo
                    FROM DeThi d
                    JOIN LopHoc l ON d.LopHocID = l.LopHocID
                    LEFT JOIN KetQuaThi kq ON d.DeThiID = kq.DeThiID AND kq.HocSinhID = @hsID
                    WHERE l.LopHocID IN (
                        SELECT LopHocID FROM HocSinh WHERE HocSinhID = @hsID
                        UNION
                        SELECT LopHocID FROM DangKyLop WHERE HocSinhID = @hsID
                    )";

                DataTable dt = DatabaseHelper.ExecuteQuery(sql, new SqlParameter[] { new SqlParameter("@hsID", Session.NguoiDungID) });
                dgvHV_DanhSachDeThi.DataSource = dt;
                dgvHV_DeThiSapToi.DataSource = dt.Copy();
            }
            catch { }
        }

        private void btnHV_BatDauLamBai_Click(object sender, EventArgs e)
        {
            if (dgvHV_DanhSachDeThi.CurrentRow != null && dgvHV_DanhSachDeThi.CurrentRow.DataBoundItem is DataRowView drv)
            {
                int deThiID = Convert.ToInt32(drv["DeThiID"]);
                string trangThai = drv["TrangThaiLamBai"]?.ToString();

                if (trangThai == "Đã hoàn thành" || _ketQuaDAL.KiemTraDaThi(Session.NguoiDungID, deThiID))
                {
                    MessageBox.Show("Bạn đã hoàn thành bài thi này rồi, không thể làm lại!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                Frm_KiemTra frm = new Frm_KiemTra(deThiID);
                frm.ShowDialog();

                // Reload lại sau khi nộp bài
                LoadDanhSachBaiThi();
                LoadLichSuThi();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn bài thi từ danh sách!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        #endregion

        #region 3. Tab Lịch Sử Thi & Xem Lại
        private void LoadLichSuThi()
        {
            try
            {
                DataTable dtKQ = _ketQuaDAL.LayLichSuTheoHocSinh(Session.NguoiDungID);
                dgvHV_LichSuThi.DataSource = dtKQ;

                // Load vào combobox bài thi đã làm
                cmbBaiThi_DaLam.DataSource = dtKQ.Copy();
                cmbBaiThi_DaLam.DisplayMember = "TenDeThi";
                cmbBaiThi_DaLam.ValueMember = "KetQuaID";
            }
            catch { }
        }

        private void btnHV_XemLaiBaiThi_Click(object sender, EventArgs e)
        {
            int kqID = 0;
            if (cmbBaiThi_DaLam.SelectedValue != null && int.TryParse(cmbBaiThi_DaLam.SelectedValue.ToString(), out kqID) && kqID > 0)
            {
                // Mở Popup Frm_XemLaiBaiThi
                Frm_XemLaiBaiThi frm = new Frm_XemLaiBaiThi(kqID);
                frm.ShowDialog();
            }
            else if (dgvHV_LichSuThi.CurrentRow != null && dgvHV_LichSuThi.CurrentRow.DataBoundItem is DataRowView drv)
            {
                kqID = Convert.ToInt32(drv["KetQuaID"]);
                Frm_XemLaiBaiThi frm = new Frm_XemLaiBaiThi(kqID);
                frm.ShowDialog();
            }
            else
            {
                MessageBox.Show("Vui lòng chọn bài thi đã làm để xem lại kết quả chi tiết!", "Thông báo");
            }
        }
        #endregion

        #region 4. Tab Thông Tin & Đổi Mật Khẩu
        private void LoadThongTinHocVien()
        {
            try
            {
                if (Session.NguoiDungID > 0)
                {
                    HocSinh hs = _hsDAL.LayTheoID(Session.NguoiDungID);
                    if (hs != null)
                    {
                        txtHV_Profile_HoTen.Text = hs.HoTen;
                        txtHV_Profile_Email.Text = hs.Email;
                        txtHV_Profile_SDT.Text = hs.SoDienThoai;
                        txtHV_Profile_DiaChi.Text = hs.DiaChi;
                        if (hs.NgaySinh.HasValue)
                            txtHV_Profile_NgaySinh.Text = hs.NgaySinh.Value.ToString("dd/MM/yyyy");
                        cboHV_Profile_GioiTinh.SelectedItem = hs.GioiTinh;
                    }
                }
            }
            catch { }
        }

        private void btnHV_LuuProfile_Click(object sender, EventArgs e)
        {
            try
            {
                HocSinh hs = new HocSinh
                {
                    HocSinhID = Session.NguoiDungID,
                    HoTen = txtHV_Profile_HoTen.Text.Trim(),
                    Email = txtHV_Profile_Email.Text.Trim(),
                    SoDienThoai = txtHV_Profile_SDT.Text.Trim(),
                    DiaChi = txtHV_Profile_DiaChi.Text.Trim(),
                    GioiTinh = cboHV_Profile_GioiTinh.SelectedItem?.ToString()
                };

                if (_hsDAL.Sua(hs))
                {
                    MessageBox.Show("Cập nhật thông tin học viên thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    lblHV_LoiChao.Text = $"Xin Chào, {hs.HoTen}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }

        private void btnHV_DoiMatKhau_Click(object sender, EventArgs e)
        {
            string cu = txtHV_MatKhauCu.Text.Trim();
            string moi = txtHV_MatKhauMoi.Text.Trim();
            string xacNhan = txtHV_XacNhanMatKhau.Text.Trim();

            if (string.IsNullOrEmpty(cu) || string.IsNullOrEmpty(moi))
            {
                MessageBox.Show("Vui lòng nhập mật khẩu hiện tại và mật khẩu mới!", "Cảnh báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (moi != xacNhan)
            {
                MessageBox.Show("Mật khẩu mới và xác nhận mật khẩu không khớp!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var tk = _tkDAL.DangNhap(Session.TenDangNhap, cu);
            if (tk == null)
            {
                MessageBox.Show("Mật khẩu hiện tại không chính xác!", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (_tkDAL.DoiMatKhau(Session.TaiKhoanID, moi))
            {
                MessageBox.Show("Đổi mật khẩu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                txtHV_MatKhauCu.Clear();
                txtHV_MatKhauMoi.Clear();
                txtHV_XacNhanMatKhau.Clear();
            }
        }
        #endregion

        #region 5. Đăng Xuất
        private void TabHocVien_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabHocVien.SelectedTab != null && tabHocVien.SelectedTab.Text.Trim().Contains("Đăng Xuất"))
            {
                if (MessageBox.Show("Bạn có chắc chắn muốn đăng xuất?", "Xác nhận", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    Session.Clear();
                    this.Hide();
                    Form loginForm = Application.OpenForms["frm_DangNhap"];
                    if (loginForm != null) loginForm.Show();
                    else new frm_DangNhap().Show();
                    this.Close();
                }
                else
                {
                    tabHocVien.SelectedIndex = 0;
                }
            }
        }
        #endregion
    }
}
```

---

## 7. FORM PHỤ: XEM LẠI CHI TIẾT BÀI THI (`Forms/Frm_XemLaiBaiThi.cs`)

Form popup hiển thị danh sách các câu hỏi của bài thi đã làm, tô màu xanh cho câu làm đúng, màu đỏ cho câu làm sai và hiển thị đáp án đúng để học viên ôn tập:

```csharp
using BLT_Quan_Ly_Hoc_Tap.DAL;
using ReaLTaiizor.Controls;
using ReaLTaiizor.Manager;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    public class Frm_XemLaiBaiThi : ReaLTaiizor.Forms.MaterialForm
    {
        private readonly int _ketQuaID;
        private readonly KetQuaDAL _ketQuaDAL = new KetQuaDAL();
        private DataGridView dgvChiTiet;

        public Frm_XemLaiBaiThi(int ketQuaID)
        {
            _ketQuaID = ketQuaID;
            InitializeComponent();
            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            LoadChiTiet();
        }

        private void InitializeComponent()
        {
            this.dgvChiTiet = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).BeginInit();
            this.SuspendLayout();

            this.dgvChiTiet.Dock = DockStyle.Fill;
            this.dgvChiTiet.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            this.dgvChiTiet.ReadOnly = true;
            this.dgvChiTiet.RowHeadersVisible = false;
            this.dgvChiTiet.AllowUserToAddRows = false;
            this.dgvChiTiet.RowPrePaint += DgvChiTiet_RowPrePaint;

            this.ClientSize = new System.Drawing.Size(950, 600);
            this.Controls.Add(this.dgvChiTiet);
            this.StartPosition = FormStartPosition.CenterParent;
            this.Text = "CHI TIẾT BÀI LÀM - KẾT QUẢ THI";

            ((System.ComponentModel.ISupportInitialize)(this.dgvChiTiet)).EndInit();
            this.ResumeLayout(false);
        }

        private void LoadChiTiet()
        {
            DataTable dt = _ketQuaDAL.LayChiTietBaiLamTheoKetQua(_ketQuaID);
            dgvChiTiet.DataSource = dt;
        }

        private void DgvChiTiet_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dgvChiTiet.Rows.Count)
            {
                var row = dgvChiTiet.Rows[e.RowIndex];
                if (row.DataBoundItem is DataRowView drv)
                {
                    bool laDung = drv["LaDung"] != DBNull.Value && Convert.ToBoolean(drv["LaDung"]);
                    if (laDung)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(230, 255, 230); // Xanh nhạt: Đúng
                        row.DefaultCellStyle.ForeColor = Color.DarkGreen;
                    }
                    else
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235); // Đỏ nhạt: Sai
                        row.DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                }
            }
        }
    }
}
```

---

## 8. THỨ TỰ TRIỂN KHAI VÀ CHECKLIST KIỂM THỬ

|  STT  | Bước thực hiện                             | Vị trí file                      | Kết quả kỳ vọng                                                 |
| :---: | :----------------------------------------- | :------------------------------- | :-------------------------------------------------------------- |
| **1** | **Chạy script DB Migration**               | SQL Server / `CreateDataBase.cs` | Thêm cột `TaiKhoanID`, `NgayBatDau`, `NgayKetThuc`              |
| **2** | **Thêm 2 hàm còn thiếu vào `TaiKhoanDAL`** | `DAL/TaiKhoanDAL.cs`             | Dự án **Build thành công 0 Error**                              |
| **3** | **Sửa lỗi DGV không reload sau khi Sửa**   | `Forms/Frm_Admin.cs`             | Bấm Sửa GV / HV -> DGV cập nhật ngay và form reset              |
| **4** | **Cập nhật Timer & ProgressBar**           | `Forms/Frm_KiemTra.cs`           | Timer đỏ khi còn 5 phút, Progress chỉ tăng khi chọn đáp án      |
| **5** | **Hoàn thiện code Form Giáo Viên**         | `Forms/frm_GiaoVien.cs`          | Tạo lớp, duyệt học viên, tạo câu hỏi, giao bài, xuất file Excel |
| **6** | **Hoàn thiện code Form Học Viên**          | `Forms/Frm_HocVien.cs`           | Xem lớp, làm bài thi, xem kết quả, đổi mật khẩu                 |
| **7** | **Thêm Form Xem Lại Chi Tiết**             | `Forms/Frm_XemLaiBaiThi.cs`      | Xem lại câu hỏi đúng (xanh) / sai (đỏ) trực quan                |

---

_Tài liệu này được tạo tự động bởi Antigravity AI để hỗ trợ triển khai code nhanh, chính xác và đồng bộ hoàn toàn với kiến trúc hiện có của dự án._
