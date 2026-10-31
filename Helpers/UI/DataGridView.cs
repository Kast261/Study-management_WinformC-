using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class DataGridViewHelper
    {
        public static void CauHinhDoRongCot(DataGridView dgv)
        {
            dgv.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.None;

            dgv.AutoSizeRowsMode =
                DataGridViewAutoSizeRowsMode.None;

            dgv.ScrollBars = ScrollBars.Both;
            dgv.AllowUserToResizeColumns = true;

            dgv.AutoResizeColumns(
                DataGridViewAutoSizeColumnsMode.DisplayedCells
            );

            foreach (DataGridViewColumn col in dgv.Columns)
            {
                if (col.Width < 100)
                    col.Width = 100;
            }
        }
    }
}
