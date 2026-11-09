# Giao đề nhiều lớp và nhiều lần

Trong tab **Câu hỏi**, tạo đề rồi thêm câu hỏi hoặc nhập Excel. Bấm **Tạo đề**
với tên và môn của một đề gốc đang có sẽ mở lại đề đó để tiếp tục soạn, giữ
nguyên câu hỏi đã lưu. Để tạo bộ đề khác, dùng tên đề khác.

Đề vừa tạo xuất hiện ngay trong ComboBox tab **Bài thi**, kể cả khi đang có
0 câu hỏi. Chuyển tab hoặc tải lại danh sách vẫn giữ đúng mã đề đang chọn.
Sau khi thêm câu hỏi hoặc nhập Excel, chọn lại tên đề sẽ nạp đúng số câu đã
lưu để giao trực tiếp, không cần nhập lại. Bảng đề tiếp tục chỉ liệt kê các
đề đã có câu hỏi. Nút giao dùng mã đề đang hiển thị trong ComboBox và kiểm tra
câu hỏi thực tế trước khi tạo lượt thi.

Trong **Giáo viên → Quản lý đề thi**, chọn đề có câu hỏi, chọn lớp do
giáo viên phụ trách rồi bấm **Giao bài thi**. Mỗi lần xác nhận tạo một lượt mới.
Sau khi giao, lượt vừa tạo được chọn ngay trên bảng và ComboBox, đồng thời
giữ lớp đích để tiếp tục giao cho lớp đó hoặc đổi sang lớp khác. Có thể dùng
lại cả đề cũ đã giao/có bài làm.

Mỗi lượt có mã riêng, bộ câu hỏi riêng, thời gian làm bài và hạn nộp riêng.
Học viên làm và nộp từng lượt một lần. Một lượt mới không di chuyển, mở lại
hay ghi đè lượt cũ; điểm và câu trả lời của những lần trước vẫn được giữ.
ComboBox chọn đề hiển thị tên đề; bộ lọc kết quả thêm nhãn **(đã xóa)** cho đề
đã ẩn để giáo viên vẫn xem lại điểm. Lớp nhận bài nằm ở ComboBox
**Giao cho lớp**; cột **Lớp được giao** trên bảng đề được ẩn.

`TaoLanGiaoDeThi` sao chép metadata và câu hỏi trong cùng transaction PostgreSQL.
Khóa đề nguồn dùng chung với luồng sửa/nhập câu hỏi để tránh sao chép một phần.
Tài khoản giáo viên phải còn hoạt động, sở hữu đề và phụ trách lớp nhận bài.
Danh sách lớp và điều kiện giao không phụ thuộc tên khóa học hay việc lớp đã
được phân loại. Môn học lưu trên lượt giao được lấy từ ô **Môn học** khi xác
nhận giao. Chẳng hạn, chọn **Lập trình C# WinForms** sẽ lưu và hiển thị đúng
tên này trên bảng của giáo viên, học viên và kết quả thi, dù tái sử dụng bộ
câu hỏi của một đề gốc mang tên môn khác. Đề gốc và những lượt trước còn nguyên.
Các caller không truyền môn học vẫn dùng môn của đề nguồn để tương thích.
Khi đổi môn/đề, form ưu tiên lớp cùng môn trong các lớp giáo viên phụ trách,
thay vì giữ lớp khác môn từ lần chọn trước. Nếu không có lớp phù hợp, để trống
ô chọn để giáo viên chủ động chọn lớp. Vẫn có thể chọn lớp khác trong danh sách.
Nếu lớp **Mạng Máy Tính N06** không có trong danh sách xổ xuống, kiểm tra lớp
đã được phân công cho đúng giáo viên đang đăng nhập ở màn hình quản lý lớp.
Lỗi sao chép sẽ rollback toàn bộ lượt mới. Các thao tác sửa câu hỏi tiếp tục
bị chặn trên những lượt đã giao; chỉ sửa đề gốc chưa giao.

Luồng này dùng các bảng hiện có `DeThi`, `CauHoi`, `KetQuaThi`, `ChiTietBaiLam`,
không cần migration. Hàm `GiaoDeThiChoLop` cũ chỉ cập nhật lần giao hiện có
trước khi có bài làm; nút giao bài trên form dùng `TaoLanGiaoDeThi`.

**Xóa Đề Thi** hỗ trợ cả lượt đã giao, thông qua overload `XoaDeThi` có tài khoản
và mã giáo viên. Thao tác đổi trạng thái thành **Đã xóa**, ẩn đề khỏi bảng và
ComboBox giao bài, đồng thời ngừng nhận bài mới. Metadata, câu hỏi, kết quả,
đáp án đã chọn, điểm và thời gian nộp còn nguyên để xem lại và xuất báo cáo.
Transaction khóa đề cùng cơ chế với nộp bài để không mất kết quả khi hai thao
tác diễn ra đồng thời. Nếu lỗi thì rollback. Bộ lọc tab **Kết quả** của giáo viên
vẫn bao gồm đề đã ẩn. Overload cũ không nhận tài khoản tiếp tục chỉ xóa đề chưa
giao và không có bài làm. Không cần thay đổi cấu trúc database.

Kiểm thử với PostgreSQL dùng một lần:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Các kiểm thử gồm giao khác lớp/cùng lớp, giao lại sau khi đã nộp, chặn truy cập
sai quyền, giao cho lớp có tên khóa học khác hoặc chưa phân loại, hai lần giao
đồng thời, rollback khi sao chép/ẩn đề lỗi, ẩn lượt đã có bài làm mà giữ toàn bộ lịch sử
và nhập file `Templates/CauHoi_CSharp_WinForms_20.xlsx` rồi giao ba lượt độc lập.
`ExamSelectionTests` kiểm tra thêm luồng tạo đề, lưu 20 câu, chọn lại đề,
giao cho N06 và nộp bài đạt 10 điểm mà không thêm lại câu hỏi.
