using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class TeacherHomeResultTests
    {
        [Theory]
        [InlineData("numeric")]
        [InlineData("double precision")]
        public void SummaryCountsSubmissionsAndAveragesScoresIncludingZero(string scoreType)
        {
            using var db = new LogicDatabase();
            // CreateDataBase.cs dùng double precision; một số database cũ dùng numeric.
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE KetQuaThi ALTER COLUMN Diem TYPE " + scoreType);
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem,ThoiGianNop)
                    VALUES (11,1,0,CURRENT_TIMESTAMP),(22,1,7.5,CURRENT_TIMESTAMP),(33,1,9.25,CURRENT_TIMESTAMP);
                INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai)
                    VALUES (1,11,'Đã duyệt'),(1,22,'Đã duyệt'),(1,33,'Đã duyệt');");
            using var table = new DeThiDAL().LayTongHopKetQuaGanDay(101);
            Assert.Single(table.Rows);
            Assert.Equal("Đề 1", table.Rows[0]["TenDeThi"]);
            Assert.Equal(3L, table.Rows[0]["DaNop"]);
            Assert.Equal(5.58M, table.Rows[0]["DiemTrungBinh"]);
            Assert.False(table.Rows[0].IsNull("LanNopGanNhat"));
        }

        [Fact]
        public void SummaryWithoutSubmissionsIsEmptyButRetainsBindingSchema()
        {
            using var db = new LogicDatabase();
            using var table = new DeThiDAL().LayTongHopKetQuaGanDay(101);
            Assert.Empty(table.Rows);
            Assert.Equal(typeof(string), table.Columns["TenDeThi"].DataType);
            Assert.Equal(typeof(long), table.Columns["DaNop"].DataType);
            Assert.Equal(typeof(decimal), table.Columns["DiemTrungBinh"].DataType);
        }

        [Fact]
        public void SummaryDoesNotIncludeAnotherTeachersExams()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (202,'Giáo viên khác');
                INSERT INTO LopHoc (LopHocID,TenLop,GiaoVienID,KhoaHocID) VALUES (3,'Lớp khác',202,1);
                INSERT INTO DeThi (DeThiID,TenDeThi,GiaoVienID,LopHocID)
                    VALUES (2,'Đề của giáo viên khác',202,3);
                INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem,ThoiGianNop)
                    VALUES (11,1,5,CURRENT_TIMESTAMP),(22,2,10,CURRENT_TIMESTAMP);");
            var dal = new DeThiDAL();
            Assert.Equal(1, Assert.Single(dal.LayTongHopKetQuaGanDay(101).Rows.Cast<DataRow>())["DeThiID"]);
            Assert.Equal(2, Assert.Single(dal.LayTongHopKetQuaGanDay(202).Rows.Cast<DataRow>())["DeThiID"]);
            Assert.Empty(dal.LayTongHopKetQuaGanDay(999).Rows);
        }

        [Fact]
        public void SameNamedExamsRemainSeparateAndSortByLatestSubmissionWithNullsLast()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE DeThi SET TenDeThi='Cùng tên',NgayTao='2030-01-20' WHERE DeThiID=1;
                INSERT INTO DeThi (DeThiID,TenDeThi,GiaoVienID,LopHocID,NgayTao)
                    VALUES (2,'Cùng tên',101,2,'2030-01-01'),(3,'Cùng tên',101,1,'2030-01-21');
                INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem,ThoiGianNop)
                    VALUES (11,1,5,'2030-01-02'),(22,1,7,'2030-01-10'),
                           (11,2,9,'2030-01-11'),(33,3,8,NULL);");
            var dal = new DeThiDAL();
            using var table = dal.LayTongHopKetQuaGanDay(101);
            Assert.Equal(new[] { 2, 1, 3 }, Ids(table));
            Assert.Equal(2L, table.Rows[1]["DaNop"]);
            Assert.Equal(6M, table.Rows[1]["DiemTrungBinh"]);
            Assert.Equal(new DateTime(2030, 1, 10), table.Rows[1]["LanNopGanNhat"]);
            DatabaseHelper.ExecuteNonQuery("UPDATE KetQuaThi SET ThoiGianNop='2030-01-11' WHERE DeThiID=1");
            Assert.Equal(new[] { 2, 1, 3 }, Ids(dal.LayTongHopKetQuaGanDay(101)));
        }

        [Fact]
        public void ReloadReflectsNewSubmissionsSavedByTheRealGradingFlow()
        {
            using var db = new LogicDatabase();
            var dal = new DeThiDAL();
            Assert.Empty(dal.LayTongHopKetQuaGanDay(101).Rows);
            var grading = new KetQuaDAL();
            grading.NopBai(1, 11, 1, new Dictionary<int, string> { { 1, "A" }, { 2, "A" } });
            using var first = dal.LayTongHopKetQuaGanDay(101);
            Assert.Equal(1L, first.Rows[0]["DaNop"]);
            Assert.Equal(5M, first.Rows[0]["DiemTrungBinh"]);
            DatabaseHelper.ExecuteNonQuery("UPDATE HocSinh SET LopHocID=1 WHERE HocSinhID=22");
            grading.NopBai(3, 22, 1, new Dictionary<int, string> { { 1, "A" }, { 2, "B" } });
            using var second = dal.LayTongHopKetQuaGanDay(101);
            Assert.Equal(2L, second.Rows[0]["DaNop"]);
            Assert.Equal(7.5M, second.Rows[0]["DiemTrungBinh"]);
        }

        [Fact]
        public void ClosingExamOrStudentLeavingClassDoesNotEraseHistoricalSummary()
        {
            using var db = new LogicDatabase();
            new KetQuaDAL().NopBai(1, 11, 1, new Dictionary<int, string> { { 1, "A" }, { 2, "B" } });
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE DeThi SET TrangThai='Đã đóng',HanNop=CURRENT_TIMESTAMP-INTERVAL '1 day';
                UPDATE HocSinh SET LopHocID=NULL WHERE HocSinhID=11;");
            using var table = new DeThiDAL().LayTongHopKetQuaGanDay(101);
            Assert.Single(table.Rows);
            Assert.Equal(1L, table.Rows[0]["DaNop"]);
            Assert.Equal(10M, table.Rows[0]["DiemTrungBinh"]);
        }

        [Fact]
        public void MissingLegacyGradeStaysNullAndIsNotInventedAsZero()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem) VALUES (11,1,NULL)");
            var dal = new DeThiDAL();
            using var missing = dal.LayTongHopKetQuaGanDay(101);
            Assert.Equal(1L, missing.Rows[0]["DaNop"]);
            Assert.True(missing.Rows[0].IsNull("DiemTrungBinh"));
            DatabaseHelper.ExecuteNonQuery("INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem) VALUES (22,1,0)");
            using var withZero = dal.LayTongHopKetQuaGanDay(101);
            Assert.Equal(2L, withZero.Rows[0]["DaNop"]);
            Assert.Equal(0M, withZero.Rows[0]["DiemTrungBinh"]);
        }

        [Fact]
        public void SummaryKeepsMoreThanOnePageOfExamsInStableOrder()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO DeThi (DeThiID,TenDeThi,GiaoVienID,LopHocID)
                    SELECT i,'Đề ' || i,101,1 FROM generate_series(2,25) i;
                INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem,ThoiGianNop)
                    SELECT 11,i,7,CURRENT_TIMESTAMP FROM generate_series(1,25) i;");
            using var table = new DeThiDAL().LayTongHopKetQuaGanDay(101);
            Assert.Equal(25, table.Rows.Count);
            Assert.Equal(Enumerable.Range(1, 25).Reverse(), Ids(table));
            Assert.All(table.Rows.Cast<DataRow>(), row => Assert.Equal(1L, row["DaNop"]));
        }

        private static int[] Ids(DataTable table) => table.Rows.Cast<DataRow>()
            .Select(row => (int)row["DeThiID"]).ToArray();
    }
}
