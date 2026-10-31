using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class KhoaHocService
    {
        private readonly KhoaHocDAL _khoaHocDAL;

        public KhoaHocService()
        {
            _khoaHocDAL = new KhoaHocDAL();
        }

        public KhoaHocService(KhoaHocDAL khoaHocDAL)
        {
            _khoaHocDAL = khoaHocDAL;
        }

        public DataTable LayTatCa()
        {
            return _khoaHocDAL.LayTatCa();
        }

        public DataTable LayKhoaHocTheoGiaoVien(int gvID)
        {
            return _khoaHocDAL.LayKhoaHocTheoGiaoVien(gvID);
        }

        public KhoaHoc LayTheoID(int id)
        {
            return _khoaHocDAL.LayTheoID(id);
        }

        public bool Them(KhoaHoc kh)
        {
            return _khoaHocDAL.Them(kh);
        }

        public bool Sua(KhoaHoc kh)
        {
            return _khoaHocDAL.Sua(kh);
        }

        public bool Xoa(int id)
        {
            return _khoaHocDAL.Xoa(id);
        }
    }
}

