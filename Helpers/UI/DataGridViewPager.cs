using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
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
        private readonly Button first = MakeButton("«", "Trang đầu");
        private readonly Button previous = MakeButton("‹", "Trang trước");
        private readonly Button next = MakeButton("›", "Trang sau");
        private readonly Button last = MakeButton("»", "Trang cuối");
        private readonly NumericUpDown page = new NumericUpDown
        {
            Width = 60, Minimum = 0, Maximum = 0, TextAlign = HorizontalAlignment.Center,
            BorderStyle = BorderStyle.FixedSingle, Margin = new Padding(4, 5, 4, 0), CausesValidation = false
        };
        private readonly ComboBox size = new ComboBox
        {
            Width = 66, DropDownStyle = ComboBoxStyle.DropDownList,
            Margin = new Padding(4, 5, 4, 0), CausesValidation = false
        };
        private readonly Label summary = new Label { AutoSize = true, Margin = new Padding(4, 9, 6, 0) };
        private readonly Font toolbarFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 10F);
        private readonly Font navigationFont = new Font(SystemFonts.MessageBoxFont.FontFamily, 16F, FontStyle.Bold);
        private readonly ToolTip tips = new ToolTip { InitialDelay = 350, ShowAlways = true };
        private readonly Action<DataGridView, PageChangeReason> changed;
        private bool updating;
        private bool rebinding;
        private bool preserveSelection;
        private bool preserveRequestedSelection;
        private int? pendingScrollRow;
        private bool renderPending;
        private bool clearSelectionPending;
        private bool uiWorkQueued;
        private bool processingUiWork;
        private bool disposed;
        private int handleVersion;
        private int requestVersion;
        private string pendingItemProperty;
        private object pendingItemValue;

        private DataGridViewPager(DataGridView grid, Action<DataGridView, PageChangeReason> changed)
        {
            this.grid = grid;
            this.changed = changed;
            grid.AllowUserToAddRows = false;
            var toolbar = new FlowLayoutPanel
            {
                Name = grid.Name + "_Pagination",
                Dock = DockStyle.Bottom,
                Height = 68,
                WrapContents = false,
                AutoScroll = true,
                Padding = new Padding(6, 8, 6, 4),
                BackColor = Color.FromArgb(245, 248, 247),
                ForeColor = Color.FromArgb(45, 65, 55),
                Font = toolbarFont
            };
            foreach (Button button in new[] { first, previous, next, last })
            {
                button.Font = navigationFont;
                tips.SetToolTip(button, button.AccessibleName);
            }
            size.Items.AddRange(new object[] { 10, 20, 50, 100 });
            size.SelectedItem = 20;
            page.AccessibleName = "Số trang";
            size.AccessibleName = "Số dòng mỗi trang";
            toolbar.Controls.AddRange(new Control[]
            {
                first, previous, new Label { Text = "Trang", AutoSize = true, Margin = new Padding(6, 9, 0, 0) },
                page, summary, next, last, size,
                new Label { Text = "dòng/trang", AutoSize = true, Margin = new Padding(0, 9, 3, 0) }
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
            grid.VisibleChanged += GridVisibleChanged;
            grid.SizeChanged += GridSizeChanged;
            grid.HandleCreated += GridHandleCreated;
            grid.HandleDestroyed += GridHandleDestroyed;
            grid.Disposed += GridDisposed;
            BindSource();
        }

        private static Button MakeButton(string text, string accessibleName)
        {
            return new PaginationButton
            {
                Text = text, AccessibleName = accessibleName, Width = 42, Height = 36,
                Margin = new Padding(2, 0, 2, 0), Cursor = Cursors.Hand, CausesValidation = false
            };
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
            if (disposed || rebinding || ReferenceEquals(grid.DataSource, rows)) return;
            rebinding = true;
            try
            {
                // Chỉ cập nhật model ở đây. Không bind lại grid trong DataSourceChanged:
                // WinForms có thể vẫn đang tự chia độ rộng các cột Fill của nguồn cũ.
                rows.SetSource(grid.DataSource);
            }
            finally { rebinding = false; }
            QueueUiWork();
        }

        private void DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (rebinding || disposed || preserveSelection || preserveRequestedSelection || pendingItemProperty != null) return;
            clearSelectionPending = true;
            QueueUiWork();
        }

        private void GridVisibleChanged(object sender, EventArgs e) { QueueUiWork(); }
        private void GridSizeChanged(object sender, EventArgs e)
        {
            if (pendingScrollRow.HasValue || pendingItemProperty != null) QueueUiWork();
        }
        private void GridHandleCreated(object sender, EventArgs e) { QueueUiWork(); }
        private void GridHandleDestroyed(object sender, EventArgs e)
        {
            handleVersion++;
            uiWorkQueued = false;
        }

        private void QueueUiWork()
        {
            if (disposed || grid.IsDisposed || grid.Disposing || rebinding || processingUiWork) return;
            if (!renderPending && !clearSelectionPending && !pendingScrollRow.HasValue && pendingItemProperty == null) return;
            if (!grid.IsHandleCreated)
            {
                // Trước khi có HWND, chưa có vòng resize/paint. Chuẩn bị binding ban đầu
                // nhưng để thao tác cuộn/chọn chờ HandleCreated và VisibleChanged.
                if (renderPending) ProcessUiWork();
                return;
            }
            if (uiWorkQueued) return;
            uiWorkQueued = true;
            int version = handleVersion;
            try
            {
                grid.BeginInvoke(new Action(() =>
                {
                    if (disposed || grid.IsDisposed || version != handleVersion) return;
                    uiWorkQueued = false;
                    ProcessUiWork();
                }));
            }
            catch (InvalidOperationException) when (disposed || grid.IsDisposed || !grid.IsHandleCreated)
            {
                // Handle vừa bị hủy: HandleCreated kế tiếp sẽ áp dụng yêu cầu còn lại.
                uiWorkQueued = false;
            }
        }

        private void ProcessUiWork()
        {
            if (disposed || grid.IsDisposed || grid.Disposing || processingUiWork) return;
            processingUiWork = true;
            int processedVersion = requestVersion;
            try
            {
                if (renderPending)
                {
                    renderPending = false;
                    ReplaceDisplayedPage();
                    UpdateToolbar();
                }
                ApplyPendingSelectionAndScroll();
            }
            finally { processingUiWork = false; }
            // Không lặp khi tab đang ẩn/không có chỗ hiển thị hàng. Các sự kiện
            // VisibleChanged/SizeChanged/HandleCreated sẽ xử lý yêu cầu đó sau.
            if (renderPending || requestVersion != processedVersion) QueueUiWork();
        }

        private void ApplyPendingSelectionAndScroll()
        {
            if (!grid.Visible || !grid.IsHandleCreated) return;
            int version = requestVersion;
            if (clearSelectionPending)
            {
                clearSelectionPending = false;
                ClearSelection();
                if (version != requestVersion) return;
            }
            if (pendingItemProperty != null)
            {
                foreach (DataGridViewRow row in grid.Rows)
                {
                    if (!Equals(rows.GetValue(row.DataBoundItem, pendingItemProperty), pendingItemValue)) continue;
                    var column = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(c => c.Visible);
                    if (column != null)
                    {
                        grid.ClearSelection();
                        if (version != requestVersion) return;
                        grid.CurrentCell = row.Cells[column.Index];
                        if (version != requestVersion) return;
                        row.Selected = true;
                        if (version != requestVersion) return;
                        pendingScrollRow = row.Index;
                    }
                    break;
                }
                pendingItemProperty = null;
                pendingItemValue = null;
            }
            if (!pendingScrollRow.HasValue || grid.Rows.Count == 0) return;
            // Một tab vừa hiện có thể chưa đủ chiều cao để WinForms hiển thị hàng.
            int header = grid.ColumnHeadersVisible ? grid.ColumnHeadersHeight : 0;
            if (grid.ClientSize.Height <= header + SystemInformation.HorizontalScrollBarHeight + 2) return;
            int target = Math.Max(0, Math.Min(pendingScrollRow.Value, grid.Rows.Count - 1));
            while (target < grid.Rows.Count && (!grid.Rows[target].Visible || grid.Rows[target].Frozen)) target++;
            if (target < grid.Rows.Count && grid.FirstDisplayedScrollingRowIndex != target)
                grid.FirstDisplayedScrollingRowIndex = target;
            if (grid.HorizontalScrollingOffset != 0) grid.HorizontalScrollingOffset = 0;
            pendingScrollRow = null;
        }

        private void ReplaceDisplayedPage()
        {
            if (rebinding) return;
            rebinding = true;
            try
            {
                // Chạy ngoài DataBindingComplete/VisibleChanged để không lồng layout Fill.
                grid.DataMember = string.Empty;
                grid.DataSource = null;
                grid.DataSource = rows;
                IBindingList sorted = rows;
                if (sorted.IsSorted)
                {
                    DataGridViewColumn column = grid.Columns.Cast<DataGridViewColumn>().FirstOrDefault(
                        c => string.Equals(c.DataPropertyName, sorted.SortProperty.Name, StringComparison.OrdinalIgnoreCase));
                    if (column != null) grid.Sort(column, sorted.SortDirection);
                }
            }
            finally { rebinding = false; }
        }

        private void ClearSelection()
        {
            grid.CurrentCell = null;
            grid.ClearSelection();
        }

        private void PageChanged(object sender, PageChangedEventArgs e)
        {
            requestVersion++;
            renderPending = true;
            pendingScrollRow = 0;
            if (!preserveSelection)
            {
                clearSelectionPending = true;
                preserveRequestedSelection = false;
                pendingItemProperty = null;
                pendingItemValue = null;
            }
            QueueUiWork();
            if (!preserveSelection) changed?.Invoke(grid, e.Reason);
        }

        private void UpdateToolbar()
        {
            updating = true;
            try
            {
                page.Minimum = 0;
                page.Maximum = rows.PageCount;
                page.Value = rows.PageNumber;
                page.Minimum = rows.PageCount == 0 ? 0 : 1;
                page.Enabled = rows.PageCount > 0;
                int start = rows.TotalCount == 0 ? 0 : rows.StartIndex + 1;
                int end = rows.StartIndex + rows.Count;
                summary.Text = $"/ {rows.PageCount} · {start:N0}–{end:N0}/{rows.TotalCount:N0} dòng";
                tips.SetToolTip(summary, $"Đang hiển thị dòng {start:N0} đến {end:N0} trên tổng {rows.TotalCount:N0} dòng");
                first.Enabled = previous.Enabled = rows.PageNumber > 1;
                next.Enabled = last.Enabled = rows.PageNumber < rows.PageCount;
            }
            finally { updating = false; }
        }

        // ComboBox selections may refer to records on later pages. Returns whether the
        // model contains the item; the actual selection/scroll runs on the next UI turn.
        public static bool ShowItem(DataGridView grid, string propertyName, object value)
        {
            if (grid == null) return false;
            if (pagers.TryGetValue(grid, out DataGridViewPager pager))
            {
                pager.preserveSelection = true;
                try
                {
                    if (!pager.rows.ShowItem(propertyName, value)) return false;
                    pager.requestVersion++;
                    pager.preserveRequestedSelection = true;
                    pager.pendingItemProperty = propertyName;
                    pager.pendingItemValue = value;
                    pager.clearSelectionPending = false;
                    pager.QueueUiWork();
                    return true;
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
            if (disposed) return;
            disposed = true;
            handleVersion++;
            uiWorkQueued = false;
            grid.DataSourceChanged -= DataSourceChanged;
            grid.DataBindingComplete -= DataBindingComplete;
            grid.VisibleChanged -= GridVisibleChanged;
            grid.SizeChanged -= GridSizeChanged;
            grid.HandleCreated -= GridHandleCreated;
            grid.HandleDestroyed -= GridHandleDestroyed;
            grid.Disposed -= GridDisposed;
            rows.PageChanged -= PageChanged;
            rows.Dispose();
            tips.Dispose();
            toolbarFont.Dispose();
            navigationFont.Dispose();
            pagers.Remove(grid);
        }

        private sealed class PaginationButton : Button
        {
            private bool hovered;
            private bool pressed;

            public PaginationButton()
            {
                SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                    | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
                FlatStyle = FlatStyle.Flat;
                FlatAppearance.BorderSize = 0;
                UseVisualStyleBackColor = false;
            }

            protected override void OnMouseEnter(EventArgs e) { hovered = true; Invalidate(); base.OnMouseEnter(e); }
            protected override void OnMouseLeave(EventArgs e) { hovered = pressed = false; Invalidate(); base.OnMouseLeave(e); }
            protected override void OnMouseDown(MouseEventArgs e) { pressed = e.Button == MouseButtons.Left; Invalidate(); base.OnMouseDown(e); }
            protected override void OnMouseUp(MouseEventArgs e) { pressed = false; Invalidate(); base.OnMouseUp(e); }
            protected override void OnEnabledChanged(EventArgs e) { hovered = pressed = false; Invalidate(); base.OnEnabledChanged(e); }

            protected override void OnPaint(PaintEventArgs e)
            {
                e.Graphics.Clear(Parent?.BackColor ?? SystemColors.Control);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                Color fill = !Enabled ? Color.FromArgb(227, 234, 230)
                    : pressed ? Color.FromArgb(17, 112, 73)
                    : hovered ? Color.FromArgb(21, 144, 94) : Color.FromArgb(35, 168, 109);
                using (var shape = new GraphicsPath())
                {
                    const int diameter = 14;
                    shape.AddArc(0, 0, diameter, diameter, 180, 90);
                    shape.AddArc(Width - diameter - 1, 0, diameter, diameter, 270, 90);
                    shape.AddArc(Width - diameter - 1, Height - diameter - 1, diameter, diameter, 0, 90);
                    shape.AddArc(0, Height - diameter - 1, diameter, diameter, 90, 90);
                    shape.CloseFigure();
                    using (var brush = new SolidBrush(fill)) e.Graphics.FillPath(brush, shape);
                }
                TextRenderer.DrawText(e.Graphics, Text, Font, ClientRectangle,
                    Enabled ? Color.White : Color.FromArgb(146, 160, 152),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
                if (Focused && ShowFocusCues)
                    ControlPaint.DrawFocusRectangle(e.Graphics, Rectangle.Inflate(ClientRectangle, -5, -5));
            }
        }
    }
}
