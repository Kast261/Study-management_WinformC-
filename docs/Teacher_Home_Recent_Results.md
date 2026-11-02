# Kết quả bài thi gần đây ở Trang chủ Giáo viên

Bảng hiện có trong Designer là `dgvGV_LichDay`. Trước đây bảng chưa có nguồn
dữ liệu và các cột chưa có `DataPropertyName`. Form hiện cấu hình ba cột trước
khi gắn phân trang, rồi nạp bằng `DeThiDAL.LayTongHopKetQuaGanDay`.

Mỗi dòng là một đề của giáo viên đang đăng nhập đã có ít nhất một bài nộp:

| Cột | Dữ liệu |
| --- | --- |
| Đề Thi | Tên đề, nhóm riêng theo DeThiID kể cả khi trùng tên |
| Đã Nộp | Số bản ghi kết quả đã lưu của đề |
| Điểm Trung Bình | Trung bình điểm các bài nộp có điểm, làm tròn và hiển thị hai chữ số thập phân |

Điểm 0 được tính vào trung bình. Dữ liệu cũ thiếu điểm không tự chuyển thành 0;
nếu cả đề chưa có điểm thì hiển thị dấu `—`. Truy vấn tương thích cả kiểu điểm
`double precision` trong CreateDataBase.cs và kiểu `numeric`.

Đề có lần nộp gần nhất lên đầu; thời gian thiếu xếp cuối và dùng DeThiID để
giữ thứ tự ổn định khi trùng thời gian. Bảng giữ toàn bộ đề có bài nộp để phân
trang, kể cả đề đã đóng/hết hạn và kết quả của học viên đã chuyển lớp.

Dữ liệu tải khi mở form và khi quay về Trang chủ từ tab khác. Nếu học viên
vừa nộp trong lúc giáo viên đang giữ nguyên Trang chủ, chuyển tab rồi quay lại
để cập nhật. Chưa có cơ chế tự cập nhật theo thời gian.

Không có bài nộp thì bảng trống, giữ tiêu đề cột và thông báo trạng thái trống.
Lỗi truy vấn sẽ xóa dữ liệu cũ, thông báo lỗi và cho phép thử lại khi quay về
Trang chủ. Không thay đổi Designer, resx hoặc schema database.

Chạy kiểm thử với PostgreSQL dùng một lần:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
```

Kiểm tra giao diện trên Windows: đăng nhập Giáo viên, nộp thêm bài bằng tài
khoản học viên, quay lại Trang chủ và đối chiếu số bài nộp/điểm trung bình với
tab Kết Quả. Thử thêm giáo viên khác, đề trùng tên, dữ liệu nhiều hơn 20 đề và
trường hợp chưa có bài nộp. Build Linux và các kiểm thử DAL không thay thế việc
kiểm tra giao diện ReaLTaiizor trên Windows.
