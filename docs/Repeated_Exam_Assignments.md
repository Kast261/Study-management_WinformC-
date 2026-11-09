# Giao đề nhiều lớp và nhiều lần

Trong **Giáo viên → Quản lý đề thi**, chọn đề có câu hỏi, chọn lớp do
giáo viên phụ trách rồi bấm **Giao bài thi**. Mỗi lần xác nhận tạo một lượt mới.
Sau khi giao, đề nguồn và lớp đích vẫn được chọn để tiếp tục giao cho lớp đó
hoặc đổi sang lớp khác. Có thể dùng lại cả đề cũ đã giao/có bài làm.

Mỗi lượt có mã riêng, bộ câu hỏi riêng, thời gian làm bài và hạn nộp riêng.
Học viên làm và nộp từng lượt một lần. Một lượt mới không di chuyển, mở lại
hay ghi đè lượt cũ; điểm và câu trả lời của những lần trước vẫn được giữ.
ComboBox chọn đề và lọc kết quả chỉ hiển thị tên đề. Lớp nhận bài nằm ở ComboBox
**Giao cho lớp**; cột **Lớp được giao** trên bảng đề được ẩn.

`TaoLanGiaoDeThi` sao chép metadata và câu hỏi trong cùng transaction PostgreSQL.
Khóa đề nguồn dùng chung với luồng sửa/nhập câu hỏi để tránh sao chép một phần.
Tài khoản giáo viên phải còn hoạt động, sở hữu đề và phụ trách lớp nhận bài.
Danh sách lớp và điều kiện giao không phụ thuộc tên khóa học hay việc lớp đã
được phân loại. Môn học lưu trên lượt giao được lấy nguyên từ đề nguồn.
Lỗi sao chép sẽ rollback toàn bộ lượt mới. Các thao tác sửa câu hỏi tiếp tục
bị chặn trên những lượt đã giao; chỉ sửa đề gốc chưa giao.

Luồng này dùng các bảng hiện có `DeThi`, `CauHoi`, `KetQuaThi`, `ChiTietBaiLam`,
không cần migration. Hàm `GiaoDeThiChoLop` cũ chỉ cập nhật lần giao hiện có
trước khi có bài làm; nút giao bài trên form dùng `TaoLanGiaoDeThi`.

**Xóa Đề Thi** hỗ trợ cả lượt đã giao, thông qua overload `XoaDeThi` có tài khoản
và mã giáo viên. Hộp xác nhận nêu rõ tên/mã đề và việc xóa cả câu hỏi, bài làm,
điểm. Transaction khóa đề cùng cơ chế với nộp bài, xóa chi tiết trước kết quả,
sau đó câu hỏi và đề. Nếu lỗi thì rollback toàn bộ. Lớp học, đề nguồn và các lượt
khác vẫn còn nguyên khi xóa một lượt sao chép. Overload cũ không nhận tài khoản
tiếp tục chỉ xóa đề chưa giao.

Kiểm thử với PostgreSQL dùng một lần:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Các kiểm thử gồm giao khác lớp/cùng lớp, giao lại sau khi đã nộp, chặn truy cập
sai quyền, giao cho lớp có tên khóa học khác hoặc chưa phân loại, hai lần giao
đồng thời, rollback khi sao chép/xóa lỗi, xóa lượt đã có bài làm theo khóa ngoại
và nhập file `Templates/CauHoi_CSharp_WinForms_20.xlsx` rồi giao ba lượt độc lập.
