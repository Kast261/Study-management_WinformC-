using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class HocSinhService : IHocSinhService
    {
        private readonly HocSinhDAL _repository;

        public HocSinhService() : this(new HocSinhDAL()) { }

        public HocSinhService(HocSinhDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayTatCa()
        {
            return _repository.LayTatCa();
        }

        public HocSinh LayTheoID(int id)
        {
            return _repository.LayTheoID(id);
        }

        public bool Them(HocSinh hs, int lopHocID = 0)
        {
            return _repository.Them(hs, lopHocID);
        }

        public HocSinh LayTheoTaiKhoan(int taiKhoanID)
        {
            return _repository.LayTheoTaiKhoan(taiKhoanID);
        }

        public bool CapNhatHoSoCaNhan(int taiKhoanID, HocSinh hs)
        {
            return _repository.CapNhatHoSoCaNhan(taiKhoanID, hs);
        }

        public bool Sua(HocSinh hs)
        {
            return _repository.Sua(hs);
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
