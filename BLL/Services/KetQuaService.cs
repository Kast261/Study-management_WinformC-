using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class KetQuaService : IKetQuaService
    {
        private readonly KetQuaDAL _repository;

        public KetQuaService() : this(new KetQuaDAL()) { }

        public KetQuaService(KetQuaDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public KetQuaThi NopBai(int taiKhoanID, int hocSinhID, int deThiID, IDictionary<int, string> dapAn)
        {
            return _repository.NopBai(taiKhoanID, hocSinhID, deThiID, dapAn);
        }

        public bool KiemTraDaThi(int hocSinhID, int deThiID)
        {
            return _repository.KiemTraDaThi(hocSinhID, deThiID);
        }

        public int LuuKetQua(KetQuaThi kq)
        {
            return _repository.LuuKetQua(kq);
        }

        public void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung)
        {
            _repository.LuuChiTietBaiLam(ketQuaID, cauHoiID, dapAnChon, laDung);
        }

        public DataTable LayLichSuTheoHocSinh(int hocSinhID)
        {
            return _repository.LayLichSuTheoHocSinh(hocSinhID);
        }

        public DataTable LayKetQuaTheoLopDangHoc(int hocSinhID, int lopHocID = 0)
        {
            return _repository.LayKetQuaTheoLopDangHoc(hocSinhID, lopHocID);
        }

        public DataTable LayChiTietBaiLamTheoKetQua(int ketQuaID)
        {
            return _repository.LayChiTietBaiLamTheoKetQua(ketQuaID);
        }
    }
}
