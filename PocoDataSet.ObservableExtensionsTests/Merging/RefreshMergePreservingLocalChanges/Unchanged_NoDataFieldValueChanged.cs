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
        /// Ensures that when the refreshed value matches the current value, the merge does not raise DataFieldValueChanged (and does not raise PropertyChanged for the column).
        /// </summary>
        [Fact]
        public void Unchanged_NoDataFieldValueChanged()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = t.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Same";
            r1.AcceptChanges();

            DataFieldValueChangedCounter cellCounter = new DataFieldValueChangedCounter("Name");
            PropertyChangedCounter propCounter = new PropertyChangedCounter("Name");
            r1.DataFieldValueChanged += cellCounter.Handler;
            r1.PropertyChanged += propCounter.Handler;

            // Refreshed is identical.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "Same";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(0, cellCounter.Count);
            Assert.Equal(0, propCounter.Count);
        }
    }
}
