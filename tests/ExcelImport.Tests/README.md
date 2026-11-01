Chạy kiểm thử đọc Excel và nhập hàng loạt với .NET SDK 8, Python 3 và Docker:

```sh
python tests/StudentProfile.Tests/run.py --project tests/ExcelImport.Tests/ExcelImport.Tests.csproj
```

Runner dùng PostgreSQL riêng và xóa container/volume sau khi chạy. Không dùng
kết nối database từ App.config của ứng dụng. Các kiểm thử biên dịch trực tiếp
bộ đọc Excel và DAL của ứng dụng, đọc file `.xlsx`/`.xls` thật, kiểm tra rollback,
quyền giáo viên, đề đã giao/có bài làm, thứ tự và số câu sau khi nhập đồng thời.

Để chỉ chạy phần đọc file, không cần database:

```sh
dotnet test tests/ExcelImport.Tests/ExcelImport.Tests.csproj --filter FullyQualifiedName~ExcelReaderTests
```

File mẫu của ứng dụng ở `Templates/CauHoi_Mau.xlsx`. Các fixture có thể được
tạo lại bằng `generate_fixtures.py` khi có openpyxl và xlwt; đây chỉ là công cụ
phát triển, không phải dependency của ứng dụng.

Giao diện hộp chọn file và thao tác nút cần kiểm tra trên Windows/Visual Studio.
