using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class KetQuaService
    {
        private readonly KetQuaDAL _ketQuaDAL;

        public KetQuaService()
        {
            _ketQuaDAL = new KetQuaDAL();
        }

        public KetQuaService(KetQuaDAL ketQuaDAL)
        {
            _ketQuaDAL = ketQuaDAL;
        }

        public bool KiemTraDaThi(int hocSinhID, int deThiID)
        {
            return _ketQuaDAL.KiemTraDaThi(hocSinhID, deThiID);
        }

        public int LuuKetQua(KetQuaThi kq)
        {
            return _ketQuaDAL.LuuKetQua(kq);
        }

        public void LuuChiTietBaiLam(int ketQuaID, int cauHoiID, string dapAnChon, bool laDung)
        {
            _ketQuaDAL.LuuChiTietBaiLam(ketQuaID, cauHoiID, dapAnChon, laDung);
        }

        public DataTable LayLichSuTheoHocSinh(int hocSinhID)
        {
            return _ketQuaDAL.LayLichSuTheoHocSinh(hocSinhID);
        }

        public DataTable LayChiTietBaiLamTheoKetQua(int ketQuaID)
        {
            return _ketQuaDAL.LayChiTietBaiLamTheoKetQua(ketQuaID);
        }
    }
}

