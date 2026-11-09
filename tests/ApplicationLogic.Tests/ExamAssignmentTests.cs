using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ExamAssignmentTests
    {
        private static void MakeSourceReusable()
        {
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao',SoCau=99 WHERE DeThiID=1");
        }

        private static int Assign(int classId = 1, int sourceId = 1, int minutes = 30)
            => new DeThiDAL().TaoLanGiaoDeThi(2, 101, sourceId, classId, minutes, DateTime.Now.AddDays(3));

        [Fact]
        public void SelectedSubjectIsSavedOnNewRoundAndDisplayedInTeacherStudentAndResultTables()
        {
            using var db = new LogicDatabase();
            MakeSourceReusable();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET MonHoc='Mạng máy tính' WHERE DeThiID=1");
            const string selectedSubject = "Lập trình C# WinForms";
            var dal = new DeThiDAL();
            int winforms = dal.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null, "  " + selectedSubject + "  ");
            int networking = dal.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null, "Mạng máy tính");
            Assert.Equal(selectedSubject, dal.LayTheoID(winforms).MonHoc);
            Assert.Equal("Mạng máy tính", dal.LayTheoID(networking).MonHoc);
            Assert.Equal("Mạng máy tính", dal.LayTheoID(1).MonHoc);
            using var teacher = dal.LayDeThiTheoGiaoVien(101);
            using var student = dal.LayDeThiChoHocSinh(11);
            foreach (var table in new[] { teacher, student })
            {
                Assert.Equal(selectedSubject, table.Rows.Cast<DataRow>().Single(row => (int)row["DeThiID"] == winforms)["MonHoc"]);
                Assert.Equal("Mạng máy tính", table.Rows.Cast<DataRow>().Single(row => (int)row["DeThiID"] == networking)["MonHoc"]);
            }
            var answers = dal.LayDanhSachCauHoi(winforms).ToDictionary(q => q.CauHoiID, q => q.DapAnDung);
            var resultDal = new KetQuaDAL();
            Assert.Equal(10M, resultDal.NopBai(1, 11, winforms, answers).Diem);
            using var results = resultDal.LayKetQuaTheoLopDangHoc(11, 1);
            Assert.Equal(selectedSubject, Assert.Single(results.Rows.Cast<DataRow>())["MonHoc"]);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void ExplicitBlankSubjectDoesNotSilentlyReuseTheSourceSubject(string subject)
        {
            using var db = new LogicDatabase();
            Assert.Throws<ArgumentException>(() => new DeThiDAL().TaoLanGiaoDeThi(2, 101, 1, 1, 45, null, subject));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Fact]
        public void SubjectLengthMatchesTheDatabaseLimit()
        {
            using var db = new LogicDatabase();
            var dal = new DeThiDAL();
            Assert.Throws<ArgumentException>(() => dal.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null, new string('X', 101)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            int assigned = dal.TaoLanGiaoDeThi(2, 101, 1, 1, 45, null, new string('X', 100));
            Assert.Equal(100, dal.LayTheoID(assigned).MonHoc.Length);
        }

        [Fact]
        public void SameSourceCanBeAssignedToMultipleClassesAndRepeatedlyToOneClass()
        {
            using var db = new LogicDatabase();
            MakeSourceReusable();
            int first = Assign(), otherClass = Assign(2), repeated = Assign();
            Assert.Equal(3, new[] { first, otherClass, repeated }.Distinct().Count());
            var dal = new DeThiDAL();
            Assert.Equal(1, dal.LayTheoID(first).LopHocID);
            Assert.Equal(2, dal.LayTheoID(otherClass).LopHocID);
            Assert.Equal(1, dal.LayTheoID(repeated).LopHocID);
            Assert.Equal(0, dal.LayTheoID(1).LopHocID);
            Assert.Equal("Chưa giao", dal.LayTheoID(1).TrangThai);
            Assert.All(new[] { first, otherClass, repeated }, id => {
                Assert.Equal(2, dal.LayTheoID(id).SoCau);
                Assert.Equal(30, dal.LayTheoID(id).ThoiGian);
                Assert.Equal("Toán", dal.LayTheoID(id).MonHoc);
                Assert.Equal("Đang mở", dal.LayTheoID(id).TrangThai);
            });
            Assert.Equal(99, dal.LayTheoID(1).SoCau);
            var visible = dal.LayBaiThiDangMoChoHocSinh(11).Rows.Cast<DataRow>()
                .Select(row => (int)row["DeThiID"]).OrderBy(id => id).ToArray();
            Assert.Equal(new[] { first, repeated }.OrderBy(id => id), visible);
            Assert.False(dal.CoTheBatDauThi(11, otherClass));
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (2,22,'Đã duyệt')");
            Assert.True(dal.CoTheBatDauThi(22, otherClass));
            Assert.False(dal.CoTheBatDauThi(22, first));
        }

        [Fact]
        public void QuestionSnapshotsHaveNewIdsAndRemainUnchangedWhenSourceIsEdited()
        {
            using var db = new LogicDatabase();
            MakeSourceReusable();
            int assigned = Assign();
            var dal = new DeThiDAL();
            var original = dal.LayDanhSachCauHoi(1);
            var copied = dal.LayDanhSachCauHoi(assigned);
            Assert.Equal(original.Select(q => q.NoiDung), copied.Select(q => q.NoiDung));
            Assert.Equal(original.Select(q => q.DapAnDung), copied.Select(q => q.DapAnDung));
            Assert.Equal(original.Select(q => q.ThuTu), copied.Select(q => q.ThuTu));
            Assert.Empty(original.Select(q => q.CauHoiID).Intersect(copied.Select(q => q.CauHoiID)));
            original[0].NoiDung = "Nội dung mới";
            Assert.True(dal.SuaCauHoi(original[0], 101));
            Assert.Equal("Câu 1", dal.LayDanhSachCauHoi(assigned)[0].NoiDung);
            Assert.Throws<InvalidOperationException>(() => dal.XoaCauHoi(copied[0].CauHoiID, 101));
        }

        [Fact]
        public void ReassignmentAfterSubmissionRetainsEarlierGradeAndAllowsIndependentNewSubmission()
        {
            using var db = new LogicDatabase();
            var dal = new DeThiDAL();
            var resultDal = new KetQuaDAL();
            var originalResult = resultDal.NopBai(1, 11, 1, new Dictionary<int, string> { [1] = "A", [2] = "B" });
            int repeated = Assign(minutes: 60);
            Assert.True(dal.CoTheBatDauThi(11, repeated));
            Assert.False(dal.CoTheBatDauThi(11, 1));
            var newResult = resultDal.NopBai(1, 11, repeated, new Dictionary<int, string>());
            Assert.Equal(10M, originalResult.Diem);
            Assert.Equal(0M, newResult.Diem);
            Assert.NotEqual(originalResult.KetQuaID, newResult.KetQuaID);
            Assert.Equal(45, dal.LayTheoID(1).ThoiGian);
            Assert.Equal(1, dal.LayTheoID(1).LopHocID);
            Assert.Equal(10M, DatabaseHelper.ExecuteScalar("SELECT Diem FROM KetQuaThi WHERE DeThiID=1"));
            Assert.Equal(4L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            Assert.Throws<InvalidOperationException>(() => resultDal.NopBai(1, 11, repeated, new Dictionary<int, string>()));
        }

        [Fact]
        public async Task ConcurrentAssignmentsCreateTwoCompleteIndependentRounds()
        {
            using var db = new LogicDatabase();
            int[] assigned = await Task.WhenAll(Task.Run(() => Assign()), Task.Run(() => Assign()));
            Assert.NotEqual(assigned[0], assigned[1]);
            Assert.All(assigned, id => Assert.Equal(2, new DeThiDAL().LayDanhSachCauHoi(id).Count));
            Assert.Equal(3L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Theory]
        [InlineData("UPDATE TaiKhoan SET TrangThaiKhoa=TRUE WHERE TaiKhoanID=2")]
        [InlineData("UPDATE TaiKhoan SET VaiTro='HocSinh' WHERE TaiKhoanID=2")]
        [InlineData("UPDATE TaiKhoan SET NguoiDungID=999 WHERE TaiKhoanID=2")]
        [InlineData("UPDATE DeThi SET GiaoVienID=999 WHERE DeThiID=1")]
        [InlineData("UPDATE LopHoc SET GiaoVienID=999 WHERE LopHocID=1")]
        [InlineData("DELETE FROM CauHoi")]
        public void InvalidOwnershipOrEmptySourceDoesNotCreateAnAssignment(string sql)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(sql);
            Assert.Throws<InvalidOperationException>(() => Assign());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Theory]
        [InlineData("UPDATE KhoaHoc SET TenKhoaHoc='Toán nâng cao' WHERE KhoaHocID=1")]
        [InlineData("UPDATE KhoaHoc SET TenKhoaHoc='Tin học' WHERE KhoaHocID=1")]
        [InlineData("UPDATE LopHoc SET KhoaHocID=NULL WHERE LopHocID=1")]
        public void OwnedClassShownByTheFormCanReceiveExamRegardlessOfItsCourseName(string sql)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(sql);
            var listed = new LopHocDAL().LayTheoGiaoVien(101);
            Assert.Contains(listed.Rows.Cast<DataRow>(), row => (int)row["LopHocID"] == 1);

            int first = Assign(), repeated = Assign();
            Assert.NotEqual(first, repeated);
            var dal = new DeThiDAL();
            Assert.All(new[] { first, repeated }, id => {
                Assert.Equal(1, dal.LayTheoID(id).LopHocID);
                Assert.Equal("Toán", dal.LayTheoID(id).MonHoc);
                Assert.Equal(2, dal.LayDanhSachCauHoi(id).Count);
                Assert.True(dal.CoTheBatDauThi(11, id));
            });
            Assert.Equal("Toán", dal.LayTheoID(1).MonHoc);
            Assert.Equal(1, dal.LayTheoID(1).LopHocID);
        }

        [Fact]
        public void AccountReverseBindingIsAcceptedButAnotherAccountCannotAssign()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET NguoiDungID=0 WHERE TaiKhoanID=2");
            Assert.True(Assign() > 1);
            Assert.Throws<InvalidOperationException>(() => new DeThiDAL().TaoLanGiaoDeThi(1, 101, 1, 1, 30, null));
        }

        [Theory]
        [InlineData(999, 1)]
        [InlineData(1, 999)]
        public void MissingSourceOrClassDoesNotCreateAnAssignment(int source, int target)
        {
            using var db = new LogicDatabase();
            Assert.Throws<InvalidOperationException>(() => Assign(target, source));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Fact]
        public void FailedQuestionCopyRollsBackTheNewExamAndAllItsQuestions()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE CauHoi ADD CONSTRAINT reject_copy CHECK (DeThiID=1 OR ThuTu<>2)");
            Assert.ThrowsAny<Exception>(() => Assign());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
        }

        [Fact]
        public void InvalidDurationAndPastDeadlineAreRejectedBeforeWriting()
        {
            using var db = new LogicDatabase();
            Assert.Throws<ArgumentException>(() => Assign(minutes: 0));
            Assert.Throws<ArgumentException>(() => new DeThiDAL().TaoLanGiaoDeThi(2, 101, 1, 1, 30, DateTime.Now.AddDays(-1)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }
    }
}
