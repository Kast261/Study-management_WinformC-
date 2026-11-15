# Rà soát logic master

Nền: `7e4b1ede33f5e8b745f1f30dccbccde478edabda` (`origin/master`).
Nhánh sửa: `fix/master-logic-review`.

## Các lỗi được sửa

1. **Đơn đăng ký cũ có trạng thái NULL:** DAL duyệt/đăng ký đã coi NULL là
   `Chờ duyệt`, nhưng danh sách học viên lại bỏ qua. Lớp có thể xuất hiện ở
   danh sách chưa đăng ký trong khi thao tác đăng ký bị từ chối, và đơn không
   có trong danh sách để học viên hủy. Các truy vấn danh sách nay thống nhất
   NULL là chờ duyệt, giữ mã đơn thật và không tạo thêm dòng lớp chính.
2. **Giáo viên đã lưu trữ vẫn từ chối đơn:** đường từ chối trước đây chỉ kiểm
   tra mã giáo viên phụ trách lớp. Nay kiểm tra hồ sơ còn hoạt động và giữ
   khóa chia sẻ trên hồ sơ trong transaction trước khi cập nhật đơn, để không
   ghi từ phiên cũ sau khi hồ sơ đã được lưu trữ. Service vẫn yêu cầu mã giáo
   viên dương; đường DAL quản trị cũ với mã 0 được giữ tương thích.
3. **Danh sách và tìm kiếm tài khoản bỏ sót vai trò:** `HocVien`, vai trò
   viết khác hoa/thường hoặc có khoảng trắng bị hiển thị chưa xác định và
   không tìm thấy theo tên. Các truy vấn nay chuẩn hóa vai trò bằng
   `LOWER(BTRIM(...))`, hỗ trợ cả `HocSinh`/`HocVien`, giữ liên kết hồ sơ trực
   tiếp và liên kết ngược của dữ liệu cũ.
4. **Đăng nhập được nhưng thao tác cá nhân bị từ chối:** đăng nhập đã trim
   vai trò nhưng các truy vấn phân quyền khác chỉ lowercase. Nay các truy
   vấn đọc/sửa hồ sơ học viên, sửa hồ sơ giáo viên, đổi mật khẩu, nộp bài,
   nhập câu hỏi Excel, giao và ẩn đề thi dùng cùng phép chuẩn hóa. Điều kiện
   tài khoản không khóa, đúng hồ sơ, vai trò và quyền sở hữu được giữ nguyên.

## Kiểm chứng

Thêm 13 trường hợp trong `RegistrationConsistencyTests`,
`AccountDirectoryTests` và `RoleNormalizationTests` (ApplicationLogic.Tests).
Trước sửa, 9/10 trường hợp danh sách/đăng ký mới thất bại; 3/3 trường hợp vai
trò cá nhân thất bại. Trường hợp `HocSinh` chuẩn là đối chứng đã đạt từ trước.
Các test chạy trên PostgreSQL tạm; không đọc App.config hay dùng DB thật.

Chạy từ thư mục gốc repository với .NET SDK 8, Python 3 và Docker:

```sh
python3 tests/check_architecture.py
python3 tests/check_profile_deletion_ui.py
dotnet test tests/Pagination.Tests/Pagination.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/StudentProfile.Tests/StudentProfile.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/Dashboard.Tests/Dashboard.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Kết quả sau sửa: **303 test .NET đạt, 0 thất bại, 0 bỏ qua**
(ApplicationLogic 219, StudentProfile 29, Dashboard 7, ExcelImport 29,
Pagination 19); kiểm tra kiến trúc đạt và 12 tình huống handler nút Xóa đạt.

Không đổi schema hoặc dữ liệu hiện có; không cần migration. Chưa xác minh
build Windows, WinForms Designer, thao tác toàn bộ giao diện và Crystal Reports
trong môi trường Linux này. Rà soát tập trung DAL, service và các luồng nghiệp
vụ được kiểm thử, không phải cam kết toàn bộ ứng dụng không còn lỗi.
