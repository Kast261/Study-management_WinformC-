using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class Session
    {
        public static int TaiKhoanID { get; set; }
        public static string TenDangNhap { get; set; }
        public static string VaiTro { get; set; }
        public static int NguoiDungID { get; set; } 
        public static string HoTen { get; set; }

        public static bool IsAdmin => string.Equals(VaiTro, "Admin", StringComparison.OrdinalIgnoreCase);
        public static bool IsGiaoVien => string.Equals(VaiTro, "GiaoVien", StringComparison.OrdinalIgnoreCase);
        public static bool IsHocSinh => string.Equals(VaiTro, "HocSinh", StringComparison.OrdinalIgnoreCase) || string.Equals(VaiTro, "HocVien", StringComparison.OrdinalIgnoreCase);
        public static void Clear()
        {
            TaiKhoanID = 0;
            TenDangNhap = null;
            VaiTro = null;
            NguoiDungID = 0;
            HoTen = null;
        }
    }
}
