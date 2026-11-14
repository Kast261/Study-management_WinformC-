using System;
using System.Data;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using BLT_Quan_Ly_Hoc_Tap.Database;

namespace BLT_Quan_Ly_Hoc_Tap.DAL
{
    // Explicit CALL is required: Npgsql 6 StoredProcedure mode invokes functions.
    internal static class ProcedureExecutor
    {
        public static NpgsqlParameter Text(string name, string value) =>
            new NpgsqlParameter(name, NpgsqlDbType.Varchar) { Value = (object)value ?? DBNull.Value };

        public static NpgsqlParameter Date(string name, DateTime? value) =>
            new NpgsqlParameter(name, NpgsqlDbType.Date) { Value = (object)value ?? DBNull.Value };

        public static bool Boolean(string sql, string output, params NpgsqlParameter[] parameters)
        {
            using (var result = Execute(sql, parameters))
                return result.Rows.Count == 1 && Convert.ToBoolean(result.Rows[0][output]);
        }

        public static int Id(string sql, params NpgsqlParameter[] parameters)
        {
            using (var result = Execute(sql, parameters))
                return result.Rows.Count == 1 ? Convert.ToInt32(result.Rows[0]["p_id"]) : 0;
        }

        private static DataTable Execute(string sql, NpgsqlParameter[] parameters)
        {
            try { return ExecuteCall(sql, parameters); }
            catch (PostgresException ex) when (IsMissingProcedure(ex))
            {
                InstallBundledProcedures();
                // Retry the original CALL once; never substitute a direct DELETE/INSERT.
                try { return ExecuteCall(sql, parameters); }
                catch (PostgresException retryError) when (IsMissingProcedure(retryError))
                {
                    throw new InvalidOperationException(
                        "Thủ tục PostgreSQL vẫn chưa sẵn sàng sau khi cập nhật. Chạy Database/PostgreSQL/02_Procedures.sql trên đúng database và schema ứng dụng.", retryError);
                }
            }
        }

        private static bool IsMissingProcedure(PostgresException error) =>
            error.SqlState == "42883" && string.IsNullOrEmpty(error.Where);

        private static DataTable ExecuteCall(string sql, NpgsqlParameter[] parameters)
        {
            // A failed command may still own its parameters after disposal.
            // Clone them so recovery can safely create a new command.
            var copy = new NpgsqlParameter[parameters.Length];
            for (int i = 0; i < parameters.Length; i++)
                copy[i] = (NpgsqlParameter)((ICloneable)parameters[i]).Clone();
            try { return DatabaseHelper.ExecuteQuery(sql, copy); }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                throw new InvalidOperationException(ex.MessageText, ex);
            }
        }

        private static void InstallBundledProcedures()
        {
            using (var stream = typeof(ProcedureExecutor).Assembly.GetManifestResourceStream("PostgresProcedures.sql"))
            {
                if (stream == null)
                    throw new InvalidOperationException(
                        "Bản ứng dụng thiếu script thủ tục đính kèm. Build lại bản mới hoặc chạy Database/PostgreSQL/02_Procedures.sql trên database ứng dụng.");
                string script;
                using (var reader = new StreamReader(stream)) script = reader.ReadToEnd();
                try
                {
                    using (var connection = DatabaseHelper.GetConnection())
                    {
                        connection.Open();
                        using (var transaction = connection.BeginTransaction())
                        {
                            // Serialize recovery by schema when several forms/processes
                            // encounter an old database at the same time.
                            using (var locked = new NpgsqlCommand(
                                "SELECT pg_advisory_xact_lock(hashtext('StudyManagement:procedures'), hashtext(current_schema()))",
                                connection, transaction)) locked.ExecuteNonQuery();
                            using (var command = new NpgsqlCommand(script, connection, transaction))
                                command.ExecuteNonQuery();
                            transaction.Commit();
                        }
                    }
                }
                catch (PostgresException ex) when (ex.SqlState == "42501")
                {
                    throw new InvalidOperationException(
                        "Database thiếu thủ tục nhưng tài khoản kết nối không đủ quyền cài/cập nhật. Nhờ quản trị viên chạy Database/PostgreSQL/02_Procedures.sql trên đúng database ứng dụng rồi thử xóa lại.", ex);
                }
            }
        }
    }
}
