using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Security;
using System.Text;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public static class ExcelExportHelper
    {
        public class ColumnDefinition
        {
            public string FieldName { get; set; }
            public string HeaderText { get; set; }
            public int Width { get; set; } = 120;
            public string Alignment { get; set; } = "Left"; // Left, Center, Right
            public bool IsNumeric { get; set; } = false;
            public Func<object, string> ValueFormatter { get; set; }
        }

        /// <summary>
        /// Hiển thị hộp thoại lưu file và xuất dữ liệu DataGridView ra file Excel (.xls) hoặc CSV (.csv)
        /// </summary>
        public static void ExportDataGridView(
            DataGridView dgv,
            string defaultFileName,
            string reportTitle,
            string subTitle = "")
        {
            if (dgv == null || dgv.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xls)|*.xls|CSV (Phân cách dấu phẩy) (*.csv)|*.csv",
                FilterIndex = 1,
                FileName = defaultFileName
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                if (ext == ".csv")
                {
                    ExportDgvToCsv(dgv, sfd.FileName, reportTitle, subTitle);
                }
                else
                {
                    ExportDgvToXmlExcel(dgv, sfd.FileName, reportTitle, subTitle);
                }

                DialogResult dr = MessageBox.Show(
                    "Xuất file Excel thành công!\nBạn có muốn mở file vừa xuất không?",
                    "Thông báo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (dr == DialogResult.Yes)
                {
                    try { System.Diagnostics.Process.Start(sfd.FileName); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Hiển thị hộp thoại lưu file và xuất dữ liệu DataTable ra file Excel (.xls) hoặc CSV (.csv)
        /// </summary>
        public static void ExportDataTable(
            DataTable dt,
            List<ColumnDefinition> columns,
            string defaultFileName,
            string reportTitle,
            string subTitle = "")
        {
            if (dt == null || dt.Rows.Count == 0)
            {
                MessageBox.Show("Không có dữ liệu để xuất!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                Filter = "Excel Workbook (*.xls)|*.xls|CSV (Phân cách dấu phẩy) (*.csv)|*.csv",
                FilterIndex = 1,
                FileName = defaultFileName
            };

            if (sfd.ShowDialog() != DialogResult.OK) return;

            try
            {
                string ext = Path.GetExtension(sfd.FileName).ToLower();
                if (ext == ".csv")
                {
                    ExportDataTableToCsv(dt, columns, sfd.FileName, reportTitle, subTitle);
                }
                else
                {
                    ExportDataTableToXmlExcel(dt, columns, sfd.FileName, reportTitle, subTitle);
                }

                DialogResult dr = MessageBox.Show(
                    "Xuất file Excel thành công!\nBạn có muốn mở file vừa xuất không?",
                    "Thông báo",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (dr == DialogResult.Yes)
                {
                    try { System.Diagnostics.Process.Start(sfd.FileName); } catch { }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi khi xuất file: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Xuất XML Spreadsheet 2003 (.xls)
        private static void ExportDgvToXmlExcel(
            DataGridView dgv,
            string filePath,
            string reportTitle,
            string subTitle)
        {
            // Lấy danh sách cột hiển thị
            List<DataGridViewColumn> visibleCols = new List<DataGridViewColumn>();
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible)
                {
                    visibleCols.Add(dgv.Columns[i]);
                }
            }

            int totalCols = visibleCols.Count + 1; // +1 cho cột STT
            int mergeAcross = Math.Max(0, totalCols - 1);

            StringBuilder sb = new StringBuilder();
            sb.Append(GetXmlHeader());

            // Định nghĩa cột và độ rộng
            sb.AppendLine("  <Table>");
            sb.AppendLine("   <Column ss:Width=\"45\"/>"); // STT
            foreach (var col in visibleCols)
            {
                int w = Math.Max(90, Math.Min(col.Width, 300));
                sb.AppendLine($"   <Column ss:Width=\"{w}\"/>");
            }

            // Dòng Tiêu Đề Báo Cáo
            sb.AppendLine("   <Row ss:Height=\"36\">");
            sb.AppendLine($"    <Cell ss:MergeAcross=\"{mergeAcross}\" ss:StyleID=\"Title\"><Data ss:Type=\"String\">{EscapeXml(reportTitle)}</Data></Cell>");
            sb.AppendLine("   </Row>");

            // Dòng Phụ Đề
            if (!string.IsNullOrEmpty(subTitle))
            {
                sb.AppendLine("   <Row ss:Height=\"22\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"{mergeAcross}\" ss:StyleID=\"SubTitle\"><Data ss:Type=\"String\">{EscapeXml(subTitle)}</Data></Cell>");
                sb.AppendLine("   </Row>");
            }

            // Dòng trống cách biệt
            sb.AppendLine("   <Row ss:Height=\"8\"/>");

            // Dòng Tiêu Đề Cột (Header)
            sb.AppendLine("   <Row ss:Height=\"26\">");
            sb.AppendLine("    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">STT</Data></Cell>");
            foreach (var col in visibleCols)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{EscapeXml(col.HeaderText)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");

            // Các Dòng Dữ Liệu
            int stt = 1;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;

                sb.AppendLine("   <Row ss:Height=\"22\">");
                sb.AppendLine($"    <Cell ss:StyleID=\"CellCenter\"><Data ss:Type=\"Number\">{stt++}</Data></Cell>");

                foreach (var col in visibleCols)
                {
                    object cellVal = row.Cells[col.Index].Value;
                    string strVal = cellVal?.ToString() ?? string.Empty;

                    // Kiểm tra xem là số hay chuỗi
                    if (IsNumber(strVal, out double numVal))
                    {
                        sb.AppendLine($"    <Cell ss:StyleID=\"CellRight\"><Data ss:Type=\"Number\">{numVal.ToString(CultureInfo.InvariantCulture)}</Data></Cell>");
                    }
                    else if (IsDateTime(strVal))
                    {
                        sb.AppendLine($"    <Cell ss:StyleID=\"CellCenter\"><Data ss:Type=\"String\">{EscapeXml(strVal)}</Data></Cell>");
                    }
                    else
                    {
                        sb.AppendLine($"    <Cell ss:StyleID=\"CellLeft\"><Data ss:Type=\"String\">{EscapeXml(strVal)}</Data></Cell>");
                    }
                }
                sb.AppendLine("   </Row>");
            }

            sb.Append(GetXmlFooter());
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static void ExportDataTableToXmlExcel(
            DataTable dt,
            List<ColumnDefinition> columns,
            string filePath,
            string reportTitle,
            string subTitle)
        {
            int totalCols = columns.Count;
            int mergeAcross = Math.Max(0, totalCols - 1);

            StringBuilder sb = new StringBuilder();
            sb.Append(GetXmlHeader());

            sb.AppendLine("  <Table>");
            foreach (var col in columns)
            {
                sb.AppendLine($"   <Column ss:Width=\"{col.Width}\"/>");
            }

            // Dòng Tiêu Đề Báo Cáo
            sb.AppendLine("   <Row ss:Height=\"36\">");
            sb.AppendLine($"    <Cell ss:MergeAcross=\"{mergeAcross}\" ss:StyleID=\"Title\"><Data ss:Type=\"String\">{EscapeXml(reportTitle)}</Data></Cell>");
            sb.AppendLine("   </Row>");

            // Dòng Phụ Đề
            if (!string.IsNullOrEmpty(subTitle))
            {
                sb.AppendLine("   <Row ss:Height=\"22\">");
                sb.AppendLine($"    <Cell ss:MergeAcross=\"{mergeAcross}\" ss:StyleID=\"SubTitle\"><Data ss:Type=\"String\">{EscapeXml(subTitle)}</Data></Cell>");
                sb.AppendLine("   </Row>");
            }

            // Dòng trống
            sb.AppendLine("   <Row ss:Height=\"8\"/>");

            // Dòng Tiêu Đề Cột
            sb.AppendLine("   <Row ss:Height=\"26\">");
            foreach (var col in columns)
            {
                sb.AppendLine($"    <Cell ss:StyleID=\"Header\"><Data ss:Type=\"String\">{EscapeXml(col.HeaderText)}</Data></Cell>");
            }
            sb.AppendLine("   </Row>");

            // Các Dòng Dữ Liệu
            int rowIndex = 1;
            foreach (DataRow row in dt.Rows)
            {
                sb.AppendLine("   <Row ss:Height=\"22\">");
                foreach (var col in columns)
                {
                    string strVal;
                    if (col.FieldName == "__STT__")
                    {
                        strVal = rowIndex.ToString();
                    }
                    else
                    {
                        object rawVal = dt.Columns.Contains(col.FieldName) ? row[col.FieldName] : null;
                        if (col.ValueFormatter != null)
                        {
                            strVal = col.ValueFormatter(rawVal);
                        }
                        else
                        {
                            strVal = rawVal?.ToString() ?? string.Empty;
                        }
                    }

                    // Tùy biến StyleID dựa trên kết quả hoặc căn lề
                    string styleId = "CellLeft";
                    if (strVal == "ĐÚNG" || strVal.ToUpper() == "ĐÚNG")
                    {
                        styleId = "CellSuccess";
                    }
                    else if (strVal == "SAI" || strVal.ToUpper() == "SAI")
                    {
                        styleId = "CellDanger";
                    }
                    else if (col.Alignment == "Center")
                    {
                        styleId = "CellCenter";
                    }
                    else if (col.Alignment == "Right" || col.IsNumeric)
                    {
                        styleId = "CellRight";
                    }

                    if (col.IsNumeric && IsNumber(strVal, out double numVal))
                    {
                        sb.AppendLine($"    <Cell ss:StyleID=\"{styleId}\"><Data ss:Type=\"Number\">{numVal.ToString(CultureInfo.InvariantCulture)}</Data></Cell>");
                    }
                    else
                    {
                        sb.AppendLine($"    <Cell ss:StyleID=\"{styleId}\"><Data ss:Type=\"String\">{EscapeXml(strVal)}</Data></Cell>");
                    }
                }
                sb.AppendLine("   </Row>");
                rowIndex++;
            }

            sb.Append(GetXmlFooter());
            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static string GetXmlHeader()
        {
            return @"<?xml version=""1.0"" encoding=""utf-8""?>
<?mso-application progid=""Excel.Sheet""?>
<Workbook xmlns=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:o=""urn:schemas-microsoft-com:office:office""
 xmlns:x=""urn:schemas-microsoft-com:office:excel""
 xmlns:ss=""urn:schemas-microsoft-com:office:spreadsheet""
 xmlns:html=""http://www.w3.org/TR/REC-html40"">
 <Styles>
  <Style ss:ID=""Default"" ss:Name=""Normal"">
   <Alignment ss:Vertical=""Center""/>
   <Borders/>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10"" ss:Color=""#000000""/>
  </Style>
  <Style ss:ID=""Title"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Font ss:FontName=""Segoe UI"" ss:Size=""16"" ss:Bold=""1"" ss:Color=""#0F5132""/>
  </Style>
  <Style ss:ID=""SubTitle"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10"" ss:Italic=""1"" ss:Color=""#555555""/>
  </Style>
  <Style ss:ID=""Header"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center"" ss:WrapText=""1""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#A0A0A0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#A0A0A0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#A0A0A0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#A0A0A0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""11"" ss:Bold=""1"" ss:Color=""#FFFFFF""/>
   <Interior ss:Color=""#23A86D"" ss:Pattern=""Solid""/>
  </Style>
  <Style ss:ID=""CellLeft"">
   <Alignment ss:Horizontal=""Left"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10""/>
  </Style>
  <Style ss:ID=""CellCenter"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10""/>
  </Style>
  <Style ss:ID=""CellRight"">
   <Alignment ss:Horizontal=""Right"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10""/>
  </Style>
  <Style ss:ID=""CellSuccess"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10"" ss:Bold=""1"" ss:Color=""#0F5132""/>
   <Interior ss:Color=""#D1E7DD"" ss:Pattern=""Solid""/>
  </Style>
  <Style ss:ID=""CellDanger"">
   <Alignment ss:Horizontal=""Center"" ss:Vertical=""Center""/>
   <Borders>
    <Border ss:Position=""Bottom"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Left"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Right"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
    <Border ss:Position=""Top"" ss:LineStyle=""Continuous"" ss:Weight=""1"" ss:Color=""#E0E0E0""/>
   </Borders>
   <Font ss:FontName=""Segoe UI"" ss:Size=""10"" ss:Bold=""1"" ss:Color=""#842029""/>
   <Interior ss:Color=""#F8D7DA"" ss:Pattern=""Solid""/>
  </Style>
 </Styles>
 <Worksheet ss:Name=""Report"">
";
        }

        private static string GetXmlFooter()
        {
            return @"  </Table>
 </Worksheet>
</Workbook>";
        }
        #endregion

        #region Xuất CSV (Chuẩn UTF-8 BOM, sep=,)
        private static void ExportDgvToCsv(
            DataGridView dgv,
            string filePath,
            string reportTitle,
            string subTitle)
        {
            List<DataGridViewColumn> visibleCols = new List<DataGridViewColumn>();
            for (int i = 0; i < dgv.Columns.Count; i++)
            {
                if (dgv.Columns[i].Visible) visibleCols.Add(dgv.Columns[i]);
            }

            StringBuilder sb = new StringBuilder();
            sb.Append("\uFEFF"); // UTF-8 BOM
            sb.AppendLine("sep=,"); // Chỉ thị cho Excel luôn dùng dấu phẩy để chia cột

            // Tiêu đề báo cáo
            if (!string.IsNullOrEmpty(reportTitle))
            {
                sb.AppendLine($"\"{EscapeCsv(reportTitle)}\"");
            }
            if (!string.IsNullOrEmpty(subTitle))
            {
                sb.AppendLine($"\"{EscapeCsv(subTitle)}\"");
            }
            sb.AppendLine();

            // Dòng tiêu đề cột
            List<string> headers = new List<string> { "\"STT\"" };
            foreach (var col in visibleCols)
            {
                headers.Add($"\"{EscapeCsv(col.HeaderText)}\"");
            }
            sb.AppendLine(string.Join(",", headers));

            // Dữ liệu
            int stt = 1;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.IsNewRow) continue;
                List<string> rowValues = new List<string> { (stt++).ToString() };
                foreach (var col in visibleCols)
                {
                    string val = row.Cells[col.Index].Value?.ToString() ?? string.Empty;
                    rowValues.Add($"\"{EscapeCsv(val)}\"");
                }
                sb.AppendLine(string.Join(",", rowValues));
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }

        private static void ExportDataTableToCsv(
            DataTable dt,
            List<ColumnDefinition> columns,
            string filePath,
            string reportTitle,
            string subTitle)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("\uFEFF");
            sb.AppendLine("sep=,");

            if (!string.IsNullOrEmpty(reportTitle))
            {
                sb.AppendLine($"\"{EscapeCsv(reportTitle)}\"");
            }
            if (!string.IsNullOrEmpty(subTitle))
            {
                sb.AppendLine($"\"{EscapeCsv(subTitle)}\"");
            }
            sb.AppendLine();

            List<string> headers = new List<string>();
            foreach (var col in columns)
            {
                headers.Add($"\"{EscapeCsv(col.HeaderText)}\"");
            }
            sb.AppendLine(string.Join(",", headers));

            int rowIndex = 1;
            foreach (DataRow row in dt.Rows)
            {
                List<string> rowValues = new List<string>();
                foreach (var col in columns)
                {
                    string strVal;
                    if (col.FieldName == "__STT__")
                    {
                        strVal = rowIndex.ToString();
                    }
                    else
                    {
                        object rawVal = dt.Columns.Contains(col.FieldName) ? row[col.FieldName] : null;
                        if (col.ValueFormatter != null)
                        {
                            strVal = col.ValueFormatter(rawVal);
                        }
                        else
                        {
                            strVal = rawVal?.ToString() ?? string.Empty;
                        }
                    }
                    rowValues.Add($"\"{EscapeCsv(strVal)}\"");
                }
                sb.AppendLine(string.Join(",", rowValues));
                rowIndex++;
            }

            File.WriteAllText(filePath, sb.ToString(), new UTF8Encoding(true));
        }
        #endregion

        #region Helper methods
        private static string EscapeXml(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("&", "&amp;")
                       .Replace("<", "&lt;")
                       .Replace(">", "&gt;")
                       .Replace("\"", "&quot;")
                       .Replace("'", "&apos;");
        }

        private static string EscapeCsv(string text)
        {
            if (string.IsNullOrEmpty(text)) return string.Empty;
            return text.Replace("\"", "\"\"");
        }

        private static bool IsNumber(string text, out double value)
        {
            value = 0;
            if (string.IsNullOrWhiteSpace(text)) return false;
            // Tránh parse nhầm ngày tháng hoặc số điện thoại có ký tự đặc biệt
            if (text.Contains("/") || text.Contains("-") || text.Contains(":")) return false;
            return double.TryParse(text.Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        private static bool IsDateTime(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            return DateTime.TryParse(text, out _);
        }
        #endregion
    }
}
