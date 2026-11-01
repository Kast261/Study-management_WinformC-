using BLT_Quan_Ly_Hoc_Tap.Models;
using ExcelDataReader;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class CauHoiExcelReader
    {
        public const int MaxQuestions = 5000;
        private static readonly string[] Fields = { "NoiDung", "DapAnA", "DapAnB", "DapAnC", "DapAnD", "DapAnDung" };

        public static List<CauHoi> Read(string fileName)
        {
            string extension = Path.GetExtension(fileName);
            if (!string.Equals(extension, ".xlsx", StringComparison.OrdinalIgnoreCase)
                && !string.Equals(extension, ".xls", StringComparison.OrdinalIgnoreCase))
                throw new InvalidDataException("Chỉ hỗ trợ file Excel .xlsx hoặc .xls.");
            if (new FileInfo(fileName).Length > 20 * 1024 * 1024)
                throw new InvalidDataException("File Excel không được lớn hơn 20 MB.");

            string closestHeader = null;
            int closestCount = 0;
            using (var stream = File.Open(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var reader = ExcelReaderFactory.CreateReader(stream))
            {
                do
                {
                    int rowNumber = 0;
                    while (rowNumber < 20 && reader.Read())
                    {
                        rowNumber++;
                        var columns = new Dictionary<string, int>();
                        var duplicates = new List<string>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            string field = HeaderField(Convert.ToString(reader.GetValue(i), CultureInfo.InvariantCulture));
                            if (field == null) continue;
                            if (columns.ContainsKey(field)) duplicates.Add(field);
                            else columns.Add(field, i);
                        }
                        if (columns.Count > closestCount)
                        {
                            closestCount = columns.Count;
                            closestHeader = $"Sheet '{reader.Name}', dòng {rowNumber}: thiếu cột "
                                + string.Join(", ", Fields.Where(f => !columns.ContainsKey(f))) + ".";
                        }
                        if (columns.Count != Fields.Length) continue;
                        if (duplicates.Count > 0)
                            throw new InvalidDataException($"Sheet '{reader.Name}', dòng {rowNumber}: trùng cột "
                                + string.Join(", ", duplicates) + ". Mỗi cột chỉ được xuất hiện một lần.");
                        return ReadQuestions(reader, columns, rowNumber);
                    }
                } while (reader.NextResult());
            }
            throw new InvalidDataException("Không tìm thấy bảng câu hỏi với đủ 6 cột: "
                + string.Join(", ", Fields) + ".\n" + closestHeader
                + "\nHãy dùng file mẫu trong thư mục Templates hoặc nhấp chuột phải nút Nhập Excel để lưu file mẫu.");
        }

        private static List<CauHoi> ReadQuestions(IExcelDataReader reader, Dictionary<string, int> columns, int rowNumber)
        {
            var questions = new List<CauHoi>();
            var errors = new List<string>();
            int dataRows = 0;
            while (reader.Read())
            {
                rowNumber++;
                string[] cells = Fields.Select(f => Convert.ToString(reader.GetValue(columns[f]), CultureInfo.InvariantCulture)?.Trim() ?? "").ToArray();
                if (cells.All(string.IsNullOrWhiteSpace)) continue;
                if (++dataRows > MaxQuestions)
                    throw new InvalidDataException($"Mỗi file chỉ được nhập tối đa {MaxQuestions:N0} câu hỏi. Chưa nhập câu hỏi nào.");
                var question = new CauHoi
                {
                    NoiDung = cells[0], DapAnA = cells[1], DapAnB = cells[2],
                    DapAnC = cells[3], DapAnD = cells[4], DapAnDung = cells[5].ToUpperInvariant()
                };
                string error = ValidateQuestion(question);
                if (error != null) errors.Add($"Sheet '{reader.Name}', dòng {rowNumber}: {error}");
                else questions.Add(question);
            }
            if (errors.Count > 0)
                throw new InvalidDataException("File Excel có lỗi. Chưa nhập câu hỏi nào.\n\n"
                    + string.Join("\n", errors.Take(15))
                    + (errors.Count > 15 ? $"\n... còn {errors.Count - 15} dòng lỗi khác." : ""));
            if (questions.Count == 0)
                throw new InvalidDataException($"Sheet '{reader.Name}' chưa có câu hỏi bên dưới dòng tiêu đề.");
            return questions;
        }

        public static string ValidateQuestion(CauHoi question)
        {
            if (question == null) return "Câu hỏi trống.";
            var errors = new List<string>();
            if (string.IsNullOrWhiteSpace(question.NoiDung)) errors.Add("Thiếu nội dung câu hỏi.");
            string[] answers = { question.DapAnA, question.DapAnB, question.DapAnC, question.DapAnD };
            for (int i = 0; i < answers.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(answers[i])) errors.Add($"Thiếu đáp án {(char)('A' + i)}.");
                else if (answers[i].Trim().Length > 255) errors.Add($"Đáp án {(char)('A' + i)} vượt quá 255 ký tự.");
            }
            if (!new[] { "A", "B", "C", "D" }.Contains(question.DapAnDung?.Trim().ToUpperInvariant()))
                errors.Add("Đáp án đúng phải là A, B, C hoặc D.");
            return errors.Count == 0 ? null : string.Join(" ", errors);
        }

        private static string HeaderField(string header)
        {
            string normalized = new string((header ?? "").Normalize(NormalizationForm.FormD)
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .Select(c => c == 'đ' || c == 'Đ' ? 'd' : char.ToLowerInvariant(c))
                .Where(char.IsLetterOrDigit).ToArray());
            switch (normalized)
            {
                case "noidung": case "noidungcauhoi": case "cauhoi": return "NoiDung";
                case "a": case "dapana": case "phuongana": return "DapAnA";
                case "b": case "dapanb": case "phuonganb": return "DapAnB";
                case "c": case "dapanc": case "phuonganc": return "DapAnC";
                case "d": case "dapand": case "phuongand": return "DapAnD";
                case "dapandung": case "dapan": return "DapAnDung";
                default: return null;
            }
        }
    }
}
