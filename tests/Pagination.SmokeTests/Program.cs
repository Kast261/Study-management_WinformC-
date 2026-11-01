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
            BoundTableAndExport();
            TypedModelList();
            ManualColumnsRemainBound();
            TableLayoutAndHiddenTab();
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
            Check(grid.Rows.Count == 10 && Page(grid).Maximum == 5, "Page size must update bound row count");

            grid.Sort(grid.Columns["ID"], ListSortDirection.Descending);
            Application.DoEvents();
            Check((int)grid.Rows[0].Cells["ID"].Value == 43, "Grid sorting must include every page");
            Check(DataGridViewPager.ShowItem(grid, "id", 5), "ComboBox selection on another page must be locatable");
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
            Check(grid.Rows.Count == 2 && Page(grid).Value == 1, "Replacing a result must reset paging");
            table.Rows.Add(44, "Old source");
            Check(grid.Rows.Count == 2, "Old source events must be detached");
            grid.DataSource = null;
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
