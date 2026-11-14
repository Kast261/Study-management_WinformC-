using System;
using System.Data;
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
            try { return DatabaseHelper.ExecuteQuery(sql, parameters); }
            catch (PostgresException ex) when (ex.SqlState == "42883")
            {
                throw new InvalidOperationException(
                    "Chưa cài thủ tục PostgreSQL. Chạy Database/PostgreSQL/02_Procedures.sql trên database ứng dụng.", ex);
            }
            catch (PostgresException ex) when (ex.SqlState == "23503")
            {
                throw new InvalidOperationException(ex.MessageText, ex);
            }
        }
    }
}
