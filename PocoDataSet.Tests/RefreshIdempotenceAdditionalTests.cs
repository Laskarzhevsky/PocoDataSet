using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Refresh idempotence: applying the same refreshed snapshot twice must be a no-op the second time.
    /// This protects against future regressions that re-touch rows and capture OriginalValues churn.
    /// </summary>
    public sealed class RefreshIdempotenceAdditionalTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_WhenRunTwice_WithSameSnapshot_IsNoOpSecondTime()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Old";
            t.AddLoadedRow(c1);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // ------------------------------------------------------------
            // Act 1 (changes value Old -> New)
            // ------------------------------------------------------------
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Sanity: first run updated once.
            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);

            // Clear to observe the second run.
            options.DataSetMergeResult.Clear();

            // ------------------------------------------------------------
            // Act 2 (same snapshot again)
            // ------------------------------------------------------------
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // ------------------------------------------------------------
            // Assert: second run is a no-op
            // ------------------------------------------------------------
            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);

            Assert.Single(t.Rows);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
            Assert.False(t.Rows[0].HasOriginalValues);
        }

        [Fact]
        public void RefreshIfNoChangesExist_WhenRunTwice_WithSameSnapshot_IsNoOpSecondTime()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Old";
            t.AddLoadedRow(c1);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "New";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // ------------------------------------------------------------
            // Act 1 (changes value Old -> New)
            // ------------------------------------------------------------
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);

            options.DataSetMergeResult.Clear();

            // ------------------------------------------------------------
            // Act 2 (same snapshot again)
            // ------------------------------------------------------------
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // ------------------------------------------------------------
            // Assert: second run is a no-op
            // ------------------------------------------------------------
            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);

            Assert.Single(t.Rows);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
            Assert.False(t.Rows[0].HasOriginalValues);
        }
    }
}
