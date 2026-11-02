# Rà soát và sửa lỗi logic ứng dụng

Nhánh: `fix/application-logic-review`, dựa trên `42c7926`.

## Các lỗi đã sửa

| Luồng | Trước đây | Sau khi sửa |
| --- | --- | --- |
| Đăng nhập | So sánh trực tiếp mật khẩu nhập với hash đã lưu, nên có thể dùng hash để đăng nhập | Chỉ chấp nhận plaintext đối với mật khẩu cũ chưa băm; hash phải được xác minh qua `Passwords.Verify` |
| Liên kết tài khoản | `NguoiDungID=0` không dùng liên kết ngược; danh sách có thể lặp do liên kết ngược cũ | Có fallback khi ID bằng 0, liên kết tường minh được ưu tiên khi hiển thị |
| Tạo tài khoản | Ghi tài khoản và hồ sơ rời nhau, có thể ghi đè liên kết hồ sơ đã có tài khoản | Transaction, khóa hồ sơ và khóa tên đăng nhập không phân biệt hoa/thường; từ chối liên kết trùng/không tồn tại |
| Hồ sơ giáo viên | Lưu từ form cá nhân làm mất ngày sinh/chuyên môn không có trong form | Chỉ ghi các trường được chỉnh; kiểm tra tài khoản sở hữu hồ sơ và trạng thái khóa |
| Đổi mật khẩu giáo viên | Dùng luồng xác thực/ghi mật khẩu tách rời, không kiểm tra trạng thái khóa lúc ghi | Xác minh mật khẩu cũ, vai trò, trạng thái và mật khẩu chưa bị thay đổi; xóa ghi nhớ sau khi đổi |
| Đăng ký lớp | Danh sách đang học chứa đơn chờ duyệt/từ chối; có thể đăng ký trùng, đếm thiếu học viên gán trực tiếp | Chỉ lớp hiện tại/đơn đã duyệt được tính là đang học; đăng ký được tuần tự hóa theo lớp |
| Duyệt/hủy lớp | Duyệt không kiểm tra sĩ số lúc ghi; có thể duyệt sai bộ ID; DAL cho hủy lớp đã duyệt | Transaction, khóa lớp, kiểm tra mã đơn/lớp/học viên/giáo viên và sĩ số; không hủy/từ chối đơn đã duyệt |
| Sĩ số/Dashboard | Lớp đã duyệt có thể mất học viên khỏi thống kê khi lớp chính thay đổi | Tính hợp của lớp gán trực tiếp và đăng ký đã duyệt, không đếm trùng |
| Sửa sĩ số lớp | Có thể giảm sức chứa xuống dưới số học viên đang học | Khóa lớp, kiểm tra sĩ số thật trước khi cập nhật; trả thông báo khi không thể giảm |
| Sửa lớp tại Admin | Dữ liệu lịch học/ngày bị ghi đè NULL vì form không nhập các trường đó | Cập nhật riêng các trường cơ bản, giữ lịch và ngày cũ |
| Vào phòng thi | Chỉ kiểm tra hạn/trạng thái từ bảng đã tải, có thể bị cũ | Kiểm tra lại quyền lớp, trạng thái mở, hạn, câu hỏi và việc đã nộp trong database khi bắt đầu |
| Nộp bài | Kết quả và đáp án ghi riêng, có thể lưu thiếu hoặc nộp hai lần đồng thời | Một transaction, khóa đề, kiểm tra tài khoản/lớp, chấm bằng đáp án trong DB và lưu đủ chi tiết |
| Đồng hồ/đóng phòng thi | Đếm số tick; handler đóng form chưa được nối; lỗi lưu có thể để form đóng hoặc đồng hồ dừng | Dựa vào thời điểm kết thúc, giới hạn bởi hạn nộp; nối FormClosing; lỗi lưu giữ form để thử lại |
| Xem lại sau khi thi | Nút yêu cầu xem lại chưa được xử lý; form xem lại chưa nằm trong csproj | Mở form với mã kết quả vừa lưu, bổ sung form/resource vào project |
| Câu hỏi | Có thể sửa đề đã giao, làm thay đổi đáp án của bài cũ; dùng số lượng+1 gây trùng thứ tự sau khi xóa | Khóa đề, chỉ sửa đề nháp chưa có kết quả; kiểm tra chủ đề, lấy MAX thứ tự+1, cập nhật số câu trong transaction |
| Giao/xóa đề | Có thể chuyển đề sang lớp khác hoặc thay đề đã có kết quả; xóa câu hỏi và đề rời nhau | Chặn giao sang lớp khác/chỉnh đề đã có kết quả; xóa đề và câu hỏi trong transaction |
| Xóa dữ liệu | Schema khởi tạo PostgreSQL không có đủ khóa ngoại, xóa hồ sơ/lớp có thể để dữ liệu mồ côi | Chặn xóa khi còn tài khoản/lớp/đăng ký/đề/kết quả liên quan; giữ lịch sử |
| Nút hủy đăng ký | Gắn Click cả ở Designer và code nên chạy hai lần | Giữ một đăng ký sự kiện ở Designer |
| Xuất bảng điểm | `7,5` trên máy vi-VN có thể bị parse theo invariant thành `75` | Chuyển giá trị số sang invariant trước khi tạo XML; kiểm tra số điểm và trung bình trên XML thực tế |

