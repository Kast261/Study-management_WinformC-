# 10 procedure PostgreSQL được ứng dụng sử dụng

Script cài đặt: `Database/PostgreSQL/02_Procedures.sql`. Bản này bổ sung **9 procedure**, giữ `study_sua_khoa_hoc` đã có, tổng cộng **10 procedure**.

## Cài đặt trên database phát triển

Database mới: tạo database riêng rồi chạy `01_Schema.sql`, sau đó `02_Procedures.sql`. Database đã có schema: sao lưu và kiểm tra tương thích, sau đó cài lại toàn bộ `02_Procedures.sql` trước khi dùng mã mới. Không cần xóa dữ liệu để cài procedure.

```sh
psql -v ON_ERROR_STOP=1 --single-transaction -d <database_phat_trien> -f Database/PostgreSQL/02_Procedures.sql
```

Trên Windows có thể dùng `scripts/Initialize-Postgres.ps1`. Nhập thông tin xác thực qua cơ chế của PostgreSQL; không đưa mật khẩu vào Git. Các kiểm thử của thay đổi này chỉ dùng PostgreSQL dùng một lần, không truy cập `App.config` hoặc database của người dùng.

## Danh mục và đường gọi

| Procedure | Đường gọi trong ứng dụng | Kết quả INOUT | Quy tắc chính |
|---|---|---|---|
| `study_them_khoa_hoc` | Admin → KhoaHocService.Them → KhoaHocDAL.Them | `p_id integer` | Tên 1–100 ký tự, mô tả NULL hoặc ≤255; trả mã mới vào model. |
| `study_sua_khoa_hoc` | Admin → KhoaHocService.Sua → KhoaHocDAL.Sua | `p_updated boolean` | ID dương, tên/mô tả hợp lệ; mã không tồn tại trả FALSE. |
| `study_xoa_khoa_hoc` | Admin → KhoaHocService.Xoa → KhoaHocDAL.Xoa | `p_deleted boolean` | Khóa bản ghi; chặn xóa khi có lớp tham chiếu. |
| `study_them_giao_vien` | Admin → GiaoVienService.Them → GiaoVienDAL.Them | `p_id integer` | Kiểm tra họ tên/độ dài; ngày sinh và thông tin tùy chọn có thể NULL. |
| `study_sua_giao_vien` | Admin → GiaoVienService.Sua → GiaoVienDAL.Sua | `p_updated boolean` | Chỉ sửa các trường hồ sơ, giữ liên kết tài khoản. |
| `study_xoa_giao_vien` | Admin → GiaoVienService.Xoa → GiaoVienDAL.Xoa | `p_deleted boolean` | Chặn khi có lớp, đề hoặc tài khoản liên kết thuận/ngược. |
| `study_them_hoc_sinh` | Admin → HocSinhService.Them → HocSinhDAL.Them | `p_id integer` | Có thể chưa xếp lớp; nếu chọn lớp thì khóa lớp mở, kiểm tra sĩ số rồi thêm trong cùng CALL. |
| `study_sua_hoc_sinh` | Admin → HocSinhService.Sua → HocSinhDAL.Sua | `p_updated boolean` | Giữ LopHocID/TaiKhoanID; ngày chưa chọn giữ NULL. |
| `study_xoa_hoc_sinh` | Admin → HocSinhService.Xoa → HocSinhDAL.Xoa | `p_deleted boolean` | Chặn khi có đăng ký, kết quả hoặc tài khoản liên kết thuận/ngược. |
| `study_dong_dang_ky_lop` | Giáo viên → LopHocService.DongDangKyLop → LopHocDAL.DongDangKyLop | `p_updated boolean` | Mã lớp và giáo viên phải dương, đúng giáo viên phụ trách, lớp đang mở. Không đóng đề hoặc thu hồi tư cách lớp. |

Tên kỹ thuật `HocSinh` tương ứng học viên. Các SELECT/tìm kiếm, cập nhật hồ sơ cá nhân và các giao dịch đề/bài làm khác vẫn có SQL có tham số trong DAL; bản này không chuyển toàn bộ ứng dụng sang procedure.

## Hợp đồng tham số

- **Khóa học:** `p_ten varchar`, `p_mota varchar`; sửa/xóa có `p_id integer` đầu vào. Thêm trả `p_id`, sửa trả `p_updated`, xóa trả `p_deleted`.
- **Giáo viên:** `p_hoten varchar`, `p_ngaysinh date`, `p_gioitinh varchar`, `p_chuyenmon varchar`, `p_email varchar`, `p_sdt varchar`, `p_diachi varchar`. Sửa thêm `p_id` đầu vào. Thêm trả `p_id`; sửa/xóa trả cờ boolean.
- **Học viên:** `p_hoten`, `p_ngaysinh`, `p_gioitinh`, `p_email`, `p_sdt`, `p_diachi`; thêm có `p_lop integer` (0/NULL là chưa xếp lớp), sửa có `p_id`. Thêm trả `p_id`; sửa/xóa trả cờ boolean.
- **Đóng đăng ký:** `p_id integer`, `p_giao_vien integer`, `p_updated boolean INOUT`.

