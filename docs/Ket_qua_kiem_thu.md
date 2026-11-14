# Kết quả kiểm tra bản bổ sung

## Xóa/lưu trữ hồ sơ và bảo toàn lịch sử — 14/11/2026

Đã chạy lại năm suite sau khi sửa cả học viên và giáo viên. Hồ sơ có dữ liệu liên quan được lưu trữ và khóa tài khoản thay cho lỗi nghiệp vụ trong ảnh; hồ sơ chưa liên kết vẫn xóa hẳn. Danh sách, đăng nhập, cấp/mở khóa tài khoản, sĩ số, thống kê và các thao tác ghi bằng phiên cũ được kiểm tra theo trạng thái hồ sơ.

| Target | Kết quả mới nhất |
|---|---|
| Kiểm tra kiến trúc | Đạt |
| ApplicationLogic.Tests | **206 đạt** |
| StudentProfile.Tests | 29 đạt |
| Dashboard.Tests | 7 đạt |
| ExcelImport.Tests | 29 đạt |
| Pagination.Tests | 19 đạt |
| Tổng .NET 8 | **290 đạt; 0 thất bại; 0 bỏ qua** |
| Hai handler nút Xóa Admin | **12 tình huống đạt** |

20 trường hợp mới trong `ProfileArchiveTests` kiểm tra xóa/lưu trữ, liên kết thuận/ngược và vai trò, bảo toàn lịch sử, không khóa nhầm tài khoản khác/Admin, không đăng nhập lại bằng mở khóa, sĩ số, hồ sơ ẩn trong tìm kiếm/cấp tài khoản, chặn thao tác từ phiên cũ, xóa đồng thời, rollback và schema/chữ ký procedure cũ. Các test procedure/khôi phục hiện có được giữ và cập nhật kết quả lưu trữ.

Script `check_profile_deletion_ui.py` biên dịch trực tiếp hai handler từ Form vào harness có service và hộp thoại giả lập. Mỗi loại hồ sơ được kiểm tra sáu tình huống: xóa thật, lưu trữ, không tìm thấy, hủy, chưa chọn và lỗi thực sự. Kiểm tra cả ID được giữ trước xác nhận, thông báo và các lần làm mới. Đây là kiểm tra logic handler; chưa nghiệm thu hiển thị và tương tác toàn bộ Form trên Windows.

Môi trường: Linux, .NET SDK 8.0.428, PostgreSQL 16 trong Docker. Các runner tạo và dọn DB tạm, không dùng cấu hình hay DB người dùng. Hướng dẫn nâng cấp và hành vi chi tiết: [Vòng đời hồ sơ](Profile_Deletion_Lifecycle.md). Các mục dưới đây ghi lại những lần kiểm thử trước đó.

## Bản vá xóa hồ sơ khi DB thiếu procedure — 14/11/2026

Đã xác minh cơ chế cài bộ procedure nhúng và retry CALL khi gặp `42883`. Suite `ApplicationLogic.Tests` có **186 test đạt** (14 trường hợp khôi phục mới); `StudentProfile.Tests` có **29 test đạt**. Không có test thất bại/bỏ qua ở hai suite này. Kiểm tra kiến trúc đạt, gồm kiểm tra tài nguyên `PostgresProcedures.sql` được nhúng vào executable.

Các trường hợp mới kiểm tra cả xóa giáo viên/học viên khi DB chưa cài thủ tục, thiếu riêng thủ tục xóa, bảo vệ dữ liệu học tập, xóa đồng thời, role thiếu quyền CREATE, rollback cài đặt, lỗi function nội bộ và retry tối đa một lần. Kiểm thử chỉ dùng PostgreSQL tạm; xem [hướng dẫn dùng bản sửa](Fix_Profile_Deletion_42883.md). Các suite khác và số liệu 256 bên dưới thuộc lần cập nhật 10 procedure trước bản vá này.

## Cập nhật 10 procedure — 14/11/2026

Script `02_Procedures.sql` hiện có **10 procedure** (thêm 9). Các thao tác thêm/sửa/xóa khóa học, giáo viên, học viên và đóng đăng ký lớp đã gọi `CALL` từ DAL qua `ProcedureExecutor`. Xem [danh mục, tham số và cài đặt](Procedures_PostgreSQL.md).

Môi trường chạy lại: Linux, .NET SDK **8.0.428**, PostgreSQL 16 trong Docker; mỗi runner dùng database tạm và dọn container/volume. Không dùng `App.config` hoặc DB thật.

| Target | Kết quả phiên cập nhật |
|---|---|
| Kiểm tra kiến trúc | Đạt; 10 interface có triển khai, GUI qua BLL, helper mới có trong csproj |
| Pagination.Tests | 19 đạt |
| StudentProfile.Tests | 29 đạt |
| ApplicationLogic.Tests | **172 đạt**, gồm 22 trường hợp mới về procedure |
| Dashboard.Tests | 7 đạt |
| ExcelImport.Tests | 29 đạt |
| Tổng .NET 8 | **256 đạt; 0 thất bại; 0 bỏ qua** |

