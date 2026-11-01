using System;
using System.Globalization;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace StudentProfile.Tests
{
    public class ProfileTests
    {
        private static HocSinh EditedProfile(int id) => new HocSinh
        {
            HocSinhID = id,
            HoTen = "Nguyễn Thị An",
            NgaySinh = new DateTime(2006, 4, 3),
            GioiTinh = "Nữ",
            Email = "an@example.com",
            SoDienThoai = "0912345678",
            DiaChi = "Hà Nội"
        };

        [Fact]
        public void SavesEveryProfileFieldWithoutChangingLinksOrOtherStudents()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11);
            db.SeedAccount(2, 22);
            db.SeedStudent(11, 1);
            db.SeedStudent(22, 2);
            var dal = new HocSinhDAL();

            Assert.True(dal.CapNhatHoSoCaNhan(1, EditedProfile(11)));
            HocSinh saved = dal.LayTheoTaiKhoan(1);
            Assert.Equal("Nguyễn Thị An", saved.HoTen);
            Assert.Equal(new DateTime(2006, 4, 3), saved.NgaySinh);
            Assert.Equal("Nữ", saved.GioiTinh);
            Assert.Equal("an@example.com", saved.Email);
            Assert.Equal("0912345678", saved.SoDienThoai);
            Assert.Equal("Hà Nội", saved.DiaChi);
            Assert.Equal(700, DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(1, DatabaseHelper.ExecuteScalar("SELECT TaiKhoanID FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal("Tên cũ", dal.LayTheoTaiKhoan(2).HoTen);
        }

        [Fact]
        public void CannotSaveAnotherStudentsProfile()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11);
            db.SeedAccount(2, 22);
            db.SeedStudent(11, 1);
            db.SeedStudent(22, 2);
            var dal = new HocSinhDAL();
            Assert.False(dal.CapNhatHoSoCaNhan(1, EditedProfile(22)));
            Assert.Equal("Tên cũ", dal.LayTheoTaiKhoan(2).HoTen);
        }

        [Theory]
        [InlineData("Admin", false)]
        [InlineData("GiaoVien", false)]
        [InlineData("HocSinh", true)]
        public void RejectsLockedAccountsAndNonStudentRoles(string role, bool locked)
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11, role, locked);
            db.SeedStudent(11, 1);
            var dal = new HocSinhDAL();
            Assert.Null(dal.LayTheoTaiKhoan(1));
            Assert.False(dal.CapNhatHoSoCaNhan(1, EditedProfile(11)));
            Assert.False(new TaiKhoanDAL().DoiMatKhauCaNhan(1, "Oldpass1", "Newpass2"));
        }

        [Fact]
        public void ResolvesReverseAccountLinkWhenNguoiDungIdIsZero()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 0);
            db.SeedStudent(11, 1);
            var dal = new HocSinhDAL();
            Assert.Equal(11, dal.LayTheoTaiKhoan(1).HocSinhID);
            Assert.True(dal.CapNhatHoSoCaNhan(1, EditedProfile(11)));
        }

        [Fact]
        public void ExplicitStudentLinkTakesPriorityOverAnOldReverseLink()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11);
            db.SeedStudent(11, 1);
            db.SeedStudent(22, 1);
            var dal = new HocSinhDAL();
            Assert.Equal(11, dal.LayTheoTaiKhoan(1).HocSinhID);
            Assert.False(dal.CapNhatHoSoCaNhan(1, EditedProfile(22)));
        }

        [Fact]
        public void ReportsAmbiguousReverseLinksInsteadOfChoosingAnArbitraryStudent()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 0);
            db.SeedStudent(11, 1);
            db.SeedStudent(22, 1);
            Assert.Throws<InvalidOperationException>(() => new HocSinhDAL().LayTheoTaiKhoan(1));
        }

        [Fact]
        public void MissingAccountOrProfileDoesNotUpdateAnyRows()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 0);
            var dal = new HocSinhDAL();
            Assert.Null(dal.LayTheoTaiKhoan(1));
            Assert.Null(dal.LayTheoTaiKhoan(99));
            Assert.False(dal.CapNhatHoSoCaNhan(1, EditedProfile(11)));
        }

        [Fact]
        public void SupportsHocVienRoleAlias()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11, "HocVien");
            db.SeedStudent(11, 1);
            Assert.Equal(11, new HocSinhDAL().LayTheoTaiKhoan(1).HocSinhID);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void PasswordChangeChecksOldPasswordAndKeepsLoginCompatible(bool legacyPlaintext)
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11, password: legacyPlaintext ? "Oldpass1" : Passwords.Hash("Oldpass1"));
            db.SeedAccount(2, 22);
            var dal = new TaiKhoanDAL();
            Assert.False(dal.DoiMatKhauCaNhan(1, "Wrongpass", "Newpass2"));
            Assert.NotNull(dal.DangNhap("student1", "Oldpass1"));
            Assert.True(dal.DoiMatKhauCaNhan(1, "Oldpass1", "Newpass2"));
            Assert.Null(dal.DangNhap("student1", "Oldpass1"));
            Assert.NotNull(dal.DangNhap("student1", "Newpass2"));
            Assert.Equal(Passwords.Hash("Newpass2"), DatabaseHelper.ExecuteScalar("SELECT MatKhau FROM TaiKhoan WHERE TaiKhoanID=1"));
            Assert.False(dal.DoiMatKhauCaNhan(1, "Oldpass1", "Thirdpass3"));
            Assert.NotNull(dal.DangNhap("student2", "Oldpass1"));
        }

        [Theory]
        [InlineData("en-US")]
        [InlineData("vi-VN")]
        public void BirthDateAlwaysUsesDayMonthOrder(string culture)
        {
            CultureInfo previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo(culture);
                Assert.True(Validation.IsValidProfileBirthDate("03/04/2006", out DateTime? date, out _));
                Assert.Equal(new DateTime(2006, 4, 3), date);
                Assert.True(Validation.IsValidProfileBirthDate("29/02/2004", out _, out _));
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }

        [Fact]
        public void StoredHashCannotBeUsedAsTheCurrentPassword()
        {
            using var db = new TestDatabase();
            db.SeedAccount(1, 11);
            var dal = new TaiKhoanDAL();
            Assert.False(dal.DoiMatKhauCaNhan(1, Passwords.Hash("Oldpass1"), "Newpass2"));
            Assert.NotNull(dal.DangNhap("student1", "Oldpass1"));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("31/02/2006")]
        [InlineData("29/02/2005")]
        [InlineData("04/20/2006")]
        [InlineData("2006-04-03")]
        public void RejectsInvalidOrUnexpectedDateFormats(string input)
        {
            Assert.False(Validation.IsValidProfileBirthDate(input, out DateTime? date, out string error));
            Assert.Null(date);
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        [Fact]
        public void RejectsFutureAndUnderageBirthDates()
        {
            foreach (DateTime date in new[] { DateTime.Today.AddDays(1), DateTime.Today.AddYears(-4), DateTime.Today.AddYears(-121) })
                Assert.False(Validation.IsValidProfileBirthDate(date.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture), out _, out _));
        }

        [Theory]
        [InlineData("", "Newpass2", "Newpass2")]
        [InlineData("Oldpass1", "123", "123")]
        [InlineData("Oldpass1", "Newpass2", "newpass2")]
        [InlineData("Oldpass1", "Oldpass1", "Oldpass1")]
        [InlineData("Oldpass1", " Newpass2", " Newpass2")]
        [InlineData("Oldpass1", "Newpass2 ", "Newpass2 ")]
        public void RejectsInvalidPasswordChanges(string oldPassword, string newPassword, string confirmation)
        {
            Assert.False(Validation.IsValidPasswordChange(oldPassword, newPassword, confirmation, out string error));
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        [Fact]
        public void AcceptsMatchingNewPassword()
        {
            Assert.True(Validation.IsValidPasswordChange("Oldpass1", "Newpass2", "Newpass2", out string error));
            Assert.Equal(string.Empty, error);
        }
    }
}
