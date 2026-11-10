using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class DeThiService : IDeThiService
    {
        private readonly DeThiDAL _repository;

        public DeThiService() : this(new DeThiDAL()) { }

        public DeThiService(DeThiDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayDeThiChoHocSinh(int hocSinhID)
        {
            return _repository.LayDeThiChoHocSinh(hocSinhID);
        }

        public DataTable LayBaiThiDangMoChoHocSinh(int hocSinhID)
        {
            return _repository.LayBaiThiDangMoChoHocSinh(hocSinhID);
        }

        public DataTable LayDeThiTheoLop(int lopID)
        {
            return _repository.LayDeThiTheoLop(lopID);
        }

        public DataTable LayDeThiTheoGiaoVien(int gvID, bool baoGomDaXoa = false)
        {
            return _repository.LayDeThiTheoGiaoVien(gvID, baoGomDaXoa);
        }

        public DeThi LayTheoID(int deThiID)
        {
            return _repository.LayTheoID(deThiID);
        }

        public bool CoTheBatDauThi(int hocSinhID, int deThiID)
        {
            return _repository.CoTheBatDauThi(hocSinhID, deThiID);
        }

        public List<CauHoi> LayDanhSachCauHoi(int deThiID)
        {
            return _repository.LayDanhSachCauHoi(deThiID);
        }

        public bool ThemDeThi(DeThi de)
        {
            return _repository.ThemDeThi(de);
        }

        public int TaoDeThiMoiLayID(DeThi de)
        {
            return _repository.TaoDeThiMoiLayID(de);
        }

        public int TaoLanGiaoDeThi(int taiKhoanID, int giaoVienID, int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            return _repository.TaoLanGiaoDeThi(taiKhoanID, giaoVienID, deThiID, lopHocID, thoiGian, hanNop, monHoc);
        }

        public bool GiaoDeThiChoLop(int deThiID, int lopHocID, int thoiGian, DateTime? hanNop, string monHoc = null)
        {
            return _repository.GiaoDeThiChoLop(deThiID, lopHocID, thoiGian, hanNop, monHoc);
        }

        public bool KiemTraDeThiChuaGiao(int deThiID)
        {
            return _repository.KiemTraDeThiChuaGiao(deThiID);
        }

        public bool XoaDeThi(int deThiID, out string thongBao)
        {
            return _repository.XoaDeThi(deThiID, out thongBao);
        }

        public bool XoaDeThi(int deThiID, int taiKhoanID, int giaoVienID, out string thongBao)
        {
            return _repository.XoaDeThi(deThiID, taiKhoanID, giaoVienID, out thongBao);
        }

        public bool CapNhatSoCau(int deThiID)
        {
            return _repository.CapNhatSoCau(deThiID);
        }

        public bool ThemCauHoi(CauHoi ch, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.ThemCauHoi(ch, giaoVienID);
        }

        public bool XoaCauHoi(int cauHoiID, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.XoaCauHoi(cauHoiID, giaoVienID);
        }

        public bool SuaCauHoi(CauHoi ch, int giaoVienID)
        {
            if (giaoVienID <= 0) throw new ArgumentOutOfRangeException(nameof(giaoVienID), "Cần xác định giáo viên thực hiện.");
            return _repository.SuaCauHoi(ch, giaoVienID);
        }

        public DataTable LayTongHopKetQuaGanDay(int giaoVienID)
        {
            return _repository.LayTongHopKetQuaGanDay(giaoVienID);
        }

        public DataTable LayKetQuaTheoDeThi(int maDeThi)
        {
            return _repository.LayKetQuaTheoDeThi(maDeThi);
        }

        public DataTable LayBangDiemHocSinh(int hocSinhID)
        {
            return _repository.LayBangDiemHocSinh(hocSinhID);
        }

        public int DemSoCauHoiHienTai(int deThiID)
        {
            return _repository.DemSoCauHoiHienTai(deThiID);
        }
    }
}
