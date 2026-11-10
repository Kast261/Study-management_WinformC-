using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IGiaoVienService
    {
        DataTable LayTatCa();
        GiaoVien LayTheoID(int id);
        bool Them(GiaoVien gv);
        bool Sua(GiaoVien gv);
        bool CapNhatHoSoCaNhan(int taiKhoanID, GiaoVien gv);
        bool Xoa(int id);
        DataTable TimKiem(string tuKhoa);
    }
}
