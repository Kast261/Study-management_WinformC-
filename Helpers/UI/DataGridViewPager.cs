using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public sealed class DataGridViewPager : IDisposable
    {
        private static readonly ConditionalWeakTable<DataGridView, DataGridViewPager> pagers
            = new ConditionalWeakTable<DataGridView, DataGridViewPager>();
        private readonly DataGridView grid;
        private readonly PagedBindingList rows = new PagedBindingList();
        private readonly Button first = MakeButton("|<", "Trang đầu");
        private readonly Button previous = MakeButton("<", "Trang trước");
        private readonly Button next = MakeButton(">", "Trang sau");
        private readonly Button last = MakeButton(">|", "Trang cuối");
        private readonly NumericUpDown page = new NumericUpDown { Width = 54, Minimum = 0, Maximum = 0 };
        private readonly ComboBox size = new ComboBox { Width = 55, DropDownStyle = ComboBoxStyle.DropDownList };
        private readonly Label summary = new Label { AutoSize = true, Margin = new Padding(3, 7, 3, 0) };
        private readonly Action<DataGridView, PageChangeReason> changed;
        private bool updating;
        private bool rebinding;
        private bool preserveSelection;

        private DataGridViewPager(DataGridView grid, Action<DataGridView, PageChangeReason> changed)
        {
            this.grid = grid;
            this.changed = changed;
            grid.AllowUserToAddRows = false;
            var toolbar = new FlowLayoutPanel
            {
                Name = grid.Name + "_Pagination",
                Dock = DockStyle.Bottom,
                Height = 52,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(3),
                BackColor = SystemColors.Control,
                Font = SystemFonts.MessageBoxFont
            };
            size.Items.AddRange(new object[] { 10, 20, 50, 100 });
            size.SelectedItem = 20;
            page.AccessibleName = "Số trang";
            size.AccessibleName = "Số dòng mỗi trang";
            toolbar.Controls.AddRange(new Control[]
            {
                first, previous, new Label { Text = "Trang", AutoSize = true, Margin = new Padding(3, 7, 0, 0) },
                page, summary, next, last, size,
                new Label { Text = "dòng/trang", AutoSize = true, Margin = new Padding(0, 7, 3, 0) }
            });
            WrapGrid(grid, toolbar);

            first.Click += (s, e) => rows.GoToPage(1);
            previous.Click += (s, e) => rows.GoToPage(rows.PageNumber - 1);
            next.Click += (s, e) => rows.GoToPage(rows.PageNumber + 1);
            last.Click += (s, e) => rows.GoToPage(rows.PageCount);
            page.ValueChanged += (s, e) => { if (!updating) rows.GoToPage((int)page.Value); };
            size.SelectedIndexChanged += (s, e) => { if (!updating) rows.SetPageSize((int)size.SelectedItem); };
            rows.PageChanged += PageChanged;
            grid.DataSourceChanged += DataSourceChanged;
            grid.DataBindingComplete += DataBindingComplete;
            grid.Disposed += GridDisposed;
            BindSource();
        }

        private static Button MakeButton(string text, string accessibleName)
        {
            return new Button { Text = text, AccessibleName = accessibleName, Width = 32, Height = 26 };
        }

        private static void WrapGrid(DataGridView grid, Control toolbar)
        {
            Control parent = grid.Parent;
            int index = parent.Controls.GetChildIndex(grid);
            var table = parent as TableLayoutPanel;
            var position = table?.GetPositionFromControl(grid) ?? new TableLayoutPanelCellPosition();
            int columnSpan = table?.GetColumnSpan(grid) ?? 1;
            int rowSpan = table?.GetRowSpan(grid) ?? 1;
            bool flowBreak = (parent as FlowLayoutPanel)?.GetFlowBreak(grid) ?? false;
            var host = new Panel
            {
                Name = grid.Name + "_PagedHost",
                Dock = grid.Dock,
                Anchor = grid.Anchor,
                Bounds = grid.Bounds,
                Margin = grid.Margin,
                MinimumSize = grid.MinimumSize,
                MaximumSize = grid.MaximumSize,
                TabIndex = grid.TabIndex
            };

            parent.SuspendLayout();
            try
            {
                parent.Controls.Remove(grid);
                parent.Controls.Add(host);
                parent.Controls.SetChildIndex(host, index);
                if (table != null)
                {
                    table.SetCellPosition(host, position);
                    table.SetColumnSpan(host, columnSpan);
                    table.SetRowSpan(host, rowSpan);
                }
                (parent as FlowLayoutPanel)?.SetFlowBreak(host, flowBreak);
                grid.Dock = DockStyle.Fill;
                grid.Margin = Padding.Empty;
                host.Controls.Add(grid);
                host.Controls.Add(toolbar);
                host.Controls.SetChildIndex(grid, 0);
            }
            finally { parent.ResumeLayout(true); }
        }

        public static void AttachAll(Control root, Action<DataGridView, PageChangeReason> onChanged = null)
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime || root.Site?.DesignMode == true) return;
            // Snapshot before inserting panels into the control tree.
            foreach (DataGridView grid in FindGrids(root).ToArray())
            {
                if (grid.Parent != null && !pagers.TryGetValue(grid, out _))
                    pagers.Add(grid, new DataGridViewPager(grid, onChanged));
            }
        }

        private static IEnumerable<DataGridView> FindGrids(Control root)
        {
            foreach (Control child in root.Controls)
            {
                if (child is DataGridView grid) yield return grid;
                else foreach (DataGridView nested in FindGrids(child)) yield return nested;
            }
        }

        private void DataSourceChanged(object sender, EventArgs e) { BindSource(); }
        private void BindSource()
        {
            if (rebinding || ReferenceEquals(grid.DataSource, rows)) return;
            rebinding = true;
            try
            {
                object source = grid.DataSource;
                grid.DataMember = string.Empty;
                rows.SetSource(source);
                grid.DataSource = rows;
            }
            finally { rebinding = false; }
            ClearSelection();
        }

        private void DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (!rebinding && !preserveSelection) ClearSelection();
        }

        private void ClearSelection()
        {
            grid.CurrentCell = null;
            grid.ClearSelection();
        }

        private void PageChanged(object sender, PageChangedEventArgs e)
        {
            updating = true;
            try
            {
                page.Minimum = 0;
                page.Maximum = rows.PageCount;
                page.Value = rows.PageNumber;
                page.Minimum = rows.PageCount == 0 ? 0 : 1;
                page.Enabled = rows.PageCount > 0;
                summary.Text = $"/ {rows.PageCount} · {rows.TotalCount:N0} dòng";
                first.Enabled = previous.Enabled = rows.PageNumber > 1;
                next.Enabled = last.Enabled = rows.PageNumber < rows.PageCount;
            }
            finally { updating = false; }
            if (!preserveSelection)
            {
                if (!rebinding) ClearSelection();
                changed?.Invoke(grid, e.Reason);
            }
        }

        // ComboBox selections may refer to records on later pages.
        public static bool ShowItem(DataGridView grid, string propertyName, object value)
        {
            if (grid == null) return false;
            if (pagers.TryGetValue(grid, out DataGridViewPager pager))
            {
                pager.preserveSelection = true;
                try
                {
                    if (!pager.rows.ShowItem(propertyName, value)) return false;
                    foreach (DataGridViewRow row in grid.Rows)
                    {
                        if (!Equals(pager.rows.GetValue(row.DataBoundItem, propertyName), value)) continue;
                        DataGridViewColumn column = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Visible);
                        if (column != null)
                        {
                            grid.ClearSelection();
                            grid.CurrentCell = row.Cells[column.Index];
                            row.Selected = true;
                            grid.FirstDisplayedScrollingRowIndex = row.Index;
                        }
                        return true;
                    }
                    return false;
                }
                finally { pager.preserveSelection = false; }
            }
            return false;
        }

        // Export the complete filtered/sorted result, not only the visible page.
        public static IEnumerable<object[]> GetExportRows(DataGridView grid, IList<DataGridViewColumn> columns)
        {
            if (pagers.TryGetValue(grid, out DataGridViewPager pager))
            {
                foreach (object item in pager.rows.AllItems)
                    yield return columns.Select(c => pager.rows.GetValue(item, c.DataPropertyName)).ToArray();
            }
            else
            {
                foreach (DataGridViewRow row in grid.Rows)
                    if (!row.IsNewRow) yield return columns.Select(c => row.Cells[c.Index].Value).ToArray();
            }
        }

        private void GridDisposed(object sender, EventArgs e) { Dispose(); }
        public void Dispose()
        {
            grid.DataSourceChanged -= DataSourceChanged;
            grid.DataBindingComplete -= DataBindingComplete;
            grid.Disposed -= GridDisposed;
            rows.PageChanged -= PageChanged;
            rows.Dispose();
            pagers.Remove(grid);
        }
    }
}
