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
        /// Locks the identity contract for RefreshPreservingLocalChanges on an identical snapshot (no-op): the merge should not replace the ObservableDataRow or InnerDataRow, and the row should remain Unchanged.
        /// </summary>
        [Fact]
        public void NoOp_KeepsInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");

            IObservableDataRow beforeRow = MergeTestingHelpers.GetObservableRowById(t, 1);
            IDataRow beforeInner = beforeRow.InnerDataRow;

            // Refreshed snapshot is identical.
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
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            Assert.Same(beforeRow, afterRow);
            Assert.Same(beforeInner, afterRow.InnerDataRow);
            Assert.Equal(DataRowState.Unchanged, afterRow.InnerDataRow.DataRowState);
        }
    }
}
