## Kiểm thử phân trang

Kiểm tra logic phân trang độc lập với WinForms bằng .NET SDK 8:

```sh
dotnet test tests/Pagination.Tests/Pagination.Tests.csproj
```

Kiểm tra liên kết DataGridView, nút điều hướng, ComboBox, lọc/sắp xếp, bố cục
TableLayoutPanel, tab ẩn và xuất đầy đủ CSV/Excel bằng .NET Framework 4.8:

```sh
msbuild tests/Pagination.SmokeTests/Pagination.SmokeTests.csproj
tests\Pagination.SmokeTests\bin\Debug\Pagination.SmokeTests.exe
```

Trên Linux, có thể build với MSBuild và net48 reference assemblies rồi chạy
trong Mono/Xvfb bằng Dockerfile ở `tests/Pagination.SmokeTests`:

```sh
docker build -t study-pagination-smoke tests/Pagination.SmokeTests
docker run --rm --mount type=bind,src="$(pwd)/tests/Pagination.SmokeTests",dst=/tests,readonly study-pagination-smoke
```

Các bài kiểm thử này không cần kết nối cơ sở dữ liệu. Phân trang áp dụng trên
DataTable/DataView/danh sách đã tải, mặc định 20 dòng/trang. Kết quả xuất Excel
và CSV vẫn bao gồm toàn bộ dữ liệu sau lọc và sắp xếp. Bố cục ReaLTaiizor và
Windows Forms Designer vẫn cần được kiểm tra trực tiếp trong Visual Studio.
