using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IKhoaHocService
    {
        DataTable LayTatCa();
        DataTable LayKhoaHocTheoGiaoVien(int gvID);
        KhoaHoc LayTheoID(int id);
        bool Them(KhoaHoc kh);
        bool Sua(KhoaHoc kh);
        bool Xoa(int id);
    }
}
