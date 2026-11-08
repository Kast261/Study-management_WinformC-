using System;
using System.Data;
using System.Globalization;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class HomeDashboardTests
    {
        [Fact]
        public void TeacherCountsDistinctStudentsAcrossOwnedClassesAndOnlyPendingRequests()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO LopHoc (LopHocID,TenLop,GiaoVienID) VALUES (3,'Lớp khác',202);
                INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES
                    (1,11,'Đã duyệt'),(2,11,'Đã duyệt'),(2,22,'Đã duyệt'),
                    (1,33,'Chờ duyệt'),(2,33,NULL),(2,22,'Từ chối'),(3,33,'Chờ duyệt');");
            using var table = new DashboardDAL().LayTongQuanGiaoVien(101);
            var row = Assert.Single(table.Rows.Cast<DataRow>());
            Assert.Equal(2L, row["SoLopHoc"]);
            Assert.Equal(2L, row["SoHocVien"]);
            Assert.Equal(2L, row["SoChoDuyet"]);
            Assert.Equal(1L, row["SoBaiThiDangMo"]);
            using var other = new DashboardDAL().LayTongQuanGiaoVien(202);
            Assert.Equal(1L, other.Rows[0]["SoLopHoc"]);
            Assert.Equal(0L, other.Rows[0]["SoHocVien"]);
            Assert.Equal(1L, other.Rows[0]["SoChoDuyet"]);
        }

        [Theory]
        [InlineData("UPDATE DeThi SET HanNop=CURRENT_TIMESTAMP-INTERVAL '1 second'")]
        [InlineData("UPDATE DeThi SET TrangThai='Đã đóng'")]
        [InlineData("UPDATE DeThi SET ThoiGian=0")]
        [InlineData("UPDATE DeThi SET LopHocID=NULL")]
        [InlineData("UPDATE DeThi SET GiaoVienID=202")]
        [InlineData("DELETE FROM CauHoi")]
        public void TeacherOpenExamsExcludeUnavailableExams(string change)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(change);
            using var table = new DashboardDAL().LayTongQuanGiaoVien(101);
            Assert.Equal(0L, table.Rows[0]["SoBaiThiDangMo"]);
        }

        [Fact]
        public void TeacherOpenExamsCountExamsOnceAndKeepNoDeadlineExams()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET HanNop=NULL");
            using var table = new DashboardDAL().LayTongQuanGiaoVien(101);
            Assert.Equal(1L, table.Rows[0]["SoBaiThiDangMo"]);
            using var absent = new DashboardDAL().LayTongQuanGiaoVien(999);
            Assert.All(absent.Rows[0].ItemArray, count => Assert.Equal(0L, count));
        }

        [Fact]
        public void StudentClassesRetainDatesAndOnlyIncludeCurrentOrApprovedMembership()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE LopHoc SET NgayBatDau=NULL,NgayKetThuc=NULL WHERE LopHocID=2;
                INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai)
                    VALUES (1,11,'Đã duyệt'),(2,11,'Đã duyệt'),(2,22,'Chờ duyệt');");
            using var table = new LopHocDAL().LayTheoHocSinh(11);
            Assert.Equal(2, table.Rows.Count);
            var primary = table.Rows.Cast<DataRow>().Single(r => (int)r["LopHocID"] == 1);
            Assert.Equal(new DateTime(2026, 1, 1), primary["NgayBatDau"]);
            Assert.Equal(new DateTime(2026, 12, 31), primary["NgayKetThuc"]);
            Assert.Equal("Giáo viên A", primary["TenGiaoVien"]);
            var approved = table.Rows.Cast<DataRow>().Single(r => (int)r["LopHocID"] == 2);
            Assert.True(approved.IsNull("NgayBatDau"));
            Assert.True(approved.IsNull("NgayKetThuc"));
            Assert.Empty(new LopHocDAL().LayTheoHocSinh(22).Rows);
        }

        [Theory]
        [InlineData("vi-VN")]
        [InlineData("en-US")]
        public void StudentSummaryCountsAllRowsAndAveragesRealScoresIncludingZero(string culture)
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
                using var classes = new DataTable();
                classes.Columns.Add("ID", typeof(int));
                classes.Rows.Add(1); classes.Rows.Add(2);
                using var open = new DataTable();
                open.Columns.Add("ID", typeof(int));
                for (int i = 0; i < 25; i++) open.Rows.Add(i);
                open.DefaultView.RowFilter = "ID < 2";
                using var results = new DataTable();
                results.Columns.Add("Diem", typeof(double));
                results.Rows.Add(0); results.Rows.Add(7.5); results.Rows.Add(DBNull.Value);
                var summary = HomeDashboardSummary.ForStudent(classes, open, results);
                Assert.Equal(2, summary.ClassCount);
                Assert.Equal(25, summary.OpenExamCount);
                Assert.Equal(3, summary.CompletedExamCount);
                Assert.Equal(3.75m, summary.AverageScore);
                Assert.Equal(11, summary.CompletionPercent);
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void StudentWithoutGradesHasNoInventedAverageOrProgress()
        {
            var empty = HomeDashboardSummary.ForStudent(null, null, null);
            Assert.Equal(0, empty.ClassCount);
            Assert.Equal(0, empty.CompletionPercent);
            Assert.Null(empty.AverageScore);
            using var results = new DataTable();
            results.Columns.Add("Diem", typeof(decimal));
            results.Rows.Add(DBNull.Value);
            var complete = HomeDashboardSummary.ForStudent(null, null, results);
            Assert.Equal(1, complete.CompletedExamCount);
            Assert.Equal(100, complete.CompletionPercent);
            Assert.Null(complete.AverageScore);
        }
    }
}
