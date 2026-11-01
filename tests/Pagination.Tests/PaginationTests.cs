using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using BLT_Quan_Ly_Hoc_Tap.Helpers;
using Xunit;

namespace Pagination.Tests
{
    public class PaginationTests
    {
        private static DataTable Table(int count)
        {
            var table = new DataTable();
            table.Columns.Add("ID", typeof(int));
            table.Columns.Add("Name", typeof(string));
            for (int i = 1; i <= count; i++) table.Rows.Add(i, "Item " + i);
            return table;
        }

        [Fact]
        public void NavigatesWithoutLosingOrDuplicatingRecords()
        {
            var table = Table(43);
            using var pages = new PagedBindingList();
            pages.SetSource(table);
            Assert.Equal(3, pages.PageCount);
            Assert.Equal(20, pages.Count);
            var ids = Enumerable.Range(1, pages.PageCount).SelectMany(page =>
            {
                pages.GoToPage(page);
                return pages.Cast<DataRowView>().Select(r => (int)r["ID"]).ToArray();
            }).ToArray();
            Assert.Equal(Enumerable.Range(1, 43), ids);
            Assert.Equal(3, pages.Count);
            Assert.Equal(43, table.Rows.Count);
        }

        [Theory]
        [InlineData(-5, 1)]
        [InlineData(0, 1)]
        [InlineData(999, 3)]
        public void ClampsNavigationToValidPages(int requested, int expected)
        {
            using var pages = new PagedBindingList();
            pages.SetSource(Table(43));
            pages.GoToPage(requested);
            Assert.Equal(expected, pages.PageNumber);
        }

        [Fact]
        public void EmptyResultsHaveNoPhantomPage()
        {
            using var pages = new PagedBindingList();
            pages.SetSource(Table(0));
            pages.GoToPage(1);
            Assert.Equal(0, pages.PageNumber);
            Assert.Equal(0, pages.PageCount);
            Assert.Empty(pages);
            Assert.Equal(2, ((ITypedList)pages).GetItemProperties(null).Count);
        }

        [Fact]
        public void ChangingPageSizeReturnsToFirstPage()
        {
            using var pages = new PagedBindingList();
            pages.SetSource(Table(43));
            pages.GoToPage(3);
            pages.SetPageSize(10);
            Assert.Equal(1, pages.PageNumber);
            Assert.Equal(5, pages.PageCount);
            Assert.Equal(10, pages.Count);
        }

        [Fact]
        public void FilteringUpdatesPageCountAndRetainsOriginalDataRowViews()
        {
            var table = Table(60);
            using var pages = new PagedBindingList();
            pages.SetSource(table);
            pages.GoToPage(3);
            table.DefaultView.RowFilter = "ID > 55";
            Assert.Equal(1, pages.PageNumber);
            Assert.Equal(5, pages.TotalCount);
            Assert.Same(table.DefaultView[0], pages[0]);
            Assert.Equal(Enumerable.Range(56, 5), pages.Cast<DataRowView>().Select(r => (int)r["ID"]));
            table.DefaultView.RowFilter = "ID > 100";
            Assert.Equal(0, pages.PageCount);
        }

        [Fact]
        public void DeletingLastPageRecordClampsToPreviousPage()
        {
            var table = Table(21);
            using var pages = new PagedBindingList();
            pages.SetSource(table);
            pages.GoToPage(2);
            table.Rows.RemoveAt(20);
            Assert.Equal(1, pages.PageNumber);
            Assert.Equal(20, pages.Count);
        }

        [Fact]
        public void ReplacingSourceDetachesOldTableEvents()
        {
            var old = Table(50);
            using var pages = new PagedBindingList();
            pages.SetSource(old);
            pages.GoToPage(3);
            pages.SetSource(Table(2));
            int changes = 0;
            pages.PageChanged += (s, e) => changes++;
            old.Rows.Add(51, "Old table");
            Assert.Equal(2, pages.TotalCount);
            Assert.Equal(1, pages.PageNumber);
            Assert.Equal(0, changes);
        }

        [Fact]
        public void SortsAllRowsNumericallyBeforeTakingPage()
        {
            using var pages = new PagedBindingList(10);
            pages.SetSource(Table(45));
            pages.GoToPage(3);
            var properties = ((ITypedList)pages).GetItemProperties(null);
            ((IBindingList)pages).ApplySort(properties["ID"], ListSortDirection.Descending);
            Assert.Equal(1, pages.PageNumber);
            Assert.Equal(Enumerable.Range(36, 10).Reverse(), pages.Cast<DataRowView>().Select(r => (int)r["ID"]));
            pages.GoToPage(5);
            Assert.Equal(new[] { 5, 4, 3, 2, 1 }, pages.Cast<DataRowView>().Select(r => (int)r["ID"]));
            ((IBindingList)pages).RemoveSort();
            Assert.Equal(1, ((DataRowView)pages[0])["ID"]);
        }

