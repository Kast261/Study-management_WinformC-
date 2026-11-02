# Các bước code Dashboard trong form Admin

Phần này nối dữ liệu vào bố cục đã push ở commit `9d9ecc3`. Không cần chạy lại script tạo database.
Bản commit này còn thiếu tiêu đề và nút làm mới so với ảnh chụp; chúng được bổ sung
vào `panel2` và `panel3`, giữ nguyên các ô trong `tableLayoutPanel13`.

## 1. Xác định các control trong Designer

Mở `Forms/Admin/Frm_Admin.cs` → **View Designer**. Chọn control và xem thuộc tính `(Name)`:

| Chức năng | Tên control hiện tại | Tên trong commit thiết kế |
| --- | --- | --- |
| Tổng học viên | `lblTQ_HocVien` | `materialLabel1` |
| Tổng giáo viên | `lblTQ_GiaoVien` | `materialLabel2` |
| Tổng khóa học | `lblTQ_KhoaHoc` | `materialLabel3` |
| Tổng lớp học | `lblTQ_LopHoc` | `materialLabel4` |
| Biểu đồ | `chart1` | Giữ nguyên |
| Danh sách lớp mới | `materialListView2` | Giữ nguyên |
| Làm mới dữ liệu | `btnTQ_LamMoi` | Bổ sung trong `panel3` |
| Xem tất cả lớp | `btnTQ_XemTatCaLopHoc` | `button1` |

Các control vẫn khai báo trong `.Designer.cs`, có thể chỉnh bằng kéo thả trong Visual Studio.
Không đưa câu SQL hoặc truy cập database vào `InitializeComponent()`.

## 2. Tách truy vấn vào DAL

File `DAL/Repositories/DashboardDAL.cs` có ba hàm:

- `LayTongQuan()`: một dòng chứa bốn số đếm từ `HocSinh`, `GiaoVien`, `KhoaHoc`, `LopHoc`.
- `LayHocVienTheoKhoaHoc()`: tối đa sáu khóa học, sắp theo số học viên giảm dần.
  Dùng `LEFT JOIN` để vẫn có khóa học chưa có học viên; đếm `HocSinhID`, không dùng `COUNT(*)`.
- `LayLopMoiNhat()`: năm lớp có `LopHocID` lớn nhất, kèm tên giáo viên và sĩ số thực tế.
  Giáo viên trống hiển thị “Chưa phân công”; sức chứa trống dùng 40 theo quy ước DAL hiện có.

Học viên được tính theo lớp hiện tại `HocSinh.LopHocID`; đăng ký chờ duyệt chưa được tính.
Học viên chưa xếp lớp vẫn có trong tổng học viên nhưng không thuộc cột nào của biểu đồ.
Database hiện chưa có `NgayTao`, nên “mới nhất” được xác định bằng mã lớp giảm dần,
không phải ngày bắt đầu lớp. Nếu cần thời điểm tạo chính xác, phải bổ sung trường đó trong tác vụ khác.

Project .NET Framework dùng danh sách file tường minh nên `DashboardDAL.cs` cũng được
thêm vào `BLT_Quan_Ly_Hoc_Tap.csproj`.

## 3. Khởi tạo hành vi của Dashboard

Trong constructor của `Frm_Admin`, sau `InitializeComponent()`, gọi `KhoiTaoDashboard()`.
Hàm này đăng ký hai sự kiện Click, căn ba cột ListView và cấu hình biểu đồ cột.
Series mặc định `Series1` được xóa để tránh giữ các cột ví dụ trong Designer.

Sự kiện đã được đăng ký trong code nên **không gán thêm lần nữa ở cửa sổ Events**.

## 4. Tải dữ liệu vào giao diện

`FrmMain_Load()` đã gọi `LoadDashboard()`. Hàm này đọc đủ ba bảng dữ liệu rồi:

1. Gán bốn số đếm vào Label, định dạng `N0` với văn hóa `vi-VN`.
2. Gọi `HienThiBieuDoDashboard()`: xóa điểm cũ, thêm tên khóa học và số học viên bằng `AddXY()`.
3. Gọi `HienThiLopMoiNhat()`: xóa dòng cũ, tạo `ListViewItem` với ba cột lớp, giáo viên, sĩ số.

Khi chưa có dữ liệu, các số đếm là 0, biểu đồ/danh sách thông báo trạng thái trống.
Khi lỗi, hiển thị thông báo, khôi phục con trỏ và bật lại nút làm mới trong `finally`.
Việc truy vấn hiện chạy đồng bộ giống các màn hình khác trong dự án.

## 5. Xử lý nút bấm và quay về Dashboard

- `btnTQ_LamMoi_Click()` gọi lại `LoadDashboard()`.
- `btnTQ_XemTatCaLopHoc_Click()` chuyển `SelectedTab` sang `pageKhoaHocVaLop`, tải lại danh sách và focus `dgvLopHoc`.
- `materialTabControl1_SelectedIndexChanged()` tải lại Dashboard khi quay về trang này.
  Các lời gọi `LoadDashboard()` sau thao tác thêm/xóa hiện có vẫn được giữ.

## 6. Build và kiểm thử

Trên Windows, mở project trong Visual Studio, restore NuGet và **Build Solution**.
Kiểm tra cấu hình kết nối `ChuoiKetNoi` của bạn trước khi chạy ứng dụng.

Trên môi trường cloud đã thiết lập:

```sh
. /workspace/.cloud-setup/study/env.sh
bash /workspace/.cloud-setup/study/build-app.sh
python3 tests/StudentProfile.Tests/run.py --project tests/Dashboard.Tests/Dashboard.Tests.csproj
```

Runner tạo PostgreSQL riêng và tự xóa sau khi chạy, không dùng database trong `App.config`.
Bộ kiểm thử kiểm tra database trống, học viên chưa xếp lớp, khóa học chưa có học viên,
giới hạn sáu khóa học/năm lớp, thứ tự lớp, giáo viên/sức chứa trống và dữ liệu sau cập nhật.

Kiểm tra giao diện thực tế trên Windows:

1. Đăng nhập Admin: bốn số đếm khớp dữ liệu; biểu đồ không còn số liệu mẫu.
2. Bấm Làm mới nhiều lần: số dòng không tăng lặp, biểu đồ không thêm series trùng.
3. Bấm Xem tất cả lớp học: mở đúng tab Khóa học và lớp.
4. Thêm/sửa/xóa dữ liệu rồi quay về Dashboard: thống kê, biểu đồ và danh sách được tải lại.
5. Thu nhỏ/phóng to form: bảng không che nút Xem tất cả; ba cột vẫn hiển thị.
6. Dùng database thử nghiệm trống hoặc ngắt kết nối: trạng thái trống/thông báo lỗi đúng,
   nút làm mới được bật lại. Không thực hiện thao tác xóa dữ liệu để thử trên database thật.
