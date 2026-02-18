using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
/// <summary>
/// Locks the schema-drift contract for **RefreshMergePreservingLocalChanges** when the refreshed snapshot contains
/// an **extra column** that does not exist in CURRENT.
///
/// Scenario:
/// - CURRENT schema: (Id PK, Name).
/// - REFRESHED schema: (Id PK, Name, Extra) where "Extra" is unknown to CURRENT.
/// - Snapshot updates the Name value for an existing row.
///
/// Expected contract (current observed behavior):
/// - The merge must **not add** the extra column to CURRENT schema.
/// - The merge must still apply the known-column update (Name).
///
/// How the test proves it:
/// - Arrange builds CURRENT with a known schema and a loaded row.
/// - REFRESHED adds an extra column and changes Name.
/// - Assert checks Name was updated while CURRENT schema does not contain "Extra".
/// </summary>
[Fact]
public void SchemaDrift_ExtraColumns_Ignored()
{
    // Arrange: CURRENT schema and a loaded row.
    IDataSet current = DataSetFactory.CreateDataSet();
    IDataTable currentTable = current.AddNewTable("T");
    currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    currentTable.AddColumn("Name", DataTypeNames.STRING);

    IDataRow currentRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);
    currentRow["Id"] = 1;
    currentRow["Name"] = "Before";
    currentTable.AddLoadedRow(currentRow);

    // REFRESHED snapshot includes an extra column.
    IDataSet refreshed = DataSetFactory.CreateDataSet();
    IDataTable refreshedTable = refreshed.AddNewTable("T");
    refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
    refreshedTable.AddColumn("Name", DataTypeNames.STRING);
    refreshedTable.AddColumn("Extra", DataTypeNames.STRING); // schema drift

    IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(refreshedTable.Columns);
    refreshedRow["Id"] = 1;
    refreshedRow["Name"] = "After";
    refreshedRow["Extra"] = "X";
    refreshedTable.AddLoadedRow(refreshedRow);

    // Act
    MergeOptions options = new MergeOptions();
    current.DoRefreshMergePreservingLocalChanges(refreshed, options);

    // Assert: known-column updated.
    Assert.Equal("After", currentRow["Name"]);

    // Assert: schema did not expand.
    Assert.False(HasColumn(currentTable, "Extra"));
}
    }
}
