# Kết quả kiểm tra bản bổ sung

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
