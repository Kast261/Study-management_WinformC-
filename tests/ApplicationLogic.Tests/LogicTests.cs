using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace ApplicationLogic.Tests
{
    public class LogicTests
    {
        [Fact]
        public void HashCannotBeUsedAsLoginPasswordButLegacyPasswordsStillWork()
        {
            using var db = new LogicDatabase();
            var dal = new TaiKhoanDAL();
            Assert.Null(dal.DangNhap("student1", Passwords.Hash("Oldpass1")));
            Assert.NotNull(dal.DangNhap("student1", "Oldpass1"));
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET MatKhau='Legacy1' WHERE TaiKhoanID=1");
            Assert.NotNull(dal.DangNhap("student1", "Legacy1"));
        }

        [Fact]
        public void LoginResolvesZeroReverseLinkAndTeacherPasswordHonorsLock()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET NguoiDungID=0 WHERE TaiKhoanID IN (1,2)");
            var dal = new TaiKhoanDAL();
            Assert.Equal(11, dal.DangNhap("student1", "Oldpass1").NguoiDungID);
            Assert.Equal(101, dal.DangNhap("student2", "Oldpass1").NguoiDungID);
            Assert.True(dal.DoiMatKhauGiaoVien(2, "Oldpass1", "Newpass2"));
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET TrangThaiKhoa=TRUE WHERE TaiKhoanID=2");
            Assert.False(dal.DoiMatKhauGiaoVien(2, "Newpass2", "Thirdpass3"));
        }

        [Fact]
        public void TeacherProfilePreservesBirthDateSpecialtyAndEnforcesOwnership()
        {
            using var db = new LogicDatabase();
            var dal = new GiaoVienDAL();
            var edit = new GiaoVien { GiaoVienID=101, HoTen="Tên mới", Email="new@example.com", GioiTinh="Nam" };
            Assert.False(dal.CapNhatHoSoCaNhan(1, edit));
            Assert.True(dal.CapNhatHoSoCaNhan(2, edit));
            Assert.Equal(new DateTime(1980,4,3), dal.LayTheoID(101).NgaySinh);
            Assert.Equal("Toán", dal.LayTheoID(101).ChuyenMon);
            Assert.Equal("Tên mới", dal.LayTheoID(101).HoTen);
        }

        [Fact]
        public void AdminClassEditPreservesDatesAndSchedule()
        {
            using var db = new LogicDatabase();
            Assert.True(new LopHocDAL().SuaThongTinLopCoBan(new LopHoc { LopHocID=1, TenLop="Đổi tên", KhoaHocID=1, GiaoVienID=101, SiSoToiDa=35 }));
            Assert.Equal("Thứ 2", DatabaseHelper.ExecuteScalar("SELECT LichHoc FROM LopHoc WHERE LopHocID=1"));
            Assert.Equal(new DateTime(2026,1,1), DatabaseHelper.ExecuteScalar("SELECT NgayBatDau FROM LopHoc WHERE LopHocID=1"));
        }

        [Fact]
        public void PendingAndRejectedRegistrationsAreNotCurrentClasses()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (2,22,'Chờ duyệt'),(1,22,'Từ chối')");
            Assert.Empty(new LopHocDAL().LayTheoHocSinh(22).Rows);
        }

        [Fact]
        public void ApprovalChecksCapacityIdentityTeacherAndDoesNotRejectApprovedRegistrations()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE LopHoc SET SiSoToiDa=1 WHERE LopHocID=1; INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (1,22,'Chờ duyệt')");
            var dal = new LopHocDAL();
            Assert.False(dal.DuyetDangKy(1,1,22,101));
            DatabaseHelper.ExecuteNonQuery("UPDATE LopHoc SET SiSoToiDa=2 WHERE LopHocID=1");
            Assert.False(dal.DuyetDangKy(1,1,33,101));
            Assert.False(dal.DuyetDangKy(1,1,22,999));
            Assert.True(dal.DuyetDangKy(1,1,22,101));
            Assert.False(dal.TuChoiDangKy(1,101));
            Assert.False(dal.HuyDangKyLop(1,22,out _));
            Assert.Equal(1, DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM HocSinh WHERE HocSinhID=22"));
        }

        [Fact]
        public void PendingCancellationWorksAndDuplicateRegistrationDoesNotCreateTwoRows()
        {
            using var db = new LogicDatabase();
            var dal = new LopHocDAL();
            Assert.True(dal.DangKyLop(2,22,out _));
            Assert.False(dal.DangKyLop(2,22,out _));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DangKyLop"));
            Assert.True(dal.HuyDangKyLop(2,22,out _));
            Assert.Empty(dal.LayLopHocVienDangHocVaChoDuyet(22).Rows);
        }

        [Fact]
        public void ApprovedOtherClassStillCountsAfterPrimaryClassChanges()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (1,11,'Đã duyệt'); UPDATE HocSinh SET LopHocID=2 WHERE HocSinhID=11; UPDATE LopHoc SET SiSoToiDa=1 WHERE LopHocID=1");
            Assert.False(new LopHocDAL().DangKyLop(1,22,out _));
            using var table = new LopHocDAL().LayTheoGiaoVien(101);
            Assert.All(table.Rows.Cast<System.Data.DataRow>(), r => Assert.StartsWith("1/", r["SiSo"].ToString()));
        }

        [Fact]
        public async Task ConcurrentApprovalsCannotExceedCapacity()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE LopHoc SET SiSoToiDa=1 WHERE LopHocID=2; INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (2,22,'Chờ duyệt'),(2,33,'Chờ duyệt')");
            bool[] approved = await Task.WhenAll(Task.Run(()=>new LopHocDAL().DuyetDangKy(1,2,22,101)),Task.Run(()=>new LopHocDAL().DuyetDangKy(2,2,33,101)));
            Assert.Single(approved, x=>x);
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DangKyLop WHERE TrangThai='Đã duyệt'"));
        }

        [Fact]
        public void ExamAccessChecksDeadlineMembershipOpenStatusAndPreviousAttempt()
        {
            using var db = new LogicDatabase();
            var dal = new DeThiDAL();
            Assert.True(dal.CoTheBatDauThi(11,1));
            Assert.False(dal.CoTheBatDauThi(22,1));
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET HanNop=CURRENT_TIMESTAMP-INTERVAL '1 second'");
            Assert.False(dal.CoTheBatDauThi(11,1));
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET HanNop=NULL,TrangThai='Chưa giao'");
            Assert.Empty(dal.LayDeThiChoHocSinh(11).Rows);
        }

        [Fact]
        public void SubmitGradesOnDatabaseAndSavesAllAnswersIncludingBlank()
        {
            using var db = new LogicDatabase();
            var result = new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>{{1," a "},{999,"B"}});
            Assert.Equal(1,result.SoCauDung); Assert.Equal(5M,result.Diem);
            Assert.Equal(2L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            Assert.Equal("", DatabaseHelper.ExecuteScalar("SELECT DapAnChon FROM ChiTietBaiLam WHERE CauHoiID=2"));
            Assert.False(new DeThiDAL().CoTheBatDauThi(11,1));
            Assert.Throws<InvalidOperationException>(()=>new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>()));
        }

        [Fact]
        public void DetailFailureRollsBackWholeSubmissionAndRetrySucceeds()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE ChiTietBaiLam ADD CONSTRAINT reject_second CHECK (CauHoiID<>2)");
            Assert.Throws<Npgsql.PostgresException>(()=>new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>()));
            Assert.Equal(0L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(0L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
            DatabaseHelper.ExecuteNonQuery("ALTER TABLE ChiTietBaiLam DROP CONSTRAINT reject_second");
            Assert.True(new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>()).KetQuaID>0);
        }

        [Fact]
        public void SubmissionRejectsAnotherAccountAndStudentsOutsideClass()
        {
            using var db = new LogicDatabase();
            Assert.Throws<InvalidOperationException>(()=>new KetQuaDAL().NopBai(3,11,1,new Dictionary<int,string>()));
            Assert.Throws<InvalidOperationException>(()=>new KetQuaDAL().NopBai(3,22,1,new Dictionary<int,string>()));
            Assert.Equal(0L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
        }

        [Fact]
        public async Task ConcurrentSubmissionsCreateOneCompleteAttempt()
        {
            using var db = new LogicDatabase();
            Func<bool> submit = ()=> {try {new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>());return true;}catch(InvalidOperationException){return false;}};
            bool[] saved = await Task.WhenAll(Task.Run(submit),Task.Run(submit));
            Assert.Single(saved, x=>x);
            Assert.Equal(1L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM KetQuaThi"));
            Assert.Equal(2L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM ChiTietBaiLam"));
        }

        [Fact]
        public void AssignmentCannotMoveExamOrChangeExistingResults()
        {
            using var db = new LogicDatabase(); var dal = new DeThiDAL();
            Assert.False(dal.GiaoDeThiChoLop(1,2,45,null));
            new KetQuaDAL().NopBai(1,11,1,new Dictionary<int,string>());
            Assert.False(dal.GiaoDeThiChoLop(1,1,60,null));
            Assert.Equal(45,dal.LayTheoID(1).ThoiGian);
            Assert.Equal(101,dal.LayTheoID(1).GiaoVienID);
            Assert.Equal(1,dal.LayTheoID(1).LopHocID);
        }

        [Fact]
        public void DeletionCannotOrphanAccountsExamsOrEnrollments()
        {
            using var db = new LogicDatabase();
            Assert.Throws<InvalidOperationException>(()=>new HocSinhDAL().Xoa(11));
            Assert.Throws<InvalidOperationException>(()=>new GiaoVienDAL().Xoa(101));
            Assert.Throws<Exception>(()=>new LopHocDAL().XoaLop(1));
        }

        [Fact]
        public void AccountCreationCannotTakeOverLinkedProfileOrLeaveBrokenLink()
        {
            using var db = new LogicDatabase(); var dal = new TaiKhoanDAL();
            Assert.False(dal.ThemTaiKhoan("second-account", "Password1", "HocSinh",11));
            Assert.False(dal.ThemTaiKhoan("missing-person", "Password1", "HocSinh",999));
            DatabaseHelper.ExecuteNonQuery("SELECT setval(pg_get_serial_sequence('taikhoan','taikhoanid'), 3)");
            Assert.True(dal.ThemTaiKhoan("NewStudent", "Password1", "HocSinh",33));
            Assert.False(dal.ThemTaiKhoan("newstudent", "Password1", "HocSinh",0));
            Assert.Equal(33,dal.DangNhap("NewStudent","Password1").NguoiDungID);
            Assert.True(Convert.ToInt32(DatabaseHelper.ExecuteScalar("SELECT TaiKhoanID FROM HocSinh WHERE HocSinhID=33"))>0);
        }

        [Fact]
        public void ExplicitAccountLinkDoesNotDuplicateRowsThroughOldReverseLink()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO GiaoVien (GiaoVienID,HoTen,TaiKhoanID) VALUES (102,'Liên kết cũ',2)");
            using var table = new TaiKhoanDAL().LayTatCaCoTenNguoiDung();
            Assert.Equal(3,table.Rows.Count);
            Assert.Single(table.Rows.Cast<System.Data.DataRow>(),r=>r["TenNguoiDung"].ToString()=="Giáo viên A");
        }

        [Fact]
        public void NullClassInOldRegistrationDoesNotHideAllAvailableClasses()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("INSERT INTO DangKyLop (LopHocID,HocSinhID,TrangThai) VALUES (NULL,22,'Chờ duyệt')");
            Assert.Equal(2,new LopHocDAL().LayLopChuaDangKy(22).Rows.Count);
        }

        [Fact]
        public void CapacityCannotBeReducedBelowActualAttendance()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE HocSinh SET LopHocID=1 WHERE HocSinhID=22");
            var lop = new LopHoc {LopHocID=1,TenLop="Lớp sửa",GiaoVienID=101,KhoaHocID=1,SiSoToiDa=1};
            var dal = new LopHocDAL();
            Assert.False(dal.SuaThongTinLopCoBan(lop));
            Assert.False(dal.SuaLop(lop));
            Assert.Equal(30,DatabaseHelper.ExecuteScalar("SELECT SiSoToiDa FROM LopHoc WHERE LopHocID=1"));
            lop.SiSoToiDa=2;
            Assert.True(dal.SuaThongTinLopCoBan(lop));
        }

        [Fact]
        public void ProfileLinkFailureRollsBackNewAccount()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                SELECT setval(pg_get_serial_sequence('taikhoan','taikhoanid'), 3);
                ALTER TABLE HocSinh ADD CONSTRAINT reject_link CHECK (HocSinhID<>33 OR TaiKhoanID IS NULL)");
            Assert.Throws<Npgsql.PostgresException>(()=>new TaiKhoanDAL().ThemTaiKhoan("must-rollback","Password1","HocSinh",33));
            Assert.Equal(3L,DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM TaiKhoan"));
            Assert.Null(new TaiKhoanDAL().DangNhap("must-rollback","Password1"));
        }

        [Fact]
        public void AmbiguousReverseLoginLinkDoesNotSelectAnArbitraryProfile()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET NguoiDungID=0 WHERE TaiKhoanID=2; INSERT INTO GiaoVien (GiaoVienID,HoTen,TaiKhoanID) VALUES (102,'Trùng liên kết',2)");
            Assert.Equal(0,new TaiKhoanDAL().DangNhap("student2","Oldpass1").NguoiDungID);
        }

        private static CauHoi Question() => new CauHoi {DeThiID=1,CauHoiID=1,NoiDung="Sửa",DapAnA="A",DapAnB="B",DapAnC="C",DapAnD="D",DapAnDung="A"};

        [Fact]
        public void AssignedExamQuestionsCannotBeAddedEditedOrDeleted()
        {
            using var db = new LogicDatabase(); var dal = new DeThiDAL();
            Assert.Throws<InvalidOperationException>(()=>dal.ThemCauHoi(Question(),101));
            Assert.Throws<InvalidOperationException>(()=>dal.SuaCauHoi(Question(),101));
            Assert.Throws<InvalidOperationException>(()=>dal.XoaCauHoi(1,101));
        }

        [Fact]
        public void DraftQuestionOrderUsesMaximumAndCountStaysAccurate()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET LopHocID=NULL,TrangThai='Chưa giao'; UPDATE CauHoi SET ThuTu=8 WHERE CauHoiID=2");
            var dal = new DeThiDAL(); var question = Question();
            Assert.Throws<InvalidOperationException>(()=>dal.ThemCauHoi(question,999));
            Assert.True(dal.ThemCauHoi(question,101)); Assert.Equal(9,question.ThuTu);
            Assert.Equal(3,DatabaseHelper.ExecuteScalar("SELECT SoCau FROM DeThi WHERE DeThiID=1"));
            Assert.True(dal.XoaCauHoi(1,101));
            Assert.Equal(2,DatabaseHelper.ExecuteScalar("SELECT SoCau FROM DeThi WHERE DeThiID=1"));
        }
    }
}
