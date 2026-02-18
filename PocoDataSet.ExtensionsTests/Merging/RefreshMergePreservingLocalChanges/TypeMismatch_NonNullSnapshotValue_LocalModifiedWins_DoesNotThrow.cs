using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Locks the "type mismatch does not throw" behavior at the cell level, while also enforcing
        /// the "local wins" rule for locally Modified rows.
        ///
        /// Scenario:
        /// - CURRENT declares Name as STRING and has a locally Modified row (Id=1).
        /// - REFRESHED snapshot provides a non-null, incompatible value type for Name (e.g., INT32 123).
        ///
        /// Expected behavior:
        /// - The merge does NOT throw.
        /// - Because the row is locally Modified, local values win: Name remains the local string.
        /// - MergeResult does not record an update for that row (since snapshot is not applied onto it).
        /// </summary>
        [Fact]
        public void TypeMismatch_NonNullSnapshotValue_LocalModifiedWins_DoesNotThrow()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(ct.Columns);
            c1["Id"] = 1;
            c1["Name"] = "Original";
            ct.AddLoadedRow(c1);

            // Local edit.
            c1["Name"] = "LOCAL";
            Assert.Equal(DataRowState.Modified, c1.DataRowState);

            // REFRESHED provides an incompatible (but non-null) value type.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = 123; // intentionally incompatible with the declared STRING schema
            rt.AddLoadedRow(r1);

            // Act
            MergeOptions options = new MergeOptions();

            // We lock "does not throw" explicitly.
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert
            // Local wins: the modified value remains.
            Assert.Equal("LOCAL", (string)ct.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Modified, ct.Rows[0].DataRowState);

            // No update should be recorded (snapshot could not override local modification).
            Assert.Empty(result.UpdatedDataRows);
            Assert.Empty(result.DeletedDataRows);
        }
    }
}
