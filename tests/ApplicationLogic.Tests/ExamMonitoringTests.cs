using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ExamMonitoringTests
    {
        [Fact]
        public void FocusTransitionsCountOncePerDepartureAndIgnoreInternalDialogs()
        {
            var state = new GiamSatPhongThi();
            Assert.False(state.RoiCuaSo(false));
            state.BatDau(0);
            Assert.False(state.RoiCuaSo(true));
            Assert.Equal(0, state.SoLan);
            for (int count = 1; count <= 3; count++)
            {
                state.KichHoat();
                Assert.True(state.RoiCuaSo(false));
                Assert.False(state.RoiCuaSo(false));
                Assert.Equal(count, state.SoLan);
                Assert.Equal(count == 3, state.BiDung);
            }
            state.KichHoat();Assert.False(state.RoiCuaSo(false));
            state.KetThuc();Assert.False(state.RoiCuaSo(false));
        }

        [Fact]
        public void ReopeningKeepsRecordedViolationLimit()
        {
            var state = new GiamSatPhongThi();state.BatDau(2);
            Assert.True(state.RoiCuaSo(false));Assert.True(state.BiDung);
            state = new GiamSatPhongThi();state.BatDau(3);
            Assert.True(state.BiDung);Assert.False(state.RoiCuaSo(false));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void SavedCountAndStoppedStatusAppearInTeacherResults(int count)
        {
            using var db = new LogicDatabase();
            var monitoring = new GiamSatThiDAL();
            Assert.Equal(0, monitoring.BatDau(1, 11, 1));
            if (count > 0) Assert.Equal(count, monitoring.GhiNhan(1, 11, 1, count));
            Assert.Equal(count, monitoring.BatDau(1, 11, 1));
            var pending = new DeThiDAL().LayKetQuaTheoDeThi(1);
            if (count == 0) Assert.Empty(pending.Rows);
            else
            {
                var violation = Assert.Single(pending.Rows.Cast<System.Data.DataRow>());
                Assert.Equal(false, violation["DaNop"]);
                Assert.True(violation.IsNull("Diem"));
                Assert.Equal(count, violation["SoLanRoiManHinh"]);
            }
            var result = new KetQuaDAL().NopBai(1, 11, 1, new Dictionary<int, string> { [1] = "A" });
            Assert.Equal(count, result.SoLanRoiManHinh);
            Assert.Equal(5m, result.Diem);
            Assert.Equal(count == 3 ? "Dừng do vi phạm" : "Hoàn thành", result.TrangThai);
            var row = Assert.Single(new DeThiDAL().LayKetQuaTheoDeThi(1).Rows.Cast<System.Data.DataRow>());
            Assert.Equal(count, row["SoLanRoiManHinh"]);
            Assert.Equal(result.TrangThai, row["TrangThaiBaiThi"]);
            Assert.Equal(true, row["DaNop"]);
            Assert.Throws<InvalidOperationException>(() => monitoring.GhiNhan(1, 11, 1, 3));
            Assert.Throws<InvalidOperationException>(() => monitoring.BatDau(1, 11, 1));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
        }

        [Fact]
        public async Task RetriedAndConcurrentReportsCannotDoubleCountOrDecreaseViolations()
        {
            using var db = new LogicDatabase();
            var monitoring = new GiamSatThiDAL();monitoring.BatDau(1, 11, 1);
            await Task.WhenAll(Task.Run(() => monitoring.GhiNhan(1, 11, 1, 1)),
                Task.Run(() => monitoring.GhiNhan(1, 11, 1, 3)), Task.Run(() => monitoring.GhiNhan(1, 11, 1, 2)));
            Assert.Equal(3, monitoring.GhiNhan(1, 11, 1, 3));
            Assert.Equal(3, monitoring.BatDau(1, 11, 1));
            Assert.NotNull(DatabaseHelper.ExecuteScalar("SELECT LanCuoi FROM study_giam_sat_thi"));
        }

        [Theory]
        [InlineData(3, 11)]
        [InlineData(1, 22)]
        [InlineData(2, 101)]
        public void OtherAccountsCannotWriteStudentViolations(int account, int student)
        {
            using var db = new LogicDatabase();
            var monitoring = new GiamSatThiDAL();monitoring.BatDau(1, 11, 1);
            Assert.Throws<InvalidOperationException>(() => monitoring.GhiNhan(account, student, 1, 3));
            Assert.Equal(0, monitoring.BatDau(1, 11, 1));
        }

        [Fact]
        public void LegacyResultsStayReadableBeforeMonitoringTableExists()
        {
            using var db = new LogicDatabase();
            new KetQuaDAL().NopBai(1, 11, 1, new Dictionary<int, string>());
            var row = new DeThiDAL().LayKetQuaTheoDeThi(1).Rows[0];
            Assert.Equal(0, row["SoLanRoiManHinh"]);
            Assert.Equal("Hoàn thành", row["TrangThaiBaiThi"]);
        }
    }
}
