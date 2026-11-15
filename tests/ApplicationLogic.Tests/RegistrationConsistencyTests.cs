using System;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class RegistrationConsistencyTests
    {
        [Fact]
        public void LegacyPendingRequestAppearsOnceAndCanBeCancelledAndRegisteredAgain()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (2,22,NULL)");
            var classes = new LopHocDAL();
            Assert.DoesNotContain(classes.LayLopChuaDangKy(22).Rows.Cast<DataRow>(), r => (int)r["LopHocID"] == 2);
            var pending = Assert.Single(classes.LayLopHocVienDangHocVaChoDuyet(22).Rows.Cast<DataRow>());
            Assert.Equal("Chờ duyệt", pending["TrangThaiDangKy"]);
            Assert.True((int)pending["DangKyID"] > 0);
            Assert.False(classes.DangKyLop(2, 22, out _));
            Assert.Single(classes.LayDangKyChoDuyet(101).Rows);
            Assert.True(classes.HuyDangKyLop(2, 22, out _));
            Assert.Empty(classes.LayLopHocVienDangHocVaChoDuyet(22).Rows);
            Assert.True(classes.DangKyLop(2, 22, out _));
        }

        [Fact]
        public void LegacyPendingRequestDoesNotProduceAnotherPrimaryClassRow()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (1,11,NULL)");
            var row = Assert.Single(new LopHocDAL().LayLopHocVienDangHocVaChoDuyet(11).Rows.Cast<DataRow>());
            Assert.Equal("Chờ duyệt", row["TrangThaiDangKy"]);
            Assert.True((int)row["DangKyID"] > 0);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void OnlyActiveAssignedTeacherCanRejectPendingRequest(bool legacyPending)
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (2,22,@status)",
                new Npgsql.NpgsqlParameter("status", legacyPending ? (object)DBNull.Value : "Chờ duyệt"));
            int request = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop"));
            var service = new LopHocService();
            Assert.False(service.TuChoiDangKy(request, 202));
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, new GiaoVienService().XoaChiTiet(101));
            Assert.False(service.TuChoiDangKy(request, 101));
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (202,'Thay thế');
                UPDATE LopHoc SET GiaoVienID=202 WHERE LopHocID=2;");
            Assert.True(service.TuChoiDangKy(request, 202));
            Assert.Equal("Từ chối", DatabaseHelper.ExecuteScalar("SELECT TrangThai FROM DangKyLop"));
            Assert.False(service.TuChoiDangKy(request, 202));
        }
    }
}
