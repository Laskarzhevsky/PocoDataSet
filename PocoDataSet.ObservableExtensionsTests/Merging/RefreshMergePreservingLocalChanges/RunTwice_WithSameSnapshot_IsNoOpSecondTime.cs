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
        /// Verifies WhenRunTwice WithSameSnapshot IsNoOpSecondTime in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void RunTwice_WithSameSnapshot_IsNoOpSecondTime()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "Old";
            c1.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            rt.AddLoadedRow(r1);

            IObservableMergeOptions options = new ObservableMergeOptions();

            // ------------------------------------------------------------
            // Act 1
            // ------------------------------------------------------------
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            Assert.Empty(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            options.ObservableDataSetMergeResult.Clear();

            // ------------------------------------------------------------
            // Act 2 (same snapshot)
            // ------------------------------------------------------------
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Empty(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            Assert.Single(t.Rows);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
        }
    }
}