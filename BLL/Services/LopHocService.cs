using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class LopHocService : ILopHocService
    {
        private readonly LopHocDAL _repository;

        public LopHocService() : this(new LopHocDAL()) { }

        public LopHocService(LopHocDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayTatCa()
        {
            return _repository.LayTatCa();
        }

        public DataTable LayTheoGiaoVien(int gvID)
        {
            return _repository.LayTheoGiaoVien(gvID);
        }

        public DataTable LayTheoHocSinh(int hsID)
        {
            return _repository.LayTheoHocSinh(hsID);
        }

        public bool ThemLop(LopHoc lop)
        {
            return _repository.ThemLop(lop);
        }

        public bool SuaLop(LopHoc lop)
        {
            return _repository.SuaLop(lop);
        }

        public bool XoaLop(int lopID)
        {
            return _repository.XoaLop(lopID);
        }

        public DataTable LayDangKyTheoLop(int lopHocID)
        {
            return _repository.LayDangKyTheoLop(lopHocID);
        }

        public DataTable LayDangKyChoDuyet(int gvID, int lopHocID = 0)
        {
            return _repository.LayDangKyChoDuyet(gvID, lopHocID);
        }

        public bool DuyetDangKy(int dangKyID, int lopHocID, int hocSinhID, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.DuyetDangKy(dangKyID, lopHocID, hocSinhID, giaoVienID);
        }

        public bool TuChoiDangKy(int dangKyID, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.TuChoiDangKy(dangKyID, giaoVienID);
        }

        public bool DongDangKyLop(int lopHocID, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.DongDangKyLop(lopHocID, giaoVienID);
        }

        public DataTable LayLopChuaDangKy(int hsID)
        {
            return _repository.LayLopChuaDangKy(hsID);
        }

        public DataTable LayLopHocVienDangHocVaChoDuyet(int hsID)
        {
            return _repository.LayLopHocVienDangHocVaChoDuyet(hsID);
        }

        public bool DangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            return _repository.DangKyLop(lopHocID, hocSinhID, out thongBao);
        }

        public bool HuyDangKyLop(int lopHocID, int hocSinhID, out string thongBao)
        {
            return _repository.HuyDangKyLop(lopHocID, hocSinhID, out thongBao);
        }

        public bool SuaThongTinLopCoBan(LopHoc lop)
        {
            return _repository.SuaThongTinLopCoBan(lop);
        }
    }
}
