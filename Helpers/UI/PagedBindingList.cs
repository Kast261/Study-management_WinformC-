using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;

namespace BLT_Quan_Ly_Hoc_Tap.Helpers
{
    public enum PageChangeReason { Data, Navigation, PageSize, Sort }

    public sealed class PageChangedEventArgs : EventArgs
    {
        public PageChangeReason Reason { get; }
        public PageChangedEventArgs(PageChangeReason reason) { Reason = reason; }
    }

    // Keep the original DataRowView/model objects so existing CellClick handlers
    // continue to use real record IDs instead of page-relative row indices.
    public sealed class PagedBindingList : BindingList<object>, ITypedList, IDisposable
    {
        private IList source;
        private IBindingList observedSource;
        private readonly List<object> allItems = new List<object>();
        private PropertyDescriptorCollection properties = new PropertyDescriptorCollection(null);
        private PropertyDescriptor sortProperty;
        private ListSortDirection sortDirection;
        private int pageSize;
        private bool disposed;

        public int PageNumber { get; private set; }
        public int PageSize => pageSize;
        public int TotalCount => allItems.Count;
        public int PageCount => TotalCount == 0 ? 0 : (TotalCount - 1) / pageSize + 1;
        public int StartIndex => PageNumber == 0 ? 0 : (PageNumber - 1) * pageSize;
        public IReadOnlyList<object> AllItems => allItems.AsReadOnly();
        public event EventHandler<PageChangedEventArgs> PageChanged;

        public PagedBindingList(int pageSize = 20)
        {
            if (pageSize <= 0) throw new ArgumentOutOfRangeException(nameof(pageSize));
            this.pageSize = pageSize;
            AllowNew = false;
            AllowRemove = false;
        }

        public void SetSource(object data)
        {
            if (disposed) throw new ObjectDisposedException(nameof(PagedBindingList));
            if (observedSource != null) observedSource.ListChanged -= SourceChanged;
            if (data is DataTable table) data = table.DefaultView;
            else if (data is IListSource listSource) data = listSource.GetList();
            if (data != null && !(data is IList))
                throw new ArgumentException("Nguồn dữ liệu phải là DataTable, DataView hoặc IList.", nameof(data));

            source = data as IList;
            observedSource = source as IBindingList;
            if (observedSource != null) observedSource.ListChanged += SourceChanged;
            properties = FindProperties(source);
            sortProperty = null;
            RefreshSource(true, PageChangeReason.Data);
        }

        private static PropertyDescriptorCollection FindProperties(IList list)
        {
            if (list is ITypedList typed) return typed.GetItemProperties(null);
            Type itemType = list?.GetType().GetInterfaces()
                .FirstOrDefault(t => t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IList<>))
                ?.GetGenericArguments()[0];
            if (itemType == null && list != null && list.Count > 0) itemType = list[0]?.GetType();
            return itemType == null ? new PropertyDescriptorCollection(null) : TypeDescriptor.GetProperties(itemType);
        }

        private void SourceChanged(object sender, ListChangedEventArgs e)
        {
            properties = FindProperties(source);
            RefreshSource(e.ListChangedType == ListChangedType.Reset, PageChangeReason.Data);
        }

        private void RefreshSource(bool firstPage, PageChangeReason reason)
        {
            allItems.Clear();
            if (source != null)
                foreach (object item in source) allItems.Add(item);
            if (sortProperty != null)
            {
                // OrderBy is stable, including duplicate keys across page boundaries.
                var ordered = sortDirection == ListSortDirection.Ascending
                    ? allItems.OrderBy(item => sortProperty.GetValue(item), ValueComparer.Instance)
                    : allItems.OrderByDescending(item => sortProperty.GetValue(item), ValueComparer.Instance);
                object[] sorted = ordered.ToArray();
                allItems.Clear();
                allItems.AddRange(sorted);
            }
            if (firstPage) PageNumber = 1;
            ShowPage(reason);
        }

        public void GoToPage(int page)
        {
            int target = PageCount == 0 ? 0 : Math.Max(1, Math.Min(page, PageCount));
            if (target == PageNumber) return;
            PageNumber = target;
            ShowPage(PageChangeReason.Navigation);
        }

        public void SetPageSize(int size)
        {
            if (size <= 0) throw new ArgumentOutOfRangeException(nameof(size));
            if (size == pageSize) return;
            pageSize = size;
            PageNumber = 1;
            ShowPage(PageChangeReason.PageSize);
        }

        private void ShowPage(PageChangeReason reason)
        {
            PageNumber = PageCount == 0 ? 0 : Math.Max(1, Math.Min(PageNumber, PageCount));
            RaiseListChangedEvents = false;
            try
            {
                Items.Clear();
                int end = (int)Math.Min(StartIndex + (long)pageSize, TotalCount);
                for (int i = StartIndex; i < end; i++) Items.Add(allItems[i]);
            }
            finally { RaiseListChangedEvents = true; }
            ResetBindings();
            PageChanged?.Invoke(this, new PageChangedEventArgs(reason));
        }

        public bool ShowItem(string propertyName, object value)
        {
            PropertyDescriptor property = properties.Find(propertyName, true);
            if (property == null) return false;
            int index = allItems.FindIndex(item => Equals(property.GetValue(item), value));
            if (index < 0) return false;
            GoToPage(index / pageSize + 1);
            return true;
        }

        public object GetValue(object item, string propertyName)
        {
            return properties.Find(propertyName ?? "", true)?.GetValue(item);
        }

        protected override bool SupportsSortingCore => true;
        protected override bool IsSortedCore => sortProperty != null;
        protected override PropertyDescriptor SortPropertyCore => sortProperty;
        protected override ListSortDirection SortDirectionCore => sortDirection;
        protected override void ApplySortCore(PropertyDescriptor property, ListSortDirection direction)
        {
            sortProperty = property;
            sortDirection = direction;
            RefreshSource(true, PageChangeReason.Sort);
        }
        protected override void RemoveSortCore()
        {
            sortProperty = null;
            RefreshSource(true, PageChangeReason.Sort);
        }
        public PropertyDescriptorCollection GetItemProperties(PropertyDescriptor[] accessors) => properties;
        public string GetListName(PropertyDescriptor[] accessors) => "PagedRows";

        public void Dispose()
        {
            if (observedSource != null) observedSource.ListChanged -= SourceChanged;
            observedSource = null;
            source = null;
            allItems.Clear();
            Items.Clear();
            PageChanged = null;
            disposed = true;
        }

        private sealed class ValueComparer : IComparer<object>
        {
            public static readonly ValueComparer Instance = new ValueComparer();
            public int Compare(object x, object y)
            {
                if (x == DBNull.Value) x = null;
                if (y == DBNull.Value) y = null;
                if (ReferenceEquals(x, y)) return 0;
                if (x == null) return -1;
                if (y == null) return 1;
                if (x is IComparable comparable && x.GetType() == y.GetType()) return comparable.CompareTo(y);
                return StringComparer.CurrentCulture.Compare(x.ToString(), y.ToString());
            }
        }
    }
}
