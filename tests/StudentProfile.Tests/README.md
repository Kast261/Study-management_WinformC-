Chạy kiểm thử hồi quy cho hồ sơ học sinh và đổi mật khẩu:

```sh
python tests/StudentProfile.Tests/run.py
```

Cần .NET SDK 8, Python 3 và Docker đang chạy. Có thể chọn SDK bằng
`--dotnet /path/to/dotnet` và Docker daemon bằng `--docker-host`.

Runner tạo PostgreSQL riêng với mật khẩu ngẫu nhiên, kiểm tra dữ liệu thật rồi
xóa container và volume nó đã tạo, kể cả khi kiểm thử thất bại. Không đọc
`App.config` của ứng dụng và không truy cập database được cấu hình trong đó.

Các bài kiểm thử biên dịch trực tiếp DAL, model, bộ kiểm tra dữ liệu và hàm băm
mật khẩu của ứng dụng. Test host cung cấp kết nối Npgsql và StartupPath để chạy
DAL trên Linux. Mỗi bài kiểm thử dùng schema riêng. Các bài kiểm thử này không
khởi chạy WinForms; kiểm tra bố cục và tương tác giao diện cần Windows.
