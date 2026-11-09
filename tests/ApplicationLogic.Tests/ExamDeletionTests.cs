using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ExamDeletionTests
    {
        private static void EnforceForeignKeys()
        {
            DatabaseHelper.ExecuteNonQuery(@"
                ALTER TABLE CauHoi ADD FOREIGN KEY (DeThiID) REFERENCES DeThi(DeThiID);
                ALTER TABLE KetQuaThi ADD FOREIGN KEY (DeThiID) REFERENCES DeThi(DeThiID);
                ALTER TABLE ChiTietBaiLam ADD FOREIGN KEY (CauHoiID) REFERENCES CauHoi(CauHoiID);");
        }

        private static int Assign()
            => new DeThiDAL().TaoLanGiaoDeThi(2, 101, 1, 1, 45, null);

        [Fact]
        public void DeletesAssignedRoundWithoutDeletingItsClassSourceOrOtherRounds()
        {
            using var db = new LogicDatabase();
            EnforceForeignKeys();
            int first = Assign(), other = Assign();
            var dal = new DeThiDAL();
            Assert.True(dal.XoaDeThi(first, 2, 101, out string message), message);
            Assert.Equal("Đã xóa", dal.LayTheoID(first).TrangThai);
            Assert.Equal(2, dal.LayDanhSachCauHoi(first).Count);
            Assert.DoesNotContain(dal.LayDeThiTheoGiaoVien(101).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == first);
            Assert.DoesNotContain(dal.LayDeThiTheoLop(1).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == first);
            Assert.DoesNotContain(dal.LayDeThiChoHocSinh(11).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == first);
            Assert.False(dal.CoTheBatDauThi(11, first));
            Assert.NotNull(dal.LayTheoID(1));
            Assert.Equal(2, dal.LayDanhSachCauHoi(other).Count);
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc"));
            Assert.True(dal.CoTheBatDauThi(11, other));
        }

        [Fact]
        public void DeletingRoundPreservesGradesAnswersAndStudentReviewHistory()
        {
            using var db = new LogicDatabase();
            EnforceForeignKeys();
            int assigned = Assign();
            var resultDal = new KetQuaDAL();
            resultDal.NopBai(1, 11, 1, new Dictionary<int, string> { [1] = "A", [2] = "B" });
            var submitted = resultDal.NopBai(1, 11, assigned, new Dictionary<int, string>());
            using var beforeHistory = resultDal.LayLichSuTheoHocSinh(11);
            using var beforeAnswers = resultDal.LayChiTietBaiLamTheoKetQua(submitted.KetQuaID);
            var dal = new DeThiDAL();
            Assert.True(dal.XoaDeThi(assigned, 2, 101, out string message), message);
            Assert.Equal("Đã xóa", dal.LayTheoID(assigned).TrangThai);
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(4L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            Assert.Equal(4L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
            using var afterHistory = resultDal.LayLichSuTheoHocSinh(11);
            foreach (DataRow before in beforeHistory.Rows)
            {
                var after = afterHistory.Rows.Cast<DataRow>().Single(row => (int)row["KetQuaID"] == (int)before["KetQuaID"]);
                Assert.Equal(before.ItemArray, after.ItemArray);
            }
            using var afterAnswers = resultDal.LayChiTietBaiLamTheoKetQua(submitted.KetQuaID);
            Assert.Equal(2, afterAnswers.Rows.Count);
            for (int i = 0; i < beforeAnswers.Rows.Count; i++)
                Assert.Equal(beforeAnswers.Rows[i].ItemArray, afterAnswers.Rows[i].ItemArray);
            Assert.Equal(2, resultDal.LayKetQuaTheoLopDangHoc(11, 1).Rows.Count);
            Assert.Single(dal.LayKetQuaTheoDeThi(assigned).Rows.Cast<DataRow>());
        }

        [Fact]
        public void DeletingSourceDoesNotDeleteAlreadyCopiedRounds()
        {
            using var db = new LogicDatabase();
            EnforceForeignKeys();
            int assigned = Assign();
            var dal = new DeThiDAL();
            Assert.True(dal.XoaDeThi(1, 2, 101, out string message), message);
            Assert.Equal(2, dal.LayDanhSachCauHoi(assigned).Count);
            Assert.True(dal.CoTheBatDauThi(11, assigned));
            Assert.NotNull(dal.LayTheoID(assigned));
        }

        [Theory]
        [InlineData("UPDATE TaiKhoan SET TrangThaiKhoa=TRUE WHERE TaiKhoanID=2")]
        [InlineData("UPDATE TaiKhoan SET VaiTro='HocSinh' WHERE TaiKhoanID=2")]
        [InlineData("UPDATE TaiKhoan SET NguoiDungID=999 WHERE TaiKhoanID=2")]
        [InlineData("UPDATE DeThi SET GiaoVienID=999 WHERE DeThiID=1")]
        public void DeletionRejectsInvalidAccountOrAnotherTeachersExam(string sql)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(sql);
            Assert.False(new DeThiDAL().XoaDeThi(1, 2, 101, out _));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
        }

        [Fact]
        public void FailedDeleteRollsBackGradesAnswersAndQuestions()
        {
            using var db = new LogicDatabase();
            EnforceForeignKeys();
            new KetQuaDAL().NopBai(1, 11, 1, new Dictionary<int, string>());
            DatabaseHelper.ExecuteNonQuery(@"
                ALTER TABLE DeThi ADD CONSTRAINT reject_archive CHECK (TrangThai <> 'Đã xóa');");
            Assert.False(new DeThiDAL().XoaDeThi(1, 2, 101, out _));
            Assert.Equal("Đang mở", new DeThiDAL().LayTheoID(1).TrangThai);
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
        }

        [Fact]
        public void DeletedDraftCannotBeEditedOrAssignedAgain()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao' WHERE DeThiID=1");
            var dal = new DeThiDAL();
            Assert.True(dal.XoaDeThi(1, 2, 101, out _));
            Assert.Throws<InvalidOperationException>(() => Assign());
            Assert.False(dal.GiaoDeThiChoLop(1, 1, 45, null));
            Assert.Throws<InvalidOperationException>(() => dal.XoaCauHoi(1, 101));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
        }

        [Fact]
        public async Task ConcurrentSubmissionAndDeletionNeverLosesASavedResult()
        {
            using var db = new LogicDatabase();
            int assigned = Assign();
            var submission = Task.Run(() => {
                try { new KetQuaDAL().NopBai(1, 11, assigned, new Dictionary<int, string>()); return true; }
                catch (InvalidOperationException) { return false; }
            });
            var deletion = Task.Run(() => new DeThiDAL().XoaDeThi(assigned, 2, 101, out _));
            await Task.WhenAll(submission, deletion);
            Assert.True(deletion.Result);
            Assert.Equal("Đã xóa", new DeThiDAL().LayTheoID(assigned).TrangThai);
            Assert.Equal(submission.Result ? 1L : 0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(submission.Result ? 2L : 0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
        }

        [Fact]
        public void MissingExamDoesNotDeleteAnything()
        {
            using var db = new LogicDatabase();
            Assert.False(new DeThiDAL().XoaDeThi(999, 2, 101, out _));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }
    }
}
