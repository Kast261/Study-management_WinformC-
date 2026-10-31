using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class DeThi
    {
        public int DeThiID { get; set; }
        public int LopHocID { get; set; }
        public int GiaoVienID { get; set; }
        public string TenDeThi { get; set; }
        public string MonHoc { get; set; }
        public int SoCau { get; set; }
        public int ThoiGian { get; set; } // Số phút
        public DateTime? HanNop { get; set; }
        public string TrangThai { get; set; }
    }

}
