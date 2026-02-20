using System;

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
        /// When the refreshed snapshot is missing a CURRENT row (a PK that existed locally), RefreshIfNoChangesExist should remove that row from the observable table.
        /// </summary>
        [Fact]
        public void DeletesRow_WhenSnapshotMissing()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1.AcceptChanges();

            IObservableDataRow c2 = t.AddNewRow();
            c2["Id"] = 2;
            c2["Name"] = "Two";
            c2.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            // Only Id=1 remains in snapshot (Id=2 is deleted server-side).
            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One (server)";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: Id=2 was removed, Id=1 remains
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(t, 2));
            Assert.Single(t.Rows);

        }
    }
}
