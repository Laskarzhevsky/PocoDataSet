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
        /// Ensures Replace merge still raises collection notifications even when refreshed is identical,
        /// because Replace rebuilds the row collection.
        /// Current contains 1 row and refreshed contains 1 row, so we expect 1 Remove and 1 Add.
        /// </summary>
        [Fact]
        public void NoOp_NoCollectionAddOrRemove()
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

            // Refreshed is identical (no-op).
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(1, counter.AddEvents);
            Assert.Equal(1, counter.RemoveEvents);
        }
    }
}
