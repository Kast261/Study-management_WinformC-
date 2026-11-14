using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Models;
using StudentProfile.Tests;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ProfileArchiveTests
    {
        private static TestDatabase Database()
        {
            var db = new TestDatabase();
            using var stream = typeof(ProfileArchiveTests).Assembly.GetManifestResourceStream("PostgresSchema.sql");
            using var reader = new StreamReader(stream);
            DatabaseHelper.ExecuteNonQuery(reader.ReadToEnd());
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (101,'Giáo viên thử');
                INSERT INTO HocSinh (HocSinhID,HoTen) VALUES (11,'Học viên thử');");
            return db;
        }

        private static KetQuaXoaHoSo Delete(bool teacher) => teacher
            ? new GiaoVienService().XoaChiTiet(101) : new HocSinhService().XoaChiTiet(11);
        private static string Table(bool teacher) => teacher ? "GiaoVien" : "HocSinh";
        private static string Key(bool teacher) => teacher ? "GiaoVienID" : "HocSinhID";

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void UnusedProfileIsPermanentlyDeletedAndResultDistinguishesIt(bool teacher)
        {
            using var db = Database();
            Assert.Equal(KetQuaXoaHoSo.DaXoa, Delete(teacher));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
            Assert.Equal(KetQuaXoaHoSo.KhongTimThay, Delete(teacher));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(!teacher)));
        }

        [Theory]
        [InlineData("GiaoVien", false)]
        [InlineData(" GIAOVIEN ", true)]
        [InlineData("HocSinh", false)]
        [InlineData("HocVien", true)]
        [InlineData(" hOCvIEN ", false)]
        public void LinkedProfileIsArchivedAndOnlyItsOwnAccountIsLocked(string role, bool reverse)
        {
            using var db = Database();
            bool teacher = role.Trim().Equals("GiaoVien", StringComparison.OrdinalIgnoreCase);
            int id = teacher ? 101 : 11;
            db.SeedAccount(1, reverse ? 0 : id, role);
            db.SeedAccount(2, id, "Admin");
            db.SeedAccount(3, id, teacher ? "HocSinh" : "GiaoVien");
            if (reverse) DatabaseHelper.ExecuteNonQuery("UPDATE " + Table(teacher) + " SET TaiKhoanID=1");
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(teacher));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT DaXoa FROM " + Table(teacher)));
            Assert.NotEqual(DBNull.Value, DatabaseHelper.ExecuteScalar("SELECT NgayXoa FROM " + Table(teacher)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=1"));
            Assert.Equal(false, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=2"));
            Assert.Equal(false, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=3"));
            Assert.Equal(3L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
            Assert.Equal(KetQuaXoaHoSo.KhongTimThay, Delete(teacher));
            var accounts = new TaiKhoanService();
            Assert.False(accounts.KhoaHoacMoKhoaTaiKhoan(1, false));
            // A manual DB unlock must not bypass the archived-profile check in login.
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET TrangThaiKhoa=FALSE WHERE TaiKhoanID=1");
            Assert.True(accounts.DangNhap("student1", "Oldpass1").TrangThaiKhoa);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ArchiveHidesProfileFromListsSearchAndAccountAssignmentAndBlocksEdits(bool teacher)
        {
            using var db = Database();
            DatabaseHelper.ExecuteNonQuery(teacher
                ? "INSERT INTO LopHoc (TenLop,GiaoVienID) VALUES ('Lớp có giáo viên',101)"
                : "INSERT INTO DangKyLop (HocSinhID,TrangThai) VALUES (11,'Chờ duyệt')");
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(teacher));
            var accounts = new TaiKhoanService();
            Assert.Empty(accounts.LayHoSoChuaCoTaiKhoan(teacher ? "GiaoVien" : "HocSinh").Rows);
            Assert.False(accounts.ThemTaiKhoan("cannot-reassign", "Example1", teacher ? "GiaoVien" : "HocSinh", teacher ? 101 : 11));
            if (teacher)
            {
                var service = new GiaoVienService();
                Assert.Empty(service.LayTatCa().Rows);Assert.Empty(service.TimKiem("thử").Rows);
                Assert.Null(service.LayTheoID(101));
                Assert.False(service.Sua(new GiaoVien { GiaoVienID = 101, HoTen = "Không được đổi" }));
            }
            else
            {
                var service = new HocSinhService();
                Assert.Empty(service.LayTatCa().Rows);Assert.Empty(service.TimKiem("thử").Rows);
                Assert.Null(service.LayTheoID(11));
                Assert.False(service.Sua(new HocSinh { HocSinhID = 11, HoTen = "Không được đổi" }));
            }
            Assert.Equal(teacher ? "Giáo viên thử" : "Học viên thử", DatabaseHelper.ExecuteScalar("SELECT HoTen FROM " + Table(teacher)));
        }

        [Fact]
        public void ArchivingStudentKeepsGradesAnswersAndMembershipButRemovesActiveCapacity()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (HocSinhID,LopHocID,TrangThai) VALUES (11,2,'Đã duyệt')");
            int result = new KetQuaService().NopBai(1, 11, 1,
                new System.Collections.Generic.Dictionary<int, string> { [1] = "A", [2] = "B" }).KetQuaID;
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, new HocSinhService().XoaChiTiet(11));
            Assert.Equal(1, DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DangKyLop WHERE HocSinhID=11"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi WHERE HocSinhID=11"));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam WHERE KetQuaID=@id", new Npgsql.NpgsqlParameter("id", result)));
            Assert.Equal(2, new KetQuaService().LayChiTietBaiLamTheoKetQua(result).Rows.Count);
            Assert.Single(new DeThiService().LayKetQuaTheoDeThi(1).Rows);
            Assert.Single(new KetQuaService().LayLichSuTheoHocSinh(11).Rows);
            var classes = new LopHocService();
            Assert.All(classes.LayTatCa().Rows.Cast<System.Data.DataRow>(), row => Assert.StartsWith("0/", row["SiSo"].ToString()));
            Assert.Empty(classes.LayTheoHocSinh(11).Rows);
            Assert.Empty(classes.LayLopChuaDangKy(11).Rows);
            Assert.Empty(classes.LayLopHocVienDangHocVaChoDuyet(11).Rows);
            Assert.Empty(new DeThiService().LayDeThiChoHocSinh(11).Rows);
            Assert.False(new DeThiService().CoTheBatDauThi(11, 1));
            var dashboard = new DashboardService();
            Assert.Equal(2L, Convert.ToInt64(dashboard.LayTongQuan().Rows[0]["SoHocVien"]));
            Assert.Equal(0L, Convert.ToInt64(dashboard.LayHocVienTheoKhoaHoc().Rows[0]["SoHocVien"]));
            Assert.Equal(0L, Convert.ToInt64(dashboard.LayTongQuanGiaoVien(101).Rows[0]["SoHocVien"]));
            Assert.All(dashboard.LayLopMoiNhat().Rows.Cast<System.Data.DataRow>(), row => Assert.Equal(0L, Convert.ToInt64(row["SoHocVien"])));
        }

        [Fact]
        public void ArchivedStudentCannotRegisterBeApprovedSubmitOrEditUsingAStaleSession()
        {
            using var db = new LogicDatabase();
            var classes = new LopHocService();Assert.True(classes.DangKyLop(2, 22, out _));
            int request = Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT DangKyID FROM DangKyLop WHERE HocSinhID=22"));
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, new HocSinhService().XoaChiTiet(22));
            Assert.False(classes.DuyetDangKy(request, 2, 22, 101));
            Assert.Empty(classes.LayDangKyChoDuyet(101).Rows);
            Assert.False(classes.DangKyLop(1, 22, out _));
            Assert.False(new HocSinhService().CapNhatHoSoCaNhan(3, new HocSinh { HocSinhID = 22, HoTen = "Không được sửa" }));
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET TrangThaiKhoa=FALSE WHERE TaiKhoanID=3");
            Assert.Throws<InvalidOperationException>(() => new KetQuaService().NopBai(3, 22, 1, new System.Collections.Generic.Dictionary<int, string>()));
        }

        [Fact]
        public void ArchivingTeacherPreservesClassesExamsAndHistoricalNameAndRejectsNewWork()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (202,'Giáo viên thay thế')");
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, new GiaoVienService().XoaChiTiet(101));
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM LopHoc WHERE GiaoVienID=101"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi WHERE GiaoVienID=101"));
            Assert.Equal("Giáo viên A", new DashboardService().LayLopMoiNhat().Rows[0]["TenGiaoVien"]);
            Assert.Equal(1L, Convert.ToInt64(new DashboardService().LayTongQuan().Rows[0]["SoGiaoVien"]));
            Assert.False(new LopHocService().DongDangKyLop(1, 101));
            Assert.False(new LopHocService().ThemLop(new LopHoc { TenLop = "Không được tạo", GiaoVienID = 101 }));
            Assert.False(new LopHocService().SuaLop(new LopHoc { LopHocID = 1, TenLop = "Không được gán", GiaoVienID = 101, SiSoToiDa = 30 }));
            Assert.False(new GiaoVienService().CapNhatHoSoCaNhan(2, new GiaoVien { GiaoVienID = 101, HoTen = "Không được sửa" }));
            Assert.True(new LopHocService().SuaLop(new LopHoc { LopHocID = 1, TenLop = "Lớp 1", GiaoVienID = 202, KhoaHocID = 1, SiSoToiDa = 30 }));
            Assert.Equal(202, DatabaseHelper.ExecuteScalar("SELECT GiaoVienID FROM LopHoc WHERE LopHocID=1"));
            Assert.True(new DeThiService().CoTheBatDauThi(11, 1));
            Assert.True(new DeThiService().TaoDeThiMoiLayID(new DeThi { GiaoVienID = 101, TenDeThi = "Không được tạo" }) <= 0);
            Assert.Throws<InvalidOperationException>(() => new DeThiService().TaoLanGiaoDeThi(2, 101, 1, 2, 30, null));
        }

        [Fact]
        public async Task ConcurrentArchiveRequestsReturnOneSuccessWithoutLosingLinkedRows()
        {
            using var db = new LogicDatabase();
            var results = await Task.WhenAll(Enumerable.Range(0, 4).Select(_ => Task.Run(() => new HocSinhService().XoaChiTiet(11))));
            Assert.Single(results.Where(result => result == KetQuaXoaHoSo.DaLuuTru));
            Assert.Equal(3, results.Count(result => result == KetQuaXoaHoSo.KhongTimThay));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(3L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
        }

        [Fact]
        public void ArchiveAndAccountLockRollBackTogetherIfEitherWriteFails()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                CREATE FUNCTION reject_profile_lock() RETURNS trigger LANGUAGE plpgsql AS $$
                BEGIN RAISE EXCEPTION 'Test: cannot lock account'; END; $$;
                CREATE TRIGGER reject_profile_lock BEFORE UPDATE ON TaiKhoan
                    FOR EACH ROW EXECUTE FUNCTION reject_profile_lock();");
            Assert.Throws<Npgsql.PostgresException>(() => new HocSinhService().XoaChiTiet(11));
            Assert.Equal(false, DatabaseHelper.ExecuteScalar("SELECT DaXoa FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(false, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=1"));
            Assert.NotNull(new HocSinhService().LayTheoID(11));
        }

        [Fact]
        public void ArchiveHandlesAdditionalForeignKeyReferencesWithoutCascadingDataLoss()
        {
            using var db = Database();
            DatabaseHelper.ExecuteNonQuery("CREATE TABLE LegacyStudentNote (HocSinhID integer REFERENCES HocSinh(HocSinhID), Note text); INSERT INTO LegacyStudentNote VALUES (11,'Giữ nguyên')");
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(false));
            Assert.Equal("Giữ nguyên", DatabaseHelper.ExecuteScalar("SELECT Note FROM LegacyStudentNote"));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT DaXoa FROM HocSinh WHERE HocSinhID=11"));
        }

        [Fact]
        public void ArchivedStudentNoLongerConsumesASeatWhenCreatingAnotherStudent()
        {
            using var db = Database();
            db.SeedAccount(1, 11);
            DatabaseHelper.ExecuteNonQuery(@"
                ALTER TABLE HocSinh ALTER COLUMN HocSinhID ADD GENERATED BY DEFAULT AS IDENTITY;
                INSERT INTO LopHoc (TenLop,GiaoVienID,SiSoToiDa) VALUES ('Lớp một chỗ',101,1);
                UPDATE HocSinh SET LopHocID=1 WHERE HocSinhID=11;
                INSERT INTO DangKyLop (HocSinhID,LopHocID,TrangThai) VALUES (11,1,'Đã duyệt');");
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(false));
            var student = new HocSinh { HoTen = "Học viên mới" };
            Assert.True(new HocSinhService().Them(student, 1));
            Assert.True(student.HocSinhID > 0);
            Assert.False(new HocSinhService().Them(new HocSinh { HoTen = "Vượt sĩ số" }, 1));
            Assert.Equal("1/1", new LopHocService().LayTatCa().Rows[0]["SiSo"]);
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void StaleReverseLinkDoesNotLockAnAccountAssignedToAnotherProfile(bool teacher)
        {
            using var db = Database();
            string table = Table(teacher);string key = Key(teacher);
            int other = teacher ? 202 : 22;
            DatabaseHelper.ExecuteNonQuery("INSERT INTO " + table + " (" + key + ",HoTen) VALUES (" + other + ",'Người khác')");
            db.SeedAccount(1, other, teacher ? "GiaoVien" : "HocSinh");
            DatabaseHelper.ExecuteNonQuery("UPDATE " + table + " SET TaiKhoanID=1 WHERE " + key + "=" + (teacher ? 101 : 11));
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(teacher));
            Assert.Equal(false, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=1"));
            Assert.False(new TaiKhoanService().DangNhap("student1", "Oldpass1").TrangThaiKhoa);
        }

        [Fact]
        public void ReinstallDoesNotReactivateArchivedProfilesOrLoseHistory()
        {
            using var db = new LogicDatabase();
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, new HocSinhService().XoaChiTiet(11));
            using var stream = typeof(ProfileArchiveTests).Assembly.GetManifestResourceStream("PostgresProcedures.sql");
            using var reader = new StreamReader(stream);
            DatabaseHelper.ExecuteNonQuery(reader.ReadToEnd());
            Assert.Null(new HocSinhService().LayTheoID(11));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(1, DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT TrangThaiKhoa FROM TaiKhoan WHERE TaiKhoanID=1"));
        }

        [Fact]
        public void OldDatabaseAndOldDeleteSignatureUpgradeWithoutRecreatingProfiles()
        {
            using var db = Database();
            db.SeedAccount(1, 11);
            DatabaseHelper.ExecuteNonQuery(@"
                DROP PROCEDURE study_xoa_hoc_sinh(integer,boolean,boolean);
                CREATE PROCEDURE study_xoa_hoc_sinh(IN p_id integer,INOUT p_deleted boolean)
                LANGUAGE plpgsql AS $$ BEGIN RAISE EXCEPTION 'Old blocking procedure'; END; $$;
                ALTER TABLE HocSinh DROP COLUMN DaXoa, DROP COLUMN NgayXoa;
                ALTER TABLE GiaoVien DROP COLUMN DaXoa, DROP COLUMN NgayXoa;");
            Assert.Single(new HocSinhService().LayTatCa().Rows);
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, Delete(false));
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT DaXoa FROM HocSinh WHERE HocSinhID=11"));
            Assert.Equal(10L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM pg_proc WHERE pronamespace=current_schema()::regnamespace AND prokind='p' AND proname LIKE 'study_%'"));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
        }
    }
}
