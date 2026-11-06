using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using BLT_Quan_Ly_Hoc_Tap.Helpers;

internal static class Program
{
    private static int assertions;
    [STAThread]
    private static int Main()
    {
        try
        {
            Application.EnableVisualStyles();
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.ThreadException += (s, e) => { Console.Error.WriteLine(e.Exception); Environment.Exit(1); };
            Console.WriteLine("Starting real DataGridView checks");
            BindingAndFillResizeDoNotReenterSourceChanges();
            BoundTableAndExport();
            TypedModelList();
            ManualColumnsRemainBound();
            DesignerHeadersSurviveAutomaticColumnGeneration();
            DateColumnsRemainBoundAcrossPages();
            ResultColumnsOnHiddenTabRemainVisibleWhenFiltering();
            ScrolledPageIsReplaced();
            TableLayoutAndHiddenTab();
            GradeReportUsesInvariantDecimals();
            DatePickerInputsKeepDayMonthOrderAndMissingDates();
            Console.WriteLine("PASS: " + assertions + " WinForms pagination assertions");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        assertions++;
        Console.WriteLine("PASS: " + message);
    }

    private static void GradeReportUsesInvariantDecimals()
    {
        var previous = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "vi-VN", "en-US" })
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo(culture);
                var table = new DataTable();
                table.Columns.Add("MonHoc", typeof(string));
                table.Columns.Add("TenDeThi", typeof(string));
                table.Columns.Add("ThoiGianNop", typeof(DateTime));
                table.Columns.Add("Diem", typeof(decimal));
                table.Rows.Add("Toán", "Đề 1", new DateTime(2026,1,1), 7.5M);
                table.Rows.Add("Toán", "Đề 2", new DateTime(2026,1,2), 8.5M);
                string xml = ExcelExportHelper.TaoXmlBangDiem(table, "A&B <Học viên>", "", null, null);
                var document = new System.Xml.XmlDocument();
                document.LoadXml(xml);
                var ns = new System.Xml.XmlNamespaceManager(document.NameTable);
                ns.AddNamespace("ss", "urn:schemas-microsoft-com:office:spreadsheet");
                var numbers = document.SelectNodes("//ss:Data[@ss:Type='Number']", ns)
                    .Cast<System.Xml.XmlNode>().Select(node => node.InnerText).ToList();
                Check(numbers.Contains("7.5") && numbers.Contains("8.5"), culture + ": grade report must retain decimal scores 7.5 and 8.5");
                Check(numbers.Last() == "8", culture + ": grade report average must be 8, not 80");
                Check(document.InnerText.Contains("A&B <Học viên>"), culture + ": grade report must escape names as valid XML");
            }
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = previous; }
    }

    private sealed class ObservedBindingGrid : DataGridView
    {
        private int sourceChangeDepth;
        public int MaximumSourceChangeDepth;
        protected override void OnDataSourceChanged(EventArgs e)
        {
            sourceChangeDepth++;
            MaximumSourceChangeDepth=Math.Max(MaximumSourceChangeDepth,sourceChangeDepth);
            try { base.OnDataSourceChanged(e); }
            finally { sourceChangeDepth--; }
        }
        public void RecreateForTest() { RecreateHandle(); }
    }

    private static void BindingAndFillResizeDoNotReenterSourceChanges()
    {
        using (var form = new Form {ClientSize=new Size(850,450)})
        {
            var tabs = new TabControl {Dock=DockStyle.Fill};
            tabs.TabPages.Add(new TabPage("Home"));
            var registration = new TabPage("Registration");
            var exams = new TabPage("Exams");
            tabs.TabPages.Add(registration); tabs.TabPages.Add(exams);
            var grids = new List<ObservedBindingGrid>();
            foreach (var tab in new[] {registration,exams})
            {
                var grid = new ObservedBindingGrid {Dock=DockStyle.Fill,AutoGenerateColumns=false,AutoSizeColumnsMode=DataGridViewAutoSizeColumnsMode.Fill};
                grid.Columns.Add(new DataGridViewTextBoxColumn {Name="ID",DataPropertyName="ID",FillWeight=35});
                grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Name",DataPropertyName="Name",FillWeight=65});
                tab.Controls.Add(grid); grids.Add(grid);
            }
            form.Controls.Add(tabs); DataGridViewPager.AttachAll(form); form.Show();
            tabs.SelectedTab=registration; Application.DoEvents();
            var first=grids[0]; first.MaximumSourceChangeDepth=0;
            first.DataSource=Table(43);
            Check(first.MaximumSourceChangeDepth==1,"Pager must not replace DataSource inside the original DataSourceChanged event");
            Application.DoEvents();
            Check(first.Rows.Count==20 && Page(first).Maximum==3,"Deferred binding must still render the first page and exact totals");
            for (int i=0;i<40;i++)
            {
                int index=i%2; tabs.SelectedTab=index==0?registration:exams;
                var grid=grids[index];
                grid.DataSource=Table(31);
                grid.DataSource=Table(41+i%5); // The latest request in the UI turn must win.
                form.ClientSize=new Size(i%2==0?680:940,i%3==0?180:470);
                Application.DoEvents();
                if (grid.Rows.Count!=20 || grid.ColumnCount!=2 || grid.MaximumSourceChangeDepth>1)
                    throw new InvalidOperationException("Rapid tab/Fill resize lost a page or reentered binding.");
                grid.Sort(grid.Columns["ID"],ListSortDirection.Descending);
                Application.DoEvents();
                Click(grid,"Trang sau");
                if (Convert.ToInt32(grid.Rows[0].Cells["ID"].Value)!=21+i%5)
                    throw new InvalidOperationException("Rapid tab/Fill resize lost sort order or page boundaries.");
            }
            Check(grids.All(g=>g.MaximumSourceChangeDepth<=1),"Repeated registration/exam tab changes and Fill resizes must never nest source changes");
            tabs.SelectedTab=registration;
            first.DataSource=Table(43); Application.DoEvents();
            Check(DataGridViewPager.ShowItem(first,"ID",42),"Deferred ComboBox navigation must find a row on a later page");
            first.DataSource=Table(2); Application.DoEvents();
            Check(first.Rows.Count==2 && first.CurrentRow==null,"A newer source must cancel a pending selection from the old source");
            first.DataSource=Table(39); first.RecreateForTest(); Application.DoEvents();
            Check(first.Rows.Count==20 && Page(first).Maximum==2,"Pending binding must survive control handle recreation");
            first.DataSource=Table(55); form.Close(); // Leave queued work while the grid is disposed.
            Check(first.IsDisposed && !DataGridViewPager.ShowItem(first,"ID",1),
                "Closing a form must dispose and unregister pagination with pending UI work");
        }
        Application.DoEvents();
    }

    private static void DatePickerInputsKeepDayMonthOrderAndMissingDates()
    {
        var original = System.Globalization.CultureInfo.CurrentCulture;
        try
        {
            foreach (string culture in new[] { "vi-VN", "en-US" })
            {
                System.Globalization.CultureInfo.CurrentCulture = System.Globalization.CultureInfo.GetCultureInfo(culture);
                using (var picker = new DateTimePicker())
                {
                    DatePickerInput.Configure(picker);
                    Check(!picker.Checked && DatePickerInput.ReadText(picker) == "",
                        culture + ": an empty date must not silently become today's date");
                    var birthday = new DateTime(2006, 4, 3);
                    DatePickerInput.SetValue(picker, birthday);
                    Check(picker.Checked && picker.Value.Date == birthday && DatePickerInput.ReadText(picker) == "03/04/2006",
                        culture + ": a selected birthday must retain day/month order");
                    DatePickerInput.SetValue(picker, DBNull.Value);
                    Check(!picker.Checked && DatePickerInput.ReadText(picker) == "",
                        culture + ": a missing database date must clear the previous selection");
                    DatePickerInput.SetValue(picker, birthday);
                    DatePickerInput.SetValue(picker, null);
                    Check(!picker.Checked && DatePickerInput.ReadText(picker) == "",
                        culture + ": null dates must remain missing");
                }
            }
        }
        finally { System.Globalization.CultureInfo.CurrentCulture = original; }
    }

    private static DataTable Table(int count)
    {
        var table = new DataTable();
        table.Columns.Add("ID", typeof(int));
        table.Columns.Add("Name", typeof(string));
        for (int i = 1; i <= count; i++) table.Rows.Add(i, "Item " + i);
        return table;
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (Control nested in Descendants(child)) yield return nested;
        }
    }

    private static FlowLayoutPanel Footer(DataGridView grid) =>
        grid.Parent.Controls.OfType<FlowLayoutPanel>().Single();
    private static NumericUpDown Page(DataGridView grid) => Footer(grid).Controls.OfType<NumericUpDown>().Single();
    private static void Click(DataGridView grid, string name)
    {
        Footer(grid).Controls.OfType<Button>().Single(b => b.AccessibleName == name).PerformClick();
        Application.DoEvents();
    }

    private static void BoundTableAndExport()
    {
        using (var form = new Form { ClientSize = new Size(850, 450) })
        {
            var grid = new DataGridView { Name = "students", Dock = DockStyle.Fill };
            grid.DataError += (s, e) => { throw e.Exception; };
            form.Controls.Add(grid);
            int selectionResets = 0;
            DataGridViewPager.AttachAll(form, (g, reason) => selectionResets++);
            DataGridViewPager.AttachAll(form);
            Check(Descendants(form).OfType<FlowLayoutPanel>().Count() == 1, "AttachAll must be idempotent");
            form.Show();
            var table = Table(43);
            grid.DataSource = table;
            Application.DoEvents();
            Check(grid.Rows.Count == 20, "First page must bind exactly 20 rows");
            Check(grid.Columns.Count == 2, "Auto-generated DataTable columns must survive paging");
            Check(ReferenceEquals(grid.Rows[0].DataBoundItem, table.DefaultView[0]), "DataRowView identity must survive");
            Check(Page(grid).Maximum == 3, "43 rows must produce 3 pages");
            Click(grid, "Trang sau");
            Check((int)grid.Rows[0].Cells["ID"].Value == 21, "Next page must start at ID 21");
            Check(grid.CurrentRow == null && grid.SelectedRows.Count == 0, "Paging must clear stale selection");
            Click(grid, "Trang cuối");
            Check(grid.Rows.Count == 3, "Last page must contain the remainder");
            Check(!Footer(grid).Controls.OfType<Button>().Single(b => b.AccessibleName == "Trang sau").Enabled, "Next must be disabled on the last page");
            Page(grid).Value = 1;
            Application.DoEvents();
            Check((int)grid.Rows[0].Cells["ID"].Value == 1, "Page number must support direct navigation");
            Footer(grid).Controls.OfType<ComboBox>().Single().SelectedItem = 10;
            Application.DoEvents();
            Check(grid.Rows.Count == 10 && Page(grid).Maximum == 5, "Page size must update bound row count");

            grid.Sort(grid.Columns["ID"], ListSortDirection.Descending);
            Application.DoEvents();
            Check((int)grid.Rows[0].Cells["ID"].Value == 43, "Grid sorting must include every page");
            Click(grid, "Trang sau");
            Check((int)grid.Rows[0].Cells["ID"].Value == 33 && grid.SortOrder == SortOrder.Descending,
                "Replacing a page must preserve both sorted data and the grid sort direction");
            Check(DataGridViewPager.ShowItem(grid, "id", 5), "ComboBox selection on another page must be locatable");
            Application.DoEvents();
            Check(grid.Rows.Cast<DataGridViewRow>().Any(r => (int)r.Cells["ID"].Value == 5), "Located row must be on the displayed page");
            Check((int)grid.CurrentRow.Cells["ID"].Value == 5, "CurrentRow must match the ComboBox selection, not the first row of its page");

            table.DefaultView.RowFilter = "ID > 5";
            Application.DoEvents();
            Check(Page(grid).Value == 1 && Page(grid).Maximum == 4, "Filtering must reset the page and total");
            var columns = grid.Columns.Cast<DataGridViewColumn>().ToList();
            Check(DataGridViewPager.GetExportRows(grid, columns).Count() == 38, "Export must include all filtered pages");
            string csv = Path.GetTempFileName();
            string xml = Path.GetTempFileName();
            try
            {
                typeof(ExcelExportHelper).GetMethod("ExportDgvToCsv", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { grid, csv, "Test", "" });
                typeof(ExcelExportHelper).GetMethod("ExportDgvToXmlExcel", BindingFlags.NonPublic | BindingFlags.Static)
                    .Invoke(null, new object[] { grid, xml, "Test", "" });
                Check(File.ReadAllLines(csv).Count(line => line.Contains("\"Item ")) == 38, "CSV must not silently export just the first page");
                string content = File.ReadAllText(xml);
                Check(content.Contains(">Item 6<") && content.Contains(">Item 43<"), "Excel must include records outside the visible page");
            }
            finally { File.Delete(csv); File.Delete(xml); }

            grid.DataSource = Table(2);
            Application.DoEvents();
            Check(grid.Rows.Count == 2 && Page(grid).Value == 1, "Replacing a result must reset paging");
            table.Rows.Add(44, "Old source");
            Check(grid.Rows.Count == 2, "Old source events must be detached");
            grid.DataSource = null;
            Application.DoEvents();
            Check(grid.Rows.Count == 0 && Page(grid).Value == 0, "Clearing a grid must display page 0/0");
            Check(!Page(grid).Enabled, "Navigation must be disabled for empty results");
            Check(selectionResets > 1, "Domain selections must be invalidated when page contents change");
            form.Close();
        }
    }

    private static void TypedModelList()
    {
        using (var form = new Form { ClientSize = new Size(850, 450) })
        {
            var grid = new DataGridView { Name = "questions", Dock = DockStyle.Fill };
            grid.DataError += (s, e) => { throw e.Exception; };
            form.Controls.Add(grid);
            DataGridViewPager.AttachAll(form);
            form.Show();
            var items = Enumerable.Range(1, 35).Select(id => new Question { ID = id, Text = "Question " + id }).ToList();
            grid.DataSource = items;
            Application.DoEvents();
            Check(grid.Rows[0].DataBoundItem is Question, "Question handlers must receive a Question, not a DataRowView");
            Check(grid.Columns.Count == 2, "Model list properties must be exposed to the grid");
            Click(grid, "Trang sau");
            Check(ReferenceEquals(grid.Rows[0].DataBoundItem, items[20]), "Model identity must survive page navigation");
            grid.Sort(grid.Columns["ID"], ListSortDirection.Descending);
            Application.DoEvents();
            Check(((Question)grid.Rows[0].DataBoundItem).ID == 35, "Model list must sort across all pages");
            form.Close();
        }
    }

    private static void ManualColumnsRemainBound()
    {
        using (var form = new Form { ClientSize = new Size(850, 450) })
        {
            var grid = new DataGridView { Name = "manual", Dock = DockStyle.Fill, AutoGenerateColumns = false };
            grid.DataError += (s, e) => { throw e.Exception; };
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colID", HeaderText = "Mã" });
            grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "Tên" });
            form.Controls.Add(grid);
            DataGridViewPager.AttachAll(form);
            // Existing forms assign DataPropertyName after InitializeComponent.
            grid.Columns["colID"].DataPropertyName = "id";
            grid.Columns["colName"].DataPropertyName = "Name";
            form.Show();
            grid.DataSource = Table(42);
            Application.DoEvents();
            Check(grid.Columns.Count == 2 && grid.Columns[0].HeaderText == "Mã", "Designer columns and headers must be retained");
            Check((int)grid.Rows[0].Cells["colID"].Value == 1, "Manual columns must bind without case sensitivity");
            Click(grid, "Trang sau");
            Check((int)grid.Rows[0].Cells["colID"].Value == 21, "Manual columns must remain bound on later pages");
            Check(DataGridViewPager.GetExportRows(grid, grid.Columns.Cast<DataGridViewColumn>().ToList()).Count() == 42, "Export must also support manual Designer columns");
            form.Close();
        }
    }

    private static void DesignerHeadersSurviveAutomaticColumnGeneration()
    {
        using (var form = new Form { ClientSize = new Size(1100, 450) })
        {
            // AutoGenerateColumns defaults to true when the Designer doesn't persist it.
            var grid = new DataGridView { Name = "dgvDSYeuCau", Dock = DockStyle.Fill,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            string[] fields = { "HoTen", "Email", "SoDienThoai", "TenLop", "SiSo", "NgayDangKy" };
            string[] headers = { "Học Viên", "Email", "Điện Thoại", "Lớp Học", "Sĩ Số", "Ngày Đăng Kí" };
            var columns = fields.Select((field, i) => new DataGridViewTextBoxColumn
            {
                Name = "designerColumn" + i, DataPropertyName = field, HeaderText = headers[i],
                FillWeight = i == 0 ? 200 : 100
            }).ToArray();
            columns[5].DefaultCellStyle.Format = "dd/MM/yyyy HH:mm";
            grid.Columns.AddRange(columns);
            form.Controls.Add(grid);
            DataGridViewPager.AttachAll(form);
            form.Show();
            Application.DoEvents();
            Check(columns.SequenceEqual(grid.Columns.Cast<DataGridViewColumn>()),
                "Attaching pagination before data loads must preserve all Designer columns");

            // PostgreSQL returns unquoted field names in lowercase, plus internal IDs.
            var table = new DataTable();
            table.Columns.Add("dangkyid", typeof(int));
            table.Columns.Add("lophocid", typeof(int));
            table.Columns.Add("hocsinhid", typeof(int));
            foreach (string field in fields.Take(4)) table.Columns.Add(field.ToLowerInvariant(), typeof(string));
            table.Columns.Add("siso", typeof(int));
            table.Columns.Add("ngaydangky", typeof(DateTime));
            table.Columns.Add("trangthai", typeof(string));
            grid.DataSource = table;
            Application.DoEvents();
            Check(grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).SequenceEqual(headers),
                "An empty registration result must retain Vietnamese Designer headers without internal ID columns");
            for (int i = 1; i <= 21; i++)
                table.Rows.Add(i, 2, i, "Học viên " + i, "hv@example.test", "0123456789", "Lớp Toán", 21,
                    new DateTime(2026, 11, 6, 9, 30, 0), "ChoDuyet");
            Application.DoEvents();
            Check(grid.Rows[0].Cells[columns[0].Name].Value.ToString() == "Học viên 1",
                "Designer columns must bind lowercase PostgreSQL fields without changing their names");
            Check(grid.Rows[0].Cells[columns[5].Name].FormattedValue.ToString() == "06/11/2026 09:30",
                "Registration dates must retain Designer formatting");
            grid.Sort(columns[0], ListSortDirection.Descending);
            Application.DoEvents();
            Click(grid, "Trang sau");
            Check(grid.Rows.Count == 1 && columns.SequenceEqual(grid.Columns.Cast<DataGridViewColumn>()),
                "Sorting and paging must reuse the original columns, not regenerate raw field headers");
            Check(columns[0].FillWeight == 200 && columns[5].DefaultCellStyle.Format == "dd/MM/yyyy HH:mm",
                "Paging must preserve Designer widths and date formats");
            table.DefaultView.RowFilter = "dangkyid < 0";
            Application.DoEvents();
            Check(grid.Rows.Count == 0 && grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).SequenceEqual(headers),
                "Filtering all rows out must keep Vietnamese headers visible");
            grid.DataSource = null;
            grid.DataSource = table.Copy();
            Application.DoEvents();
            Check(grid.Rows.Count == 20 && columns.SequenceEqual(grid.Columns.Cast<DataGridViewColumn>()),
                "Reloading a source must not replace Designer columns");
            var exported = DataGridViewPager.GetExportRows(grid, grid.Columns.Cast<DataGridViewColumn>().ToList()).ToList();
            Check(exported.Count == 21 && exported.All(row => row.Length == 6 && row[0].ToString().StartsWith("Học viên ")),
                "Export must use the six designed columns with their bound values across all pages");
            form.Close();
        }
    }

    private static void DateColumnsRemainBoundAcrossPages()
    {
        using (var form = new Form { ClientSize = new Size(850, 450) })
        {
            var grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false };
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name="Column11", DataPropertyName="NgayBatDau",
                DefaultCellStyle=new DataGridViewCellStyle { Format="dd/MM/yyyy", NullValue="" }
            });
            grid.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name="Column12", DataPropertyName="NgayKetThuc",
                DefaultCellStyle=new DataGridViewCellStyle { Format="dd/MM/yyyy", NullValue="" }
            });
            var table = new DataTable();
            table.Columns.Add("NgayBatDau",typeof(DateTime));
            table.Columns.Add("NgayKetThuc",typeof(DateTime));
            for (int i=0;i<21;i++) table.Rows.Add(new DateTime(2027,3,4),new DateTime(2027,6,15));
            table.Rows[20]["NgayKetThuc"]=DBNull.Value;
            form.Controls.Add(grid);
            DataGridViewPager.AttachAll(form);
            form.Show(); grid.DataSource=table; Application.DoEvents();
            Check(grid.Rows[0].Cells["Column11"].FormattedValue.ToString()=="04/03/2027", "Start date must display as dd/MM/yyyy through the pager");
            Check(grid.Rows[0].Cells["Column12"].FormattedValue.ToString()=="15/06/2027", "End date must display as dd/MM/yyyy through the pager");
            Click(grid,"Trang sau");
            Check(grid.Rows.Count==1 && grid.Rows[0].Cells["Column11"].FormattedValue.ToString()=="04/03/2027", "Date binding and formatting must survive moving to the next page");
            Check(grid.Rows[0].Cells["Column12"].FormattedValue.ToString()=="", "Missing class dates must stay blank without inventing a default date");
            form.Close();
        }
    }

    private static void ResultColumnsOnHiddenTabRemainVisibleWhenFiltering()
    {
        using (var form = new Form { ClientSize=new Size(900,520) })
        {
            var tabs = new TabControl {Dock=DockStyle.Fill};
            tabs.TabPages.Add(new TabPage("Home"));
            var resultsTab = new TabPage("Results"); tabs.TabPages.Add(resultsTab);
            var grid = new DataGridView {Dock=DockStyle.Fill,AutoGenerateColumns=false,ReadOnly=true};
            grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Column22",HeaderText="Đề thi",DataPropertyName="TenDeThi"});
            grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Column23",HeaderText="Môn học",DataPropertyName="MonHoc"});
            grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Column24",HeaderText="Nộp lúc",DataPropertyName="ThoiGianNop",DefaultCellStyle=new DataGridViewCellStyle {Format="dd/MM/yyyy HH:mm"}});
            grid.Columns.Add(new DataGridViewTextBoxColumn {Name="Column25",HeaderText="Điểm",DataPropertyName="Diem",DefaultCellStyle=new DataGridViewCellStyle {Format="0.00"}});
            resultsTab.Controls.Add(grid); form.Controls.Add(tabs);
            DataGridViewPager.AttachAll(form);
            var table = new DataTable();
            table.Columns.Add("LopHocID",typeof(int)); table.Columns.Add("TenDeThi",typeof(string));
            table.Columns.Add("MonHoc",typeof(string)); table.Columns.Add("ThoiGianNop",typeof(DateTime)); table.Columns.Add("Diem",typeof(decimal));
            for (int i=1;i<=4;i++) table.Rows.Add(i<=2?1:2,"Đề "+i,"Toán",new DateTime(2027,3,4,9,30,0),7.5M);
            grid.DataSource=table; form.Show(); tabs.SelectedTab=resultsTab; Application.DoEvents();
            Check(grid.ColumnCount==4 && grid.ColumnHeadersVisible && grid.Rows.Count==4,"Hidden Results tab must show its four Designer columns and existing result rows");
            Check(grid.Rows[0].Cells["Column22"].FormattedValue.ToString()=="Đề 1","Result names must bind to the actual Designer columns");
            Check(grid.Rows[0].Cells["Column24"].FormattedValue.ToString()=="04/03/2027 09:30","Result submission time must display in day/month order");
            table.DefaultView.RowFilter="LopHocID=2"; Application.DoEvents();
            Check(grid.Rows.Count==2 && grid.Rows[0].Cells["Column22"].FormattedValue.ToString()=="Đề 3","Filtering results by class must replace the rows and reset paging");
            table.DefaultView.RowFilter="LopHocID=999"; Application.DoEvents();
            Check(grid.Rows.Count==0 && grid.ColumnCount==4,"A class with no results must keep the table headers visible");
            table.DefaultView.RowFilter=""; Application.DoEvents();
            Check(grid.Rows.Count==4 && grid.ColumnCount==4,"Clearing the class filter must restore results without losing columns");
            form.Close();
        }
    }

    private static void ScrolledPageIsReplaced()
    {
        using (var form = new Form { ClientSize = new Size(660, 245) })
        {
            var grid = new DataGridView { Name = "classes", Dock = DockStyle.Fill };
            grid.DataError += (s, e) => { throw e.Exception; };
            form.Controls.Add(grid);
            DataGridViewPager.AttachAll(form);
            form.Show();
            grid.DataSource = Table(31);
            Footer(grid).Controls.OfType<ComboBox>().Single().SelectedItem = 10;
            Application.DoEvents();
            grid.FirstDisplayedScrollingRowIndex = 5;
            Click(grid, "Trang sau");
            Check(Page(grid).Value == 2, "Next button must change the current page number");
            Check(grid.Rows.Cast<DataGridViewRow>().Select(r => (int)r.Cells["ID"].Value).SequenceEqual(Enumerable.Range(11, 10)),
                "Page 2 must replace page 1 with exactly IDs 11-20");
            Check(grid.FirstDisplayedScrollingRowIndex == 0, "A new page must start at its first row even after scrolling the previous page");
            Click(grid, "Trang trước");
            Check((int)grid.Rows[0].Cells["ID"].Value == 1 && grid.FirstDisplayedScrollingRowIndex == 0,
                "Previous page must also replace rows and reset scrolling");
            grid.DataSource = Table(11);
            Click(grid, "Trang sau");
            Check(grid.Rows.Count == 1 && (int)grid.Rows[0].Cells["ID"].Value == 11,
                "11 records at 10 per page must show only record 11 on page 2");
            form.Close();
        }
    }

    private static void TableLayoutAndHiddenTab()
    {
        using (var form = new Form { ClientSize = new Size(900, 520) })
        {
            var tabs = new TabControl { Dock = DockStyle.Fill };
            var firstTab = new TabPage("First");
            var hiddenTab = new TabPage("Tables");
            tabs.TabPages.AddRange(new[] { firstTab, hiddenTab });
            form.Controls.Add(tabs);
            var table = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
            hiddenTab.Controls.Add(table);
            var grid = new DataGridView { Name = "nested", Dock = DockStyle.Fill, Margin = new Padding(8) };
            grid.DataError += (s, e) => { throw e.Exception; };
            table.Controls.Add(grid, 0, 0);
            table.SetRowSpan(grid, 2);
            var sibling = new Button { Text = "Existing action", Dock = DockStyle.Fill };
            table.Controls.Add(sibling, 1, 1);
            DataGridViewPager.AttachAll(form);
            grid.DataSource = Table(25);
            form.Show();
            tabs.SelectedTab = hiddenTab;
            Application.DoEvents();
            Check(table.GetRowSpan(grid.Parent) == 2, "TableLayoutPanel row span must be preserved");
            Check(table.GetPositionFromControl(grid.Parent).Column == 0, "TableLayoutPanel cell must be preserved");
            Check(table.GetPositionFromControl(sibling).Column == 1, "Existing sibling controls must not move");
            Check(grid.Visible && Footer(grid).Visible, "Grids on initially hidden tabs must become visible");
            Check(grid.Height > 0 && grid.Bottom <= Footer(grid).Top, "Pager must not overlap the grid");
            string screenshot = Environment.GetEnvironmentVariable("PAGINATION_SCREENSHOT");
            if (!string.IsNullOrEmpty(screenshot))
            {
                using (var bitmap = new Bitmap(form.ClientSize.Width, form.ClientSize.Height))
                {
                    using (Graphics graphics = Graphics.FromImage(bitmap))
                        graphics.CopyFromScreen(form.PointToScreen(Point.Empty), Point.Empty, form.ClientSize);
                    bitmap.Save(screenshot);
                }
            }
            form.Close();
        }
    }

    public sealed class Question
    {
        public int ID { get; set; }
        public string Text { get; set; }
    }
}