Không thay đổi thiết kế các control trong Designer của bạn. Không chạy migration hay sửa dữ liệu database thật.

## Kiểm thử tự động

```sh
. /workspace/.cloud-setup/study/env.sh
bash /workspace/.cloud-setup/study/run-checks.sh
python3 tests/StudentProfile.Tests/run.py --project tests/Dashboard.Tests/Dashboard.Tests.csproj
python3 tests/StudentProfile.Tests/run.py --project tests/ApplicationLogic.Tests/ApplicationLogic.Tests.csproj
```

Các bộ .NET 8: Pagination, StudentProfile, ExcelImport, Dashboard, ApplicationLogic.
Smoke test dùng Mono/Xvfb để kiểm tra DataGridView, xuất dữ liệu và XML điểm ở hai văn hóa.
Mỗi bộ tích hợp dùng PostgreSQL riêng. Không kiểm thử bằng cách xóa dữ liệu thật.

## Kiểm tra thực tế trên Windows

1. Rebuild Solution, đăng nhập từng vai trò. Đổi mật khẩu giáo viên/học viên rồi đăng nhập lại.
2. Cập nhật hồ sơ giáo viên và thông tin lớp tại Admin; xác nhận ngày sinh/chuyên môn/lịch/ngày lớp còn nguyên.
3. Đăng ký, duyệt và hủy đơn chờ; thử duyệt lớp đủ sĩ số và hủy lớp đã được duyệt.
4. Tạo đề nháp, thêm/xóa/thêm câu hỏi; giao đề rồi thử sửa/xóa câu hỏi, giao sang lớp khác.
5. Làm bài, chuyển câu, mở hộp thoại xác nhận rồi chờ đồng hồ. Hết giờ phải tự nộp một lần.
6. Trong database thử nghiệm, mô phỏng mất kết nối lúc nộp: form giữ mở, không có kết quả nửa chừng,
   kết nối lại cho phép thử nộp; sau khi hết giờ không sửa đáp án được.
7. Bấm nút đóng phòng thi: No tiếp tục làm, Yes lưu bài; lỗi lưu không đóng mất bài.
8. Xem lại sau khi thi và xuất bảng điểm trên máy định dạng Việt Nam; điểm 7,5 không biến thành 75.

## Giới hạn

- Chưa chạy được toàn bộ giao diện ReaLTaiizor và các hộp thoại trên Windows trong phiên cloud Linux.
- Chưa có lưu nháp/thời điểm bắt đầu thi bền vững trong DB. Nếu tắt tiến trình đột ngột, đáp án chưa nộp
  vẫn mất; cần thiết kế lưu nháp/lần thi nếu muốn khôi phục sau sự cố hoặc kiểm soát thời gian qua khởi động lại.
- Hạn nộp được kiểm tra khi mở phòng thi và giới hạn đồng hồ. Bài đã mở có thể thử ghi lại sau sự cố
  kết nối dù đã hết giờ; DB hiện không có bản ghi lần bắt đầu để phân biệt chính xác một lần thi đang diễn ra.
- Ghi nhớ mật khẩu hiện vẫn dùng cơ chế Base64 cũ, không phải mã hóa bảo mật. Chuyển sang Windows
  Credential Manager/DPAPI và chuyển đổi dữ liệu cũ là hạng mục riêng cần kiểm tra trên Windows.
- Đây là rà soát tĩnh và kiểm thử các luồng nghiệp vụ; các thay đổi đồng thời từ công cụ SQL bên ngoài
  không được bảo vệ bởi khóa trong ứng dụng. Khóa ngoại/ràng buộc DB nên được bổ sung bằng migration riêng.

## Kết quả xác minh của bản sửa

- Build ứng dụng .NET Framework 4.8: thành công.
- Pagination: 19/19; StudentProfile: 29/29; ExcelImport: 27/27.
- Dashboard: 5/5; ApplicationLogic: 24/24. Tổng cộng 104 kiểm thử .NET 8 đạt.
- Mono/Xvfb: 49 assertion DataGridView, xuất dữ liệu và báo cáo điểm đạt.
- Các kiểm tra giao diện Windows ở trên chưa chạy trong phiên Linux này.
