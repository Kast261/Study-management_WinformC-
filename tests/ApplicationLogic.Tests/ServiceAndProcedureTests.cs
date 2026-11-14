using System;
using System.Data;
using System.IO;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using StudentProfile.Tests;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ServiceAndProcedureTests
    {
        private static string Script(string name)
        {
            using var stream = typeof(ServiceAndProcedureTests).Assembly.GetManifestResourceStream(name);
            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }

        [Fact]
        public void ProcedureInstallsTwiceAndCrudRoundTripsQuotedVietnameseInput()
        {
            using var database = new TestDatabase();
            DatabaseHelper.ExecuteNonQuery(Script("PostgresSchema.sql"));
            DatabaseHelper.ExecuteNonQuery(Script("PostgresProcedures.sql"));
            DatabaseHelper.ExecuteNonQuery(Script("PostgresProcedures.sql"));
            IKhoaHocService service = new KhoaHocService();
            Assert.True(service.Them(new KhoaHoc { TenKhoaHoc = "Ban đầu" }));
            using var all = service.LayTatCa();
            int id = Convert.ToInt32(all.Rows[0]["KhoaHocID"]);
            var course = new KhoaHoc { KhoaHocID = id, TenKhoaHoc = "  Tiếng Việt ' ; DROP TABLE HocSinh; --", MoTa = null };
            Assert.True(service.Sua(course));
            var saved = service.LayTheoID(id);
            Assert.Equal(course.TenKhoaHoc.Trim(), saved.TenKhoaHoc);
            Assert.False(service.Sua(new KhoaHoc { KhoaHocID = 99999, TenKhoaHoc = "Không có" }));
            Assert.Equal(0L, Convert.ToInt64(DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh")));
            Assert.ThrowsAny<Exception>(() => DatabaseHelper.ExecuteQuery(
                "CALL study_sua_khoa_hoc(1, '', NULL, FALSE)"));
            Assert.True(service.Xoa(id));
            Assert.Null(service.LayTheoID(id));
        }

        [Fact]
        public void MissingProcedureHasActionableSetupError()
        {
            using var database = new TestDatabase(installProcedures: false);
            DatabaseHelper.ExecuteNonQuery(Script("PostgresSchema.sql"));
            var ex = Assert.Throws<InvalidOperationException>(() => new KhoaHocService().Sua(
                new KhoaHoc { KhoaHocID = 1, TenKhoaHoc = "Tên" }));
            Assert.Contains("02_Procedures.sql", ex.Message);
        }

        [Fact]
        public void ServiceKeepsTeacherOwnershipWhenApprovingOrChangingQuestions()
        {
            using var database = new LogicDatabase();
            ILopHocService classes = new LopHocService();
            Assert.True(classes.DangKyLop(2, 22, out _));
            int request = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop WHERE HocSinhID=22"));
            Assert.Throws<ArgumentOutOfRangeException>(() => classes.DuyetDangKy(request, 2, 22, 0));
            Assert.False(classes.DuyetDangKy(request, 2, 22, 999));
            Assert.True(classes.DuyetDangKy(request, 2, 22, 101));
            IDeThiService exams = new DeThiService();
            Assert.Throws<ArgumentOutOfRangeException>(() => exams.XoaCauHoi(1, 0));
            Assert.Throws<InvalidOperationException>(() => exams.XoaCauHoi(1, 999));
            IKetQuaService results = new KetQuaService();
            var result = results.NopBai(1, 11, 1, new System.Collections.Generic.Dictionary<int, string> { [1] = "A", [2] = "B" });
            Assert.Equal(10m, result.Diem);
            Assert.Throws<InvalidOperationException>(() => results.NopBai(1, 11, 1, new System.Collections.Generic.Dictionary<int, string>()));
        }

        [Theory]
        [InlineData("vi-VN")]
        [InlineData("en-US")]
        public void ReportContractPreservesDecimalAndNullsAndMatchesDesignerSchema(string culture)
        {
            var old = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
                var source = new DataTable();
                source.Columns.Add("monhoc"); source.Columns.Add("tendethi");
                source.Columns.Add("diem", typeof(decimal)); source.Columns.Add("thoigiannop", typeof(DateTime));
                source.Rows.Add("Toán", "Bài 1", 7.5m, new DateTime(2026, 4, 3));
                source.Rows.Add("Toán", "Bài 2", 0m, DBNull.Value);
                source.Rows.Add("Văn", "Bài 3", DBNull.Value, DBNull.Value);
                IBaoCaoService service = new BaoCaoService();
                using var result = service.TaoDuLieuBangDiem(source, "Nguyễn Văn A", "giaovien1");
                Assert.Equal(3, result.Tables["BangDiem"].Rows.Count);
                Assert.Equal(7.5m, result.Tables[0].Rows[0]["Diem"]);
                Assert.Equal(0m, result.Tables[0].Rows[1]["Diem"]);
                Assert.True(result.Tables[0].Rows[2].IsNull("Diem"));
                Assert.True(result.Tables[0].Rows[1].IsNull("ThoiGianNop"));
                using var schema = new DataSet();
                using var xsd = typeof(ServiceAndProcedureTests).Assembly.GetManifestResourceStream("BangDiem.xsd");
                schema.ReadXmlSchema(xsd);
                foreach (DataColumn col in result.Tables[0].Columns)
                    Assert.Equal(col.DataType, schema.Tables["BangDiem"].Columns[col.ColumnName].DataType);
                Assert.Equal("Nguyễn Văn A", result.Tables[0].Rows[0]["HoTen"]);
                Assert.Equal(7.5m, source.Rows[0]["diem"]);
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = old; }
        }

        [Fact]
        public void DisplayNameLookupUsesAccountAndSupportsLegacyReverseLink()
        {
            using var database = new LogicDatabase();
            ITaiKhoanService accounts = new TaiKhoanService();
            Assert.Equal("A", accounts.LayHoTen(1));
            Assert.Equal("Giáo viên A", accounts.LayHoTen(2));
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET NguoiDungID=0 WHERE TaiKhoanID=1");
            Assert.Equal("A", accounts.LayHoTen(1));
            Assert.Null(accounts.LayHoTen(999));
        }
    }
}
