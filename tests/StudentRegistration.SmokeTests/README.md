# Đăng ký lớp qua hộp xác nhận

Kiểm thử này dùng đúng mã handler trích từ `Forms/HocSinh/Frm_HocVien.cs`,
DataGridView/DataRowView WinForms thật và mô phỏng `OnActivated` trước khi hộp
xác nhận trả về. Dialog và DAL được thay bằng test double để điều khiển chính
xác thời điểm refresh, không truy cập database của ứng dụng. Đây là kiểm thử
handler và vòng đời form, không thay thế thao tác hộp thoại đầy đủ trên Windows.

Các trường hợp: xác nhận đăng ký/hủy đăng ký; activation và tải lại bảng trong
lúc xác nhận; từ chối xác nhận; refresh hoạt động lại sau khi thao tác kết thúc.
Kiểm tra mã lớp và học viên được ghi, không chỉ thông báo trên màn hình.

Từ thư mục gốc repository, với .NET SDK/MSBuild và net48 reference assemblies:

```sh
python3 tests/StudentRegistration.SmokeTests/generate.py
dotnet msbuild tests/StudentRegistration.SmokeTests/StudentRegistration.SmokeTests.csproj /p:FrameworkPathOverride=/path/to/net48/build/.NETFramework/v4.8
```

Trên Windows chạy exe trong `bin/Debug`. Trên Linux dùng image Mono/Xvfb theo
Dockerfile của `tests/Pagination.SmokeTests`:

```sh
docker build -t study-pagination-smoke tests/Pagination.SmokeTests
docker run --rm --mount type=bind,src="$(pwd)/tests/StudentRegistration.SmokeTests",dst=/tests,readonly study-pagination-smoke timeout 40s xvfb-run -a mono /tests/bin/Debug/StudentRegistration.SmokeTests.exe
```

Kỳ vọng `PASS: 16 registration/cancellation activation assertions`, exit 0.
Generator nhận `--source /path/to/older/Frm_HocVien.cs` để kiểm chứng hồi quy.
Với master `c55dafe`, xác nhận đăng ký gọi DAL bằng mã lớp 0 thay vì mã 7 và
kiểm thử thất bại. Với bản sửa, giữ mã 7 kể cả khi bảng bị bind lại.

Luồng database thực (đăng ký → duyệt → thấy đề đã giao từ trước) được kiểm tra
bằng `StudentRegistrationFlowTests` trong `tests/ApplicationLogic.Tests` trên
PostgreSQL dùng một lần; chạy bằng runner `tests/StudentProfile.Tests/run.py`.
