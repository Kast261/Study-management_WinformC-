# Bài thi đang mở ở Trang chủ Học viên

Bảng `dgvHV_DeThiSapToi` trước đây chưa được gán nguồn dữ liệu; ba cột Designer
`BT_DeThi`, `BT_HanNop`, `BT_SoCau` chưa có `DataPropertyName`. Thông báo có bài
thi là chữ cố định và hai nút làm bài ở Trang chủ chưa có sự kiện Click.

Form hiện liên kết các cột trước khi gắn phân trang, tải dữ liệu bằng
`DeThiDAL.LayBaiThiDangMoChoHocSinh` và hiển thị số bài thực tế. Bài được liệt kê
khi đã mở, thuộc lớp chính hoặc lớp đã duyệt của học viên, có câu hỏi, thời
gian làm bài dương, còn hạn (hoặc không giới hạn hạn nộp), và học viên chưa nộp.
Số câu lấy từ bảng câu hỏi, không phụ thuộc số câu lưu sẵn trong đề.

Trang chủ dùng nguồn riêng với tab Bài Thi nên lọc môn học không tác động tới
bảng Trang chủ. Danh sách tải lại khi vào Trang chủ/Bài Thi, nhấn Làm mới hoặc
đóng phòng thi. Không có cơ chế cập nhật tự động khi giữ nguyên màn hình.

Hai nút Làm Bài/Làm Bài Ngay mở dòng đang chọn, hoặc bài đầu tiên theo hạn nộp
nếu chưa chọn. Nhấp đúp một dòng cũng mở bài đó. Trước khi vào phòng thi,
ứng dụng kiểm tra lại quyền làm bài trong database; phòng thi vẫn giữ kiểm tra
riêng. Sau khi nộp, bài không còn trong danh sách đang mở của Trang chủ nhưng
vẫn xuất hiện với trạng thái Đã làm ở tab Bài Thi và trong Kết Quả.

Khi tải thất bại, thông báo nêu lỗi, xóa dữ liệu cũ và khóa nút làm bài.
Không có bài thì thông báo rõ trạng thái trống. Tiêu đề bảng bên phải được
đổi thành Lớp Đang Học cho đúng dữ liệu hiện có. Không sửa Designer/resx/schema.

Kiểm thử logic với PostgreSQL dùng một lần:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
```

Kiểm tra thêm trên Windows:

1. Giáo viên giao đề có câu hỏi, còn hạn cho lớp học viên đang học hoặc đã duyệt.
2. Đăng nhập học viên: Trang chủ hiển thị tên đề, hạn nộp và số câu; số bài phía
   trên khớp tổng số dòng, kể cả khi có nhiều trang.
3. Lọc môn học ở Bài Thi rồi quay về Trang chủ; bảng vẫn hiển thị đủ bài còn làm được.
4. Chọn bài bất kỳ, nhấn một trong hai nút hoặc nhấp đúp để vào đúng đề; nộp bài
   rồi kiểm tra bài biến mất ở Trang chủ và có kết quả ở Kết Quả.
5. Kiểm tra đề không hạn nộp hiển thị Không giới hạn; đề đóng/hết hạn/đã nộp,
   đề chưa có câu hỏi và đề của lớp chờ duyệt không xuất hiện ở Trang chủ.
6. Để giáo viên đóng/giao đề khi học viên đang mở form, chuyển sang tab khác
   rồi quay về Trang chủ để kiểm tra danh sách đã cập nhật.
