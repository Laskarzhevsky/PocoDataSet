using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge schema contract: column ORDER is owned by CURRENT and must not be altered by REFRESHED.
    ///
    /// Meaning:
    /// - Replace is a destructive reload of rows, but it must keep CURRENT schema as-is.
    /// - "As-is" includes column ordering, because UI rendering and serialization can depend on it.
    ///
    /// Scenario:
    /// - CURRENT table schema is [Id, Name, City] (in this exact order).
    /// - REFRESHED table contains the same columns but in a different order [City, Id, Name].
    ///
    /// How the test proves the contract:
    /// - After DoReplaceMerge, CURRENT.Columns are inspected by index.
    /// - The column names must remain in the original CURRENT order.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Keeps_ColumnOrder()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable currentTable = current.AddNewTable("T");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn("City", DataTypeNames.STRING);

            // Seed at least one row so that Replace actually rebuilds rows.
            IDataRow cur = currentTable.AddNewRow();
            cur["Id"] = 1;
            cur["Name"] = "Old";
            cur["City"] = "OldCity";

            IDataSet refreshed = new DataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("T");

            // Intentionally different order than CURRENT.
            refreshedTable.AddColumn("City", DataTypeNames.STRING);
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 2;
            r1["Name"] = "New";
            r1["City"] = "NewCity";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: CURRENT column order is preserved.
            Assert.Equal("Id", currentTable.Columns[0].ColumnName);
            Assert.Equal("Name", currentTable.Columns[1].ColumnName);
            Assert.Equal("City", currentTable.Columns[2].ColumnName);
        }
    }
}
