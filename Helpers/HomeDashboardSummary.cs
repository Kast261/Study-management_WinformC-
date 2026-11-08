using System;
using System.Data;
using System.Globalization;
using System.Linq;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public sealed class HomeDashboardSummary
    {
        public int ClassCount { get; private set; }
        public int OpenExamCount { get; private set; }
        public int CompletedExamCount { get; private set; }
        public decimal? AverageScore { get; private set; }
        public int CompletionPercent { get; private set; }

        public static HomeDashboardSummary ForStudent(DataTable classes, DataTable openExams, DataTable results)
        {
            var summary = new HomeDashboardSummary
            {
                ClassCount = classes?.Rows.Count ?? 0,
                OpenExamCount = openExams?.Rows.Count ?? 0,
                CompletedExamCount = results?.Rows.Count ?? 0
            };
            if (results != null)
            {
                var scores = results.Rows.Cast<DataRow>().Where(row => !row.IsNull("Diem"))
                    .Select(row => Convert.ToDecimal(row["Diem"], CultureInfo.InvariantCulture)).ToArray();
                if (scores.Length > 0) summary.AverageScore = scores.Average();
            }
            // Tiến độ gồm các bài đã nộp và các bài đang mở chưa làm.
            long total = (long)summary.OpenExamCount + summary.CompletedExamCount;
            summary.CompletionPercent = total == 0 ? 0
                : (int)Math.Round(100m * summary.CompletedExamCount / total, MidpointRounding.AwayFromZero);
            return summary;
        }
    }
}
