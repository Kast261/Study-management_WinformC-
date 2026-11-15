using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Npgsql;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class AccountDirectoryTests
    {
        [Theory]
        [InlineData("HocSinh", false)]
        [InlineData("HocVien", false)]
        [InlineData(" hOCvIEN ", false)]
        [InlineData(" hOCsINH ", true)]
        [InlineData("HocVien", true)]
        public void StudentAliasesHaveNamesAndCanBeFoundByNameOrLogin(string role, bool reverseLink)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro=@role,NguoiDungID=@person WHERE TaiKhoanID=1",
                new NpgsqlParameter("role", role), new NpgsqlParameter("person", reverseLink ? 0 : 11));
            var accounts = new TaiKhoanDAL();
            var row = accounts.LayTatCaCoTenNguoiDung().Rows.Cast<DataRow>().Single(r => (int)r["TaiKhoanID"] == 1);
            Assert.Equal("A", row["TenNguoiDung"]);
            Assert.Equal(1, Assert.Single(accounts.TimKiemTheoTenSinhVien("A").Rows.Cast<DataRow>())["TaiKhoanID"]);
            Assert.Equal(1, Assert.Single(accounts.TimKiemTheoTenSinhVien("student1").Rows.Cast<DataRow>())["TaiKhoanID"]);
            Assert.Empty(accounts.TimKiemTheoTenSinhVien("không tồn tại").Rows);
            Assert.Equal(3, accounts.TimKiemTheoTenSinhVien(" ").Rows.Count);
        }

        [Fact]
        public void NormalizedTeacherAndAdminRolesKeepCorrectNamesAndStayOutOfStudentSearch()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE TaiKhoan SET VaiTro=' gIAOvIEN ' WHERE TaiKhoanID=2;
                UPDATE TaiKhoan SET VaiTro=' aDMIN ',NguoiDungID=0 WHERE TaiKhoanID=3;");
            var accounts = new TaiKhoanDAL();
            using var all = accounts.LayTatCaCoTenNguoiDung();
            Assert.Equal("Giáo viên A", all.Rows.Cast<DataRow>().Single(r => (int)r["TaiKhoanID"] == 2)["TenNguoiDung"]);
            Assert.Equal("Quản trị viên hệ thống", all.Rows.Cast<DataRow>().Single(r => (int)r["TaiKhoanID"] == 3)["TenNguoiDung"]);
            Assert.Empty(accounts.TimKiemTheoTenSinhVien("student2").Rows);
            Assert.Empty(accounts.TimKiemTheoTenSinhVien("student3").Rows);
        }
    }
}
