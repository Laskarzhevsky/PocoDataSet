using System;
using System.Collections.Specialized;
using System.ComponentModel;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Ensures that when the merge updates a cell value, the observable row raises exactly one DataFieldValueChanged notification (and one PropertyChanged for the column name).
        /// </summary>
        [Fact]
        public void Update_RaisesDataFieldValueChangedOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = t.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Old";
            r1.AcceptChanges();

            // Subscribe to row-level change notifications.
            DataFieldValueChangedCounter cellCounter = new DataFieldValueChangedCounter("Name");
            PropertyChangedCounter propCounter = new PropertyChangedCounter("Name");
            r1.DataFieldValueChanged += cellCounter.Handler;
            r1.PropertyChanged += propCounter.Handler;

            // Refreshed changes Name from Old -> New.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "New";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal("New", t.Rows[0]["Name"]);
            Assert.Equal(1, cellCounter.Count);
            Assert.Equal(1, propCounter.Count);
        }
    }
}