# Xóa và lưu trữ hồ sơ học viên/giáo viên

## Vấn đề được sửa

Ảnh lỗi tại `ProcedureExecutor.ExecuteCall` hiển thị `InvalidOperationException` với thông báo không thể xóa hồ sơ đã liên kết tài khoản hoặc có dữ liệu học tập. Procedure cũ chủ động báo `23503` để bảo vệ lịch sử; đây là trường hợp khác với lỗi thiếu procedure `42883` trước đó.

Bản hiện tại xử lý cả hai: tự cài bộ procedure khi thiếu chữ ký cần gọi, đồng thời trả kết quả lưu trữ thay vì ném lỗi khi hồ sơ có liên kết.

## Hành vi nút Xóa

| Trạng thái hồ sơ | Kết quả | Thông báo |
|---|---|---|
| Chưa có dữ liệu hoặc tài khoản tham chiếu | Xóa bản ghi hồ sơ | Đã xóa học viên/giáo viên |
| Có tài khoản, đăng ký, kết quả, lớp/đề của giáo viên hoặc FK khác chặn xóa | Đặt `DaXoa=TRUE`, ghi `NgayXoa`; khóa tài khoản tương ứng | Đã ngừng hoạt động, giữ lịch sử |
| Không tồn tại hoặc đã lưu trữ | Không ghi thêm | Hồ sơ không còn tồn tại hoặc đã ngừng hoạt động |
| Hủy hộp thoại / chưa chọn dòng | Không ghi DB | Hủy hoặc nhắc chọn hồ sơ |

Hộp thoại xác nhận giải thích trước cả hai cách xử lý. Form giữ ID đã chọn trước khi mở hộp thoại, rồi làm mới danh sách hồ sơ, tài khoản, thống kê và sĩ số/danh sách giáo viên liên quan sau thành công. Lỗi DB thực sự vẫn được báo cho người dùng.

## Dữ liệu và quyền truy cập

- Giữ nguyên ID hồ sơ đã lưu trữ, tài khoản, đăng ký lớp, kết quả, chi tiết bài làm, lớp và đề thi. Báo cáo và lịch sử vẫn tìm được họ tên.
- Danh sách quản lý, tìm kiếm, hồ sơ cá nhân và danh sách cấp tài khoản mới chỉ lấy hồ sơ hoạt động. Dropdown phân công không hiển thị giáo viên đã lưu trữ.
- Tổng học viên/giáo viên, sĩ số lớp, số học viên theo khóa và đơn chờ duyệt loại hồ sơ đã lưu trữ. Chỗ học được giải phóng cho học viên mới; bản ghi đăng ký cũ được giữ lại.
- Khóa tài khoản theo vai trò đã chuẩn hóa và liên kết đúng người. `NguoiDungID` dương là liên kết chính; liên kết ngược `TaiKhoanID` được dùng khi chưa có mã người dùng. Không khóa Admin hoặc tài khoản của người khác chỉ vì trùng mã số hồ sơ.
- Đăng nhập kiểm tra cả trạng thái hồ sơ, nên việc sửa riêng cờ khóa trong DB không kích hoạt lại hồ sơ. Luồng mở khóa tài khoản từ chối hồ sơ đã lưu trữ.
- Phiên còn mở không thể đăng ký/duyệt cho học viên đã lưu trữ, nộp bài hoặc dùng hồ sơ giáo viên đã lưu trữ để tạo/giao/sửa đề. Không phân công mới giáo viên đã lưu trữ; Admin có thể chuyển lớp cũ sang giáo viên còn hoạt động.

Ứng dụng hiện chưa có nút khôi phục hồ sơ lưu trữ. Mở khóa tài khoản không phải thao tác khôi phục hồ sơ.

## Procedure và nâng cấp database

Hai bảng `HocSinh`, `GiaoVien` bổ sung:

```sql
DaXoa boolean NOT NULL DEFAULT FALSE,
NgayXoa timestamp
```

Script `Database/PostgreSQL/02_Procedures.sql` thêm cột khi chưa có và thay hai chữ ký xóa cũ bằng chữ ký ba tham số:

