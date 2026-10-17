using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public class LopHoc
    {
        public int LopHocID { get; set; }
        public int KhoaHocID { get; set; }
        public int GiaoVienID { get; set; }
        public string TenLop { get; set; }
        public string LichHoc { get; set; }
        public int SiSoToiDa { get; set; }
        public string TrangThai { get; set; }

        // Thuộc tính mở rộng hiển thị
        public string TenGiaoVien { get; set; }
        public string TenKhoaHoc { get; set; }
    }
}
