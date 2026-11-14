-- PostgreSQL 11+. Run after 01_Schema.sql, in the application's database/schema.
-- Rerunnable; no data deletion or default password. No COMMIT inside the procedure.
CREATE OR REPLACE PROCEDURE study_sua_khoa_hoc(
    IN p_id integer,
    IN p_ten varchar,
    IN p_mota varchar,
    INOUT p_updated boolean
)
LANGUAGE plpgsql
AS $$
DECLARE
    affected integer;
BEGIN
    IF p_id IS NULL OR p_id <= 0 THEN
        RAISE EXCEPTION 'Mã khóa học không hợp lệ' USING ERRCODE = '22023';
    END IF;
    IF p_ten IS NULL OR length(btrim(p_ten)) = 0 OR length(p_ten) > 100 THEN
        RAISE EXCEPTION 'Tên khóa học phải có từ 1 đến 100 ký tự' USING ERRCODE = '22023';
    END IF;
    IF length(p_mota) > 255 THEN
        RAISE EXCEPTION 'Mô tả không được quá 255 ký tự' USING ERRCODE = '22023';
    END IF;
    UPDATE KhoaHoc SET TenKhoaHoc = btrim(p_ten), MoTa = p_mota
    WHERE KhoaHocID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_updated := affected = 1;
END;
$$;

-- All ten procedures run in the caller's transaction. No COMMIT/ROLLBACK here.
-- Output IDs are zero and output flags are false when no row is written.
CREATE OR REPLACE PROCEDURE study_them_khoa_hoc(
    IN p_ten varchar, IN p_mota varchar, INOUT p_id integer)
