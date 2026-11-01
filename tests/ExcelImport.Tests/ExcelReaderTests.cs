using System;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ExcelImport.Tests
{
    internal static class ExcelTestStartup
    {
        [ModuleInitializer]
        internal static void Init() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    public class ExcelReaderTests
    {
        internal static string Fixture(string name) => Path.Combine(AppContext.BaseDirectory, "Fixtures", name);

        [Theory]
        [InlineData("CauHoi_Mau.xlsx")]
        [InlineData("valid.xls")]
        public void ReadsRealExcelFormatsAndVietnameseText(string file)
        {
            var questions = CauHoiExcelReader.Read(Fixture(file));
            Assert.Equal(2, questions.Count);
            Assert.Contains("C#", questions[0].NoiDung);
            Assert.Contains("từ khóa", questions[0].NoiDung);
            Assert.Equal("class", questions[0].DapAnA);
            Assert.Equal("A", questions[0].DapAnDung);
            Assert.Equal("C", questions[1].DapAnDung);
        }

        [Fact]
        public void FindsHeadersAfterNotesAndOnLaterSheets()
        {
            var questions = CauHoiExcelReader.Read(Fixture("notes-first.xlsx"));
            Assert.Equal(2, questions.Count);
        }

        [Fact]
        public void SupportsReorderedColumnsOptionalSttAndNumericAnswers()
        {
            var question = Assert.Single(CauHoiExcelReader.Read(Fixture("reordered.xlsx")));
            Assert.Equal("Câu hỏi tiếng Việt", question.NoiDung);
            Assert.Equal("1", question.DapAnA);
            Assert.Equal("2", question.DapAnB);
            Assert.Equal("3", question.DapAnC);
            Assert.Equal("4", question.DapAnD);
            Assert.Equal("B", question.DapAnDung);
        }

        [Fact]
        public void SkipsBlankRowsAndRetainsQuestionOrder()
        {
            var questions = CauHoiExcelReader.Read(Fixture("blank-rows.xlsx"));
            Assert.Equal(2, questions.Count);
            Assert.Equal("A", questions[0].DapAnDung);
            Assert.Equal("C", questions[1].DapAnDung);
        }

        [Fact]
        public void ReportsSheetAndActualRowForEveryInvalidRecord()
        {
            string message = Assert.Throws<InvalidDataException>(() => CauHoiExcelReader.Read(Fixture("invalid.xlsx"))).Message;
            Assert.Contains("CauHoi", message);
            Assert.Contains("dòng 2", message);
            Assert.Contains("Thiếu đáp án A", message);
            Assert.Contains("A, B, C hoặc D", message);
            Assert.Contains("dòng 3", message);
            Assert.Contains("255", message);
        }

        [Theory]
        [InlineData("missing-column.xlsx", "DapAnDung")]
        [InlineData("duplicate-column.xlsx", "trùng cột")]
        [InlineData("empty.xlsx", "chưa có câu hỏi")]
        [InlineData("no-header.xlsx", "6 cột")]
        [InlineData("too-many.xlsx", "5,000")]
        public void RejectsInvalidWorkbooksBeforeAnyDatabaseWrite(string file, string error)
        {
            string message = Assert.Throws<InvalidDataException>(() => CauHoiExcelReader.Read(Fixture(file))).Message;
            if (file == "too-many.xlsx") Assert.Contains("5000", message.Replace(",", "").Replace(".", ""));
            else Assert.Contains(error, message);
        }

        [Fact]
        public void RejectsUnsupportedExtension()
        {
            Assert.Throws<InvalidDataException>(() => CauHoiExcelReader.Read("questions.csv"));
        }

        [Fact]
        public void RejectsOversizedFilesBeforeParsing()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
            try
            {
                using (var stream = File.Create(path)) stream.SetLength(20 * 1024 * 1024 + 1);
                Assert.Contains("20 MB", Assert.Throws<InvalidDataException>(() => CauHoiExcelReader.Read(path)).Message);
            }
            finally { File.Delete(path); }
        }

        [Fact]
        public void RejectsCorruptExcelFiles()
        {
            string path = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".xlsx");
            try
            {
                File.WriteAllText(path, "This is not an Excel file");
                Assert.ThrowsAny<Exception>(() => CauHoiExcelReader.Read(path));
            }
            finally { File.Delete(path); }
        }
    }
}
