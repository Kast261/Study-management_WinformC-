using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class GiaoVien
    {
        public int GiaoVienID { get; set; }
        public int TaiKhoanID { get; set; }
        public string HoTen { get; set; }
        public string ChuyenMon { get; set; }
        public string Email { get; set; }
        public string SoDienThoai { get; set; }
        public string DiaChi { get; set; }
        public string AnhDaiDien { get; set; }
        public string GioiTinh { get; set; }
        public DateTime? NgaySinh { get; set; }
    }
}
