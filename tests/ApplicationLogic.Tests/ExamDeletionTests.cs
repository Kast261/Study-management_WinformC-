using System;
using System.Collections.Generic;
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
            Assert.Null(dal.LayTheoID(first));
            Assert.Empty(dal.LayDanhSachCauHoi(first));
            Assert.NotNull(dal.LayTheoID(1));
            Assert.Equal(2, dal.LayDanhSachCauHoi(other).Count);
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc"));
            Assert.True(dal.CoTheBatDauThi(11, other));
        }

        [Fact]
        public void DeletesOnlySelectedRoundsGradesAndAnswersInForeignKeyOrder()
        {
            using var db = new LogicDatabase();
            EnforceForeignKeys();
            int assigned = Assign();
            var resultDal = new KetQuaDAL();
            var original = resultDal.NopBai(1, 11, 1, new Dictionary<int, string> { [1] = "A", [2] = "B" });
            resultDal.NopBai(1, 11, assigned, new Dictionary<int, string>());
            var dal = new DeThiDAL();
            Assert.True(dal.XoaDeThi(assigned, 2, 101, out string message), message);
            Assert.Null(dal.LayTheoID(assigned));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            Assert.Equal(10M, DatabaseHelper.ExecuteScalar("SELECT Diem FROM KetQuaThi"));
            Assert.Equal(original.KetQuaID, DatabaseHelper.ExecuteScalar("SELECT KetQuaID FROM KetQuaThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
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
                CREATE TABLE KeepExam (DeThiID integer REFERENCES DeThi(DeThiID));
                INSERT INTO KeepExam VALUES (1);");
            Assert.False(new DeThiDAL().XoaDeThi(1, 2, 101, out _));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
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
