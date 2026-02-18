using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the "identical snapshot produces no churn" invariant for RefreshIfNoChangesExist.
    ///
    /// Meaning:
    /// - If REFRESHED is logically identical to CURRENT (same rows and same values),
    ///   refresh merge must not change row instances or create MergeResult entries.
    ///
    /// Why this matters:
    /// - Idempotence alone (running twice) does not fully prove "no-op when identical".
    ///   A buggy merge could rewrite rows (creating churn) yet still appear idempotent on the second run.
    ///
    /// Expected behavior (current observed contract):
    /// - CURRENT table remains unchanged.
    /// - Row instances remain the same objects.
    /// - MergeResult has no Added/Updated/Deleted entries.
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Id=1 with Name="One". REFRESHED has the exact same row/value.
    /// - Act: run DoRefreshMergeIfNoChangesExist.
    /// - Assert: same instance remains, values unchanged, and MergeResult is empty.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void NoOp_WhenIdentical()
        {
            // Arrange CURRENT.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // Capture the row instance to ensure merge does not replace it.
            IDataRow instanceBefore = t.Rows[0];

            // Arrange REFRESHED identical snapshot.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: still one row, still same instance, same values.
            Assert.Single(t.Rows);
            Assert.Same(instanceBefore, t.Rows[0]);
            Assert.Equal("One", (string)t.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);

            // No churn: nothing added/updated/deleted.
            Assert.Empty(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Empty(options.DataSetMergeResult.DeletedDataRows);
        }
    }
}
