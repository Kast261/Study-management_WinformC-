# Khôi phục frm_GiaoVien.resx khi Visual Studio báo lỗi XML

Thông báo `Name cannot begin with the '<' character, hexadecimal value 0x3C`
ở dòng 135, cột 2 có thể xuất hiện nếu file còn dòng `<<<<<<< HEAD` do merge
chưa xử lý. File trên nhánh sửa lỗi hợp lệ: đã đọc thành công bằng .NET
`ResXResourceReader` và tạo lại `.resources`, gồm 35 mục metadata của Designer.

Trong Visual Studio, lưu file đang mở rồi vào **View → Terminal**, chọn
**PowerShell**, đứng tại thư mục gốc repository và chạy:

```powershell
git fetch origin fix/preserve-exam-results-after-delete
if ($LASTEXITCODE -ne 0) { throw "Không tải được nhánh sửa lỗi." }
$resxPath = ".\Forms\GiaoVien\frm_GiaoVien.resx"
$resxBackup = $resxPath + ".bak-" + (Get-Date -Format "yyyyMMdd-HHmmss-fff")
Copy-Item -LiteralPath $resxPath -Destination $resxBackup -ErrorAction Stop
git restore --source=FETCH_HEAD -- Forms/GiaoVien/frm_GiaoVien.resx
```

Lệnh tạo bản sao file hiện tại trước khi khôi phục bản hợp lệ. Có thể đối chiếu
bản sao nếu trước đó có chỉnh Designer trên máy. Sau đó chọn **Build → Rebuild
Solution**. Nếu đang giải quyết merge, kiểm tra file và dùng `git add
Forms/GiaoVien/frm_GiaoVien.resx` để đánh dấu file đã xử lý.

Bản sửa chức năng xóa đề cần được merge hoặc checkout từ nhánh trên. Nó giữ
nguyên lịch sử học sinh khi giáo viên xóa/ẩn đề; dữ liệu đã bị xóa bằng phiên
bản cũ cần bản sao lưu database để khôi phục, mã mới không tái tạo được bài làm.
