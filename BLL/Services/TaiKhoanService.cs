using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly TaiKhoanDAL _taiKhoanDAL;

        public TaiKhoanService()
        {
            _taiKhoanDAL = new TaiKhoanDAL();
        }

        public TaiKhoanService(TaiKhoanDAL taiKhoanDAL)
        {
            _taiKhoanDAL = taiKhoanDAL;
        }

        public TaiKhoan DangNhap(string tenDangNhap, string matKhauNhap)
        {
            return _taiKhoanDAL.DangNhap(tenDangNhap, matKhauNhap);
        }

        public void LuuGhiNho(string tenDangNhap, string matKhau = "")
        {
            _taiKhoanDAL.LuuGhiNho(tenDangNhap, matKhau);
        }

        public (string TenDangNhap, string MatKhau) DocGhiNho()
        {
            return _taiKhoanDAL.DocGhiNho();
        }

        public string LayMatKhauGhiNho(string tenDangNhap)
        {
            return _taiKhoanDAL.LayMatKhauGhiNho(tenDangNhap);
        }

        public void XoaGhiNho(string tenDangNhap = null)
        {
            _taiKhoanDAL.XoaGhiNho(tenDangNhap);
        }

        public List<TaiKhoan> GetAll()
        {
            return _taiKhoanDAL.GetAll();
        }

        public bool DoiMatKhau(int taiKhoanID, string matKhauMoiPlain)
        {
            return _taiKhoanDAL.DoiMatKhau(taiKhoanID, matKhauMoiPlain);
        }

        public bool KhoaHoacMoKhoaTaiKhoan(int taiKhoanID, bool khoa)
        {
            return _taiKhoanDAL.KhoaHoacMoKhoaTaiKhoan(taiKhoanID, khoa);
        }

        public bool XoaTaiKhoan(int taiKhoanID)
        {
            return _taiKhoanDAL.XoaTaiKhoan(taiKhoanID);
        }

        public DataTable LayTatCaCoTenNguoiDung()
        {
            return _taiKhoanDAL.LayTatCaCoTenNguoiDung();
        }

        public DataTable TimKiemTheoTenSinhVien(string tenSinhVien)
        {
            return _taiKhoanDAL.TimKiemTheoTenSinhVien(tenSinhVien);
        }

        public bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)
        {
            return _taiKhoanDAL.ThemTaiKhoan(tenDangNhap, matKhauPlain, vaiTro, nguoiDungID);
        }
    }
}