```sql
CALL study_xoa_hoc_sinh(301, FALSE, FALSE);
CALL study_xoa_giao_vien(201, FALSE, FALSE);
```

Kết quả gồm `p_deleted`, `p_archived`; chỉ một cờ TRUE khi thành công. Cả hai FALSE khi không có hồ sơ hoạt động. Tổng vẫn **10 procedure**. Service trả `KetQuaXoaHoSo.DaXoa`, `DaLuuTru` hoặc `KhongTimThay`; API bool `Xoa` cũ vẫn được giữ để tương thích.

Procedure khóa bản ghi hồ sơ, thực hiện lưu trữ và khóa tài khoản trong cùng transaction. Nếu ghi tài khoản thất bại, cờ lưu trữ cũng rollback. Lệnh xóa đồng thời chỉ có một lần thành công. Nếu gặp FK bổ sung trong schema cũ, DELETE bị rollback trong khối con rồi chuyển sang lưu trữ, không xóa dây chuyền.

Các SELECT dùng `to_jsonb(...)->>'daxoa'` để coi cột chưa có là FALSE, giúp DB cũ vẫn đọc được trước khi nâng cấp. Khi bản mới gọi procedure ba tham số trên DB cũ, lỗi `42883` kích hoạt cài script nhúng trong transaction và thử lại một lần. Xem [cơ chế khôi phục](Fix_Profile_Deletion_42883.md).

## Chạy bản sửa trên Windows

1. Lấy mã nhánh `docs/bao-cao-hoan-chinh`, đóng ứng dụng đang chạy.
2. Trong Visual Studio chọn **Clean Solution**, sau đó **Rebuild Solution** và chạy executable vừa build.
3. Với DB phát triển đã có schema, có thể chạy trước toàn bộ `Database/PostgreSQL/02_Procedures.sql` trên đúng database/schema của ứng dụng. Không cần xóa database. Nếu chưa chạy, cơ chế khôi phục sẽ cập nhật lúc gặp thiếu chữ ký procedure.
4. Tài khoản DB cần quyền thay đổi bảng hồ sơ và tạo/cập nhật procedure. Nếu ứng dụng báo thiếu quyền, người quản trị DB chạy script bằng tài khoản sở hữu schema rồi thử lại.
5. Kiểm tra một hồ sơ chưa có liên kết và một hồ sơ có tài khoản/kết quả. Hồ sơ thứ hai biến mất khỏi danh sách hoạt động, tài khoản bị khóa, điểm và lịch sử vẫn còn.

Nếu vẫn thấy đúng thông báo cũ trong ảnh, kiểm tra đang chạy executable vừa Rebuild và đúng nhánh; không bỏ `throw` hoặc tắt kiểm tra FK để ép xóa dữ liệu.

## Bằng chứng kiểm thử

`ProfileArchiveTests` có **20 trường hợp** kiểm tra cả hai loại hồ sơ: xóa thật, liên kết thuận/ngược, vai trò có khoảng trắng/chữ hoa thường, giữ tài khoản khác/Admin, chặn mở khóa và đăng nhập, tìm kiếm/cấp tài khoản, dữ liệu lịch sử, sĩ số, quyền phiên cũ, ghi đồng thời, rollback, FK cũ và nâng cấp chữ ký/cột. Các test khôi phục procedure và CRUD hiện có cũng được cập nhật theo hợp đồng mới.

`tests/check_profile_deletion_ui.py` lấy trực tiếp hai handler từ mã nguồn Form và chạy **12 tình huống** với service/hộp thoại giả lập: xóa thật, lưu trữ, không tìm thấy, hủy, chưa chọn dòng và lỗi thực sự. Kiểm tra cả ID trước xác nhận, thông báo và làm mới danh sách. Đây là kiểm tra logic handler, chưa phải chạy toàn bộ giao diện ReaLTaiizor trên Windows.

Tổng **290 test .NET đạt**, **12 tình huống handler đạt**, kiểm tra kiến trúc đạt. Kiểm thử dùng PostgreSQL tạm trong Docker, không sử dụng `App.config` hay thay đổi database của người dùng. Kết quả từng suite tại [Kết quả kiểm thử](Ket_qua_kiem_thu.md).
