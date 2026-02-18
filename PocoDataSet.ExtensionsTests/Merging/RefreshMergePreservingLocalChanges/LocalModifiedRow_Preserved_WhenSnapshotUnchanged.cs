using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
/// <summary>
/// Locks the behavior for **RefreshMergePreservingLocalChanges** when a row is **Modified locally** but the
/// refreshed snapshot contains the **original (unchanged) values** for that same row.
///
/// Scenario:
/// - CURRENT contains a single loaded row (Id=1, Name="Original"), then the client edits it to Name="LocalEdit"
///   which makes the row state Modified.
/// - REFRESHED snapshot contains the same row (Id=1) with Name still equal to the original value ("Original").
///
/// Expected contract:
/// - The merge must **preserve the local modification** and must not reset it back to the snapshot value.
/// - The row stays in state Modified (the client still has pending changes).
/// - No "Updated" merge-result entry is produced for this row because nothing from the server was applied.
///
/// How the test proves it:
/// - Arrange creates the exact local Modified state and a snapshot that did not change the row.
/// - Act runs DoRefreshMergePreservingLocalChanges.
/// - Assert verifies value and state are preserved, and merge result has no update entries.
/// </summary>
[Fact]
public void LocalModifiedRow_Preserved_WhenSnapshotUnchanged()
{
    // Arrange: CURRENT has a single row that is locally Modified.
    IDataSet current = DataSetFactory.CreateDataSet();
    IDataTable currentTable = current.AddNewTable("T");
    currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    currentTable.AddColumn("Name", DataTypeNames.STRING);

    IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
    currentRow["Id"] = 1;
    currentRow["Name"] = "Original";
    currentTable.AddLoadedRow(currentRow);

    // Client edits the loaded row -> Modified.
    currentRow["Name"] = "LocalEdit";
    Assert.Equal(DataRowState.Modified, currentRow.DataRowState);

    // REFRESHED snapshot still has the original value (server did not change it).
    IDataSet refreshed = DataSetFactory.CreateDataSet();
    IDataTable refreshedTable = refreshed.AddNewTable("T");
    refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    refreshedTable.AddColumn("Name", DataTypeNames.STRING);

    IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
    refreshedRow["Id"] = 1;
    refreshedRow["Name"] = "Original";
    refreshedTable.AddLoadedRow(refreshedRow);

    // Act: refresh-preserving merge.
    // Note: for this merge mode, the merge method returns void and writes merge accounting
            // into options.DataSetMergeResult.
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

    // Assert: local modification is preserved and not reset by the unchanged snapshot.
    Assert.Equal(1, currentTable.Rows.Count);
    Assert.Equal(DataRowState.Modified, currentRow.DataRowState);
    Assert.Equal("LocalEdit", currentRow["Name"]);

    // Because the server snapshot did not apply anything onto this row,
    // we expect no Updated entries (and no Added/Deleted entries either).
    Assert.Empty(result.UpdatedDataRows);
    Assert.Empty(result.AddedDataRows);
    Assert.Empty(result.DeletedDataRows);
}
    }
}
