using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using Xunit;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Helpers;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

// Only the Windows host is substituted; the production helper, SQL and DAL run unchanged.
namespace System.Windows.Forms
{
    public static class Application { public static string StartupPath => AppContext.BaseDirectory; }
    public enum MessageBoxButtons { OK }
    public enum MessageBoxIcon { Warning }
    public static class MessageBox
    {
        public static void Show(string message, string title, MessageBoxButtons buttons, MessageBoxIcon icon)
            => throw new InvalidOperationException(message);
    }
}

namespace DatabaseSetup.Tests
{
    public sealed class SetupTests : IDisposable
    {
        private readonly string adminConnection = Environment.GetEnvironmentVariable("PROFILE_TEST_CONNECTION")
            ?? throw new InvalidOperationException("Run the disposable PostgreSQL runner; do not use the app database.");
        private readonly string role = "setup_" + Guid.NewGuid().ToString("N");
        private readonly string schema = "schema_" + Guid.NewGuid().ToString("N");
        private readonly string appConnection;

        public SetupTests()
        {
            string password = Guid.NewGuid().ToString("N");
            Admin("CREATE ROLE " + role + " LOGIN PASSWORD '" + password + "'; CREATE SCHEMA " + schema + " AUTHORIZATION " + role);
            appConnection = new NpgsqlConnectionStringBuilder(adminConnection)
            {
                Username = role, Password = password, SearchPath = schema, Pooling = false
            }.ConnectionString;
            DatabaseHelper.ConnectionString = appConnection;
        }

        private void Admin(string sql)
        {
            using var connection = new NpgsqlConnection(adminConnection);
            connection.Open();
            using var command = new NpgsqlCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        [Fact]
        public void FreshSetupAsUnprivilegedRoleCreatesTenTablesAndThreeWorkingAccounts()
        {
            Assert.True(DatabaseHelper.KiemTraKetNoi(out string error), error);
            CreateDataBase.Init();
            Assert.Equal(10L, Convert.ToInt64(DatabaseHelper.ExecuteScalar(
                "SELECT count(*) FROM information_schema.tables WHERE table_schema=current_schema()")));
            var accounts = new TaiKhoanDAL();
            Assert.Equal("Admin", accounts.DangNhap("admin", "123456").VaiTro);
            foreach (var sample in new[] { ("giaovien1", "GiaoVien"), ("hocsinh1", "HocSinh") })
            {
                var account = accounts.DangNhap(sample.Item1, "123456");
                Assert.Equal(sample.Item2, account.VaiTro);
                Assert.True(account.NguoiDungID > 0);
                string table = sample.Item2 == "GiaoVien" ? "GiaoVien" : "HocSinh";
                Assert.Equal(account.TaiKhoanID, Convert.ToInt32(DatabaseHelper.ExecuteScalar(
                    "SELECT TaiKhoanID FROM " + table + " WHERE " + table + "ID=" + account.NguoiDungID)));
                Assert.True(Passwords.Verify("123456", account.MatKhau));
            }
            Assert.Null(accounts.DangNhap("admin", "wrong-password"));
        }

        [Fact]
        public void RepeatedSetupPreservesAccountsChangedPasswordsAndUserData()
        {
            CreateDataBase.Init();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET MatKhau=@hash WHERE TenDangNhap='admin'",
                new NpgsqlParameter("hash", Passwords.Hash("Changedpass1")));
            DatabaseHelper.ExecuteNonQuery("INSERT INTO KhoaHoc(TenKhoaHoc) VALUES ('Khóa học người dùng')");
            CreateDataBase.Init();
            Assert.Equal(3L, Convert.ToInt64(DatabaseHelper.ExecuteScalar("SELECT count(*) FROM TaiKhoan")));
            Assert.Equal(2L, Convert.ToInt64(DatabaseHelper.ExecuteScalar("SELECT count(*) FROM KhoaHoc")));
            Assert.NotNull(new TaiKhoanDAL().DangNhap("admin", "Changedpass1"));
            Assert.Null(new TaiKhoanDAL().DangNhap("admin", "123456"));
        }

        [Fact]
        public void SeededDatabaseSupportsRealDashboardAndSubmittingAnExam()
        {
            CreateDataBase.Init();
            var dashboard = new DashboardDAL();
            Assert.Single(dashboard.LayTongQuan().Rows);
            Assert.Single(dashboard.LayHocVienTheoKhoaHoc().Rows);
            Assert.Single(dashboard.LayLopMoiNhat().Rows);
            var student = new TaiKhoanDAL().DangNhap("hocsinh1", "123456");
            int exam = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DeThiID FROM DeThi LIMIT 1"));
            var answers = new Dictionary<int, string>();
            foreach (System.Data.DataRow row in DatabaseHelper.ExecuteQuery("SELECT CauHoiID,DapAnDung FROM CauHoi").Rows)
                answers.Add(Convert.ToInt32(row["CauHoiID"]), row["DapAnDung"].ToString());
            var result = new KetQuaDAL().NopBai(student.TaiKhoanID, student.NguoiDungID, exam, answers);
            Assert.Equal(10M, result.Diem);
            Assert.Equal(2, result.SoCauDung);
            Assert.Equal(2L, Convert.ToInt64(DatabaseHelper.ExecuteScalar("SELECT count(*) FROM ChiTietBaiLam")));
        }

        [Fact]
        public void WrongDatabasePasswordReportsAuthenticationRatherThanSsl()
        {
            DatabaseHelper.ConnectionString = new NpgsqlConnectionStringBuilder(appConnection)
                { Password = "intentionally-wrong" }.ConnectionString;
            Assert.False(DatabaseHelper.KiemTraKetNoi(out string error));
            Assert.Contains("28P01", error);
            Assert.DoesNotContain("Lỗi SSL", error);
            Assert.DoesNotContain("intentionally-wrong", error);
        }

        [Fact]
        public void MissingDatabaseAndMissingConfigHaveActionableErrors()
        {
            DatabaseHelper.ConnectionString = new NpgsqlConnectionStringBuilder(appConnection)
                { Database = "missing_" + Guid.NewGuid().ToString("N") }.ConnectionString;
            Assert.False(DatabaseHelper.KiemTraKetNoi(out string error));
            Assert.Contains("3D000", error);
            DatabaseHelper.ConnectionString = null;
            Assert.False(DatabaseHelper.KiemTraKetNoi(out error));
            Assert.Contains("ChuoiKetNoi", error);
        }

        public void Dispose()
        {
            DatabaseHelper.ConnectionString = null;
            Admin("DROP SCHEMA " + schema + " CASCADE; DROP ROLE " + role);
        }
    }
}
