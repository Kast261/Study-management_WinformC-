using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    public sealed class CauHoiImportDAL
    {
        public KetQuaNhapCauHoi Nhap(int taiKhoanID, DeThi deThi, IList<CauHoi> questions)
        {
            if (deThi == null || taiKhoanID <= 0 || deThi.GiaoVienID <= 0)
                throw new InvalidOperationException("Không xác định được tài khoản giáo viên.");
            if (questions == null || questions.Count == 0 || questions.Count > CauHoiExcelReader.MaxQuestions)
                throw new ArgumentException("Số lượng câu hỏi không hợp lệ.");
            foreach (CauHoi question in questions)
            {
                string error = CauHoiExcelReader.ValidateQuestion(question);
                if (error != null) throw new ArgumentException(error);
            }
            bool create = deThi.DeThiID <= 0;
            if (create && (string.IsNullOrWhiteSpace(deThi.TenDeThi) || deThi.TenDeThi.Trim().Length > 150
                || string.IsNullOrWhiteSpace(deThi.MonHoc) || deThi.MonHoc.Trim().Length > 100))
                throw new ArgumentException("Tên đề thi phải có từ 1 đến 150 ký tự và môn học từ 1 đến 100 ký tự.");

            using (NpgsqlConnection conn = DatabaseHelper.GetConnection())
            {
                conn.Open();
                using (NpgsqlTransaction transaction = conn.BeginTransaction())
                {
                    using (var auth = new NpgsqlCommand(@"
                        SELECT gv.GiaoVienID FROM GiaoVien gv
                        JOIN TaiKhoan tk ON tk.TaiKhoanID = @tk
                        WHERE gv.GiaoVienID = @gv
                          AND LOWER(tk.VaiTro) = 'giaovien'
                          AND COALESCE(tk.TrangThaiKhoa, FALSE) = FALSE
                          AND (tk.NguoiDungID = gv.GiaoVienID
                               OR (COALESCE(tk.NguoiDungID, 0) = 0 AND gv.TaiKhoanID = tk.TaiKhoanID))
                        FOR SHARE OF tk", conn, transaction))
                    {
                        auth.Parameters.AddWithValue("tk", taiKhoanID);
                        auth.Parameters.AddWithValue("gv", deThi.GiaoVienID);
                        if (auth.ExecuteScalar() == null)
                            throw new InvalidOperationException("Tài khoản không có quyền nhập câu hỏi cho giáo viên này.");
                    }

                    int deID = deThi.DeThiID;
                    string tenDe;
                    if (create)
                    {
                        tenDe = deThi.TenDeThi.Trim();
                        using (var insert = new NpgsqlCommand(@"
                            INSERT INTO DeThi (GiaoVienID, TenDeThi, MonHoc, SoCau, ThoiGian, TrangThai)
                            VALUES (@gv, @ten, @mon, 0, 45, 'Chưa giao') RETURNING DeThiID", conn, transaction))
                        {
                            insert.Parameters.AddWithValue("gv", deThi.GiaoVienID);
                            insert.Parameters.AddWithValue("ten", tenDe);
                            insert.Parameters.AddWithValue("mon", deThi.MonHoc.Trim());
                            deID = Convert.ToInt32(insert.ExecuteScalar());
                        }
                    }
                    else
                    {
                        using (var select = new NpgsqlCommand(@"
                            SELECT d.GiaoVienID, d.TenDeThi, COALESCE(d.LopHocID, 0),
                                   EXISTS(SELECT 1 FROM KetQuaThi kq WHERE kq.DeThiID = d.DeThiID)
                            FROM DeThi d WHERE d.DeThiID = @id FOR UPDATE OF d", conn, transaction))
                        {
                            select.Parameters.AddWithValue("id", deID);
                            using (NpgsqlDataReader reader = select.ExecuteReader())
                            {
                                if (!reader.Read() || reader.GetInt32(0) != deThi.GiaoVienID)
                                    throw new InvalidOperationException("Không tìm thấy đề thi thuộc giáo viên đang đăng nhập.");
                                tenDe = reader.GetString(1);
                                if (reader.GetInt32(2) > 0 || reader.GetBoolean(3))
                                    throw new InvalidOperationException("Đề thi đã giao cho lớp hoặc đã có bài làm. Hãy tạo đề mới để nhập Excel.");
                            }
                        }
                    }

                    int lastOrder;
                    using (var max = new NpgsqlCommand("SELECT GREATEST(COALESCE(MAX(ThuTu), 0), 0) FROM CauHoi WHERE DeThiID = @id", conn, transaction))
                    {
                        max.Parameters.AddWithValue("id", deID);
                        lastOrder = Convert.ToInt32(max.ExecuteScalar());
                    }
                    if (lastOrder > int.MaxValue - questions.Count)
                        throw new InvalidOperationException("Số thứ tự câu hỏi vượt giới hạn cho phép.");
                    using (NpgsqlBinaryImporter importer = conn.BeginBinaryImport(@"
                        COPY CauHoi (DeThiID, NoiDung, DapAnA, DapAnB, DapAnC, DapAnD, DapAnDung, ThuTu)
                        FROM STDIN (FORMAT BINARY)"))
                    {
                        foreach (CauHoi question in questions)
                        {
                            importer.StartRow();
                            importer.Write(deID, NpgsqlDbType.Integer);
                            importer.Write(question.NoiDung.Trim(), NpgsqlDbType.Text);
                            importer.Write(question.DapAnA.Trim(), NpgsqlDbType.Varchar);
                            importer.Write(question.DapAnB.Trim(), NpgsqlDbType.Varchar);
                            importer.Write(question.DapAnC.Trim(), NpgsqlDbType.Varchar);
                            importer.Write(question.DapAnD.Trim(), NpgsqlDbType.Varchar);
                            importer.Write(question.DapAnDung.Trim().ToUpperInvariant(), NpgsqlDbType.Varchar);
                            importer.Write(++lastOrder, NpgsqlDbType.Integer);
                        }
                        importer.Complete();
                    }

                    int total;
                    using (var update = new NpgsqlCommand(@"
                        UPDATE DeThi SET SoCau = (SELECT COUNT(*) FROM CauHoi WHERE DeThiID = @id)
                        WHERE DeThiID = @id RETURNING SoCau", conn, transaction))
                    {
                        update.Parameters.AddWithValue("id", deID);
                        total = Convert.ToInt32(update.ExecuteScalar());
                    }
                    transaction.Commit();
                    return new KetQuaNhapCauHoi
                    {
                        DeThiID = deID, TenDeThi = tenDe, SoCauDaNhap = questions.Count,
                        TongSoCau = total, TaoDeThiMoi = create
                    };
                }
            }
        }
    }
}
