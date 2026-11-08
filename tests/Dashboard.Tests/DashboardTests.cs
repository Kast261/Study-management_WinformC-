using System;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using StudentProfile.Tests;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Dashboard.Tests
{
    public class DashboardTests
    {
        private sealed class DashboardDatabase : IDisposable
        {
            private readonly TestDatabase database = new TestDatabase();

            public DashboardDatabase()
            {
                DatabaseHelper.ExecuteNonQuery(@"
                    CREATE TABLE DangKyLop (LopHocID integer, HocSinhID integer, TrangThai text);
                    CREATE TABLE GiaoVien (GiaoVienID integer PRIMARY KEY, HoTen text);
                    CREATE TABLE KhoaHoc (KhoaHocID integer PRIMARY KEY, TenKhoaHoc text);
                    CREATE TABLE LopHoc (
                        LopHocID integer PRIMARY KEY, TenLop text,
                        GiaoVienID integer, KhoaHocID integer,
                        SiSoToiDa integer, NgayBatDau date);");
            }

            public void Dispose() => database.Dispose();
        }

        [Fact]
        public void EmptyDatabaseReturnsZeroCountsAndEmptyLists()
        {
            using var db = new DashboardDatabase();
            var dal = new DashboardDAL();
            using var summary = dal.LayTongQuan();
            Assert.Single(summary.Rows);
            Assert.All(summary.Rows[0].ItemArray, value => Assert.Equal(0L, Convert.ToInt64(value)));
            Assert.Empty(dal.LayHocVienTheoKhoaHoc().Rows);
            Assert.Empty(dal.LayLopMoiNhat().Rows);
        }

        [Fact]
        public void CountsIncludeUnassignedStudentsAndCourseTotalsIncludeEmptyCourses()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien VALUES (1, 'Giáo viên A'), (2, 'Giáo viên B');
                INSERT INTO KhoaHoc VALUES (1, 'Tiếng Anh'), (2, 'Chưa có lớp');
                INSERT INTO LopHoc VALUES (1, 'Anh A', 1, 1, 30, NULL), (2, 'Anh B', 1, 1, 40, NULL);
                INSERT INTO HocSinh (HocSinhID, LopHocID) VALUES (1, 1), (2, 1), (3, 2), (4, NULL);");
            var dal = new DashboardDAL();
            using var summary = dal.LayTongQuan();
            Assert.Equal(new long[] { 4, 2, 2, 2 }, summary.Rows[0].ItemArray.Select(Convert.ToInt64));
            using var courses = dal.LayHocVienTheoKhoaHoc();
            Assert.Equal(2, courses.Rows.Count);
            Assert.Equal(3L, Convert.ToInt64(courses.Rows[0]["SoHocVien"]));
            Assert.Equal(0L, Convert.ToInt64(courses.Rows[1]["SoHocVien"]));

            DatabaseHelper.ExecuteNonQuery("UPDATE HocSinh SET LopHocID = 2 WHERE HocSinhID = 4");
            using var refreshed = dal.LayHocVienTheoKhoaHoc();
            Assert.Equal(4L, Convert.ToInt64(refreshed.Rows[0]["SoHocVien"]));
        }

        [Fact]
        public void SummaryRefreshesAllFourCountsAfterInsertAndDelete()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien SELECT n,'Giáo viên ' || n FROM generate_series(1,7) n;
                INSERT INTO KhoaHoc SELECT n,'Khóa ' || n FROM generate_series(1,5) n;
                INSERT INTO LopHoc (LopHocID,TenLop,GiaoVienID,KhoaHocID)
                    SELECT n,'Lớp ' || n,1,1 FROM generate_series(1,9) n;
                INSERT INTO HocSinh (HocSinhID,LopHocID)
                    SELECT n,CASE WHEN n%2=0 THEN 1 ELSE NULL END FROM generate_series(1,33) n;");
            var dal = new DashboardDAL();
            using var before = dal.LayTongQuan();
            Assert.Equal(new long[] { 33,7,5,9 }, before.Rows[0].ItemArray.Select(Convert.ToInt64));
            DatabaseHelper.ExecuteNonQuery(@"
                DELETE FROM HocSinh WHERE HocSinhID<=5;
                DELETE FROM GiaoVien WHERE GiaoVienID=7;
                DELETE FROM KhoaHoc WHERE KhoaHocID=5;
                DELETE FROM LopHoc WHERE LopHocID=9;");
            using var after = dal.LayTongQuan();
            Assert.Equal(new long[] { 28,6,4,8 }, after.Rows[0].ItemArray.Select(Convert.ToInt64));
        }

        [Fact]
        public void SummaryDoesNotDependOnEnrollmentDataUsedByTheChart()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO HocSinh (HocSinhID) VALUES (1);
                DROP TABLE DangKyLop;");
            var dal = new DashboardDAL();
            Assert.Throws<Npgsql.PostgresException>(() => dal.LayHocVienTheoKhoaHoc());
            using var summary = dal.LayTongQuan();
            Assert.Equal(new long[] { 1,0,0,0 }, summary.Rows[0].ItemArray.Select(Convert.ToInt64));
        }

        [Fact]
        public void CourseChartLimitsToSixAndUsesIdsToResolveDuplicateNameTies()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO KhoaHoc SELECT n, 'Khóa học' FROM generate_series(1, 8) n;
                INSERT INTO LopHoc VALUES (1, 'Lớp đông nhất', NULL, 8, 40, NULL);
                INSERT INTO HocSinh (HocSinhID, LopHocID) VALUES (1, 1);");
            using var courses = new DashboardDAL().LayHocVienTheoKhoaHoc();
            Assert.Equal(6, courses.Rows.Count);
            Assert.Equal(new[] { 8, 1, 2, 3, 4, 5 },
                courses.Rows.Cast<System.Data.DataRow>().Select(r => Convert.ToInt32(r["KhoaHocID"])));
        }

        [Fact]
        public void DashboardCountsApprovedClassesEvenWhenPrimaryClassChanges()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO KhoaHoc VALUES (1,'Toán'),(2,'Anh');
                INSERT INTO LopHoc VALUES (1,'Toán A',NULL,1,30),(2,'Anh A',NULL,2,30);
                INSERT INTO HocSinh (HocSinhID,LopHocID) VALUES (1,2);
                INSERT INTO DangKyLop VALUES (1,1,'Đã duyệt'),(2,1,'Đã duyệt');");
            var dal = new DashboardDAL();
            using var courses = dal.LayHocVienTheoKhoaHoc();
            Assert.All(courses.Rows.Cast<System.Data.DataRow>(),r=>Assert.Equal(1L,Convert.ToInt64(r["SoHocVien"])));
            using var classes = dal.LayLopMoiNhat();
            Assert.All(classes.Rows.Cast<System.Data.DataRow>(),r=>Assert.Equal(1L,Convert.ToInt64(r["SoHocVien"])));
        }

        [Fact]
        public void LatestClassesUseIdsCountStudentsAndHandleMissingTeacherAndCapacity()
        {
            using var db = new DashboardDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien VALUES (1, 'Nguyễn Minh Anh');
                INSERT INTO LopHoc
                    SELECT n, 'Lớp ' || n, NULL, NULL, NULL, '2030-01-01'::date
                    FROM generate_series(1, 7) n;
                UPDATE LopHoc SET GiaoVienID = 1, SiSoToiDa = 30, NgayBatDau = '2020-01-01'
                    WHERE LopHocID = 7;
                INSERT INTO HocSinh (HocSinhID, LopHocID) VALUES (1, 7), (2, 7), (3, 6), (4, NULL);");
            var dal = new DashboardDAL();
            using var classes = dal.LayLopMoiNhat();
            Assert.Equal(new[] { 7, 6, 5, 4, 3 },
                classes.Rows.Cast<System.Data.DataRow>().Select(r => Convert.ToInt32(r["LopHocID"])));
            Assert.Equal("Nguyễn Minh Anh", classes.Rows[0]["TenGiaoVien"]);
            Assert.Equal(2L, Convert.ToInt64(classes.Rows[0]["SoHocVien"]));
            Assert.Equal(30, Convert.ToInt32(classes.Rows[0]["SiSoToiDa"]));
            Assert.Equal("Chưa phân công", classes.Rows[1]["TenGiaoVien"]);
            Assert.Equal(40, Convert.ToInt32(classes.Rows[1]["SiSoToiDa"]));
            Assert.Equal(0L, Convert.ToInt64(classes.Rows[2]["SoHocVien"]));

            DatabaseHelper.ExecuteNonQuery("DELETE FROM LopHoc WHERE LopHocID = 7");
            using var refreshed = dal.LayLopMoiNhat();
            Assert.Equal(6, Convert.ToInt32(refreshed.Rows[0]["LopHocID"]));
        }
    }
}
