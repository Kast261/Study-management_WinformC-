using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IKetQuaService
    {
        KetQuaThi NopBai(int taiKhoanID, int hocSinhID, int deThiID, IDictionary<int, string> dapAn);
        bool KiemTraDaThi(int hocSinhID, int deThiID);
        int LuuKetQua(KetQuaThi kq);
        void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung);
        DataTable LayLichSuTheoHocSinh(int hocSinhID);
        DataTable LayKetQuaTheoLopDangHoc(int hocSinhID, int lopHocID = 0);
        DataTable LayChiTietBaiLamTheoKetQua(int ketQuaID);
    }
}
