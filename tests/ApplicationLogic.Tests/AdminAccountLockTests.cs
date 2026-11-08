using System;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using StudentProfile.Tests;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class AdminAccountLockTests
    {
        [Theory]
        [InlineData("Admin")]
        [InlineData("admin")]
        [InlineData("ADMIN")]
        [InlineData(" AdMiN ")]
        public void AdminCannotBeLockedEvenWhenCallingDalDirectly(string role)
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 0, role);
            Assert.False(new TaiKhoanDAL().KhoaHoacMoKhoaTaiKhoan(1, true));
            Assert.False(IsLocked(1));
        }

        [Fact]
        public void PreviouslyLockedAdminCanBeUnlockedButCannotBeLockedAgain()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 0, "Admin", locked: true);
            var dal = new TaiKhoanDAL();
            Assert.True(dal.KhoaHoacMoKhoaTaiKhoan(1, false));
            Assert.False(IsLocked(1));
            Assert.NotNull(dal.DangNhap("student1", "Oldpass1"));
            Assert.False(dal.KhoaHoacMoKhoaTaiKhoan(1, true));
            Assert.False(IsLocked(1));
        }

        [Theory]
        [InlineData("HocSinh")]
        [InlineData("HocVien")]
        [InlineData("GiaoVien")]
        public void NonAdminAccountsCanStillBeLockedAndUnlocked(string role)
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11, role);
            var dal = new TaiKhoanDAL();
            Assert.True(dal.KhoaHoacMoKhoaTaiKhoan(1, true));
            Assert.True(IsLocked(1));
            Assert.True(dal.KhoaHoacMoKhoaTaiKhoan(1, false));
            Assert.False(IsLocked(1));
        }

        [Fact]
        public void LockChecksCurrentRoleEvenIfTheAccountListHasBecomeStale()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11);
            var dal = new TaiKhoanDAL();
            var oldList = dal.GetAll();
            Assert.Equal("HocSinh", oldList.Single().VaiTro);
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro='Admin' WHERE TaiKhoanID=1");
            Assert.False(dal.KhoaHoacMoKhoaTaiKhoan(1, true));
            Assert.False(IsLocked(1));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MissingAccountsDoNotReportSuccess(bool locked)
        {
            using var db = new TestDatabase();
            Assert.False(new TaiKhoanDAL().KhoaHoacMoKhoaTaiKhoan(999, locked));
        }

        private static bool IsLocked(int id) => Convert.ToBoolean(DatabaseHelper.ExecuteScalar(
            "SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=" + id));
    }
}
