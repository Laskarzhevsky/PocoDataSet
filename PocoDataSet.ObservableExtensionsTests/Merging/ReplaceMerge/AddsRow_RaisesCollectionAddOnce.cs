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
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Ensures Replace merge raises collection notifications consistent with its "replace all rows" behavior.
        /// Current contains 1 row and refreshed contains 2 rows, so we expect 1 Remove and 2 Add events.
        /// </summary>
        [Fact]
        public void AddsRow_RaisesCollectionAddOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");

            CollectionChangedCounter counter = new CollectionChangedCounter();
            t.CollectionChanged += counter.Handler;

            // Refreshed contains existing row plus one new row.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IDataRow rr2 = rt.AddNewRow();
            rr2["Id"] = 2;
            rr2["Name"] = "B";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(2, counter.AddEvents);
            Assert.Equal(1, counter.RemoveEvents);
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 2));
        }
    }
}
