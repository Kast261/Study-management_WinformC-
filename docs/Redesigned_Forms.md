# Nối logic với form mới

Bản sửa dựa trên `ed2ee65` (`build form new`). Giữ các tên control và bố cục mới,
thay những tham chiếu tới control đã bị xóa trong Designer bằng control tương ứng.

- Admin: nối lại cột giáo viên/học viên và tài khoản; hai nút tải lại, bộ đếm và
  nhãn đang chọn; đọc ngày sinh từ `DateTimePicker` theo `dd/MM/yyyy`.
- Giáo viên: nối lớp, duyệt yêu cầu, câu hỏi, giao đề, kết quả, hồ sơ và mật khẩu.
  Ô khóa học gợi ý tên trong danh mục hiện có và xác định ID trước khi lưu,
  không tự chọn khóa học ID 1. Sĩ số đọc từ `NumericUpDown.Value`; ngày bắt đầu/
  kết thúc đọc từ `DateTimePicker.Value`. Tìm lớp và lọc tên học viên chạy trên
  dữ liệu đã tải. Các bảng dùng cột của Designer, không xóa rồi tạo lại cột câu hỏi.
- Học viên: nối lại đăng ký/hủy lớp, làm bài, lọc kết quả, lưu hồ sơ và đổi mật khẩu.
  Khôi phục textbox `txtTTHV_NgaySinh` trong hồ sơ; không thêm trường nghiệp vụ mới.
  Nhãn tên/điểm/đề đang chọn lấy dữ liệu thực. Nút Xuất File Excel xuất chi tiết bài
  đang chọn; Báo Cáo xuất bảng điểm theo bộ lọc lớp. Nhấp đúp kết quả để xem bài làm.
- Các textbox mật khẩu mới được che ký tự. Các sự kiện nút được đăng ký một lần.

Tại tab **Đề Thi** của giáo viên, chọn đề trong `cmbDT_DeThi` hoặc bảng; nhấp đúp
để chuyển sang **Câu Hỏi** và sửa câu hỏi của đề đó. Muốn tạo đề mới, nhập tên mới
vào `txtCH_TenDeThi` rồi bấm **Tạo Đề Thi**. `txtLH_KhoaHoc` nhận tên khóa học
đã có trong danh mục của Admin.

Không thay đổi schema database, quy tắc phân quyền, chấm bài, transaction hay dữ
liệu hiện có. Kiểm thử tích hợp dùng PostgreSQL tạm, không đọc kết nối App.config.

## Quản lý control trong Designer

Cấu hình cột (`DataPropertyName`, định dạng ngày/điểm, chế độ chỉ đọc) và các sự
kiện cố định nằm trong `.Designer.cs`. Có thể chỉnh bằng **Properties → Events**
(biểu tượng tia sét) hoặc **Edit Columns** trong Visual Studio. Các hàm
`SetupEventHandlers`, `SetupTaiKhoanEvents`, `ConfigureDataGrids` và cấu hình
lưới lặp trong code xử lý đã được bỏ. Hai nút lưu hồ sơ/đổi mật khẩu học viên
được nối với hàm xử lý sẵn có. Bảng xem lại bài làm cũng nằm trong Designer.

File xử lý giữ phần tải `DataSource`, lọc, phân trang và nghiệp vụ. Timer phòng
thi và thao tác tạm ngắt/nối lại sự kiện radio khi đổi câu hỏi vẫn cần thực hiện
khi chạy. Form kết quả bài thi vẫn có phần giao diện tạo bằng code từ trước.

### Các thiết lập cố định còn lại đã chuyển về Designer

- Admin: cấu hình `cAD_HocVien` (series, tiêu đề, màu, trục, nhãn và tooltip) nằm
  trong `Frm_Admin.Designer.cs`. Không còn xóa rồi tạo lại series/tiêu đề khi mở
  form. Dữ liệu biểu đồ, khoảng chia trục theo số học viên và thông báo khi chưa
  có dữ liệu vẫn cập nhật khi tải database. Giữ tên series `HocVien` và tiêu đề
  trạng thái `TrangThaiDuLieu` khi chỉnh biểu đồ vì phần nạp dữ liệu dùng các tên này.
- Admin: bỏ xử lý Resize tự chia lại độ rộng các cột `LvAD_LopHoc`. Chỉnh độ rộng
  bằng **Columns → Width** trong Designer; giá trị đó không còn bị code ghi đè.
