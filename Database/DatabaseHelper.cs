using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["ChuoiKetNoi"].ConnectionString;

        public static SqlConnection GetConnection() => new SqlConnection(ConnectionString);

        public static bool KiemTraKetNoi()
        {
            try
            {
                using (SqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch { return false; }
        }

        public static DataTable ExecuteQuery(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                using (SqlDataAdapter da = new SqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int ExecuteNonQuery(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string sql, params SqlParameter[] pars)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteScalar();
                }
            }
        }

        // 1. Thực thi chuỗi lệnh SQL có Transaction (Atomic)
        public static bool ExecuteTransaction(Action<SqlCommand> action)
        {
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (SqlCommand cmd = conn.CreateCommand())
                        {
                            cmd.Transaction = trans;
                            action(cmd);
                        }
                        trans.Commit();
                        return true;
                    }
                    catch (Exception)
                    {
                        trans.Rollback();
                        throw; // Ném lỗi ra ngoài để UI hiển thị cảnh báo
                    }
                }
            }
        }

        // 2. Hàm Generic tự động chuyển SqlDataReader sang List<T>
        public static List<T> ExecuteReaderToList<T>(string sql, params SqlParameter[] pars) where T : new()
        {
            List<T> list = new List<T>();
            using (SqlConnection conn = GetConnection())
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        PropertyInfo[] props = typeof(T).GetProperties();
                        while (reader.Read())
                        {
                            T item = new T();
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                if (reader.IsDBNull(i)) continue;
                                string colName = reader.GetName(i);
                                foreach (var prop in props)
                                {
                                    if (string.Equals(prop.Name, colName, StringComparison.OrdinalIgnoreCase) && prop.CanWrite)
                                    {
                                        prop.SetValue(item, Convert.ChangeType(reader.GetValue(i), prop.PropertyType), null);
                                        break;
                                    }
                                }
                            }
                            list.Add(item);
                        }
                    }
                }
            }
            return list;
        }
    }
}