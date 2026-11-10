using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IHocSinhService
    {
        DataTable LayTatCa();
        HocSinh LayTheoID(int id);
        bool Them(HocSinh hs, int lopHocID = 0);
        HocSinh LayTheoTaiKhoan(int taiKhoanID);
        bool CapNhatHoSoCaNhan(int taiKhoanID, HocSinh hs);
        bool Sua(HocSinh hs);
        bool Xoa(int id);
        DataTable TimKiem(string tuKhoa);
    }
}
