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
        /// Ensures that when the merge removes exactly one row from the observable table, the table raises exactly one CollectionChanged(Remove) notification (no Add).
        /// </summary>
        [Fact]
        public void DeletesRow_RaisesCollectionRemoveOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");
            MergeTestingHelpers.AddAcceptedIdNameRow(t, 2, "B");

            CollectionChangedCounter counter = new CollectionChangedCounter();
            t.CollectionChanged += counter.Handler;

            // Refreshed contains only the first row, so the second row should be removed by the merge.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(0, counter.AddEvents);
            Assert.Equal(1, counter.RemoveEvents);
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));
        }
    }
}
