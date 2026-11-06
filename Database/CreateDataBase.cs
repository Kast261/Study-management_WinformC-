using System;
using System.IO;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Database
{
    public static class CreateDataBase
    {
        // Creates tables inside the configured database, not the server login/database.
        // See Database/PostgreSQL/README.md for first-time local installation.
        public static void Init()
        {
            try
            {
                string directory = Path.Combine(Application.StartupPath, "Database", "PostgreSQL");
                DatabaseHelper.ExecuteNonQuery(File.ReadAllText(Path.Combine(directory, "01_Schema.sql")));
                DatabaseHelper.ExecuteNonQuery(File.ReadAllText(Path.Combine(directory, "02_DemoData.sql")));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khởi tạo DB: " + ex.Message, "Thông Báo Lỗi",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
