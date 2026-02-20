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
        /// When the refreshed snapshot contains an extra row (a PK that does not exist in CURRENT), RefreshIfNoChangesExist should add that row into the observable table and finalize it as Unchanged.
        /// </summary>
        [Fact]
        public void AddsRow_WhenSnapshotHasExtra()
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

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One (server)";

            IDataRow r2 = rt.AddNewRow();
            r2["Id"] = 2;
            r2["Name"] = "Two (server)";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: both rows exist and are Unchanged
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 1));
            Assert.True(MergeTestingHelpers.ContainsRowWithId(t, 2));

            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
            Assert.Equal(DataRowState.Unchanged, t.Rows[1].DataRowState);

        }
    }
}
