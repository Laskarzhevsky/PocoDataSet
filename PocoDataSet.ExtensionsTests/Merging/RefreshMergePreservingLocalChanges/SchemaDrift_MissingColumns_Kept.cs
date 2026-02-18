using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
/// <summary>
/// Locks the schema-drift contract for **RefreshMergePreservingLocalChanges** when the refreshed snapshot is
/// **missing a column** that exists in CURRENT.
///
/// Scenario:
/// - CURRENT schema: (Id PK, Name, Keep).
/// - REFRESHED schema: (Id PK, Name) - the "Keep" column is missing from snapshot.
/// - Snapshot updates Name for the row.
///
/// Expected contract (based on current observed behavior in RefreshIfNoChangesExist and consistent with this mode):
/// - CURRENT schema is preserved: "Keep" column remains present.
/// - For refreshed rows, missing-column values become **null** (the server did not send them).
///
/// How the test proves it:
/// - Arrange sets Keep="KeepMe" in CURRENT.
/// - Act runs the merge with a snapshot that lacks "Keep".
/// - Assert verifies Keep column still exists but value is null after refresh, while Name was updated.
/// </summary>
[Fact]
public void SchemaDrift_MissingColumns_Kept()
{
    // Arrange: CURRENT schema includes a column that snapshot will omit.
    IDataSet current = DataSetFactory.CreateDataSet();
    IDataTable currentTable = current.AddNewTable("T");
    currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    currentTable.AddColumn("Name", DataTypeNames.STRING);
    currentTable.AddColumn("Keep", DataTypeNames.STRING);

    IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
    currentRow["Id"] = 1;
    currentRow["Name"] = "Before";
    currentRow["Keep"] = "KeepMe";
    currentTable.AddLoadedRow(currentRow);

    // REFRESHED snapshot omits the "Keep" column.
    IDataSet refreshed = DataSetFactory.CreateDataSet();
    IDataTable refreshedTable = refreshed.AddNewTable("T");
    refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    refreshedTable.AddColumn("Name", DataTypeNames.STRING);

    IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
    refreshedRow["Id"] = 1;
    refreshedRow["Name"] = "After";
    refreshedTable.AddLoadedRow(refreshedRow);

    // Act
    MergeOptions options = new MergeOptions();
    current.DoRefreshMergePreservingLocalChanges(refreshed, options);

    // Assert: schema kept.
    Assert.True(HasColumn(currentTable, "Keep"));

    // Assert: known-column updated.
    Assert.Equal("After", currentRow["Name"]);

    // Assert: missing snapshot column results in null value for refreshed row.
    Assert.Null(currentRow["Keep"]);
}
    }
}
