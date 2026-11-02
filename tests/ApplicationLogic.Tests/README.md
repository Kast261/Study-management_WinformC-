# Kiểm thử hồi quy logic ứng dụng

Cần .NET SDK 8, Python 3 và Docker đang chạy:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
```

Runner cung cấp PostgreSQL dùng một lần, schema riêng cho mỗi bài kiểm thử và tự dọn container/volume.
Không đọc `App.config`, không truy cập database đang dùng của ứng dụng.

Các bài kiểm thử biên dịch trực tiếp DAL/model/helper thật. Phạm vi gồm đăng nhập bằng hash,
liên kết tài khoản, thay đổi hồ sơ giáo viên, giữ lịch/ngày của lớp khi Admin sửa thông tin,
đăng ký/duyệt/hủy lớp, sĩ số nhiều lớp, truy cập đề thi, lưu bài nguyên tử, rollback và
các lần nộp/duyệt đồng thời, sửa câu hỏi sau khi giao, thứ tự câu hỏi sau khi xóa,
giao lại đề đã có kết quả, xóa hồ sơ có dữ liệu liên kết và tạo tài khoản.

Chạy kiểm tra báo cáo XML với `vi-VN`/`en-US` và DataGridView thực qua
`tests/Pagination.SmokeTests` theo README của `tests/Pagination.Tests`.
Các thao tác phòng thi, hộp thoại đóng form và giao diện ReaLTaiizor cần kiểm tra trên Windows.
