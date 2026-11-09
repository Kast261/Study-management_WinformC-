using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class AccountAssignmentTests
    {
        private static int[] Ids(DataTable table)
            => table.Rows.Cast<DataRow>().Select(row => (int)row["NguoiDungID"]).ToArray();

        [Fact]
        public void ListsUnlinkedTeachersAndStudentsUsingStableDisplayAndValueColumns()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (202,'Giáo viên chưa có tài khoản')");
            var dal = new TaiKhoanDAL();
            using var teachers = dal.LayHoSoChuaCoTaiKhoan("GiaoVien");
            using var students = dal.LayHoSoChuaCoTaiKhoan("HocSinh");
            Assert.Equal(new[] { 202 }, Ids(teachers));
            Assert.Equal(new[] { 33 }, Ids(students));
            Assert.Equal("Giáo viên chưa có tài khoản", teachers.Rows[0]["HoTen"]);
            Assert.Equal("C", students.Rows[0]["HoTen"]);
            Assert.Equal(typeof(int), teachers.Columns["NguoiDungID"].DataType);
            Assert.Equal("HoTen", teachers.Columns["HoTen"].ColumnName);
        }

        [Theory]
        [InlineData("HocSinh")]
        [InlineData("HocVien")]
        [InlineData(" hOCvIEN ")]
        public void ExistingStudentAccountAliasesCannotBeOfferedOrOverwritten(string existingRole)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE TaiKhoan SET VaiTro=@role,NguoiDungID=33 WHERE TaiKhoanID=3;
                UPDATE HocSinh SET TaiKhoanID=NULL WHERE HocSinhID=33",
                new Npgsql.NpgsqlParameter("role", existingRole));
            var dal = new TaiKhoanDAL();
            Assert.DoesNotContain(33, Ids(dal.LayHoSoChuaCoTaiKhoan("HocSinh")));
            Assert.DoesNotContain(33, Ids(dal.LayHoSoChuaCoTaiKhoan("HocVien")));
            Assert.False(dal.ThemTaiKhoan("must-not-overwrite", "Password1", "HocSinh", 33));
            Assert.Equal(3L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
        }

        [Fact]
        public void ReverseLinksAndLockedAccountsRemainAssigned()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                UPDATE TaiKhoan SET NguoiDungID=0,TrangThaiKhoa=TRUE WHERE TaiKhoanID=2;
                UPDATE TaiKhoan SET NguoiDungID=0,TrangThaiKhoa=TRUE WHERE TaiKhoanID=1;");
            var dal = new TaiKhoanDAL();
            Assert.DoesNotContain(101, Ids(dal.LayHoSoChuaCoTaiKhoan("GiaoVien")));
            Assert.DoesNotContain(11, Ids(dal.LayHoSoChuaCoTaiKhoan("HocSinh")));
        }

        [Fact]
        public void SameNumericIdInDifferentRolesDoesNotHideAnUnlinkedTeacher()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (11,'Giáo viên số 11')");
            var dal = new TaiKhoanDAL();
            Assert.Contains(11, Ids(dal.LayHoSoChuaCoTaiKhoan("GiaoVien")));
            Assert.DoesNotContain(11, Ids(dal.LayHoSoChuaCoTaiKhoan("HocSinh")));
        }

        [Theory]
        [InlineData("GiaoVien", 202)]
        [InlineData("HocSinh", 33)]
        public void CreatingAccountLinksTheChosenProfileAndRemovesItFromAvailableOptions(string role, int personId)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (202,'Giáo viên mới');
                SELECT setval(pg_get_serial_sequence('TaiKhoan','taikhoanid'),3);");
            var dal = new TaiKhoanDAL();
            Assert.Contains(personId, Ids(dal.LayHoSoChuaCoTaiKhoan(role)));
            Assert.True(dal.ThemTaiKhoan("new-person", "Password1", role, personId));
            Assert.DoesNotContain(personId, Ids(dal.LayHoSoChuaCoTaiKhoan(role)));
            Assert.Equal(personId, dal.DangNhap("new-person", "Password1").NguoiDungID);
            Assert.False(dal.ThemTaiKhoan("second-person", "Password1", role, personId));
        }

        [Fact]
        public async Task ConcurrentAccountCreationCannotAssignTheSameProfileTwice()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("SELECT setval(pg_get_serial_sequence('TaiKhoan','taikhoanid'),3)");
            var dal = new TaiKhoanDAL();
            bool[] created = await Task.WhenAll(
                Task.Run(() => dal.ThemTaiKhoan("first-account", "Password1", "HocSinh", 33)),
                Task.Run(() => dal.ThemTaiKhoan("second-account", "Password1", "HocSinh", 33)));
            Assert.Single(created, result => result);
            Assert.DoesNotContain(33, Ids(dal.LayHoSoChuaCoTaiKhoan("HocSinh")));
            Assert.Equal(4L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
        }

        [Fact]
        public void BlankNamesHaveAVisibleFallbackAndAdminNeedsNoProfile()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE HocSinh SET HoTen='   ' WHERE HocSinhID=33");
            var dal = new TaiKhoanDAL();
            Assert.Equal("Chưa có họ tên", dal.LayHoSoChuaCoTaiKhoan("HocSinh").Rows[0]["HoTen"]);
            Assert.Empty(Ids(dal.LayHoSoChuaCoTaiKhoan("Admin")));
        }
    }
}
