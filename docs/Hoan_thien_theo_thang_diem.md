# Bổ sung theo thang điểm BTL Lập trình trực quan

## Phạm vi thay đổi

Nhánh `feat/complete-coursework-requirements` xuất phát từ master `f8d6a5f3dc0aea88229a900939178e31c3bd7006`. Bản cập nhật hiện ở nhánh docs/bao-cao-hoan-chinh; không tự merge vào master.

### Interface, class và 3 tầng

Có 10 hợp đồng `I*Service` trong BLL/Interfaces và các lớp triển khai tương ứng. Các Form sử dụng interface-typed Service, không tạo DAL hoặc thực thi SQL. Mọi lời gọi DB từ GUI đi qua BLL rồi DAL. Các validation nhập liệu trên giao diện vẫn giữ; các kiểm tra quyền/trạng thái và transaction ở DAL được giữ nguyên để không phá tính nguyên tử.

Service đã bổ sung chữ ký nhận giáo viên ở duyệt/từ chối/đóng đăng ký và sửa câu hỏi. Mã giáo viên không còn có giá trị mặc định bỏ qua quyền ở các phương thức Service này. Dashboard, import, hồ sơ cá nhân, kiểm tra bắt đầu thi và lấy tên hiển thị đều có luồng qua Service. BLL vẫn có một số hàm chuyển tiếp; đây không phải thiết kế Domain đầy đủ hoặc DI container.

`python3 tests/check_architecture.py` phát hiện GUI bỏ qua BLL, source bị thiếu trong csproj hoặc interface thiếu triển khai. Test DB bổ sung kiểm tra rằng lời gọi qua Service vẫn chặn giáo viên sai quyền và nộp lặp.

### 10 PostgreSQL procedure được ứng dụng gọi

`Database/PostgreSQL/01_Schema.sql` là schema PostgreSQL; `02_Procedures.sql` hiện có **10 procedure**, gồm 9 bổ sung mới. CRUD ghi khóa học, giáo viên, học viên và đóng đăng ký lớp đi qua GUI → Service → DAL → ProcedureExecutor → CALL. SELECT/tìm kiếm, cập nhật hồ sơ cá nhân và những giao dịch khác vẫn có SQL tham số trong DAL.

Danh mục, tham số, ví dụ và quy tắc ở [Procedures_PostgreSQL.md](Procedures_PostgreSQL.md). Thêm học viên giữ khóa lớp/kiểm tra sĩ số; xóa chặn dữ liệu liên quan; cập nhật hồ sơ giữ liên kết; đóng lớp yêu cầu giáo viên phụ trách dương và không đóng đề. Thiếu thủ tục báo script cần cài, không fallback SQL. Script không COMMIT nội bộ; đã kiểm tra rollback nhiều CALL.

**Trước khi chạy bản mới**, trên database thử nghiệm/phát triển phù hợp, chạy `02_Procedures.sql` bằng pgAdmin. Database mới chạy cả 01 rồi 02. Hoặc từ PowerShell có psql:

```powershell
./scripts/Initialize-Postgres.ps1 -Database study_management -UserName postgres
```

Script không tạo database, không xóa dữ liệu và không seed mật khẩu. PostgreSQL tự yêu cầu thông tin xác thực theo cấu hình máy. App.config vẫn dùng `ChuoiKetNoi`. Không tự cài thủ tục hoặc sửa DB thật bằng tài khoản đang có trong App.config từ cloud.

Trên DB cũ, kiểm tra schema/backup theo quy trình của nhóm. `IF NOT EXISTS` không phải migration cho mọi khác biệt. Các script SQL Server ở Database/Scripts chỉ còn là tài liệu lịch sử và không được dùng thay script PostgreSQL.

### Crystal Reports: tích hợp đã chuẩn bị, chưa hoàn tất nghiệm thu

Nút báo cáo kết quả của giáo viên/học viên mở hộp thoại có Excel và Crystal. Đường Excel được giữ. Đường Crystal tìm SAP engine/viewer tại Windows, load `Reports/BangDiem.rpt` (hoặc mẫu do người dùng chọn), nhận DataSet `StudyReports/BangDiem` từ BaoCaoService, hiển thị và in qua viewer, dispose viewer/report/data.

Đã có `Reports/BangDiem.xsd` và kiểm thử dữ liệu tiếng Việt, 7.5, 0, NULL, ngày NULL ở vi-VN/en-US. Không gắn thông tin đăng nhập DB vào report: dùng push dataset từ kết quả đã truy vấn.

**Repository đã có `Reports/BangDiem.rpt` và `BangDiem.cs`. Chưa có bằng chứng nghiệm thu nạp dữ liệu/xem/in trên Windows với SAP runtime phù hợp.** Không dùng Excel thay rồi ghi là Crystal đã hoàn tất; các câu cũ nói chưa có .rpt được đính chính trong bản cập nhật tài liệu.

### Hồ sơ nhóm, Word và PowerPoint

Phân công/review mẫu ở `docs/Phan_cong_va_danh_gia_cheo.md`; tên, kết quả review và đóng góp phải do nhóm xác nhận thực tế. Đã chuẩn bị nội dung báo cáo 3 chương và slide nháp trong docs/submission. Báo cáo theo mẫu và hướng dẫn thuyết trình đã có ở docs/bao_cao_hoan_chinh; cần thông tin nhóm, ảnh Windows và xác nhận đóng góp thực tế trước khi nộp.

## Checklist đối chiếu

| Mục gốc | Trạng thái sau thay đổi |
|---|---|
| 1 — 5 interface | Có 10 interface ứng dụng và lớp triển khai, được dùng trong GUI |
| 2 — class/kế thừa/kết tập | Có Service triển khai hợp đồng; Form kế thừa, Service chứa DAL |
| 3 — hoàn thiện class | Build app và chạy các kiểm thử được ghi trong Ket_qua_kiem_thu.md; UI Windows chưa nghiệm thu |
| 4 — CRUD ADO.NET | CRUD ghi khóa học/GV/HV và đóng đăng ký dùng 10 CALL thật |
| 5 — Crystal | Có .rpt, code tích hợp/XSD; **còn kiểm tra SAP/Windows** |
| 6 — phân công/đánh giá chéo | Có kế hoạch/mẫu; **còn thành viên làm và ghi nhận thực tế** |
| 7 — thủ tục | Có 10 procedure và 22 test mới; tổng 256 test đạt; cần cài lại script trên DB chạy app |
| 10 — SQL trực tiếp | Giữ các truy vấn tham số ở DAL; không cộng riêng điểm nếu rubric coi là phương án thay thế |
| 11 — 3 tầng | GUI → Service → DAL, kiểm tra kiến trúc tự động |
| 13 — Word | Có báo cáo theo mẫu .docx/PDF, **cần thông tin và ảnh chạy của nhóm** |
| 14 — PowerPoint | Có slide nháp, **cần tên và ảnh demo/bằng chứng nhóm** |

Thang điểm không có STT 8–9; mục 12 trống. Tài liệu này không tự cấp điểm hoặc xác nhận đóng góp của sinh viên.
