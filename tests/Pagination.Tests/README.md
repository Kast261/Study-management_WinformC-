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

## Giữ tiêu đề cột đã thiết kế

Khi bảng đã có cột trong Designer, helper tắt `AutoGenerateColumns` trước khi
liên kết dữ liệu, kể cả khi bảng chưa có dòng nào. Nếu để tự sinh cột, WinForms
có thể thay cột đã thiết kế hoặc thêm các cột mang tên trường PostgreSQL như
`hoten`, `dangkyid`, làm mất tiêu đề tiếng Việt hoặc lộ cột ID không cần hiển thị.
Bảng chưa có cột định nghĩa sẵn vẫn được tự sinh cột như trước.

Trong **Edit Columns**, dùng `HeaderText` cho chữ hiển thị (ví dụ `Học Viên`),
`DataPropertyName` cho tên trường dữ liệu (ví dụ `HoTen`). Không cần đổi `(Name)`
của cột hoặc tên trường trong database. Có thể tiếp tục sửa tiêu đề, thứ tự,
độ rộng và định dạng ngày trong Designer.

Smoke test kiểm tra các tiêu đề của bảng duyệt đăng ký với tên trường viết
thường từ PostgreSQL, dữ liệu rỗng, phân trang, sắp xếp, lọc, tải lại và xuất dữ liệu.

Kiểm thử giao diện cũng kiểm tra trường hợp cuộn xuống ở trang trước rồi bấm
trang sau: dữ liệu phải được thay hoàn toàn và vị trí cuộn trở về dòng đầu.
Với 11 dòng và kích thước trang là 10, trang 2 chỉ chứa dòng thứ 11.

## Kiểm thử layout Fill và binding lồng nhau

Helper phân trang cập nhật binding, toolbar, ô chọn và cuộn qua `BeginInvoke` khi grid đã có handle.
Điều này tránh thay đổi layout ngay trong `DataSourceChanged`, `DataBindingComplete` hoặc lúc tab vừa hiện.
Model và dữ liệu xuất được cập nhật trước; các thay đổi hiển thị được gộp ở lượt UI kế tiếp.
`DataGridViewPager.ShowItem` trả về việc tìm thấy item; ô chọn/cuộn cũng được áp dụng ở lượt UI đó.

Smoke test bơm message loop sau thao tác UI để kiểm tra trạng thái đã hoàn tất. Ứng dụng không cần
thêm `Application.DoEvents()`, `Thread.Sleep()` hoặc bắt lỗi toàn cục trong `Program.Main`.

Các tình huống hồi quy gồm kiểm tra độ sâu `DataSourceChanged`, 40 lần chuyển tab đăng ký/bài thi
kèm resize cột Fill và sắp xếp/phân trang, yêu cầu đổi nguồn mới thay thế yêu cầu cũ,
đóng form khi còn cập nhật chờ và tạo lại handle của grid.

Trên Windows, chạy smoke executable và thử chuyển hai tab thật với dữ liệu nhiều hơn một trang.
Nếu vẫn gặp `InvalidOperationException`, cần lấy toàn bộ Call Stack để phân biệt thao tác WinForms
khác với đường cập nhật helper đã được kiểm tra; tên grid hay vị trí `Application.Run` chưa đủ xác định stack đó.
