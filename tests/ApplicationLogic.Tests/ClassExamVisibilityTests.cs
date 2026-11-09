using System;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ClassExamVisibilityTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ApprovalBeforeOrAfterAssignmentGrantsAllClassRounds(bool approveFirst)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao' WHERE DeThiID=1");
            var classes = new LopHocDAL();
            var exams = new DeThiDAL();
            Assert.True(classes.DangKyLop(2, 22, out _));
            int registration = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop WHERE HocSinhID=22"));
            Assert.Empty(exams.LayDeThiChoHocSinh(22).Rows);
            if (approveFirst) Assert.True(classes.DuyetDangKy(registration, 2, 22, 101));

            int first = exams.TaoLanGiaoDeThi(2, 101, 1, 2, 45, DateTime.Now.AddDays(3));
            int repeated = exams.TaoLanGiaoDeThi(2, 101, 1, 2, 45, DateTime.Now.AddDays(4));
            int otherClass = exams.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null);
            if (!approveFirst)
            {
                Assert.Empty(exams.LayDeThiChoHocSinh(22).Rows);
                Assert.False(exams.CoTheBatDauThi(22, first));
                Assert.True(classes.DuyetDangKy(registration, 2, 22, 101));
            }

            foreach (var table in new[] { exams.LayDeThiChoHocSinh(22), exams.LayBaiThiDangMoChoHocSinh(22) })
            {
                using (table)
                {
                    Assert.Equal(new[] { first, repeated }.OrderBy(id => id), Ids(table));
                    Assert.All(table.Rows.Cast<DataRow>(), row => Assert.Equal("Lớp 2", row["TenLop"]));
                }
            }
            Assert.True(exams.CoTheBatDauThi(22, first));
            Assert.False(exams.CoTheBatDauThi(22, otherClass));
            var answers = exams.LayDanhSachCauHoi(first).ToDictionary(q => q.CauHoiID, q => q.DapAnDung);
            Assert.Equal(10M, new KetQuaDAL().NopBai(3, 22, first, answers).Diem);
            Assert.Equal("Đã làm", exams.LayDeThiChoHocSinh(22).Rows.Cast<DataRow>()
                .Single(row => (int)row["DeThiID"] == first)["TrangThai"]);
            Assert.Equal(new[] { repeated }, Ids(exams.LayBaiThiDangMoChoHocSinh(22)));
        }

        [Theory]
        [InlineData(false, "Hết hạn")]
        [InlineData(true, "Đã đóng")]
        public void LateApprovalRetainsEarlierExpiredOrClosedAssignmentInExamTab(bool closed, string status)
        {
            using var db = new LogicDatabase();
            var exams = new DeThiDAL();
            int assigned = exams.TaoLanGiaoDeThi(2, 101, 1, 2, 45, null);
            DatabaseHelper.ExecuteNonQuery(closed
                ? "UPDATE DeThi SET TrangThai='Đã đóng' WHERE DeThiID=" + assigned
                : "UPDATE DeThi SET HanNop=CURRENT_TIMESTAMP-INTERVAL '1 day' WHERE DeThiID=" + assigned);
            var classes = new LopHocDAL();
            Assert.True(classes.DangKyLop(2, 22, out _));
            int registration = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop WHERE HocSinhID=22"));
            Assert.True(classes.DuyetDangKy(registration, 2, 22, 101));

            using var all = exams.LayDeThiChoHocSinh(22);
            DataRow row = Assert.Single(all.Rows.Cast<DataRow>());
            Assert.Equal(assigned, row["DeThiID"]);
            Assert.Equal(status, row["TrangThai"]);
            Assert.Empty(exams.LayBaiThiDangMoChoHocSinh(22).Rows);
            Assert.False(exams.CoTheBatDauThi(22, assigned));
        }

        [Fact]
        public void EarlierAssignmentsRemainVisibleAcrossMultipleApprovedClassesWithoutDuplicates()
        {
            using var db = new LogicDatabase();
            var exams = new DeThiDAL();
            int secondClass = exams.TaoLanGiaoDeThi(2, 101, 1, 2, 45, null);
            var classes = new LopHocDAL();
            Assert.True(classes.DangKyLop(2, 11, out _));
            int registration = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop WHERE HocSinhID=11"));
            Assert.True(classes.DuyetDangKy(registration, 2, 11, 101));
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES
                    (1,11,'Đã duyệt'),(2,11,'Đã duyệt');");
            Assert.Equal(new[] { 1, secondClass }, Ids(exams.LayDeThiChoHocSinh(11)));
            Assert.All(new[] { 1, secondClass }, id => Assert.True(exams.CoTheBatDauThi(11, id)));
        }

        [Fact]
        public void TeacherGridReceivesTheActualClassOfEachRound()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao' WHERE DeThiID=1");
            var exams = new DeThiDAL();
            int first = exams.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null);
            int second = exams.TaoLanGiaoDeThi(2, 101, 1, 2, 45, null);
            using var grid = exams.LayDeThiTheoGiaoVien(101);
            Assert.Equal("Lớp 1", grid.Rows.Cast<DataRow>().Single(row => (int)row["DeThiID"] == first)["TenLop"]);
            Assert.Equal("Lớp 2", grid.Rows.Cast<DataRow>().Single(row => (int)row["DeThiID"] == second)["TenLop"]);
            Assert.True(grid.Rows.Cast<DataRow>().Single(row => (int)row["DeThiID"] == 1).IsNull("TenLop"));
        }

        private static int[] Ids(DataTable table) => table.Rows.Cast<DataRow>()
            .Select(row => (int)row["DeThiID"]).OrderBy(id => id).ToArray();
    }
}