        [Fact]
        public void ModelListsKeepTheirOriginalTypeAndEmptyListMetadata()
        {
            var items = new BindingList<Question>();
            using var pages = new PagedBindingList(1);
            pages.SetSource(items);
            Assert.NotNull(((ITypedList)pages).GetItemProperties(null)["ID"]);
            var first = new Question { ID = 11 };
            items.Add(first);
            items.Add(new Question { ID = 22 });
            Assert.Same(first, pages[0]);
            Assert.Equal(2, pages.PageCount);
            pages.GoToPage(2);
            Assert.Equal(22, ((Question)pages[0]).ID);
        }

        [Fact]
        public void RestoringSortAfterRebindingDoesNotJumpBackToPageOne()
        {
            using var pages = new PagedBindingList(10);
            pages.SetSource(Table(45));
            PropertyDescriptor property = ((ITypedList)pages).GetItemProperties(null)["ID"];
            ((IBindingList)pages).ApplySort(property, ListSortDirection.Descending);
            pages.GoToPage(3);
            ((IBindingList)pages).ApplySort(property, ListSortDirection.Descending);
            Assert.Equal(3, pages.PageNumber);
            Assert.Equal(25, ((DataRowView)pages[0])["ID"]);
        }

        [Fact]
        public void CanLocateComboBoxSelectionOnAnotherPage()
        {
            using var pages = new PagedBindingList(10);
            pages.SetSource(Table(35));
            Assert.True(pages.ShowItem("id", 32));
            Assert.Equal(4, pages.PageNumber);
            Assert.Contains(pages, row => (int)((DataRowView)row)["ID"] == 32);
            Assert.False(pages.ShowItem("ID", 100));
            Assert.Equal(4, pages.PageNumber);
        }

        [Fact]
        public void FullExportRetainsEveryFilteredAndSortedRow()
        {
            var table = Table(75);
            table.DefaultView.RowFilter = "ID > 10";
            using var pages = new PagedBindingList(10);
            pages.SetSource(table);
            ((IBindingList)pages).ApplySort(((ITypedList)pages).GetItemProperties(null)["ID"], ListSortDirection.Descending);
            pages.GoToPage(4);
            Assert.Equal(10, pages.Count);
            Assert.Equal(Enumerable.Range(11, 65).Reverse(), pages.AllItems.Select(r => (int)pages.GetValue(r, "id")));
        }

        [Fact]
        public void SortingIsStableForDuplicateKeysAndHandlesNulls()
        {
            var table = Table(3);
            table.Rows[0]["Name"] = "same";
            table.Rows[1]["Name"] = DBNull.Value;
            table.Rows[2]["Name"] = "same";
            using var pages = new PagedBindingList(1);
            pages.SetSource(table);
            ((IBindingList)pages).ApplySort(((ITypedList)pages).GetItemProperties(null)["Name"], ListSortDirection.Ascending);
            Assert.Equal(new[] { 2, 1, 3 }, pages.AllItems.Select(r => (int)pages.GetValue(r, "ID")));
        }

        [Fact]
        public void ClearingAndReloadingSourceResetsPagingAndMetadata()
        {
            using var pages = new PagedBindingList();
            pages.SetSource(Table(30));
            pages.GoToPage(2);
            pages.SetSource(null);
            Assert.Equal(0, pages.PageNumber);
            Assert.Empty(pages.AllItems);
            pages.SetSource(Table(1));
            Assert.Equal(1, pages.PageNumber);
        }

        [Fact]
        public void DisposedPagerStopsObservingSource()
        {
            var table = Table(10);
            var pages = new PagedBindingList();
            pages.SetSource(table);
            pages.Dispose();
            table.Rows.Add(11, "After dispose");
            Assert.Empty(pages.AllItems);
            Assert.Throws<ObjectDisposedException>(() => pages.SetSource(table));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RejectsInvalidPageSize(int size)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PagedBindingList(size));
            using var pages = new PagedBindingList();
            Assert.Throws<ArgumentOutOfRangeException>(() => pages.SetPageSize(size));
        }

        public sealed class Question { public int ID { get; set; } }
    }
}