Họ tên và tên khóa được trim. Giới hạn: họ tên/email/chuyên môn ≤100, giới tính ≤10, điện thoại ≤20, địa chỉ/mô tả ≤255. Đây là kiểm tra bắt buộc và độ dài tại DB, không thay thế kiểm tra định dạng email/điện thoại/ngày của ứng dụng.

## Ví dụ kiểm tra trong transaction thử

Các mã 101/201 dưới đây chỉ là minh họa; thay bằng ID thực sự do CALL trả về. Không chạy chuỗi ví dụ trên database sử dụng thật.

```sql
BEGIN;
CALL study_them_khoa_hoc('C# cơ bản', NULL, 0);
CALL study_sua_khoa_hoc(101, 'C# nâng cao', 'Mô tả có dấu nháy ''', FALSE);
CALL study_xoa_khoa_hoc(101, FALSE);

CALL study_them_giao_vien('Nguyễn An', NULL, 'Nam', 'C#', NULL, NULL, NULL, 0);
CALL study_sua_giao_vien(201, 'Nguyễn An', DATE '1980-04-03', 'Nam', 'C#', NULL, NULL, NULL, FALSE);
CALL study_xoa_giao_vien(201, FALSE);

CALL study_them_hoc_sinh('Trần Bình', NULL, NULL, NULL, NULL, NULL, 0, 0);
CALL study_sua_hoc_sinh(301, 'Trần Bình', DATE '2006-04-03', NULL, NULL, NULL, NULL, FALSE);
CALL study_xoa_hoc_sinh(301, FALSE);

CALL study_dong_dang_ky_lop(401, 201, FALSE);
ROLLBACK;
```

## Kết quả, ngoại lệ và giao dịch

Mã mới lấy bằng `INSERT … RETURNING`. Thêm học viên vào lớp không tồn tại/đóng/đầy trả `p_id=0`, không tạo hồ sơ dở. Không tìm thấy bản ghi cần sửa/xóa trả cờ FALSE. Dữ liệu bắt buộc hoặc độ dài không hợp lệ báo SQLSTATE `22023`. Xóa đối tượng còn liên quan báo `23503`; DAL chuyển thành `InvalidOperationException` để giữ cách xử lý nghiệp vụ.

`ProcedureExecutor` dùng `CALL` rõ ràng, đọc cột INOUT từ DataTable và gắn kiểu `varchar`/`date` cho tham số NULL. Với Npgsql 6 không dùng `CommandType.StoredProcedure` vì cơ chế đó gọi PostgreSQL function. Thiếu procedure (`42883`) báo cần cài lại `02_Procedures.sql`, không tự fallback sang SQL trực tiếp.

Không procedure nào COMMIT nội bộ. Một CALL chạy trong transaction của PostgreSQL; caller có thể gộp nhiều CALL trong một transaction rồi rollback tất cả. Thêm học viên dùng cùng khóa bản ghi lớp và cách đếm lớp chính OR đăng ký Đã duyệt như luồng duyệt C#. Đóng lớp từ chối giáo viên 0 ngay tại procedure, không giữ lối bỏ qua quyền cũ của DAL ở thao tác này.

Khóa bản ghi và kiểm tra xóa không thay thế FK/UNIQUE còn thiếu trong schema. Các ghi ngoài ứng dụng hoặc luồng không tuân thủ cùng khóa vẫn cần được bảo vệ bằng ràng buộc và quyền DB khi triển khai rộng.

## Kiểm thử

`tests/ApplicationLogic.Tests/CourseworkProcedureTests.cs` bổ sung 22 trường hợp: cài lặp vẫn đúng 10 procedure, CRUD hồ sơ qua Service, Unicode/dấu nháy/NULL/ngày, giữ liên kết lớp/tài khoản, chặn xóa với chín dạng dữ liệu liên quan, cập nhật mã không tồn tại, kiểm tra dữ liệu tại DB, rollback nhiều CALL, thiếu procedure không ghi lén bằng SQL, quyền đóng lớp và thêm học viên chạy đồng thời với duyệt đơn.

Các test cũ tiếp tục kiểm tra không vượt sĩ số khi nhiều Admin thêm cùng lúc, không tạo hồ sơ khi lớp đầy/đóng/không tồn tại, và đóng đăng ký không làm mất quyền làm đề đã giao. Xem `docs/Ket_qua_kiem_thu.md` để biết kết quả chạy thực tế và giới hạn Windows/Crystal.
