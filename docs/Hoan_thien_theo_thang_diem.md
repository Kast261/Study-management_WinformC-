# Bổ sung theo thang điểm BTL Lập trình trực quan

## Phạm vi thay đổi

Nhánh `feat/complete-coursework-requirements` xuất phát từ master `f8d6a5f3dc0aea88229a900939178e31c3bd7006`. Không merge vào master tự động.

### Interface, class và 3 tầng

Có 10 hợp đồng `I*Service` trong BLL/Interfaces và các lớp triển khai tương ứng. Các Form sử dụng interface-typed Service, không tạo DAL hoặc thực thi SQL. Mọi lời gọi DB từ GUI đi qua BLL rồi DAL. Các validation nhập liệu trên giao diện vẫn giữ; các kiểm tra quyền/trạng thái và transaction ở DAL được giữ nguyên để không phá tính nguyên tử.

Service đã bổ sung chữ ký nhận giáo viên ở duyệt/từ chối/đóng đăng ký và sửa câu hỏi. Mã giáo viên không còn có giá trị mặc định bỏ qua quyền ở các phương thức Service này. Dashboard, import, hồ sơ cá nhân, kiểm tra bắt đầu thi và lấy tên hiển thị đều có luồng qua Service. BLL vẫn có một số hàm chuyển tiếp; đây không phải thiết kế Domain đầy đủ hoặc DI container.

`python3 tests/check_architecture.py` phát hiện GUI bỏ qua BLL, source bị thiếu trong csproj hoặc interface thiếu triển khai. Test DB bổ sung kiểm tra rằng lời gọi qua Service vẫn chặn giáo viên sai quyền và nộp lặp.

### PostgreSQL procedure được ứng dụng gọi

`Database/PostgreSQL/01_Schema.sql` lấy schema PostgreSQL từ lớp khởi tạo, không sử dụng cú pháp SQL Server cũ. `02_Procedures.sql` tạo `study_sua_khoa_hoc` (PL/pgSQL), có validation và trả INOUT boolean báo số dòng cập nhật.

Tại Admin → Khóa học → Sửa, GUI gọi KhoaHocService.Sua → KhoaHocDAL.Sua → `CALL study_sua_khoa_hoc(@id, @ten, @mota, FALSE)`. Dùng CALL rõ ràng vì Npgsql 6 xử lý CommandType.StoredProcedure theo cách gọi function. Không có fallback UPDATE trực tiếp để giả vờ đã dùng thủ tục. Thiếu procedure được thông báo kèm tên script cần cài.

**Trước khi chạy bản mới**, trên database thử nghiệm/phát triển phù hợp, chạy `02_Procedures.sql` bằng pgAdmin. Database mới chạy cả 01 rồi 02. Hoặc từ PowerShell có psql:

```powershell
./scripts/Initialize-Postgres.ps1 -Database study_management -UserName postgres
```

Script không tạo database, không xóa dữ liệu và không seed mật khẩu. PostgreSQL tự yêu cầu thông tin xác thực theo cấu hình máy. App.config vẫn dùng `ChuoiKetNoi`. Không tự cài thủ tục hoặc sửa DB thật bằng tài khoản đang có trong App.config từ cloud.

Trên DB cũ, kiểm tra schema/backup theo quy trình của nhóm. `IF NOT EXISTS` không phải migration cho mọi khác biệt. Các script SQL Server ở Database/Scripts chỉ còn là tài liệu lịch sử và không được dùng thay script PostgreSQL.

### Crystal Reports: tích hợp đã chuẩn bị, chưa hoàn tất nghiệm thu

Nút báo cáo kết quả của giáo viên/học viên mở hộp thoại có Excel và Crystal. Đường Excel được giữ. Đường Crystal tìm SAP engine/viewer tại Windows, load `Reports/BangDiem.rpt` (hoặc mẫu do người dùng chọn), nhận DataSet `StudyReports/BangDiem` từ BaoCaoService, hiển thị và in qua viewer, dispose viewer/report/data.

Đã có `Reports/BangDiem.xsd` và kiểm thử dữ liệu tiếng Việt, 7.5, 0, NULL, ngày NULL ở vi-VN/en-US. Không gắn thông tin đăng nhập DB vào report: dùng push dataset từ kết quả đã truy vấn.

**Nhóm xác nhận hiện có Visual Studio nhưng chưa cài Crystal Reports. Môi trường đang dùng là Linux, không có SAP designer/runtime và chưa có mẫu .rpt. Vì vậy chưa thể đánh dấu mục Crystal hoàn thành.** Làm tiếp theo Reports/README.md trên Windows, tạo mẫu thật và kiểm tra viewer/in. Không dùng ReportViewer/Excel thay rồi ghi là Crystal.

### Hồ sơ nhóm, Word và PowerPoint

Phân công/review mẫu ở `docs/Phan_cong_va_danh_gia_cheo.md`; tên, kết quả review và đóng góp phải do nhóm xác nhận thực tế. Đã chuẩn bị nội dung báo cáo 3 chương và slide nháp trong docs/submission. Chưa có template của trường và ảnh Windows nên không gọi đây là bản nộp đã hoàn thiện.

## Checklist đối chiếu

| Mục gốc | Trạng thái sau thay đổi |
|---|---|
| 1 — 5 interface | Có 10 interface ứng dụng và lớp triển khai, được dùng trong GUI |
| 2 — class/kế thừa/kết tập | Có Service triển khai hợp đồng; Form kế thừa, Service chứa DAL |
| 3 — hoàn thiện class | Build app và chạy các kiểm thử được ghi trong Ket_qua_kiem_thu.md; UI Windows chưa nghiệm thu |
| 4 — CRUD ADO.NET | Giữ CRUD Npgsql, sửa khóa học có CALL thủ tục thật |
| 5 — Crystal | Có code tích hợp/XSD; **còn cài SAP, tạo .rpt và kiểm tra Windows** |
| 6 — phân công/đánh giá chéo | Có kế hoạch/mẫu; **còn thành viên làm và ghi nhận thực tế** |
| 7 — thủ tục | Có procedure và test gọi qua Service/DAL; cần cài script trên DB chạy app |
| 10 — SQL trực tiếp | Giữ các truy vấn tham số ở DAL; không cộng riêng điểm nếu rubric coi là phương án thay thế |
| 11 — 3 tầng | GUI → Service → DAL, kiểm tra kiến trúc tự động |
| 13 — Word | Có bản nháp .docx, **cần template và thông tin/ảnh của nhóm** |
| 14 — PowerPoint | Có slide nháp, **cần tên và ảnh demo/bằng chứng nhóm** |

Thang điểm không có STT 8–9; mục 12 trống. Tài liệu này không tự cấp điểm hoặc xác nhận đóng góp của sinh viên.
