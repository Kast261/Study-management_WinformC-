# Giao đề cho lớp và hiển thị bài thi cho học viên

## Giáo viên

Chọn đề đã lưu câu hỏi, môn học, lớp nhận bài và thời gian làm bài trong tab
**Đề thi**, rồi bấm **Giao Bài Thi**. Nút dùng sự kiện Click chuẩn của WinForms,
hiển thị trạng thái kiểm tra/lưu và luôn được bật lại sau khi hoàn tất hoặc hủy.
Lỗi kiểm tra câu hỏi và lỗi lưu đều được thông báo trên cửa sổ giáo viên.

Ô **Giao cho lớp** hiển thị tên, mã lớp và lịch học để phân biệt các lớp trùng
tên. Khi có nhiều lớp phù hợp và chưa chọn lớp nhận, giáo viên phải chọn rõ
một lớp; ứng dụng không tự lấy lớp đầu tiên. Hộp xác nhận và thông báo thành
công giữ mã lớp này để đối chiếu với đăng ký của học viên. Đề đã giao nhầm
lớp cần giao lại cho đúng mã lớp; bản sửa không tự chuyển đề hoặc mở quyền
cho các học viên ở lớp khác chỉ vì tên lớp giống nhau.

Không cần tạo lại đề hoặc thêm lại câu hỏi khi giao. Mỗi lần xác nhận tạo một
lượt giao độc lập, sao chép các câu hỏi đã lưu và đặt hạn nộp sau 7 ngày. Giáo
viên vẫn có thể giao cùng đề cho nhiều lớp hoặc giao nhiều lần cho một lớp.
Kết quả và câu hỏi của những lượt trước được giữ nguyên.

Cột **Lớp** mới trong bảng lấy dữ liệu từ `TenLop` của từng lượt giao. Đề gốc
chưa giao hiển thị **Chưa giao**. Chọn một lượt đã giao sẽ chọn đúng lớp nhận
của lượt đó, kể cả khi một lớp khác có cùng môn học.

## Học viên

Khi Admin thêm học viên, có thể chọn lớp trong ô xếp lớp ngay dưới ngày sinh/
giới tính. Lớp hiển thị tên, mã lớp và lịch học; không tự chọn một lớp bất kỳ.
Chọn lớp trước khi bấm **Thêm mới**, rồi tạo tài khoản liên kết đúng hồ sơ vừa
thêm. Học viên được xếp lớp sẽ thấy cả các đề đã giao trước lúc tạo hồ sơ.
Chỉ nhận học viên mới vào lớp đang mở và còn chỗ; tạo hồ sơ và xếp lớp cùng
một transaction, không để lại hồ sơ dở dang khi lớp đóng/đầy.

Nếu giữ **Chưa xếp lớp**, tạo tài khoản chưa tự cấp quyền xem đề. Học viên cần
đăng ký lớp và được giáo viên duyệt. Các hồ sơ đã tạo trước bản sửa mà chưa
xếp lớp cũng cần thực hiện bước đăng ký/duyệt này. Ô xếp lớp dành cho thêm mới,
không chuyển lớp của học viên cũ khi sửa thông tin cá nhân.

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

**Đóng đăng kí** trên form giáo viên lấy mã từ dòng đang được chọn, xác nhận
cả tên và mã lớp, rồi cập nhật duy nhất mã lớp đó với điều kiện giáo viên sở
hữu lớp. Đóng đăng ký không đóng các lớp trùng tên/cùng môn, không đóng đề thi
và không thu hồi quyền làm bài của học viên đã thuộc lớp.

`tests/ApplicationLogic.Tests/AdminEnrollmentAndClassClosureTests.cs` kiểm tra
tạo hồ sơ có xếp lớp → tạo tài khoản → đăng nhập → xem/làm đề đã giao; hồ sơ
chưa xếp lớp; lớp đóng/đầy/không tồn tại; thêm đồng thời không vượt sĩ số; đóng
một lớp trùng tên vẫn đăng ký được lớp khác và vẫn làm được đề cũ.

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
