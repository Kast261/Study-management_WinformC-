using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface IDeThiService
    {
        DataTable LayDeThiChoHocSinh(int hocSinhID);
        DataTable LayBaiThiDangMoChoHocSinh(int hocSinhID);
        DataTable LayDeThiTheoLop(int lopID);
        DataTable LayDeThiTheoGiaoVien(int gvID, bool baoGomDaXoa = false);
        DeThi LayTheoID(int deThiID);
        bool CoTheBatDauThi(int hocSinhID, int deThiID);
        List<CauHoi> LayDanhSachCauHoi(int deThiID);
        bool ThemDeThi(DeThi de);
        int TaoDeThiMoiLayID(DeThi de);
        int TaoLanGiaoDeThi(int taiKhoanID, int giaoVienID, int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null);
        bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null);
        bool KiemTraDeThiChuaGiao(int deThiID);
        bool XoaDeThi(int deThiID, out string thongBao);
        bool XoaDeThi(int deThiID, int taiKhoanID, int giaoVienID, out string thongBao);
        bool CapNhatSoCau(int deThiID);
        bool ThemCauHoi(CauHoi ch, int giaoVienID);
        bool XoaCauHoi(int cauHoiID, int giaoVienID);
        bool SuaCauHoi(CauHoi ch, int giaoVienID);
        DataTable LayTongHopKetQuaGanDay(int giaoVienID);
        DataTable LayKetQuaTheoDeThi(int maDeThi);
        DataTable LayBangDiemHocSinh(int hocSinhID);
        int DemSoCauHoiHienTai(int deThiID);
    }
}
