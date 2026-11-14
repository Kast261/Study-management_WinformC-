using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BLT_Quan_Ly_Hoc_Tap.BLL.Services;
using BLT_Quan_Ly_Hoc_Tap.DAL;
using BLT_Quan_Ly_Hoc_Tap.Models;
using BLT_Quan_Ly_Hoc_Tap.Database;
using Npgsql;
using StudentProfile.Tests;
using Xunit;

namespace ApplicationLogic.Tests
{
    public class ProfileDeletionRecoveryTests
    {
        private static TestDatabase Database(bool install = false)
        {
            var db = new TestDatabase(installProcedures: install);
            using var stream = typeof(ProfileDeletionRecoveryTests).Assembly.GetManifestResourceStream("PostgresSchema.sql");
            using var reader = new StreamReader(stream);
            DatabaseHelper.ExecuteNonQuery(reader.ReadToEnd());
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (101,'Giáo viên cần xóa');
                INSERT INTO HocSinh (HocSinhID,HoTen) VALUES (11,'Học viên cần xóa');");
            return db;
        }

        private static bool Delete(bool teacher, int id) => teacher
            ? new GiaoVienService().Xoa(id) : new HocSinhService().Xoa(id);
        private static string Table(bool teacher) => teacher ? "GiaoVien" : "HocSinh";
        private static string Procedure(bool teacher) => teacher ? "study_xoa_giao_vien" : "study_xoa_hoc_sinh";
        private static long ProcedureCount() => Convert.ToInt64(DatabaseHelper.ExecuteScalar(@"
            SELECT COUNT(*) FROM pg_proc WHERE pronamespace=current_schema()::regnamespace
                AND prokind='p' AND proname LIKE 'study_%'"));

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeletionInstallsProceduresOnAnOldDatabaseAndRetries(bool teacher)
        {
            using var db = Database();
            Assert.Equal(0L, ProcedureCount());
            Assert.True(Delete(teacher, teacher ? 101 : 11));
            Assert.Equal(10L, ProcedureCount());
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(!teacher)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void DeletionRepairsOnlyMissingDeleteEntryPointAndStillUsesItsResult(bool teacher)
        {
            using var db = Database(install: true);
            DatabaseHelper.ExecuteNonQuery("DROP PROCEDURE " + Procedure(teacher) + "(integer,boolean,boolean)");
            Assert.True(Delete(teacher, teacher ? 101 : 11));
            Assert.Equal(10L, ProcedureCount());
            Assert.False(Delete(teacher, teacher ? 101 : 11));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void RecoveredProcedureArchivesProfilesAndKeepsLearningHistory(bool teacher)
        {
            using var db = Database();
            DatabaseHelper.ExecuteNonQuery(teacher
                ? "INSERT INTO LopHoc (TenLop,GiaoVienID) VALUES ('Lớp đang dạy',101)"
                : "INSERT INTO KetQuaThi (HocSinhID,Diem) VALUES (11,7.5)");
            var result = teacher ? new GiaoVienService().XoaChiTiet(101) : new HocSinhService().XoaChiTiet(11);
            Assert.Equal(KetQuaXoaHoSo.DaLuuTru, result);
            Assert.Equal(true, DatabaseHelper.ExecuteScalar("SELECT DaXoa FROM " + Table(teacher)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + (teacher ? "LopHoc" : "KetQuaThi")));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void MissingProfileDoesNotTurnIntoASuccessfulDeleteAfterRecovery(bool teacher)
        {
            using var db = Database();
            Assert.False(Delete(teacher, 99999));
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
        }

        [Fact]
        public async Task SimultaneousDeletesRecoverWithoutConflictingProcedureCreation()
        {
            using var db = Database();
            DatabaseHelper.ExecuteNonQuery(@"
                INSERT INTO GiaoVien (GiaoVienID,HoTen) VALUES (102,'Giáo viên 2');
                INSERT INTO HocSinh (HocSinhID,HoTen) VALUES (12,'Học viên 2');");
            var outcomes = await Task.WhenAll(
                Task.Run(() => Delete(true, 101)), Task.Run(() => Delete(true, 102)),
                Task.Run(() => Delete(false, 11)), Task.Run(() => Delete(false, 12)));
            Assert.All(outcomes, result => Assert.True(result));
            Assert.Equal(10L, ProcedureCount());
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
            Assert.Equal(0L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM HocSinh"));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void NoCreatePermissionGivesActionableMessageAndNeverDeletesDirectly(bool teacher)
        {
            using var db = Database();
            string original = DatabaseHelper.ConnectionString;
            string role = "profile_delete_" + Guid.NewGuid().ToString("N");
            DatabaseHelper.ExecuteNonQuery("CREATE ROLE " + role);
            try
            {
                DatabaseHelper.ExecuteNonQuery("GRANT USAGE ON SCHEMA "
                    + new NpgsqlConnectionStringBuilder(original).SearchPath + " TO " + role);
                DatabaseHelper.ExecuteNonQuery("GRANT SELECT,DELETE ON GiaoVien,HocSinh TO " + role);
                DatabaseHelper.ConnectionString = new NpgsqlConnectionStringBuilder(original)
                {
                    Options = "-c role=" + role
                }.ConnectionString;
                var error = Assert.Throws<InvalidOperationException>(() => Delete(teacher, teacher ? 101 : 11));
                Assert.Contains("không đủ quyền", error.Message);
                Assert.Contains("02_Procedures.sql", error.Message);
            }
            finally
            {
                DatabaseHelper.ConnectionString = original;
                DatabaseHelper.ExecuteNonQuery("DROP OWNED BY " + role + "; DROP ROLE " + role);
            }
            Assert.Equal(0L, ProcedureCount());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM " + Table(teacher)));
        }

        [Fact]
        public void FailedInstallationRollsBackAllNewProceduresAndLeavesProfiles()
        {
            using var db = Database();
            string trigger = "reject_procedure_" + Guid.NewGuid().ToString("N");
            DatabaseHelper.ExecuteNonQuery(@"
                CREATE FUNCTION reject_teacher_procedure() RETURNS event_trigger LANGUAGE plpgsql AS $$
                DECLARE command record;
                BEGIN
                    FOR command IN SELECT * FROM pg_event_trigger_ddl_commands() LOOP
                        IF command.object_identity LIKE '%study_them_giao_vien(%' THEN
                            RAISE EXCEPTION 'Test: fail installation midway';
                        END IF;
                    END LOOP;
                END; $$;
                CREATE EVENT TRIGGER " + trigger + @" ON ddl_command_end
                    WHEN TAG IN ('CREATE PROCEDURE') EXECUTE FUNCTION reject_teacher_procedure();");
            try
            {
                var error = Assert.Throws<PostgresException>(() => Delete(true, 101));
                Assert.Equal("P0001", error.SqlState);
            }
            finally { DatabaseHelper.ExecuteNonQuery("DROP EVENT TRIGGER " + trigger); }
            Assert.Equal(0L, ProcedureCount());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
        }

        [Fact]
        public void UndefinedFunctionInsideAnExistingProcedureIsNotMistakenForMissingProcedure()
        {
            using var db = Database();
            DatabaseHelper.ExecuteNonQuery(@"
                CREATE PROCEDURE study_xoa_giao_vien(IN p_id integer, INOUT p_deleted boolean, INOUT p_archived boolean)
                LANGUAGE plpgsql AS $$ BEGIN PERFORM study_internal_function_not_installed(); END; $$;");
            var error = Assert.Throws<PostgresException>(() => Delete(true, 101));
            Assert.Equal("42883", error.SqlState);
            Assert.Contains("study_internal_function_not_installed", error.MessageText);
            Assert.Equal(1L, ProcedureCount());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
        }

        [Fact]
        public void RetryIsBoundedWhenTheBundledScriptCannotSupplyTheRequestedProcedure()
        {
            using var db = Database();
            var error = Assert.Throws<InvalidOperationException>(() => ProcedureExecutor.Boolean(
                "CALL study_procedure_not_in_this_release(@id,FALSE)", "p_deleted", new NpgsqlParameter("id", 101)));
            Assert.Contains("sau khi cập nhật", error.Message);
            Assert.Equal("42883", Assert.IsType<PostgresException>(error.InnerException).SqlState);
            Assert.Equal(10L, ProcedureCount());
            Assert.Equal(1L, DatabaseHelper.ExecuteScalar("SELECT COUNT(*) FROM GiaoVien"));
        }
    }
}
