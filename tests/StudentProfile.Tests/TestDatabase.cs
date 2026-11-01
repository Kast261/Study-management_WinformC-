using System;
using System.Data;
using Npgsql;
using Database = BLT_Quan_Ly_Hoc_Tap.Database;
using Helpers = BLT_Quan_Ly_Hoc_Tap.Helpers;

// Run the production DAL on Linux without loading the WinForms application or
// its App.config. SQL still executes against a real, disposable PostgreSQL DB.
namespace System.Windows.Forms
{
    public static class Application
    {
        public static string StartupPath => AppContext.BaseDirectory;
    }
}

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class DatabaseHelper
    {
        public static string ConnectionString;

        public static DataTable ExecuteQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            using var adapter = new NpgsqlDataAdapter(cmd);
            var table = new DataTable();
            adapter.Fill(table);
            return table;
        }

        public static object ExecuteScalar(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteScalar();
        }

        public static int ExecuteNonQuery(string sql, params NpgsqlParameter[] parameters)
        {
            using var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddRange(parameters);
            return cmd.ExecuteNonQuery();
        }
    }
}

namespace StudentProfile.Tests
{
    internal sealed class TestDatabase : IDisposable
    {
        private readonly NpgsqlConnection connection;
        private readonly string schema = "profile_" + Guid.NewGuid().ToString("N");

        public TestDatabase()
        {
            string connectionString = Environment.GetEnvironmentVariable("PROFILE_TEST_CONNECTION")
                ?? throw new InvalidOperationException("Run run.py to create a disposable test database.");
            connection = new NpgsqlConnection(connectionString);
            connection.Open();
            using (var cmd = new NpgsqlCommand("CREATE SCHEMA " + schema, connection))
                cmd.ExecuteNonQuery();
            Database.DatabaseHelper.ConnectionString = new NpgsqlConnectionStringBuilder(connectionString)
            {
                SearchPath = schema,
                Pooling = false
            }.ConnectionString;
            Database.DatabaseHelper.ExecuteNonQuery(@"
                CREATE TABLE TaiKhoan (
                    TaiKhoanID integer PRIMARY KEY, TenDangNhap text UNIQUE,
                    MatKhau text, VaiTro text, NguoiDungID integer,
                    TrangThaiKhoa boolean DEFAULT FALSE);
                CREATE TABLE HocSinh (
                    HocSinhID integer PRIMARY KEY, TaiKhoanID integer,
                    HoTen text, NgaySinh date, GioiTinh text, Email text,
                    SoDienThoai text, DiaChi text, LopHocID integer);");
        }

        public void SeedAccount(int id, int studentId, string role = "HocSinh",
            bool locked = false, string password = null)
        {
            Database.DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO TaiKhoan VALUES (@id, @user, @pass, @role, @student, @locked)",
                new NpgsqlParameter("@id", id),
                new NpgsqlParameter("@user", "student" + id),
                new NpgsqlParameter("@pass", password ?? Helpers.Passwords.Hash("Oldpass1")),
                new NpgsqlParameter("@role", role),
                new NpgsqlParameter("@student", studentId),
                new NpgsqlParameter("@locked", locked));
        }

        public void SeedStudent(int id, int accountId)
        {
            Database.DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO HocSinh VALUES
                (@id, @account, 'Tên cũ', '2006-04-03', 'Nam',
                 'old@example.com', '0987654321', 'Địa chỉ cũ', 700)",
                new NpgsqlParameter("@id", id),
                new NpgsqlParameter("@account", accountId));
        }

        public void Dispose()
        {
            using var cmd = new NpgsqlCommand("DROP SCHEMA " + schema + " CASCADE", connection);
            cmd.ExecuteNonQuery();
            connection.Dispose();
        }
    }
}
