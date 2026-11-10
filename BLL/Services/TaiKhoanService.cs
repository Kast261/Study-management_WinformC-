using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    public class TaiKhoanService : ITaiKhoanService
    {
        private readonly TaiKhoanDAL _repository;

        public TaiKhoanService() : this(new TaiKhoanDAL()) { }

        public TaiKhoanService(TaiKhoanDAL repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public string LayHoTen(int taiKhoanID)
        {
            return _repository.LayHoTen(taiKhoanID);
        }

        public TaiKhoan DangNhap(string tenDangNhap, string matKhauNhap)
        {
            return _repository.DangNhap(tenDangNhap, matKhauNhap);
        }

        public void LuuGhiNho(string tenDangNhap, string matKhau = "")
        {
            _repository.LuuGhiNho(tenDangNhap, matKhau);
        }

        public (string TenDangNhap, string MatKhau) DocGhiNho()
        {
            return _repository.DocGhiNho();
        }

        public string LayMatKhauGhiNho(string tenDangNhap)
        {
            return _repository.LayMatKhauGhiNho(tenDangNhap);
        }

        public void XoaGhiNho(string tenDangNhap = null)
        {
            _repository.XoaGhiNho(tenDangNhap);
        }

        public List<TaiKhoan> GetAll()
        {
            return _repository.GetAll();
        }

        public bool DoiMatKhau(int taiKhoanID, string matKhauMoiPlain)
        {
            return _repository.DoiMatKhau(taiKhoanID, matKhauMoiPlain);
        }

        public bool DoiMatKhauCaNhan(int taiKhoanID, string matKhauCu, string matKhauMoi)
        {
            return _repository.DoiMatKhauCaNhan(taiKhoanID, matKhauCu, matKhauMoi);
        }

        public bool DoiMatKhauGiaoVien(int taiKhoanID, string matKhauCu, string matKhauMoi)
        {
            return _repository.DoiMatKhauGiaoVien(taiKhoanID, matKhauCu, matKhauMoi);
        }

        public bool KhoaHoacMoKhoaTaiKhoan(int taiKhoanID, bool khoa)
        {
            return _repository.KhoaHoacMoKhoaTaiKhoan(taiKhoanID, khoa);
        }

        public bool XoaTaiKhoan(int taiKhoanID)
        {
            return _repository.XoaTaiKhoan(taiKhoanID);
        }

        public DataTable LayTatCaCoTenNguoiDung()
        {
            return _repository.LayTatCaCoTenNguoiDung();
        }

        public DataTable TimKiemTheoTenSinhVien(string tenSinhVien)
        {
            return _repository.TimKiemTheoTenSinhVien(tenSinhVien);
        }

        public DataTable LayHoSoChuaCoTaiKhoan(string vaiTro)
        {
            return _repository.LayHoSoChuaCoTaiKhoan(vaiTro);
        }

        public bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID)
        {
            return _repository.ThemTaiKhoan(tenDangNhap, matKhauPlain, vaiTro, nguoiDungID);
        }
    }
}
