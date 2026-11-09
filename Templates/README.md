Nhập câu hỏi từ Excel trong form Giáo viên:

File `CauHoi_CSharp_WinForms_20.xlsx` có sẵn 20 câu trắc nghiệm môn **Lập trình
C# WinForms**, đủ bốn phương án và đáp án đúng. Có thể nhập trực tiếp vào một
đề gốc mới; hướng dẫn nằm ở sheet `HuongDan`.

1. Nhấp chuột phải nút **Nhập Excel** → **Lưu file Excel mẫu...**.
2. Mở `CauHoi_Mau.xlsx`, điền dữ liệu trong sheet **CauHoi**. Thay hoặc xóa hai
   câu hỏi ví dụ trước khi nhập bộ câu hỏi thật.
3. Mỗi dòng gồm đủ sáu cột:

   | Nội dung | Đáp án A | Đáp án B | Đáp án C | Đáp án D | Đáp án đúng |
   |---|---|---|---|---|---|
   | 2 + 3 bằng bao nhiêu? | 3 | 4 | 5 | 6 | C |

4. Lưu file `.xlsx` hoặc `.xls`. Nội dung và cả bốn đáp án đều bắt buộc. Mỗi
   đáp án tối đa 255 ký tự; đáp án đúng là A/B/C/D (không phân biệt hoa thường).
5. Trong tab **Câu hỏi**, chọn môn học và nhập **tên đề mới** để tạo cả bộ câu
   hỏi, hoặc chọn **đề chưa giao** để thêm câu hỏi vào cuối đề đó. Khi nhập tên
   trùng với đề đang có, ứng dụng dùng đề đó. Nếu có nhiều đề trùng tên, chọn
   đúng đề trong ComboBox trước.
6. Bấm **Nhập Excel**, chọn file trên máy, kiểm tra tên đề và số câu rồi xác nhận.
7. Danh sách câu hỏi và số câu được tải lại sau khi lưu thành công; phân trang
   vẫn hoạt động như trước.

Dòng trống được bỏ qua; có thể thêm cột STT và đổi thứ tự các cột. Các tên cột
không dấu `NoiDung`, `DapAnA`, `DapAnB`, `DapAnC`, `DapAnD`, `DapAnDung` cũng
được hỗ trợ. Ứng dụng dùng sheet đầu tiên có đủ cột tiêu đề trong 20 dòng đầu.
Tối đa 5.000 câu và 20 MB cho mỗi file.

File sai sẽ báo sheet và số dòng lỗi, không lưu một phần dữ liệu. Các lần nhập
thành công được lưu trọn gói trong một transaction. Nhập lại cùng file sẽ thêm
câu hỏi lần nữa; các câu hỏi cũ không bị xóa. Đề đã giao hoặc có bài làm cần
được giữ nguyên; hãy tạo đề mới để nhập bộ câu hỏi khác.

Giao nhiều lần: chọn đề gốc ở **Quản lý đề thi**, chọn lớp rồi bấm **Giao bài thi**.
Đề đã nhập câu hỏi được chọn và giao trực tiếp; không cần nhập Excel lại khi
chuyển tab. Đề vừa tạo xuất hiện ngay trong ComboBox tab **Bài thi**, kể cả
khi chưa thêm câu hỏi, và vẫn được chọn khi chuyển tab. Số câu được cập nhật
sau khi thêm hoặc nhập Excel.
Mỗi lần xác nhận tạo một lượt thi riêng có mã riêng. Có thể chọn lớp khác do
giáo viên phụ trách hoặc giao tiếp cho cùng lớp, kể cả khi học viên đã nộp lượt
trước. ComboBox đề thi chỉ hiện tên đề; chọn lớp nhận bài ở **Giao cho lớp**.
Môn học của lượt giao dùng đúng tên đang chọn ở ô **Môn học**. Sau khi giao,
bảng và ComboBox chọn ngay lượt vừa tạo để hiển thị đúng môn đã lưu.
Điểm, câu trả lời, thời gian và hạn nộp của mỗi lượt độc lập; sửa hoặc nhập thêm
câu hỏi vào đề gốc không đổi nội dung các lượt đã giao.

**Xóa Đề Thi** xóa đúng đề/lượt đang chọn sau khi xác nhận, kể cả lượt đã giao.
Đề được ẩn khỏi danh sách giao bài; câu hỏi, bài làm và điểm vẫn được giữ để
học sinh xem lại lịch sử, đáp án và xuất kết quả. Giáo viên xem lại trong tab
**Kết quả**, với nhãn **(đã xóa)** trên những đề đã ẩn.
