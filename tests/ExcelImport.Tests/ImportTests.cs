using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Npgsql;
using Xunit;

namespace ExcelImport.Tests
{
    public class ImportTests
    {
        private static List<CauHoi> Questions() => new List<CauHoi>
        {
            new CauHoi { NoiDung = "  Câu thứ nhất  ", DapAnA = " A ", DapAnB = "B", DapAnC = "C", DapAnD = "D", DapAnDung = "a" },
            new CauHoi { NoiDung = "Câu thứ hai", DapAnA = "1", DapAnB = "2", DapAnC = "3", DapAnD = "4", DapAnDung = "C" }
        };
        private static DeThi Target(int id = 0) => new DeThi
        {
            DeThiID = id, GiaoVienID = 11, TenDeThi = "Đề nhập Excel", MonHoc = "Lập trình C#"
        };

        [Fact]
        public void ImportsRealTemplateIntoANewDraftAndReloadsUsingExistingQuestionTabDal()
        {
            using var db = new ImportDatabase();
            var questions = CauHoiExcelReader.Read(ExcelReaderTests.Fixture("CauHoi_Mau.xlsx"));
            var result = new CauHoiImportDAL().Nhap(1, Target(), questions);
            Assert.True(result.TaoDeThiMoi);
            Assert.Equal(2, result.SoCauDaNhap);
            Assert.Equal(2, result.TongSoCau);
            Assert.Equal("Chưa giao", DatabaseHelper.ExecuteScalar("SELECT TrangThai FROM DeThi"));
            Assert.Equal(DBNull.Value, DatabaseHelper.ExecuteScalar("SELECT LopHocID FROM DeThi"));
            Assert.Equal(45, DatabaseHelper.ExecuteScalar("SELECT ThoiGian FROM DeThi"));
            var saved = new DeThiDAL().LayDanhSachCauHoi(result.DeThiID);
            Assert.Equal(2, saved.Count);
            Assert.Equal(questions[0].NoiDung, saved[0].NoiDung);
            Assert.Equal(questions[0].DapAnA, saved[0].DapAnA);
            Assert.Equal("A", saved[0].DapAnDung);
            Assert.Equal("C", saved[1].DapAnDung);
            Assert.Equal(new[] { 1, 2 }, saved.Select(q => q.ThuTu));
        }

        [Fact]
        public void AppendsAfterMaximumOrderAndRecalculatesStaleQuestionCount()
        {
            using var db = new ImportDatabase();
            int examId = ImportDatabase.CreateExam();
            ImportDatabase.AddOldQuestion(examId, 1);
            ImportDatabase.AddOldQuestion(examId, 5);
            DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET SoCau=999");
            var result = new CauHoiImportDAL().Nhap(1, Target(examId), Questions());
            Assert.False(result.TaoDeThiMoi);
            Assert.Equal(4, result.TongSoCau);
            var saved = new DeThiDAL().LayDanhSachCauHoi(examId);
            Assert.Equal(new[] { 1, 5, 6, 7 }, saved.Select(q => q.ThuTu));
            Assert.Equal("Câu hỏi cũ", saved[0].NoiDung);
            Assert.Equal("Câu thứ nhất", saved[2].NoiDung);
            Assert.Equal("A", saved[2].DapAnA);
            Assert.Equal("A", saved[2].DapAnDung);
        }

        [Fact]
        public void CannotImportIntoAnotherTeachersExam()
        {
            using var db = new ImportDatabase();
            int examId = ImportDatabase.CreateExam(22);
            Assert.Throws<InvalidOperationException>(() => new CauHoiImportDAL().Nhap(1, Target(examId), Questions()));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
        }

        [Theory]
        [InlineData("HocSinh", false)]
        [InlineData("Admin", false)]
        [InlineData("GiaoVien", true)]
        public void RejectsWrongRolesAndLockedAccounts(string role, bool locked)
        {
            using var db = new ImportDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET VaiTro=@role, TrangThaiKhoa=@locked WHERE TaiKhoanID=1",
                new NpgsqlParameter("role", role), new NpgsqlParameter("locked", locked));
            Assert.Throws<InvalidOperationException>(() => new CauHoiImportDAL().Nhap(1, Target(), Questions()));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Fact]
        public void SupportsLegacyReverseTeacherLink()
        {
            using var db = new ImportDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE TaiKhoan SET NguoiDungID=0 WHERE TaiKhoanID=1");
            Assert.Equal(2, new CauHoiImportDAL().Nhap(1, Target(), Questions()).TongSoCau);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DoesNotChangeAssignedOrAttemptedExams(bool assigned)
        {
            using var db = new ImportDatabase();
            int examId = ImportDatabase.CreateExam(classId: assigned ? 7 : (int?)null);
            if (!assigned) DatabaseHelper.ExecuteNonQuery("INSERT INTO KetQuaThi VALUES (1,@id)", new NpgsqlParameter("id", examId));
            Assert.Throws<InvalidOperationException>(() => new CauHoiImportDAL().Nhap(1, Target(examId), Questions()));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RollsBackEntireBatchIncludingNewExamWhenDatabaseRejectsARow(bool create)
        {
            using var db = new ImportDatabase();
            int examId = create ? 0 : ImportDatabase.CreateExam();
            if (!create)
            {
                ImportDatabase.AddOldQuestion(examId, 1);
                DatabaseHelper.ExecuteNonQuery("UPDATE DeThi SET SoCau=1");
            }
            var questions = Questions();
            questions[1].NoiDung = "DB_REJECT";
            Assert.Throws<PostgresException>(() => new CauHoiImportDAL().Nhap(1, Target(examId), questions));
            Assert.Equal(create ? 0L : 1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
            Assert.Equal(create ? 0L : 1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM CauHoi"));
            if (!create) Assert.Equal(1, DatabaseHelper.ExecuteScalar("SELECT SoCau FROM DeThi"));
        }

        [Fact]
        public void InvalidQuestionPreventsCreatingAnEmptyExam()
        {
            using var db = new ImportDatabase();
            var questions = Questions();
            questions[1].DapAnB = "";
            Assert.Throws<ArgumentException>(() => new CauHoiImportDAL().Nhap(1, Target(), questions));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM DeThi"));
        }

        [Fact]
        public async Task ConcurrentImportsIntoTheSameExamHaveDistinctConsecutiveOrderNumbers()
        {
            using var db = new ImportDatabase();
            int examId = ImportDatabase.CreateExam();
            await Task.WhenAll(
                Task.Run(() => new CauHoiImportDAL().Nhap(1, Target(examId), Questions())),
                Task.Run(() => new CauHoiImportDAL().Nhap(1, Target(examId), Questions())));
            Assert.Equal(new[] { 1, 2, 3, 4 }, new DeThiDAL().LayDanhSachCauHoi(examId).Select(q => q.ThuTu));
            Assert.Equal(4, DatabaseHelper.ExecuteScalar("SELECT SoCau FROM DeThi"));
        }
    }
}
