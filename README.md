# Quản lý học tập — C# WinForms / PostgreSQL

Ứng dụng .NET Framework 4.8 với ba vai trò Admin, giáo viên và học viên. Nhánh bổ sung theo thang điểm: `feat/complete-coursework-requirements`.

## Chạy trên Windows

1. Có Visual Studio với .NET desktop development và .NET Framework 4.8 targeting pack.
2. Khôi phục NuGet từ `packages.config`, mở `BLT_Quan_Ly_Hoc_Tap.csproj`.
3. Chuẩn bị PostgreSQL và cấu hình connection string `ChuoiKetNoi` tại máy; không công khai mật khẩu DB.
4. Database mới: chạy `Database/PostgreSQL/01_Schema.sql` rồi `02_Procedures.sql`. Database đã có schema: cài `02_Procedures.sql` trước khi dùng chức năng sửa khóa học. Có thể dùng `scripts/Initialize-Postgres.ps1` với psql.
5. Build và chạy. `Program.Main` không tự seed DB. Tài khoản đầu tiên cần được chuẩn bị trên DB phát triển theo quy trình của nhóm; không tự đưa mật khẩu mẫu lên môi trường sử dụng.

Khôi phục bằng NuGet CLI cần chép native binaries SQLServer.Types theo `docs/Redesigned_Forms.md`. Các tài liệu docs cũ mô tả master trước bổ sung; trạng thái mới theo tài liệu dưới đây.

## Bổ sung và bằng chứng

- [Đối chiếu thang điểm, thay đổi và cài procedure](docs/Hoan_thien_theo_thang_diem.md)
- [Kết quả build, 234 test .NET và 105 assertion smoke](docs/Ket_qua_kiem_thu.md)
- [Phân công 5 người và mẫu đánh giá chéo](docs/Phan_cong_va_danh_gia_cheo.md)
- [Hướng dẫn hoàn tất Crystal Reports](Reports/README.md)
- [Báo cáo 3 chương dạng Markdown](docs/submission/Bao_cao_3_chuong.md)
- [Bản Word nháp](docs/submission/Bao_cao_BTL_nhap.docx)
- [PowerPoint nháp 12 slide](docs/submission/Thuyet_trinh_BTL_nhap.pptx)

**Chưa hoàn tất Crystal:** có code engine/viewer và XSD, nhưng cần cài SAP trên Windows, tạo mẫu `BangDiem.rpt` và chạy nghiệm thu. Word/slide chưa có template của khoa, tên nhóm và ảnh Windows. Phân công/review chỉ là mẫu, cần đóng góp và xác nhận thực tế. Không coi bản bổ sung là đã được giảng viên chấm đạt mọi mục.

## Kiểm thử

```bash
python3 tests/check_architecture.py
dotnet test tests/Pagination.Tests/Pagination.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
```

.NET 8, Python và Docker dùng cho kiểm thử. Runner tạo PostgreSQL riêng và tự dọn, không dùng App.config. Các suite còn lại và smoke test xem `docs/Ket_qua_kiem_thu.md`.

## Tạo lại tài liệu nháp

Có `pandoc`, Graphviz (`dot`) và Python package `python-pptx`, chạy:

```bash
python3 scripts/build_submission.py
```

Nguồn báo cáo là Markdown và nguồn sơ đồ trong script; ảnh sơ đồ được xuất bằng Graphviz, không phải ảnh chụp ứng dụng. Chưa kiểm tra phân trang Word trên Microsoft Word. Cần điền thông tin và áp dụng template trước nộp.
