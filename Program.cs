using BLT_Quan_Ly_Hoc_Tap.Database;
using ReaLTaiizor.Colors;
using ReaLTaiizor.Manager;
using ReaLTaiizor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //CreateDataBase.Init();

            // TASK-33: Cấu hình MaterialSkinManager dùng chung toàn App
            MaterialSkinManager materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new MaterialColorScheme(
                MaterialPrimary.Blue600,
                MaterialPrimary.Blue700,
                MaterialPrimary.Blue200,
                MaterialAccent.LightBlue200,
                MaterialTextShade.WHITE
            );

            Application.Run(new frm_DangNhap());
        }
    }
}
