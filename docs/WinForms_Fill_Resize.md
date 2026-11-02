# Lỗi DataGridView khi cột Fill đang resize

Hai bảng được báo lỗi: `dgvDangKyChoDuyet` của Giáo viên và `dgvHV_DanhSachDeThi` của Học viên.
Lỗi có thể nổi lên tại `Application.Run(new frm_DangNhap())` vì ngoại lệ phát sinh trong message loop,
không phải do dòng mở form đăng nhập.

## Đường cập nhật đã sửa

Helper cũ gọi lại `grid.DataSource = rows` ngay từ `DataSourceChanged` của nguồn vừa gán,
đồng thời chọn/cuộn bảng trong các sự kiện binding/hiển thị. WinForms có thể còn đang tự chia
độ rộng các cột Fill; thay đổi layout lồng nhau ở thời điểm đó không an toàn.

- Cập nhật model đồng bộ, nhưng gộp binding/toolbar/selection/scroll qua một `BeginInvoke` khi đã có handle.
- Không cuộn trực tiếp trong `VisibleChanged` hoặc `DataBindingComplete`.
- Chờ grid hiện và đủ chỗ hiển thị hàng trước khi cuộn.
- Nguồn dữ liệu/yêu cầu mới hủy vị trí chọn của nguồn cũ.
- Bỏ qua callback thuộc handle đã hủy; xử lý lại yêu cầu khi handle được tạo lại; Dispose hủy đăng ký an toàn.
- Form Giáo viên chỉ dùng `ShowItem` để chọn/cuộn; metadata của đề lấy từ DataTable gốc thay vì đọc grid đang chờ bind.
- Giữ nguyên chế độ Fill và các file Designer. Không sửa Program.cs và không thêm catch để che ngoại lệ layout.

## Bằng chứng và giới hạn

Smoke test dùng một DataGridView thật có ghi nhận độ sâu của `OnDataSourceChanged`.
Chạy chính kiểm thử đó với helper trước sửa thất bại vì DataSourceChanged bị lồng.
Bản sửa vượt qua đối chứng cùng các kịch bản đổi tab/resize Fill/sắp xếp/phân trang, đóng form và tạo lại handle.

Các kiểm tra tự động chạy bằng Mono/Xvfb trên Linux. Chúng xác minh lỗi reentrancy và hành vi phân trang,
chưa tái hiện chính xác stack native của ngoại lệ hiếm trên máy Windows được báo. Cần thử lại hai tab
trên Windows; nếu còn lỗi, lấy Debug → Windows → Call Stack khi dừng, thay vì chỉ tên hai grid.

## Thử trên Windows

1. Pull bản đã merge, Rebuild Solution.
2. Đăng nhập Giáo viên; chuyển qua lại tab duyệt đăng ký, lớp và đề thi, chọn lớp/đề liên tục.
3. Đăng nhập Học viên; chuyển qua lại Bài thi, Kết quả và Lớp học; lọc lớp/môn và chuyển trang.
4. Thu nhỏ/phóng to cửa sổ, thử bảng có nhiều hơn 20 dòng và chọn đề ở trang khác.
5. Đóng form trong lúc vừa chuyển tab. Không có ngoại lệ từ callback của grid đã dispose.

Không cần thay đổi `AutoSizeColumnsMode.Fill` thành None để tránh lỗi.
