# Cài PostgreSQL cho ứng dụng WinForms

Ảnh lỗi `28P01: password authentication failed for user "study_app"` có nghĩa
PostgreSQL từ chối tên đăng nhập hoặc mật khẩu database. Tạo thêm bảng không sửa
được lỗi này: mật khẩu của role PostgreSQL phải trùng mật khẩu trong cấu hình.
Tài khoản PostgreSQL `study_app` khác tài khoản đăng nhập ứng dụng `admin`.

Các script tại đây dành cho PostgreSQL 16/18, có đủ 10 bảng theo DAL hiện tại.
Các file trong `Database/Scripts` là SQL Server cũ, không chạy bằng pgAdmin.
Đây là thiết lập local; không đọc, sửa hoặc sao chép dữ liệu từ Supabase.

## Cách nhanh trên máy Windows

1. Cài PostgreSQL Server, pgAdmin và Command Line Tools. Giữ cổng 5432.
2. Tải nhánh có bộ script này về máy và giải nén. Đóng ứng dụng WinForms.
3. Mở PowerShell tại thư mục có `App.config`, chạy:

   ```powershell
   powershell -NoProfile -ExecutionPolicy RemoteSigned -File .\Database\PostgreSQL\Setup-LocalDatabase.ps1
   ```

   Nếu Windows chặn file tải về, đọc script rồi dùng `Unblock-File` cho chính
   file `Setup-LocalDatabase.ps1` và chạy lại. Không cần tắt policy toàn máy.

4. Nhập mật khẩu tài khoản PostgreSQL **postgres** đã đặt khi cài đặt.
5. Nhập mật khẩu muốn đặt cho tài khoản PostgreSQL **study_app**. Script sẽ tạo
   role này nếu chưa có, hoặc đặt lại mật khẩu role hiện có theo giá trị vừa nhập.
   Không đưa mật khẩu vào câu lệnh hoặc gửi mật khẩu lên chat.
6. Đợi `SUCCESS`. Script tạo database `btl_ql_hoc_tap` do `study_app` sở hữu,
   tạo bảng, thêm dữ liệu mẫu cho database chưa có tài khoản, kiểm tra kết nối
   bằng chính `study_app`, rồi cập nhật `ChuoiKetNoi` trong `App.config`.
7. Mở Visual Studio, **Rebuild Solution**, rồi F5. Giữ
   `//CreateDataBase.Init();` trong `Program.cs`: các bảng đã được tạo bởi script.

Script tự tìm `psql.exe` trong `C:\Program Files\PostgreSQL\<version>\bin`.
Nếu cài ở nơi khác:

```powershell
.\Database\PostgreSQL\Setup-LocalDatabase.ps1 -PsqlPath 'D:\PostgreSQL\18\bin\psql.exe'
```

Dùng `-Port 5433` nếu server dùng cổng khác. Dùng `-SkipDemoData` nếu chỉ muốn
schema trống; khi đó cần tự tạo tài khoản ứng dụng trước khi đăng nhập.

Chỉ cấu hình kết nối được cập nhật; các phần `startup`, `runtime` và binding
redirect được giữ nguyên. Cấu hình cũ được sao lưu cạnh file đích với đuôi `.user`
(Git bỏ qua). Bản sao có thể chứa thông tin kết nối riêng: không chia sẻ/commit nó.
Mật khẩu mới chỉ được lưu trong cấu hình local, không nằm trong bộ script.
Các ký tự `&`, dấu nháy hoặc `;` trong mật khẩu được xử lý bằng XML và connection
string builder, không cần tự escape.

Để cấu hình bản đã build, truyền đường dẫn file đang dùng và mở lại app:

```powershell
.\Database\PostgreSQL\Setup-LocalDatabase.ps1 -ConfigPath .\bin\Release\BLT_Quan_Ly_Hoc_Tap.exe.config
```

