using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class DeThiService : IDeThiService
    {
        private readonly DeThiDAL _deThiDAL;

        public DeThiService()
        {
            _deThiDAL = new DeThiDAL();
        }

        public DeThiService(DeThiDAL deThiDAL)
        {
            _deThiDAL = deThiDAL;
        }

        public DataTable LayDeThiChoHocSinh(int hocSinhID)
        {
            return _deThiDAL.LayDeThiChoHocSinh(hocSinhID);
        }

        public DataTable LayDeThiTheoLop(int lopID)
        {
            return _deThiDAL.LayDeThiTheoLop(lopID);
        }

        public DataTable LayDeThiTheoGiaoVien(int gvID)
        {
            return _deThiDAL.LayDeThiTheoGiaoVien(gvID);
        }

        public DeThi LayTheoID(int deThiID)
        {
            return _deThiDAL.LayTheoID(deThiID);
        }

        public List<CauHoi> LayDanhSachCauHoi(int deThiID)
        {
            return _deThiDAL.LayDanhSachCauHoi(deThiID);
        }

        public bool ThemDeThi(DeThi de)
        {
            return _deThiDAL.ThemDeThi(de);
        }

        public int TaoDeThiMoiLayID(DeThi de)
        {
            return _deThiDAL.TaoDeThiMoiLayID(de);
        }

        public bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            return _deThiDAL.GiaoDeThiChoLop(deThiID, lopHocID, thoiGian, hanNop, monHoc);
        }

        public bool KiemTraDeThiChuaGiao(int deThiID)
        {
            return _deThiDAL.KiemTraDeThiChuaGiao(deThiID);
        }

        public bool XoaDeThi(int deThiID, out string thongBao)
        {
            return _deThiDAL.XoaDeThi(deThiID, out thongBao);
        }

        public bool CapNhatSoCau(int deThiID)
        {
            return _deThiDAL.CapNhatSoCau(deThiID);
        }

        public bool ThemCauHoi(CauHoi ch)
        {
            return _deThiDAL.ThemCauHoi(ch);
        }

        public bool XoaCauHoi(int cauHoiID)
        {
            return _deThiDAL.XoaCauHoi(cauHoiID);
        }

        public bool SuaCauHoi(CauHoi ch)
        {
            return _deThiDAL.SuaCauHoi(ch);
        }

        public DataTable LayKetQuaTheoDeThi(int maDeThi)
        {
            return _deThiDAL.LayKetQuaTheoDeThi(maDeThi);
        }

        public DataTable LayBangDiemHocSinh(int hocSinhID)
        {
            return _deThiDAL.LayBangDiemHocSinh(hocSinhID);
        }

        public int DemSoCauHoiHienTai(int deThiID)
        {
            return _deThiDAL.DemSoCauHoiHienTai(deThiID);
        }
    }
}

