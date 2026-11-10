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
