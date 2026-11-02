using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class StudentHomeExamTests
    {
        [Fact]
        public void HomeListsPrimaryAndApprovedClassesOnlyAndCountsActualQuestions()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO LopHoc (LopHocID,TenLop,GiaoVienID,KhoaHocID)
                    VALUES (3,'Chờ duyệt',101,1),(4,'Từ chối',101,1),(5,'Lớp khác',101,1);
                INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai)
                    VALUES (1,11,'Đã duyệt'),(2,11,'Đã duyệt'),(3,11,'Chờ duyệt'),(4,11,'Từ chối');
                INSERT INTO DeThi (DeThiID,TenDeThi,ThoiGian,MonHoc,SoCau,GiaoVienID,LopHocID,HanNop,TrangThai)
                    SELECT i,'Đề ' || i,45,'Toán',99,101,i,CURRENT_TIMESTAMP+INTERVAL '1 day','Đang mở'
                    FROM generate_series(2,5) i;
                INSERT INTO CauHoi (DeThiID,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,ThuTu)
                    SELECT i,'Câu hỏi','A','B','C','D','A',1 FROM generate_series(2,5) i;
                UPDATE DeThi SET SoCau=99 WHERE DeThiID=1;");
            var dal = new DeThiDAL();
            using var table = dal.LayBaiThiDangMoChoHocSinh(11);
            Assert.Equal(new[] { 1, 2 }, Ids(table).OrderBy(id => id));
            Assert.Equal(2L, table.Rows.Cast<DataRow>().Single(r => (int)r["DeThiID"] == 1)["SoCau"]);
            Assert.Equal(1L, table.Rows.Cast<DataRow>().Single(r => (int)r["DeThiID"] == 2)["SoCau"]);
            Assert.All(table.Rows.Cast<DataRow>(), r => {
                Assert.False(string.IsNullOrWhiteSpace((string)r["TenDeThi"]));
                Assert.Equal("Chưa làm", r["TrangThai"]);
                Assert.True(dal.CoTheBatDauThi(11, (int)r["DeThiID"]));
            });
            Assert.Empty(dal.LayBaiThiDangMoChoHocSinh(22).Rows);
            Assert.Empty(dal.LayBaiThiDangMoChoHocSinh(999).Rows);
        }

        [Theory]
        [InlineData("UPDATE DeThi SET HanNop=CURRENT_TIMESTAMP-INTERVAL '1 second'")]
        [InlineData("UPDATE DeThi SET TrangThai='Đã đóng'")]
        [InlineData("UPDATE DeThi SET TrangThai='Chưa giao',LopHocID=NULL")]
        [InlineData("UPDATE DeThi SET ThoiGian=0")]
        [InlineData("DELETE FROM CauHoi")]
        [InlineData("UPDATE HocSinh SET LopHocID=2 WHERE HocSinhID=11")]
        [InlineData("INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem) VALUES (11,1,10)")]
        public void HomeExcludesExamsThatStudentCannotStart(string change)
        {
            using var db = new LogicDatabase();
            var dal = new DeThiDAL();
            Assert.Single(dal.LayBaiThiDangMoChoHocSinh(11).Rows);
            DatabaseHelper.ExecuteNonQuery(change);
            Assert.Empty(dal.LayBaiThiDangMoChoHocSinh(11).Rows);
            Assert.False(dal.CoTheBatDauThi(11, 1));
        }

        [Fact]
        public void HomeKeepsNoDeadlineExamAndIgnoresOtherStudentsAttempts()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE DeThi SET HanNop=NULL;
                INSERT INTO KetQuaThi (HocSinhID,DeThiID,Diem) VALUES (22,1,10);");
            var dal = new DeThiDAL();
            using var table = dal.LayBaiThiDangMoChoHocSinh(11);
            Assert.Single(table.Rows);
            Assert.True(table.Rows[0].IsNull("HanNop"));
            Assert.True(dal.CoTheBatDauThi(11, 1));
            // Lọc môn học ở tab Bài Thi không được làm mất dữ liệu Trang chủ.
            using var allExams = dal.LayDeThiChoHocSinh(11);
            allExams.DefaultView.RowFilter = "MonHoc='Không tồn tại'";
            Assert.Empty(allExams.DefaultView);
            Assert.Single(table.DefaultView);
        }

        [Fact]
        public void ReloadShowsNewlyAssignedExamAndRemovesItAfterSubmission()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao'");
            var dal = new DeThiDAL();
            Assert.Empty(dal.LayBaiThiDangMoChoHocSinh(11).Rows);
            Assert.True(dal.GiaoDeThiChoLop(1, 1, 45, DateTime.Now.AddDays(1)));
            Assert.Single(dal.LayBaiThiDangMoChoHocSinh(11).Rows);
            new KetQuaDAL().NopBai(1, 11, 1, new Dictionary<int, string> { { 1, "A" }, { 2, "B" } });
            Assert.Empty(dal.LayBaiThiDangMoChoHocSinh(11).Rows);
            Assert.Equal("Đã làm", dal.LayDeThiChoHocSinh(11).Rows[0]["TrangThai"]);
        }

        [Fact]
        public void HomePrioritizesNearestDeadlineAndRetainsExamWithoutDeadline()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO DeThi (DeThiID,TenDeThi,ThoiGian,MonHoc,SoCau,GiaoVienID,LopHocID,HanNop,TrangThai)
                    VALUES (2,'Sắp hết hạn',45,'Toán',1,101,1,CURRENT_TIMESTAMP+INTERVAL '1 hour','Đang mở'),
                           (3,'Không hạn nộp',45,'Toán',1,101,1,NULL,'Đang mở');
                INSERT INTO CauHoi (DeThiID,NoiDung,DapAnA,DapAnB,DapAnC,DapAnD,DapAnDung,ThuTu)
                    VALUES (2,'Câu hỏi','A','B','C','D','A',1),(3,'Câu hỏi','A','B','C','D','A',1);");
            Assert.Equal(new[] { 2, 1, 3 }, Ids(new DeThiDAL().LayBaiThiDangMoChoHocSinh(11)));
        }

        private static int[] Ids(DataTable table) => table.Rows.Cast<DataRow>()
            .Select(row => (int)row["DeThiID"]).ToArray();
    }
}
