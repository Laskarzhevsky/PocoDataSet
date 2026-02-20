using System;

using PocoDataSet.Data;
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
        /// Ensures RefreshPreservingLocalChanges removes the correct row instance when it is missing from the refreshed snapshot and has no pending local changes: the captured ObservableDataRow reference must be removed from the table after the merge.
        /// </summary>
        [Fact]
        public void Delete_RemovesInstanceOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");
            MergeTestingHelpers.AddAcceptedIdNameRow(t, 2, "B");

            // Row 2 has no local changes, so it can be removed if missing from refreshed.
            IObservableDataRow rowToDelete = MergeTestingHelpers.GetObservableRowById(t, 2);

            // Refreshed snapshot only contains row 1.
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
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, rowToDelete));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
        }
    }
}
