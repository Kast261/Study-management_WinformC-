# Sửa lỗi xóa học viên/giáo viên do thiếu procedure

## Nguyên nhân được xác nhận

Thông báo `Chưa cài thủ tục PostgreSQL`, `02_Procedures.sql` hoặc SQLSTATE `42883` xuất hiện khi chạy mã mới nhưng database chưa có `study_xoa_hoc_sinh` / `study_xoa_giao_vien`. Đây là lỗi thiếu thủ tục, khác với việc hồ sơ còn dữ liệu liên quan và cần được giữ lại.

## Thay đổi trong bản vá

`02_Procedures.sql` được nhúng vào executable với tên tài nguyên `PostgresProcedures.sql`. Khi một CALL bị lỗi thiếu procedure ở cấp gọi ngoài, `ProcedureExecutor`:

1. Đọc script nhúng trong chính bản ứng dụng đang chạy.
2. Cài/cập nhật bộ 10 procedure trong một transaction; dùng advisory lock theo schema để các phiên không tạo procedure đồng thời.
3. Commit sau khi cài đủ bộ, rồi thử lại **đúng CALL ban đầu một lần** với tham số được sao chép cho command mới.
4. Đọc cờ `p_deleted` để trả kết quả; không thay bằng câu DELETE trực tiếp và không bỏ qua kiểm tra dữ liệu liên quan.

Lỗi function bên trong một procedure đã tồn tại không được coi là thiếu procedure ngoài để tự ghi đè. Nếu cài đặt thất bại giữa chừng, transaction rollback toàn bộ phần cài mới. Nếu tài khoản DB không đủ quyền tạo/cập nhật, ứng dụng hướng dẫn quản trị viên cài script thay vì tự xóa bằng SQL.

## Cách dùng bản sửa trên máy Windows

1. Lấy mã mới trên nhánh `docs/bao-cao-hoan-chinh`.
2. **Clean/Rebuild** project bằng Visual Studio rồi chạy executable vừa build, không chạy lại bản exe cũ.
3. Thử xóa hồ sơ không còn tài khoản hoặc dữ liệu học tập liên quan. Với tài khoản DB có quyền cài thủ tục, lần gặp `42883` sẽ tự khôi phục rồi thực hiện lại lệnh xóa.
4. Nếu được báo không đủ quyền cài, nhờ người quản trị mở **đúng database/schema trong `ChuoiKetNoi`** và chạy toàn bộ `Database/PostgreSQL/02_Procedures.sql`, sau đó thử lại. Không cần xóa hoặc tạo lại database; không đưa mật khẩu lên Git hoặc gửi vào chat.

Hồ sơ còn lớp/đề/đăng ký/kết quả hoặc tài khoản liên kết vẫn được chặn xóa để tránh mất lịch sử. Bản vá không tự xóa dây chuyền những dữ liệu đó. Mã không còn tồn tại trả FALSE.

## Bằng chứng kiểm thử

Trên PostgreSQL dùng một lần, 14 trường hợp mới trong `ProfileDeletionRecoveryTests` kiểm tra: xóa cả hai loại hồ sơ khi thiếu toàn bộ procedure hoặc chỉ thiếu procedure xóa; vẫn chặn xóa dữ liệu học tập; mã không tồn tại; nhiều lệnh xóa đồng thời; role không có quyền CREATE; rollback khi cài lỗi giữa chừng; lỗi function nội bộ và giới hạn chỉ retry một lần.

Suite `ApplicationLogic.Tests`: **186 đạt**. Suite `StudentProfile.Tests`: **29 đạt**. Kiểm tra kiến trúc cũng xác nhận executable khai báo đúng tài nguyên script nhúng. Các kiểm thử chỉ truy cập DB tạm trong Docker, không thay đổi database của người dùng. Giao diện đầy đủ vẫn cần kiểm tra trên máy Windows chạy ứng dụng.
