using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class GiaoVienService
    {
        private readonly GiaoVienDAL _giaoVienDAL;

        public GiaoVienService()
        {
            _giaoVienDAL = new GiaoVienDAL();
        }

        public GiaoVienService(GiaoVienDAL giaoVienDAL)
        {
            _giaoVienDAL = giaoVienDAL;
        }

        public DataTable LayTatCa()
        {
            return _giaoVienDAL.LayTatCa();
        }

        public GiaoVien LayTheoID(int id)
        {
            return _giaoVienDAL.LayTheoID(id);
        }

        public bool Them(GiaoVien gv)
        {
            return _giaoVienDAL.Them(gv);
        }

        public bool Sua(GiaoVien gv)
        {
            return _giaoVienDAL.Sua(gv);
        }

        public bool Xoa(int id)
        {
            return _giaoVienDAL.Xoa(id);
        }

        public DataTable TimKiem(string tuKhoa)
        {
            return _giaoVienDAL.TimKiem(tuKhoa);
        }
    }
}

