using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class KetQuaThi
    {
        public int KetQuaID { get; set; }
        public int HocSinhID { get; set; }
        public int DeThiID { get; set; }
        public int SoCauDung { get; set; }
        public decimal Diem { get; set; }
        public DateTime ThoiGianNop { get; set; }
        public string TrangThai { get; set; }
        // Thuộc tính phụ trợ hiển thị bảng
        public string TenHocSinh { get; set; }
        public string TenDeThi { get; set; }
    }
    
}
