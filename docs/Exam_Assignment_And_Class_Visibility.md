# Giao đề cho lớp và hiển thị bài thi cho học viên

## Giáo viên

Chọn đề đã lưu câu hỏi, môn học, lớp nhận bài và thời gian làm bài trong tab
**Đề thi**, rồi bấm **Giao Bài Thi**. Nút dùng sự kiện Click chuẩn của WinForms,
hiển thị trạng thái kiểm tra/lưu và luôn được bật lại sau khi hoàn tất hoặc hủy.
Lỗi kiểm tra câu hỏi và lỗi lưu đều được thông báo trên cửa sổ giáo viên.

Không cần tạo lại đề hoặc thêm lại câu hỏi khi giao. Mỗi lần xác nhận tạo một
lượt giao độc lập, sao chép các câu hỏi đã lưu và đặt hạn nộp sau 7 ngày. Giáo
viên vẫn có thể giao cùng đề cho nhiều lớp hoặc giao nhiều lần cho một lớp.
Kết quả và câu hỏi của những lượt trước được giữ nguyên.

Cột **Lớp** mới trong bảng lấy dữ liệu từ `TenLop` của từng lượt giao. Đề gốc
chưa giao hiển thị **Chưa giao**. Chọn một lượt đã giao sẽ chọn đúng lớp nhận
của lượt đó, kể cả khi một lớp khác có cùng môn học.

## Học viên

Quyền xem bài dựa trên lớp hiện tại hoặc đăng ký **Đã duyệt**, không phụ thuộc
ngày đăng ký/duyệt có trước hay sau ngày giao đề. Học viên chờ duyệt, bị từ
chối hoặc thuộc lớp khác không nhìn thấy bài của lớp này.

Khi duyệt thêm lớp, giữ lớp chính hiện có của học viên; lớp mới được ghi nhận
qua đăng ký đã duyệt. Việc duyệt lớp mới không được ghi đè lớp chính và làm
mất bài thi của lớp cũ. Mã lớp quyết định quyền truy cập; hai lớp trùng tên
vẫn là hai lớp độc lập.

Danh sách lớp và bài thi được cập nhật khi đổi tab hoặc quay lại cửa sổ học
viên. Bộ lọc môn học và bài đang chọn được giữ nếu vẫn còn trong danh sách.
Tab **Bài thi** giữ cả bài hết hạn/đã đóng để học viên vào lớp sau có thể thấy
lịch sử giao bài. Trang chủ chỉ hiển thị bài còn có thể bắt đầu làm. Đề đã
được giáo viên xóa vẫn tuân theo cơ chế giữ kết quả trong tab Kết quả.

## Kiểm tra

`tests/ApplicationLogic.Tests/ClassExamVisibilityTests.cs` kiểm tra với
PostgreSQL: duyệt trước/sau lúc giao, giao nhiều lượt, nộp bài sau khi được
duyệt, bài hết hạn/đã đóng, nhiều lớp đã duyệt, và tên lớp của từng lượt giao.

Kiểm tra trên Windows sau khi build:

1. Đăng nhập giáo viên, chọn đề đã có câu hỏi và giao cho một lớp.
2. Kiểm tra có thông báo xác nhận/thành công và cột Lớp hiển thị đúng tên.
3. Giao lại cho cùng lớp, sau đó giao cho lớp khác; kiểm tra các lượt độc lập.
4. Đăng ký một học viên vào lớp sau khi giao bài, rồi duyệt đăng ký đó.
5. Quay lại cửa sổ học viên đang mở hoặc tab Bài thi: các lượt đã giao trước
   đó xuất hiện; bài đang mở có thể làm, bài hết hạn/đã đóng vẫn xem được trong
   danh sách nhưng không thể bắt đầu làm.
6. Kiểm tra học viên chờ duyệt không thấy các bài của lớp chưa tham gia.

Nếu một tài khoản vẫn không thấy bài dù đã duyệt, chạy truy vấn chỉ đọc trong
`docs/Diagnose_Student_Assigned_Exams.sql` trên database đang dùng. Kết quả
đối chiếu mã hồ sơ, mã lớp chính/đăng ký, giáo viên, lượt giao và trạng thái đề;
không lấy mật khẩu hay tự thêm quyền vào lớp. Dữ liệu đã bị mất liên kết lớp
trước bản sửa cần đối chiếu để khôi phục đúng lớp, không tự suy luận theo tên.
