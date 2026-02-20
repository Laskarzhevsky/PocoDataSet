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
    public partial class RefreshMergeIfNoChangesExist
    {
        /// <summary>
        /// Ensures RefreshIfNoChangesExist removes the correct row instance when it is missing from the refreshed snapshot: the exact ObservableDataRow reference captured before the merge must no longer be present in the table after the merge.
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

            // Capture the instance that should be deleted.
            IObservableDataRow rowToDelete = MergeTestingHelpers.GetObservableRowById(t, 2);

            // Refreshed snapshot does not contain Id=2, so the row should be removed.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert
            Assert.False(MergeTestingHelpers.ContainsRowInstance(t, rowToDelete));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));

            // Remaining row should still be present.
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
        }
    }
}
