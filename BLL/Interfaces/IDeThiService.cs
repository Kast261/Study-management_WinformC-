using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IDeThiService
    {
        DataTable LayDeThiChoHocSinh(int hocSinhID);
        DataTable LayDeThiTheoLop(int lopID);
        DataTable LayDeThiTheoGiaoVien(int gvID);
        DeThi LayTheoID(int deThiID);
        List<CauHoi> LayDanhSachCauHoi(int deThiID);
        bool ThemDeThi(DeThi de);
        int TaoDeThiMoiLayID(DeThi de);
        int TaoLanGiaoDeThi(int taiKhoanID, int giaoVienID, int deThiID, int lopHocID, int thoiGian, DateTime? hanNop);
        bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null);
        bool KiemTraDeThiChuaGiao(int deThiID);
        bool XoaDeThi(int deThiID, out string thongBao);
        bool XoaDeThi(int deThiID, int taiKhoanID, int giaoVienID, out string thongBao);
        bool CapNhatSoCau(int deThiID);
        bool ThemCauHoi(CauHoi ch);
        bool XoaCauHoi(int cauHoiID);
        bool SuaCauHoi(CauHoi ch);
        DataTable LayKetQuaTheoDeThi(int maDeThi);
        DataTable LayBangDiemHocSinh(int hocSinhID);
        int DemSoCauHoiHienTai(int deThiID);
    }
}

