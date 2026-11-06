using Npgsql;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Reflection;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class DatabaseHelper
    {
        public static string ConnectionString = ConfigurationManager.ConnectionStrings["ChuoiKetNoi"]?.ConnectionString;

        public static NpgsqlConnection GetConnection() => new NpgsqlConnection(ConnectionString);

        public static bool KiemTraKetNoi(out string loi)
        {
            try
            {
                using (NpgsqlConnection conn = GetConnection())
                {
                    conn.Open();

                    using (NpgsqlCommand cmd =
                           new NpgsqlCommand("SELECT 1", conn))
                    {
                        cmd.ExecuteScalar();
                    }
                }

                loi = null;
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "28P01")
            {
                loi = "PostgreSQL từ chối tên đăng nhập hoặc mật khẩu (28P01). "
                    + "Hãy đặt lại mật khẩu tài khoản study_app trong pgAdmin và cập nhật cùng mật khẩu "
                    + "trong mục ChuoiKetNoi của App.config (hoặc file .exe.config đang chạy). "
                    + "Đây là tài khoản kết nối database, không phải tài khoản đăng nhập ứng dụng.";
                return false;
            }
            catch (PostgresException ex) when (ex.SqlState == "3D000")
            {
                loi = "Database PostgreSQL chưa tồn tại (3D000). Hãy tạo database theo "
                    + "Database/PostgreSQL/README.md rồi kiểm tra tên Database trong ChuoiKetNoi.";
                return false;
            }
            catch (NpgsqlException ex)
            {
                loi = "Không thể kết nối PostgreSQL.\n" + ex.Message;

                Exception chiTiet = ex.InnerException;
                while (chiTiet != null)
                {
                    loi += "\n" + chiTiet.GetType().Name
                        + ": " + chiTiet.Message;

                    chiTiet = chiTiet.InnerException;
                }

                return false;
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is ArgumentException)
            {
                loi = "Chuỗi kết nối PostgreSQL chưa có hoặc không hợp lệ. Hãy kiểm tra mục ChuoiKetNoi "
                    + "trong App.config hoặc file .exe.config đang chạy.";
                return false;
            }
        }

        public static DataTable ExecuteQuery(string sql, params NpgsqlParameter[] pars)
        {
            using (NpgsqlConnection conn = GetConnection())
            using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
            {
                if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                using (NpgsqlDataAdapter da = new NpgsqlDataAdapter(cmd))
                {
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    return dt;
                }
            }
        }

        public static int ExecuteNonQuery(string sql, params NpgsqlParameter[] pars)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteNonQuery();
                }
            }
        }

        public static object ExecuteScalar(string sql, params NpgsqlParameter[] pars)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    return cmd.ExecuteScalar();
                }
            }
        }

        // 1. Thực thi chuỗi lệnh SQL có Transaction (Atomic)
        public static bool ExecuteTransaction(Action<NpgsqlCommand> action)
        {
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();
                using (NpgsqlTransaction trans = conn.BeginTransaction())
                {
                    try
                    {
                        using (NpgsqlCommand cmd = conn.CreateCommand())
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

        // 2. Hàm Generic tự động chuyển NpgsqlDataReader sang List<T>
        public static List<T> ExecuteReaderToList<T>(string sql, params NpgsqlParameter[] pars) where T : new()
        {
            List<T> list = new List<T>();
            using (NpgsqlConnection conn = GetConnection())
            {
                conn.Open();
                using (NpgsqlCommand cmd = new NpgsqlCommand(sql, conn))
                {
                    if (pars != null && pars.Length > 0) cmd.Parameters.AddRange(pars);
                    using (NpgsqlDataReader reader = cmd.ExecuteReader())
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
