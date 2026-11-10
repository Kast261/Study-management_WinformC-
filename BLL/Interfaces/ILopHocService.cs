using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface ILopHocService
    {
        DataTable LayTatCa();
        DataTable LayTheoGiaoVien(int gvID);
        DataTable LayTheoHocSinh(int hsID);
        bool ThemLop(LopHoc lop);
        bool SuaLop(LopHoc lop);
        bool XoaLop(int lopID);
        DataTable LayDangKyTheoLop(int lopHocID);
        DataTable LayDangKyChoDuyet(int gvID, int lopHocID = 0);
        bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID, int giaoVienID);
        bool TuChoiDangKy(int dangKyID, int giaoVienID);
        bool DongDangKyLop(int lopHocID, int giaoVienID);
        DataTable LayLopChuaDangKy(int hsID);
        DataTable LayLopHocVienDangHocVaChoDuyet(int hsID);
        bool DangKyLop(int lopHocID, int hocSinhID, out string thongBao);
        bool HuyDangKyLop(int lopHocID, int hocSinhID, out string thongBao);
        bool SuaThongTinLopCoBan(LopHoc lop);
    }
}