Kiểm tra mới bao gồm cài lặp đúng 10 procedure, CRUD qua Service/DAL, NULL/ngày/Unicode/dấu nháy, giữ liên kết lớp/tài khoản, chín dạng dữ liệu chặn xóa, mã không tồn tại, validation tại DB, rollback nhiều CALL, thiếu procedure không fallback, quyền đóng lớp và thêm học viên chạy đồng thời với duyệt đơn. Các test sĩ số/đóng lớp cũ tiếp tục đạt sau khi chuyển SQL vào procedure.

Không chạy lại build WinForms net48 hoặc hai suite Mono/Xvfb trong phiên cập nhật này. Chạy GUI và xem/in Crystal trên Windows vẫn cần nghiệm thu. Repository đã có `Reports/BangDiem.rpt` và `BangDiem.cs`; mô tả thiếu mẫu trong tài liệu cũ không phản ánh trạng thái hiện tại.

## Bằng chứng lịch sử trước cập nhật procedure

Mã nguồn thay đổi trên nhánh `feat/complete-coursework-requirements`, nền master `f8d6a5f3dc0aea88229a900939178e31c3bd7006`. Kết quả này thuộc lần chạy sau thay đổi, không dùng số liệu của master để thay thế.

## Đã chạy

Môi trường: Linux, .NET SDK 8.0.427, .NET Framework 4.8 reference assemblies 1.0.3, PostgreSQL 16 trong Docker.

| Target | Kết quả |
|---|---|
| Build BLT_Quan_Ly_Hoc_Tap.csproj | Thành công, tạo WinExe net48 với override resource chỉ dành cho build Linux |
| tests/check_architecture.py | Đạt: GUI qua BLL, 10 hợp đồng có triển khai, source được đưa vào csproj |
| Pagination.Tests | 19 đạt |
| StudentProfile.Tests | 29 đạt |
| ApplicationLogic.Tests | 150 đạt |
| Dashboard.Tests | 7 đạt |
| ExcelImport.Tests | 29 đạt |
| Tổng .NET 8 | **234 đạt**, không thất bại/bỏ qua |
| Pagination.SmokeTests | **89 assertion đạt** bằng Mono/Xvfb |
| StudentRegistration.SmokeTests | **16 assertion đạt** bằng Mono/Xvfb, handler trích từ Form đã chuyển Service |

Sáu test case mới trong ApplicationLogic (Theory có 2 lượt) kiểm tra: script schema/procedure trên DB tách biệt, cài procedure lặp lại, CRUD qua Service/CALL với dữ liệu có dấu nháy/tiếng Việt, lỗi thiếu thủ tục có hướng dẫn, giữ quyền giáo viên và chống nộp lặp khi qua Service, tìm họ tên qua tài khoản/liên kết ngược, hợp đồng dataset báo cáo/XSD tại vi-VN và en-US (0/7.5/NULL và ngày NULL).

Lần test mới đầu tiên có một assertion giả định sai rằng xóa câu hỏi sai chủ trả false; DAL thực tế ném InvalidOperationException. Đã sửa test theo hợp đồng báo lỗi hiện có và chạy lại toàn bộ bộ kiểm thử, không bỏ kiểm tra sai quyền.

## Cách chạy lại

```bash
python3 tests/check_architecture.py
python3 tests/check_profile_deletion_ui.py
dotnet test tests/Pagination.Tests/Pagination.Tests.csproj
python3 tests/StudentProfile.Tests/run.py
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/Dashboard.Tests/Dashboard.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Smoke theo README của hai project, build executable trước khi chạy Mono. Các container kiểm thử dùng một lần; không dùng App.config hoặc DB thật.

## Chưa kiểm chứng / cần bên ngoài

- Chạy tương tác toàn bộ ứng dụng ReaLTaiizor trên Windows, hộp thoại, bố cục Designer và máy thực tế của nhóm.
- SAP Crystal Reports: runtime/designer chưa cài; chưa tạo .rpt và chưa chạy/in report thật. Chỉ dataset và code tích hợp được chuẩn bị; build .NET không xác minh API SAP qua reflection.
- PowerShell Initialize-Postgres.ps1 chưa thực thi trên Windows; các SQL bên trong đã được kiểm thử trực tiếp qua PostgreSQL.
- Bản Word và PowerPoint là bản nháp nội dung, chưa có template của khoa, thông tin sinh viên và ảnh chạy thật.
- Phân công/đánh giá chéo chưa có xác nhận của 5 thành viên.

Không kết luận đạt toàn bộ thang điểm hoặc bảo đảm mức điểm từ các kết quả trên.
