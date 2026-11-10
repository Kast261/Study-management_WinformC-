using BLT_Quan_Ly_Hoc_Tap.Database;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public sealed class UngDungService
    {
        public bool KiemTraKetNoi(out string loi)
        {
            return DatabaseHelper.KiemTraKetNoi(out loi);
        }
    }
}