Chạy lại script không xóa database/bảng/dữ liệu, không đổi mật khẩu tài khoản
**ứng dụng** đã có. Mỗi lần chạy vẫn đặt lại mật khẩu **PostgreSQL study_app**
theo giá trị được nhập và cập nhật file cấu hình đã chọn. Những bản app khác dùng
role này cũng cần cập nhật mật khẩu. Script dừng nếu database đã có owner khác;
không tự chuyển quyền hay chạy migration đối với database cũ không rõ cấu trúc.
Nếu thất bại giữa chừng, role/database vừa tạo có thể vẫn còn nhưng cấu hình
chưa được lưu; sửa nguyên nhân và chạy lại, không cần xóa dữ liệu.

## Tài khoản ứng dụng mẫu

Chỉ tạo khi bảng `TaiKhoan` chưa có tài khoản nào:

| Tài khoản | Mật khẩu mẫu | Vai trò |
|---|---|---|
| admin | 123456 | Admin |
| giaovien1 | 123456 | Giáo viên |
| hocsinh1 | 123456 | Học viên |

Mật khẩu được lưu dạng SHA-256 tương thích với `Passwords.Hash` của dự án.
Có một khóa học, lớp, giáo viên, học viên, đăng ký đã duyệt và đề thi 2 câu để
thử. Ngày lớp/hạn đề dựa theo ngày khởi tạo. Đổi mật khẩu mẫu trước khi dùng thật.

## Làm từng bước bằng pgAdmin

1. Kết nối server `localhost:5432` bằng user **postgres**.
2. **Login/Group Roles → study_app → Properties → Definition → Password**:
   đặt lại mật khẩu. Trong **Privileges**, bật **Can login**, giữ **Superuser = No**.
   Nếu role chưa có: **Login/Group Roles → Create → Login/Group Role**, Name `study_app`.
3. **Databases → Create → Database**: Name `btl_ql_hoc_tap`, Owner `study_app`.
   Nếu database đã có thì giữ nguyên dữ liệu và kiểm tra quyền của owner/bảng.
4. Đăng ký thêm kết nối pgAdmin bằng user `study_app`, mật khẩu vừa đặt,
   Maintenance database `btl_ql_hoc_tap`. Kết nối thành công mới làm tiếp.
5. Chọn database đó → **Query Tool → Open File** → mở `01_Schema.sql` → **F5**.
6. Mở `02_DemoData.sql` → **F5** nếu cần dữ liệu mẫu. Không sửa các script này
   để thêm dấu nháy kép quanh tên bảng/cột; DAL dùng tên PostgreSQL viết thường.
7. Kiểm tra trong Query Tool:

   ```sql
   SELECT current_database(), current_user;
   SELECT tablename FROM pg_tables WHERE schemaname = 'public' ORDER BY tablename;
   SELECT TenDangNhap, VaiTro FROM TaiKhoan;
   ```

8. Đặt cùng mật khẩu `study_app` trong `App.config`, giữ các phần khác:

   ```xml
   <connectionStrings>
     <add name="ChuoiKetNoi" providerName="Npgsql"
          connectionString="Host=localhost;Port=5432;Database=btl_ql_hoc_tap;Username=study_app;Password=MAT_KHAU_VUA_DAT;" />
   </connectionStrings>
   ```

   Nếu sửa bằng tay, escape ký tự XML (`&` thành `&amp;`). Mật khẩu chứa `;`
   hoặc dấu nháy nên dùng script thiết lập để connection string được tạo đúng.
9. Rebuild và chạy lại. Không cần bật `CreateDataBase.Init()` khi đã chạy SQL.

## Kiểm thử tự động

```sh
python3 tests/StudentProfile.Tests/run.py --project tests/DatabaseSetup.Tests/DatabaseSetup.Tests.csproj
```

Cần .NET 8, Python 3 và Docker. Runner dùng PostgreSQL tạm, không đọc `App.config`
và không truy cập database thật. Các kiểm thử chạy chính helper/DAL/schema/seed
của ứng dụng bằng role không có quyền quản trị, kiểm tra đăng nhập, Dashboard,
nộp bài, chạy lại không mất dữ liệu và các lỗi xác thực/database chưa tồn tại.
