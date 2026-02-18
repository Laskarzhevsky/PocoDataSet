using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge schema contract (as implemented today):
    /// - Current schema remains authoritative.
    /// - Refreshed rows replace current rows.
    /// - Extra columns present only in refreshed are ignored.
    /// - Columns missing from refreshed remain in current schema (values become default/null).
    /// - System columns (e.g. __ClientKey) may be auto-added by infrastructure and are not treated as user schema.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Replace_PreservesCurrentSchema_WhenRefreshedIsMissingColumns()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow existing = currentTable.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            // NOTE: "Name" intentionally missing

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 10;

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: user schema is preserved
            Assert.True(HasColumn(currentTable, "Id"));
            Assert.True(HasColumn(currentTable, "Name"));
            Assert.Equal(2, CountUserColumns(currentTable));

            // Assert: rows replaced, missing column becomes default/null
            Assert.Single(currentTable.Rows);
            Assert.Equal(10, (int)currentTable.Rows[0]["Id"]!);

            object? name = currentTable.Rows[0]["Name"];
            Assert.True(name == null || name == DBNull.Value);
        }
    }
}
