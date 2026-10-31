using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class HocSinhService
    {
        private readonly HocSinhDAL _hocSinhDAL;

        public HocSinhService()
        {
            _hocSinhDAL = new HocSinhDAL();
        }

        public HocSinhService(HocSinhDAL hocSinhDAL)
        {
            _hocSinhDAL = hocSinhDAL;
        }

        public DataTable LayTatCa()
        {
            return _hocSinhDAL.LayTatCa();
        }

        public HocSinh LayTheoID(int id)
        {
            return _hocSinhDAL.LayTheoID(id);
        }

        public bool Them(HocSinh hs)
        {
            return _hocSinhDAL.Them(hs);
        }

        public bool Sua(HocSinh hs)
        {
            return _hocSinhDAL.Sua(hs);
        }

        public bool Xoa(int id)
        {
            return _hocSinhDAL.Xoa(id);
        }

        public DataTable TimKiem(string tuKhoa)
        {
            return _hocSinhDAL.TimKiem(tuKhoa);
        }
    }
}

