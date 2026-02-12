using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableMerge_MultipleSubscribersTests
    {
        [Fact]
        public void Merge_Refresh_RemovesRow_RaisesRowsRemoved_ForMultipleSubscribers()
        {
            // Arrange
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true, false);
            t.AddColumn("Name", DataTypeNames.STRING);

            AddLoadedRow(t, 1, "A");
            AddLoadedRow(t, 2, "B");

            RowsRemovedCounter c1 = new RowsRemovedCounter();
            RowsRemovedCounter c2 = new RowsRemovedCounter();

            t.RowsRemoved += c1.Handler;
            t.RowsRemoved += c2.Handler;

            // Refreshed snapshot contains only Id=1
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 1;
            r["Name"] = "A";
            rt.AddLoadedRow(r);

            // Act
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.Refresh;
            ds.MergeWith(refreshed, observableMergeOptions);

            // Assert
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(1, c1.Count);
            Assert.Equal(1, c2.Count);
        }

        private static IObservableDataRow AddLoadedRow(IObservableDataTable t, int id, string name)
        {
            IObservableDataRow r = t.AddNewRow();
            r["Id"] = id;
            r["Name"] = name;
            r.AcceptChanges();
            return r;
        }

        private class RowsRemovedCounter
        {
            public int Count;

            public void Handler(object? sender, RowsChangedEventArgs e)
            {
                Count++;
            }
        }
    }
}
