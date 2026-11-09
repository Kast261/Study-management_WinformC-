using System;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Database;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using BLT_Quan_Ly_Hoc_Tap.Models;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ExamSelectionTests
    {
        private static DataTable Exams()
        {
            var table = new DataTable();
            table.Columns.Add("DeThiID", typeof(int));
            table.Columns.Add("TenDeThi"); table.Columns.Add("MonHoc");
            table.Columns.Add("LopHocID", typeof(int)); table.Columns.Add("TrangThai");
            table.Columns.Add("SoCau", typeof(int));
            table.Rows.Add(1, "Bài Thi Thử", "Mạng máy tính", DBNull.Value, "Chưa giao", 20);
            table.Rows.Add(2, "Bài Thi Thử", "Mạng máy tính", DBNull.Value, "Chưa giao", 0);
            table.Rows.Add(3, "Bài Thi Thử", "Lập trình C# WinForms", DBNull.Value, "Chưa giao", 30);
            table.Rows.Add(4, "Bài Thi Thử", "Mạng máy tính", 10, "Đang mở", 20);
            return table;
        }

        [Fact]
        public void ReopeningDraftPrefersExistingQuestionsOverAnEmptyNamesake()
        {
            var found = ExamSelection.FindDraft(Exams(), "Bài Thi Thử", "MẠNG MÁY TÍNH", 0);
            Assert.Equal(1, found["DeThiID"]);
            Assert.Equal(20, found["SoCau"]);
        }

        [Fact]
        public void ExplicitlySelectedDraftRemainsSelectedDespiteDuplicateNames()
        {
            Assert.Equal(2, ExamSelection.FindDraft(Exams(), "Bài Thi Thử", "Mạng máy tính", 2)["DeThiID"]);
        }

        [Fact]
        public void DraftLookupKeepsSubjectAndExcludesAlreadyAssignedCopies()
        {
            Assert.Equal(3, ExamSelection.FindDraft(Exams(), "Bài Thi Thử", "Lập trình C# WinForms", 1)["DeThiID"]);
            Assert.Equal(1, ExamSelection.FindDraft(Exams(), "Bài Thi Thử", "Mạng máy tính", 4)["DeThiID"]);
            Assert.Null(ExamSelection.FindDraft(Exams(), "Đề mới", "Mạng máy tính", 1));
        }

        [Fact]
        public void ExamGridKeepsOnlyExamsWithSavedQuestions()
        {
            var ready = ExamSelection.WithQuestions(Exams());
            Assert.Equal(new[] { 1, 3, 4 }, ready.Rows.Cast<DataRow>().Select(row => (int)row["DeThiID"]));
            Assert.All(ready.Rows.Cast<DataRow>(), row => Assert.True((int)row["SoCau"] > 0));
        }

        private static DataTable Classes()
        {
            var classes = new DataTable();
            classes.Columns.Add("LopHocID", typeof(int));
            classes.Columns.Add("TenLop"); classes.Columns.Add("TenKhoaHoc");
            classes.Rows.Add(1, "Lập Trình Winform 2", "Lập trình C# WinForms");
            classes.Rows.Add(2, "Mạng Máy Tính N06", "Mạng máy tính");
            classes.Rows.Add(3, "Mạng Máy Tính N07", "Mạng máy tính");
            return classes;
        }

        [Fact]
        public void MultipleMatchingClassesRequireExplicitChoice()
        {
            Assert.Equal(0, ExamSelection.PreferredClassID(Classes(), " Mạng  máy tính ", 1, true));
            Assert.Equal(0, ExamSelection.PreferredClassID(Classes(), "Mạng máy tính", 0, false));
        }

        [Fact]
        public void KeepsAnExplicitMatchingClassWhenMultipleClassesTeachTheSameSubject()
        {
            Assert.Equal(3, ExamSelection.PreferredClassID(Classes(), "Mạng máy tính", 3, true));
            Assert.Equal(1, ExamSelection.PreferredClassID(Classes(), "Mạng máy tính", 1, false));
        }

        [Fact]
        public void LegacyClassNameCanIdentifyTheSubjectWithoutCourseMetadata()
        {
            var classes = Classes();
            classes.Columns.Remove("TenKhoaHoc");
            classes.Rows.RemoveAt(2);
            Assert.Equal(2, ExamSelection.PreferredClassID(classes, "Mạng máy tính", 1, true));
        }

        [Fact]
        public void DuplicateClassNamesAreDistinguishableAndAssignmentReachesSelectedStudents()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery("UPDATE LopHoc SET TenLop='Toán Lớp 2'; UPDATE HocSinh SET LopHocID=2 WHERE HocSinhID=22");
            using var classes = new LopHocDAL().LayTheoGiaoVien(101);
            Assert.Equal(0, ExamSelection.PreferredClassID(classes, "Toán", 0, true));
            Assert.Equal(0, ExamSelection.PreferredClassID(classes, "Toán", 999, false));
            Assert.NotEqual(ExamSelection.ClassDisplayName(classes.Rows[0]), ExamSelection.ClassDisplayName(classes.Rows[1]));
            Assert.All(classes.Rows.Cast<DataRow>(), row =>
                Assert.Contains("Mã lớp #" + row["LopHocID"], ExamSelection.ClassDisplayName(row)));

            int selected = ExamSelection.PreferredClassID(classes, "Toán", 2, true);
            Assert.Equal(2, selected);
            var exams = new DeThiDAL();
            int assigned = exams.TaoLanGiaoDeThi(2, 101, 1, selected, 45, null);
            Assert.Contains(exams.LayDeThiChoHocSinh(22).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == assigned);
            Assert.Contains(exams.LayBaiThiDangMoChoHocSinh(22).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == assigned);
            Assert.True(exams.CoTheBatDauThi(22, assigned));
            Assert.False(exams.CoTheBatDauThi(11, assigned));
        }

        [Fact]
        public void MissingOwnedClassRequiresExplicitChoiceInsteadOfPickingAnUnrelatedClass()
        {
            var classes = Classes();
            classes.Rows.RemoveAt(2); classes.Rows.RemoveAt(1);
            Assert.Equal(0, ExamSelection.PreferredClassID(classes, "Mạng máy tính", 1, true));
        }

        [Fact]
        public void DraftQuestionsCanBeSelectedAndAssignedDirectlyToN06WithoutAddingThemAgain()
        {
            using var db = new LogicDatabase();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO KhoaHoc VALUES (2,'Mạng máy tính');
                UPDATE LopHoc SET TenLop='Lập Trình Winform 2' WHERE LopHocID=1;
                UPDATE LopHoc SET TenLop='Mạng Máy Tính N06',KhoaHocID=2 WHERE LopHocID=2;
                UPDATE HocSinh SET LopHocID=2 WHERE HocSinhID=22;");
            var dal = new DeThiDAL();
            int source = dal.TaoDeThiMoiLayID(new DeThi {
                TenDeThi = "Bài Thi Thử", MonHoc = "Mạng máy tính", GiaoVienID = 101,
                ThoiGian = 45, TrangThai = "Chưa giao"
            });
            for (int i = 1; i <= 20; i++)
                Assert.True(dal.ThemCauHoi(new CauHoi {
                    DeThiID = source, NoiDung = "Câu mạng " + i, DapAnA = "A", DapAnB = "B",
                    DapAnC = "C", DapAnD = "D", DapAnDung = "A"
                }, 101));
            using var exams = dal.LayDeThiTheoGiaoVien(101);
            int selected = (int)ExamSelection.FindDraft(exams, "Bài Thi Thử", "Mạng máy tính", 1)["DeThiID"];
            Assert.Equal(source, selected);
            Assert.Contains(ExamSelection.WithQuestions(exams).Rows.Cast<DataRow>(), row => (int)row["DeThiID"] == source);
            int target = ExamSelection.PreferredClassID(new LopHocDAL().LayTheoGiaoVien(101), "Mạng máy tính", 1, true);
            Assert.Equal(2, target);
            int assigned = dal.TaoLanGiaoDeThi(2, 101, selected, target, 45, null);
            Assert.Equal(20, dal.DemSoCauHoiHienTai(assigned));
            Assert.True(dal.CoTheBatDauThi(22, assigned));
            Assert.Equal(20, dal.DemSoCauHoiHienTai(source));
            var answers = dal.LayDanhSachCauHoi(assigned).ToDictionary(q => q.CauHoiID, q => q.DapAnDung);
            Assert.Equal(10M, new KetQuaDAL().NopBai(3, 22, assigned, answers).Diem);
        }
    }
}
