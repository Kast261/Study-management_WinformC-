using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Npgsql;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class UsernameUniquenessTests
    {
        [Theory]
        [InlineData("student1", "student1", false)]
        [InlineData("student1", "STUDENT1", false)]
        [InlineData("student1", "  Student1  ", true)]
        [InlineData("  Student1  ", "student1", false)]
        [InlineData("  Student1  ", " STUDENT1 ", true)]
        public void DuplicateNameCannotCreateOrLinkAnotherAccount(string stored, string requested, bool locked)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                SELECT setval(pg_get_serial_sequence('TaiKhoan','taikhoanid'),3);
                UPDATE TaiKhoan SET TenDangNhap=@name,TrangThaiKhoa=@locked WHERE TaiKhoanID=1;",
                new NpgsqlParameter("name", stored), new NpgsqlParameter("locked", locked));
            Assert.False(new TaiKhoanDAL().ThemTaiKhoan(requested, "Password1", "HocSinh", 33));
            Assert.Equal(3L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
            Assert.Equal(System.DBNull.Value, DatabaseHelper.ExecuteScalar("SELECT TaiKhoanID FROM HocSinh WHERE HocSinhID=33"));
            Assert.Equal(stored, DatabaseHelper.ExecuteScalar("SELECT TenDangNhap FROM TaiKhoan WHERE TaiKhoanID=1"));
        }

        [Fact]
        public async Task ConcurrentCaseAndSpaceVariantsCreateOnlyOneAccount()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("SELECT setval(pg_get_serial_sequence('TaiKhoan','taikhoanid'),3)");
            string[] names = { "NewUser", "newuser", " NEWUSER ", "  NewUser", "newuser  ", "NewUser" };
            var outcomes = await Task.WhenAll(names.Select(name => Task.Run(() =>
                new TaiKhoanDAL().ThemTaiKhoan(name, "Password1", "Admin", 0))));
            Assert.Single(outcomes.Where(created => created));
            Assert.Equal(4L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan WHERE LOWER(BTRIM(TenDangNhap))='newuser'"));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT TenDangNhap=BTRIM(TenDangNhap) FROM TaiKhoan WHERE LOWER(TenDangNhap)='newuser'"));
            Assert.True(new TaiKhoanDAL().ThemTaiKhoan("AnotherUser", "Password1", "Admin", 0));
        }
    }
}
