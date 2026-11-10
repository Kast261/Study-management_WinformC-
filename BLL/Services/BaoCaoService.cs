using System;
using System.Data;
using System.Globalization;
using BLT_Quan_Ly_Hoc_Tap.BLL.Interfaces;

namespace BLT_Quan_Ly_Hoc_Tap.BLL.Services
{
    // Typed, culture-independent data contract shared with the Crystal designer.
    public sealed class BaoCaoService : IBaoCaoService
    {
        public DataSet TaoDuLieuBangDiem(DataTable results, string tenHocVien, string nguoiLap)
        {
            if (results == null) throw new ArgumentNullException(nameof(results));
            foreach (string column in new[] { "MonHoc", "TenDeThi", "Diem", "ThoiGianNop" })
                if (!results.Columns.Contains(column)) throw new ArgumentException("Thiếu cột báo cáo: " + column);
            var data = new DataSet("StudyReports");
            var table = data.Tables.Add("BangDiem");
            table.Columns.Add("HoTen", typeof(string));
            table.Columns.Add("MonHoc", typeof(string));
            table.Columns.Add("TenDeThi", typeof(string));
            table.Columns.Add("Diem", typeof(decimal));
            table.Columns.Add("ThoiGianNop", typeof(DateTime));
            table.Columns.Add("NguoiLap", typeof(string));
            foreach (DataRow row in results.Rows)
            {
                table.Rows.Add(tenHocVien ?? "", Convert.ToString(row["MonHoc"]),
                    Convert.ToString(row["TenDeThi"]),
                    row.IsNull("Diem") ? (object)DBNull.Value : Convert.ToDecimal(row["Diem"], CultureInfo.InvariantCulture),
                    row.IsNull("ThoiGianNop") ? (object)DBNull.Value : Convert.ToDateTime(row["ThoiGianNop"], CultureInfo.InvariantCulture),
                    nguoiLap ?? "");
            }
            return data;
        }
    }
}
