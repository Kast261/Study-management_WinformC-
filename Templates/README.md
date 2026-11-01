Nhập câu hỏi từ Excel trong form Giáo viên:

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
