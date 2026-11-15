using System.Collections.Generic;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Npgsql;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class RoleNormalizationTests
    {
        [Theory]
        [InlineData(" hOCSinh ")]
        [InlineData(" hOCVien ")]
        public void StudentAcceptedAtLoginCanReadEditSubmitAndChangePassword(string role)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro=@role WHERE TaiKhoanID=1", new NpgsqlParameter("role", role));
            var accounts = new TaiKhoanDAL();
            Assert.False(accounts.DangNhap("student1", "Oldpass1").TrangThaiKhoa);
            var students = new HocSinhDAL();
            var student = students.LayTheoTaiKhoan(1);
            Assert.NotNull(student);
            student.HoTen = "Đã cập nhật";
            Assert.True(students.CapNhatHoSoCaNhan(1, student));
            Assert.Equal(10m, new KetQuaDAL().NopBai(1, 11, 1,
                new Dictionary<int, string> { [1] = "A", [2] = "B" }).Diem);
            Assert.True(accounts.DoiMatKhauCaNhan(1, "Oldpass1", "Newpass2"));
            Assert.NotNull(accounts.DangNhap("student1", "Newpass2"));
        }

        [Fact]
        public void TeacherAcceptedAtLoginCanEditImportAssignDeleteAndChangePassword()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro=' gIAOVien ' WHERE TaiKhoanID=2");
            var accounts = new TaiKhoanDAL();
            Assert.False(accounts.DangNhap("student2", "Oldpass1").TrangThaiKhoa);
            Assert.True(new GiaoVienDAL().CapNhatHoSoCaNhan(2,
                new GiaoVien { GiaoVienID = 101, HoTen = "Giáo viên sửa" }));
            var imported = new CauHoiImportDAL().Nhap(2,
                new DeThi { GiaoVienID = 101, TenDeThi = "Đề mới", MonHoc = "Toán" },
                new List<CauHoi> { new CauHoi { NoiDung = "Câu hỏi", DapAnA = "A", DapAnB = "B",
                    DapAnC = "C", DapAnD = "D", DapAnDung = "A" } });
            var exams = new DeThiDAL();
            int assigned = exams.TaoLanGiaoDeThi(2, 101, imported.DeThiID, 2, 45, null);
            Assert.True(assigned > 0);
            Assert.True(exams.XoaDeThi(assigned, 2, 101, out _));
            Assert.True(accounts.DoiMatKhauGiaoVien(2, "Oldpass1", "Newpass2"));
            Assert.NotNull(accounts.DangNhap("student2", "Newpass2"));
        }
    }
}
