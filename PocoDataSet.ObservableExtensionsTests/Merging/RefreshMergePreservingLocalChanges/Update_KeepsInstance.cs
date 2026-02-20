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
        /// Locks the identity contract for RefreshPreservingLocalChanges when there are no pending local changes: applying a refreshed snapshot update must keep the same ObservableDataRow and InnerDataRow instances (update in place).
        /// </summary>
        [Fact]
        public void Update_KeepsInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "A");

            // Capture identity before merge.
            IObservableDataRow beforeRow = MergeTestingHelpers.GetObservableRowById(t, 1);
            IDataRow beforeInner = beforeRow.InnerDataRow;

            // Refreshed snapshot updates the name.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = rt.AddNewRow();
            rr1["Id"] = 1;
            rr1["Name"] = "A2";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            // With no pending local changes, RefreshPreservingLocalChanges should update in place.
            Assert.Same(beforeRow, afterRow);
            Assert.Same(beforeInner, afterRow.InnerDataRow);
            Assert.Equal("A2", afterRow["Name"]);
        }
    }
}
