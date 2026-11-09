using System;
using System.Data;
using System.Linq;
using System.Text;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class ExamSelection
    {
        private static string Normalize(string value)
            => string.Join(" ", (value ?? "").Normalize(NormalizationForm.FormC)
                .Split((char[])null, StringSplitOptions.RemoveEmptyEntries));

        public static DataRow FindDraft(DataTable exams, string title, string subject, int preferredID)
        {
            if (exams == null) return null;
            return exams.Rows.Cast<DataRow>().Where(row =>
                string.Equals(Normalize(Convert.ToString(row["TenDeThi"])), Normalize(title), StringComparison.OrdinalIgnoreCase)
                && string.Equals(Normalize(Convert.ToString(row["MonHoc"])), Normalize(subject), StringComparison.OrdinalIgnoreCase)
                && (row["LopHocID"] == DBNull.Value || Convert.ToInt32(row["LopHocID"]) <= 0)
                && (row["TrangThai"] == DBNull.Value
                    || string.Equals(Convert.ToString(row["TrangThai"]).Trim(), "Chưa giao", StringComparison.OrdinalIgnoreCase)))
                .OrderByDescending(row => Convert.ToInt32(row["DeThiID"]) == preferredID)
                .ThenByDescending(row => row["SoCau"] == DBNull.Value ? 0 : Convert.ToInt32(row["SoCau"]))
                .ThenByDescending(row => Convert.ToInt32(row["DeThiID"]))
                .FirstOrDefault();
        }

        public static DataTable WithQuestions(DataTable exams)
        {
            DataTable ready = exams.Clone();
            foreach (DataRow row in exams.Rows)
                if (row["SoCau"] != DBNull.Value && Convert.ToInt32(row["SoCau"]) > 0)
                    ready.ImportRow(row);
            return ready;
        }

        public static int PreferredClassID(DataTable classes, string subject, int currentID, bool preferSubject)
        {
            if (classes == null || classes.Rows.Count == 0) return 0;
            var rows = classes.Rows.Cast<DataRow>().ToArray();
            DataRow current = rows.FirstOrDefault(row => Convert.ToInt32(row["LopHocID"]) == currentID);
            if (!preferSubject)
                return current != null ? currentID : rows.Length == 1 ? Convert.ToInt32(rows[0]["LopHocID"]) : 0;

            string normalizedSubject = Normalize(subject);
            if (normalizedSubject.Length == 0) return 0;
            var matching = rows.Where(row => {
                string course = classes.Columns.Contains("TenKhoaHoc") ? Normalize(Convert.ToString(row["TenKhoaHoc"])) : "";
                string name = Normalize(Convert.ToString(row["TenLop"]));
                return string.Equals(course, normalizedSubject, StringComparison.OrdinalIgnoreCase)
                    || string.Equals(name, normalizedSubject, StringComparison.OrdinalIgnoreCase)
                    || name.StartsWith(normalizedSubject + " ", StringComparison.OrdinalIgnoreCase);
            }).ToArray();
            if (matching.Any(row => Convert.ToInt32(row["LopHocID"]) == currentID)) return currentID;
            // Nhiều lớp cùng môn không có nghĩa là cùng nhóm học viên.
            // Không tự chọn lớp mới nhất khi giáo viên chưa chỉ định lớp nhận.
            return matching.Length == 1 ? Convert.ToInt32(matching[0]["LopHocID"]) : 0;
        }

        public static string ClassDisplayName(DataRow classroom)
        {
            string schedule = classroom.Table.Columns.Contains("LichHoc")
                ? Convert.ToString(classroom["LichHoc"]).Trim() : "";
            return $"{classroom["TenLop"]} (Mã lớp #{classroom["LopHocID"]})"
                + (schedule.Length == 0 ? "" : " · " + schedule);
        }
    }
}
