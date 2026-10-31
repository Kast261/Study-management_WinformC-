using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BLT_Quan_Ly_Hoc_Tap.Models
{
    public  class TaiKhoan
    {
        public int TaiKhoanID { get; set; }
        public string TenDangNhap { get; set; }
        public string MatKhau { get; set; }       
        public string VaiTro { get; set; }        
        public int NguoiDungID { get; set; }    
        public bool TrangThaiKhoa { get; set; }  
        public bool GhiNho { get; set; }
    }
}
