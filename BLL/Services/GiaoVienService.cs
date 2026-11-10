using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class GiaoVienService : IGiaoVienService
    {
        private readonly GiaoVienDAL _repository;

        public GiaoVienService() : this(new GiaoVienDAL()) { }

        public GiaoVienService(GiaoVienDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayTatCa()
        {
            return _repository.LayTatCa();
        }

        public GiaoVien LayTheoID(int id)
        {
            return _repository.LayTheoID(id);
        }

        public bool Them(GiaoVien gv)
        {
            return _repository.Them(gv);
        }

        public bool Sua(GiaoVien gv)
        {
            return _repository.Sua(gv);
        }

        public bool CapNhatHoSoCaNhan(int taiKhoanID, GiaoVien gv)
        {
            return _repository.CapNhatHoSoCaNhan(taiKhoanID, gv);
        }

        public bool Xoa(int id)
        {
            return _repository.Xoa(id);
        }

        public DataTable TimKiem(string tuKhoa)
        {
            return _repository.TimKiem(tuKhoa);
        }
    }
}
