# Giao đề nhiều lớp và nhiều lần

Trong **Giáo viên → Quản lý đề thi**, chọn đề có câu hỏi, chọn lớp cùng môn do
giáo viên phụ trách rồi bấm **Giao bài thi**. Mỗi lần xác nhận tạo một lượt mới.
Sau khi giao, đề nguồn và lớp đích vẫn được chọn để tiếp tục giao cho lớp đó
hoặc đổi sang lớp khác. Có thể dùng lại cả đề cũ đã giao/có bài làm.

Mỗi lượt có mã riêng, bộ câu hỏi riêng, thời gian làm bài và hạn nộp riêng.
Học viên làm và nộp từng lượt một lần. Một lượt mới không di chuyển, mở lại
hay ghi đè lượt cũ; điểm và câu trả lời của những lần trước vẫn được giữ.
ComboBox chọn đề và lọc kết quả của giáo viên hiển thị tên lớp và mã lượt.

`TaoLanGiaoDeThi` sao chép metadata và câu hỏi trong cùng transaction PostgreSQL.
Khóa đề nguồn dùng chung với luồng sửa/nhập câu hỏi để tránh sao chép một phần.
Tài khoản giáo viên phải còn hoạt động, sở hữu đề và phụ trách lớp đúng môn.
Lỗi sao chép sẽ rollback toàn bộ lượt mới. Các thao tác sửa câu hỏi tiếp tục
bị chặn trên những lượt đã giao; chỉ sửa đề gốc chưa giao.

Luồng này dùng các bảng hiện có `DeThi`, `CauHoi`, `KetQuaThi`, `ChiTietBaiLam`,
không cần migration. Hàm `GiaoDeThiChoLop` cũ chỉ cập nhật lần giao hiện có
trước khi có bài làm; nút giao bài trên form dùng `TaoLanGiaoDeThi`.

Kiểm thử với PostgreSQL dùng một lần:

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Các kiểm thử gồm giao khác lớp/cùng lớp, giao lại sau khi đã nộp, chặn truy cập
sai quyền/sai môn, hai lần giao đồng thời, rollback khi sao chép lỗi và nhập
file `Templates/CauHoi_CSharp_WinForms_20.xlsx` rồi giao ba lượt độc lập.
