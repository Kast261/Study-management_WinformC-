using BLT_Quan_Ly_Hoc_Tap.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces
{
    public interface ITaiKhoanService
    {
        string LayHoTen(int taiKhoanID);
        TaiKhoan DangNhap(string tenDangNhap, string matKhauNhap);
        void LuuGhiNho(string tenDangNhap, string matKhau = "");
        (string TenDangNhap, string MatKhau) DocGhiNho();
        string LayMatKhauGhiNho(string tenDangNhap);
        void XoaGhiNho(string tenDangNhap = null);
        List<TaiKhoan> GetAll();
        bool DoiMatKhau(int taiKhoanID, string matKhauMoiPlain);
        bool DoiMatKhauCaNhan(int taiKhoanID, string matKhauCu, string matKhauMoi);
        bool DoiMatKhauGiaoVien(int taiKhoanID, string matKhauCu, string matKhauMoi);
        bool KhoaHoacMoKhoaTaiKhoan(int taiKhoanID, bool khoa);
        bool XoaTaiKhoan(int taiKhoanID);
        DataTable LayTatCaCoTenNguoiDung();
        DataTable TimKiemTheoTenSinhVien(string tenSinhVien);
        DataTable LayHoSoChuaCoTaiKhoan(string vaiTro);
        bool ThemTaiKhoan(string tenDangNhap, string matKhauPlain, string vaiTro, int nguoiDungID);
    }
}
