using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly DashboardDAL _repository;

        public DashboardService() : this(new DashboardDAL()) { }

        public DashboardService(DashboardDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public DataTable LayTongQuanGiaoVien(int giaoVienID)
        {
            return _repository.LayTongQuanGiaoVien(giaoVienID);
        }

        public DataTable LayTongQuan()
        {
            return _repository.LayTongQuan();
        }

        public DataTable LayHocVienTheoKhoaHoc()
        {
            return _repository.LayHocVienTheoKhoaHoc();
        }

        public DataTable LayLopMoiNhat()
        {
            return _repository.LayLopMoiNhat();
        }
    }
}