LANGUAGE plpgsql AS $$
BEGIN
    p_id := 0;
    IF p_ten IS NULL OR length(btrim(p_ten)) = 0 OR length(p_ten) > 100
       OR length(p_mota) > 255 THEN
        RAISE EXCEPTION 'Tên khóa học phải có từ 1 đến 100 ký tự; mô tả tối đa 255 ký tự'
            USING ERRCODE = '22023';
    END IF;
    INSERT INTO KhoaHoc (TenKhoaHoc, MoTa) VALUES (btrim(p_ten), p_mota)
        RETURNING KhoaHocID INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE study_xoa_khoa_hoc(
    IN p_id integer, INOUT p_deleted boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_deleted := FALSE;
    PERFORM 1 FROM KhoaHoc WHERE KhoaHocID = p_id FOR UPDATE;
    IF NOT FOUND THEN RETURN; END IF;
    IF EXISTS (SELECT 1 FROM LopHoc WHERE KhoaHocID = p_id) THEN
        RAISE EXCEPTION 'Không thể xóa khóa học này vì đang có lớp học thuộc khóa học!'
            USING ERRCODE = '23503';
    END IF;
    DELETE FROM KhoaHoc WHERE KhoaHocID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_deleted := affected = 1;
END;
$$;

CREATE OR REPLACE PROCEDURE study_them_giao_vien(
    IN p_hoten varchar, IN p_ngaysinh date, IN p_gioitinh varchar,
    IN p_chuyenmon varchar, IN p_email varchar, IN p_sdt varchar,
    IN p_diachi varchar, INOUT p_id integer)
LANGUAGE plpgsql AS $$
BEGIN
    p_id := 0;
    IF p_hoten IS NULL OR length(btrim(p_hoten)) = 0 OR length(p_hoten) > 100
       OR length(p_gioitinh) > 10 OR length(p_chuyenmon) > 100
       OR length(p_email) > 100 OR length(p_sdt) > 20 OR length(p_diachi) > 255 THEN
        RAISE EXCEPTION 'Thông tin giáo viên trống hoặc vượt độ dài cho phép'
            USING ERRCODE = '22023';
    END IF;
    INSERT INTO GiaoVien (HoTen, NgaySinh, GioiTinh, ChuyenMon, Email, SoDienThoai, DiaChi)
        VALUES (btrim(p_hoten), p_ngaysinh, p_gioitinh, p_chuyenmon, p_email, p_sdt, p_diachi)
        RETURNING GiaoVienID INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE study_sua_giao_vien(
    IN p_id integer, IN p_hoten varchar, IN p_ngaysinh date, IN p_gioitinh varchar,
    IN p_chuyenmon varchar, IN p_email varchar, IN p_sdt varchar,
    IN p_diachi varchar, INOUT p_updated boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_updated := FALSE;
    IF p_id IS NULL OR p_id <= 0 OR p_hoten IS NULL
       OR length(btrim(p_hoten)) = 0 OR length(p_hoten) > 100
       OR length(p_gioitinh) > 10 OR length(p_chuyenmon) > 100
       OR length(p_email) > 100 OR length(p_sdt) > 20 OR length(p_diachi) > 255 THEN
        RAISE EXCEPTION 'Mã hoặc thông tin giáo viên không hợp lệ'
            USING ERRCODE = '22023';
    END IF;
    UPDATE GiaoVien SET HoTen = btrim(p_hoten), NgaySinh = p_ngaysinh,
        GioiTinh = p_gioitinh, ChuyenMon = p_chuyenmon, Email = p_email,
        SoDienThoai = p_sdt, DiaChi = p_diachi WHERE GiaoVienID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_updated := affected = 1;
END;
$$;

CREATE OR REPLACE PROCEDURE study_xoa_giao_vien(
    IN p_id integer, INOUT p_deleted boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_deleted := FALSE;
    PERFORM 1 FROM GiaoVien WHERE GiaoVienID = p_id FOR UPDATE;
    IF NOT FOUND THEN RETURN; END IF;
    IF EXISTS (SELECT 1 FROM LopHoc WHERE GiaoVienID = p_id)
       OR EXISTS (SELECT 1 FROM DeThi WHERE GiaoVienID = p_id)
       OR EXISTS (SELECT 1 FROM TaiKhoan tk JOIN GiaoVien gv ON gv.GiaoVienID = p_id
            WHERE (lower(tk.VaiTro) = 'giaovien' AND tk.NguoiDungID = p_id)
               OR tk.TaiKhoanID = gv.TaiKhoanID) THEN
        RAISE EXCEPTION 'Không thể xóa hồ sơ đã liên kết tài khoản hoặc có dữ liệu học tập. Hãy khóa tài khoản để giữ lịch sử.'
            USING ERRCODE = '23503';
    END IF;
    DELETE FROM GiaoVien WHERE GiaoVienID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_deleted := affected = 1;
END;
$$;

CREATE OR REPLACE PROCEDURE study_them_hoc_sinh(
    IN p_hoten varchar, IN p_ngaysinh date, IN p_gioitinh varchar,
    IN p_email varchar, IN p_sdt varchar, IN p_diachi varchar,
    IN p_lop integer, INOUT p_id integer)
LANGUAGE plpgsql AS $$
DECLARE capacity integer; current_count bigint;
BEGIN
    p_id := 0;
    IF p_lop < 0 THEN RETURN; END IF;
    IF p_hoten IS NULL OR length(btrim(p_hoten)) = 0 OR length(p_hoten) > 100
       OR length(p_gioitinh) > 10 OR length(p_email) > 100
       OR length(p_sdt) > 20 OR length(p_diachi) > 255 THEN
        RAISE EXCEPTION 'Thông tin học viên trống hoặc vượt độ dài cho phép'
            USING ERRCODE = '22023';
    END IF;
    IF COALESCE(p_lop, 0) > 0 THEN
        -- Same row lock and membership count as registration/approval in LopHocDAL.
        SELECT COALESCE(SiSoToiDa, 40) INTO capacity FROM LopHoc
            WHERE LopHocID = p_lop AND COALESCE(TrangThai, 'Đang mở') = 'Đang mở'
            FOR UPDATE;
        IF NOT FOUND THEN RETURN; END IF;
        SELECT COUNT(*) INTO current_count FROM HocSinh h
            WHERE h.LopHocID = p_lop OR EXISTS (
                SELECT 1 FROM DangKyLop dk WHERE dk.HocSinhID = h.HocSinhID
                    AND dk.LopHocID = p_lop AND dk.TrangThai = 'Đã duyệt');
        IF current_count >= capacity THEN RETURN; END IF;
    END IF;
    INSERT INTO HocSinh (HoTen, NgaySinh, GioiTinh, Email, SoDienThoai, DiaChi, LopHocID)
        VALUES (btrim(p_hoten), p_ngaysinh, p_gioitinh, p_email, p_sdt, p_diachi,
            NULLIF(p_lop, 0)) RETURNING HocSinhID INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE study_sua_hoc_sinh(
    IN p_id integer, IN p_hoten varchar, IN p_ngaysinh date, IN p_gioitinh varchar,
    IN p_email varchar, IN p_sdt varchar, IN p_diachi varchar,
    INOUT p_updated boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_updated := FALSE;
    IF p_id IS NULL OR p_id <= 0 OR p_hoten IS NULL
       OR length(btrim(p_hoten)) = 0 OR length(p_hoten) > 100
       OR length(p_gioitinh) > 10 OR length(p_email) > 100
       OR length(p_sdt) > 20 OR length(p_diachi) > 255 THEN
        RAISE EXCEPTION 'Mã hoặc thông tin học viên không hợp lệ'
            USING ERRCODE = '22023';
    END IF;
    -- Admin edit must not overwrite class membership or account links.
    UPDATE HocSinh SET HoTen = btrim(p_hoten), NgaySinh = p_ngaysinh,
        GioiTinh = p_gioitinh, Email = p_email, SoDienThoai = p_sdt,
        DiaChi = p_diachi WHERE HocSinhID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_updated := affected = 1;
END;
$$;

CREATE OR REPLACE PROCEDURE study_xoa_hoc_sinh(
    IN p_id integer, INOUT p_deleted boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_deleted := FALSE;
    PERFORM 1 FROM HocSinh WHERE HocSinhID = p_id FOR UPDATE;
    IF NOT FOUND THEN RETURN; END IF;
    IF EXISTS (SELECT 1 FROM DangKyLop WHERE HocSinhID = p_id)
       OR EXISTS (SELECT 1 FROM KetQuaThi WHERE HocSinhID = p_id)
       OR EXISTS (SELECT 1 FROM TaiKhoan tk JOIN HocSinh hs ON hs.HocSinhID = p_id
            WHERE (lower(tk.VaiTro) IN ('hocsinh', 'hocvien') AND tk.NguoiDungID = p_id)
               OR tk.TaiKhoanID = hs.TaiKhoanID) THEN
        RAISE EXCEPTION 'Không thể xóa hồ sơ đã liên kết tài khoản hoặc có dữ liệu học tập. Hãy khóa tài khoản để giữ lịch sử.'
            USING ERRCODE = '23503';
    END IF;
    DELETE FROM HocSinh WHERE HocSinhID = p_id;
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_deleted := affected = 1;
END;
$$;

CREATE OR REPLACE PROCEDURE study_dong_dang_ky_lop(
    IN p_id integer, IN p_giao_vien integer, INOUT p_updated boolean)
LANGUAGE plpgsql AS $$
DECLARE affected integer;
BEGIN
    p_updated := FALSE;
    IF p_id IS NULL OR p_id <= 0 OR p_giao_vien IS NULL OR p_giao_vien <= 0 THEN
        RETURN;
    END IF;
    UPDATE LopHoc SET TrangThai = 'Đã đóng'
        WHERE LopHocID = p_id AND GiaoVienID = p_giao_vien
            AND COALESCE(TrangThai, 'Đang mở') = 'Đang mở';
    GET DIAGNOSTICS affected = ROW_COUNT;
    p_updated := affected = 1;
    -- Closing registration must not change exams or approved memberships.
END;
$$;
