using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class KhoaHocService : IKhoaHocService
    {
        private readonly KhoaHocDAL _repository;

        public KhoaHocService() : this(new KhoaHocDAL()) { }

        public KhoaHocService(KhoaHocDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayTatCa()
        {
            return _repository.LayTatCa();
        }

        public DataTable LayKhoaHocTheoGiaoVien(int gvID)
        {
            return _repository.LayKhoaHocTheoGiaoVien(gvID);
        }

        public KhoaHoc LayTheoID(int id)
        {
            return _repository.LayTheoID(id);
        }

        public bool Them(KhoaHoc kh)
        {
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            if (string.IsNullOrWhiteSpace(kh.TenKhoaHoc) || kh.TenKhoaHoc.Trim().Length > 100) throw new ArgumentException("Tên khóa học phải có từ 1 đến 100 ký tự.");
            return _repository.Them(kh);
        }

        public bool Sua(KhoaHoc kh)
        {
            if (kh == null) throw new ArgumentNullException(nameof(kh));
            if (string.IsNullOrWhiteSpace(kh.TenKhoaHoc) || kh.TenKhoaHoc.Trim().Length > 100) throw new ArgumentException("Tên khóa học phải có từ 1 đến 100 ký tự.");
            return _repository.Sua(kh);
        }

        public bool Xoa(int id)
        {
            return _repository.Xoa(id);
        }
    }
}
