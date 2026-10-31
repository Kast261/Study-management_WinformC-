using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class LopHocService
    {
        private readonly LopHocDAL _lopHocDAL;

        public LopHocService()
        {
            _lopHocDAL = new LopHocDAL();
        }

        public LopHocService(LopHocDAL lopHocDAL)
        {
            _lopHocDAL = lopHocDAL;
        }

        public DataTable LayTatCa()
        {
            return _lopHocDAL.LayTatCa();
        }

        public DataTable LayTheoGiaoVien(int gvID)
        {
            return _lopHocDAL.LayTheoGiaoVien(gvID);
        }

        public DataTable LayTheoHocSinh(int hsID)
        {
            return _lopHocDAL.LayTheoHocSinh(hsID);
        }

        public bool ThemLop(LopHoc lop)
        {
            return _lopHocDAL.ThemLop(lop);
        }

        public bool SuaLop(LopHoc lop)
        {
            return _lopHocDAL.SuaLop(lop);
        }

        public bool XoaLop(int lopID)
        {
            return _lopHocDAL.XoaLop(lopID);
        }

        public DataTable LayDangKyTheoLop(int lopHocID)
        {
            return _lopHocDAL.LayDangKyTheoLop(lopHocID);
        }

        public DataTable LayDangKyChoDuyet(int gvID, int lopHocID = 0)
        {
            return _lopHocDAL.LayDangKyChoDuyet(gvID, lopHocID);
        }

        public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID)
        {
            return _lopHocDAL.DuyetDangKy(dangKyID, lopHocID, hocSinhID);
        }

        public bool TuChoiDangKy(int dangKyID)
        {
            return _lopHocDAL.TuChoiDangKy(dangKyID);
        }

        public bool DongDangKyLop(int lopHocID)
        {
            return _lopHocDAL.DongDangKyLop(lopHocID);
        }

        public DataTable LayLopChuaDangKy(int hsID)
        {
            return _lopHocDAL.LayLopChuaDangKy(hsID);
        }

        public DataTable LayLopHocVienDangHocVaChoDuyet(int hsID)
        {
            return _lopHocDAL.LayLopHocVienDangHocVaChoDuyet(hsID);
        }

        public bool DangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            return _lopHocDAL.DangKyLop(lopHocID, hocSinhID, out thongBao);
        }

        public bool HuyDangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            return _lopHocDAL.HuyDangKyLop(lopHocID, hocSinhID, out thongBao);
        }
    }
}