- Admin, Giáo viên và Học viên: liên kết menu với tab (`DrawerTabControl`) nằm
  trong Designer. Các cấu hình `DisplayMember`/`ValueMember` cố định của ComboBox
  cũng nằm trong Designer. Riêng tài khoản gán cho giáo viên/học viên vẫn phải
  thay `ValueMember` khi người dùng chọn vai trò khác nhau.
- Giáo viên: chế độ gợi ý tên khóa học nằm trong Designer. Danh sách gợi ý vẫn
  được lấy từ database lúc chạy.
- Đăng nhập: nút Enter mặc định (`AcceptButton`) nằm trong Designer.
- Các form Học viên, Làm bài và Xem lại bài thi không còn đặt lại theme khi mở.
  Theme Material dùng chung vẫn được khởi tạo một lần trong `Program.Main`.

Đã rà cả 7 form trong thư mục `Forms`. Ngoài form `FrmKetQuaBaiThi` được giữ
nguyên, không còn phần tạo panel/nút/nhãn hoặc dựng lại bố cục trong file xử lý
của các form. Helper phân trang được giữ nguyên. MessageBox, SnackBar, hộp thoại
mở/lưu file, timer và nội dung lấy từ database vẫn tạo/cập nhật khi cần; chúng
không thay thế các control đã kéo thả. Màu đúng/sai trong bảng xem lại bài làm,
trạng thái nút và ô nhập cũng vẫn thay đổi theo dữ liệu/nghiệp vụ.

## Kiểm tra trên Windows

Khôi phục NuGet, sau đó Rebuild project bằng Visual Studio/.NET Framework 4.8.
Nếu dùng NuGet CLI thay vì cài lại package trong Visual Studio, cần chép native
binaries của SQL Server Types theo script cài đặt của package (các thư mục x86/x64
đang được Git bỏ qua):

```powershell
nuget restore packages.config -PackagesDirectory packages
foreach ($arch in @('x86', 'x64')) {
    New-Item -ItemType Directory -Force "SqlServerTypes/$arch" | Out-Null
    Copy-Item "packages/Microsoft.SqlServer.Types.14.0.314.76/nativeBinaries/$arch/*.dll" "SqlServerTypes/$arch/"
}
msbuild BLT_Quan_Ly_Hoc_Tap.csproj /t:Rebuild /p:Configuration=Debug
msbuild tests/Pagination.SmokeTests/Pagination.SmokeTests.csproj
& ./tests/Pagination.SmokeTests/bin/Debug/Pagination.SmokeTests.exe
```

Trên database thử nghiệm:

1. Admin: tải/tìm giáo viên và học viên; kiểm tra các cột, bộ đếm, chọn dòng, tải lại,
   sửa hồ sơ. Ngày sinh `03/04/2006` phải là ngày 3 tháng 4 trên cả máy vi-VN/en-US;
   ngày NULL phải ở trạng thái chưa chọn, không tự ghi ngày hôm nay.
2. Giáo viên: tạo/sửa lớp với khóa học có sẵn, ngày và sĩ số; tìm lớp, tải lại,
   duyệt/từ chối yêu cầu. Chọn đề, chuyển tab Câu Hỏi, tạo/sửa/xóa/nhập Excel;
   giao đề và kiểm tra số câu/thời gian/lớp được giao. Lọc tên trong kết quả và xuất file.
3. Học viên: đăng ký/hủy đơn chờ, làm bài, chọn lớp lọc kết quả, xem điểm,
   xuất chi tiết/báo cáo và nhấp đúp xem bài làm. Lưu hồ sơ có ngày sinh, đổi mật khẩu
   rồi đăng nhập lại. Giáo viên cũng kiểm tra lưu hồ sơ và đổi mật khẩu.

## Kết quả trên cloud Linux

- Build ứng dụng .NET Framework 4.8 thành công với reference assemblies và cấu hình
  resource dành riêng cho Linux, không đổi cấu hình build Windows của project.
- 129 kiểm thử .NET 8 đạt (Pagination 19, StudentProfile 29, ApplicationLogic 49,
  Dashboard 5, ExcelImport 27).
- 74 assertion Mono/Xvfb đạt, gồm 8 assertion mới cho ngày trống và thứ tự ngày/tháng.
- Chưa xác thực giao diện ReaLTaiizor đầy đủ: Mono không có
  `System.Windows.Forms.DpiChangedEventArgs` mà `MaterialForm` cần. Các thao tác
  giao diện Windows phía trên vẫn cần chạy trước khi phát hành.
