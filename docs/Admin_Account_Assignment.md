# Gán hồ sơ khi Admin tạo tài khoản

Trong tab **Tài khoản**, chọn **Vai trò** trước rồi chọn người ở **Gán cho**:

- **GiaoVien**: các hồ sơ giáo viên chưa có tài khoản.
- **HocSinh**: các hồ sơ học viên chưa có tài khoản.
- **Admin**: không cần hồ sơ giáo viên/học viên, ô Gán cho được ẩn.

Ô Gán cho hiển thị họ tên và lưu mã hồ sơ tương ứng. Hồ sơ đã liên kết với
tài khoản, kể cả tài khoản đang khóa, không xuất hiện để tránh gán trùng.
Nếu hiện **Không còn ... chưa có tài khoản**, tạo hồ sơ trong tab **Giáo viên**
hoặc **Học viên** rồi trở lại tab **Tài khoản**; danh sách sẽ được tải mới.

Khi tạo thành công, tài khoản được liên kết đúng người và hồ sơ đó được gỡ
khỏi lựa chọn tiếp theo. Đổi vai trò sẽ xóa lựa chọn người cũ. Màn hình yêu cầu
chọn một hồ sơ cụ thể trước khi tạo tài khoản giáo viên/học viên.

Kiểm thử PostgreSQL trong `AccountAssignmentTests` gồm liên kết xuôi/ngược,
vai trò học viên cũ, hồ sơ đang có tài khoản khóa, mã trùng giữa hai vai trò,
tạo tài khoản rồi đăng nhập và hai lần tạo đồng thời cho cùng một hồ sơ.
