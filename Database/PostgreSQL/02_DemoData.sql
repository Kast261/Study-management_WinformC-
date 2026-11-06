-- Optional local demonstration data. Never replaces existing accounts/passwords.
-- The sample application password is 123456 (stored using the application's SHA-256 format).
BEGIN;
LOCK TABLE TaiKhoan IN SHARE ROW EXCLUSIVE MODE;
DO $seed$
DECLARE
    teacher_id integer;
    student_id integer;
    course_id integer;
    class_id integer;
    account_id integer;
    exam_id integer;
    password_hash text := encode(sha256(convert_to('123456', 'UTF8')), 'hex');
BEGIN
    IF EXISTS (SELECT 1 FROM TaiKhoan) THEN
        RETURN;
    END IF;

    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID)
    VALUES ('admin', password_hash, 'Admin', 0);

    INSERT INTO GiaoVien (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, ChuyenMon, GioiTinh)
    VALUES ('Nguyễn Văn Thầy', 'gv1@example.test', '0901234567', 'Hà Nội', '1988-05-15', 'Công nghệ thông tin', 'Nam')
    RETURNING GiaoVienID INTO teacher_id;
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID)
    VALUES ('giaovien1', password_hash, 'GiaoVien', teacher_id)
    RETURNING TaiKhoanID INTO account_id;
    UPDATE GiaoVien SET TaiKhoanID = account_id WHERE GiaoVienID = teacher_id;

    INSERT INTO KhoaHoc (TenKhoaHoc, MoTa)
    VALUES ('Lập trình C# WinForms', 'Khóa học mẫu để kiểm tra ứng dụng')
    RETURNING KhoaHocID INTO course_id;
    INSERT INTO LopHoc (TenLop, GiaoVienID, KhoaHocID, NgayBatDau, NgayKetThuc, SiSoToiDa, LichHoc, TrangThai)
    VALUES ('Lớp WinForms mẫu', teacher_id, course_id, CURRENT_DATE, CURRENT_DATE + 90, 40,
        'Thứ 2, 4, 6 (18:00 - 20:00)', 'Đang mở')
    RETURNING LopHocID INTO class_id;

    INSERT INTO HocSinh (HoTen, Email, SoDienThoai, DiaChi, NgaySinh, GioiTinh, LopHocID)
    VALUES ('Trần Thị Trò', 'hs1@example.test', '0987654321', 'Hà Nội', '2006-09-18', 'Nữ', class_id)
    RETURNING HocSinhID INTO student_id;
    INSERT INTO TaiKhoan (TenDangNhap, MatKhau, VaiTro, NguoiDungID)
    VALUES ('hocsinh1', password_hash, 'HocSinh', student_id)
    RETURNING TaiKhoanID INTO account_id;
    UPDATE HocSinh SET TaiKhoanID = account_id WHERE HocSinhID = student_id;
    INSERT INTO DangKyLop (LopHocID, HocSinhID, TrangThai)
    VALUES (class_id, student_id, 'Đã duyệt');

    INSERT INTO DeThi (TenDeThi, MoTa, ThoiGian, MonHoc, SoCau, GiaoVienID, LopHocID, HanNop, TrangThai)
    VALUES ('Bài kiểm tra mẫu', 'Dữ liệu thử kết nối và nộp bài', 15, 'Lập trình C# WinForms', 2,
        teacher_id, class_id, CURRENT_TIMESTAMP + INTERVAL '90 days', 'Đang mở')
    RETURNING DeThiID INTO exam_id;
    INSERT INTO CauHoi (DeThiID, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, ThuTu)
    VALUES
        (exam_id, 'Control nào hiển thị dữ liệu dạng bảng?', 'DataGridView', 'Label', 'Button', 'Panel', 'A', 1),
        (exam_id, 'Sự kiện nào thường dùng khi nhấn nút?', 'Load', 'Click', 'Resize', 'Paint', 'B', 2);
END;
$seed$;
COMMIT;
