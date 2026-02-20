using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Observable Replace merge schema contract (as implemented today):
    /// - Current schema remains authoritative.
    /// - Refreshed rows replace current rows.
    /// - Extra columns present only in refreshed are ignored.
    /// - Columns missing from refreshed remain in current schema (values become default/null).
    /// - System columns (e.g. __ClientKey) may be auto-added by infrastructure and are not treated as user schema.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void PreservesCurrentSchema_AndIgnoresExtraColumnsInRefreshed()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow existing = currentTable.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            existing.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 10;
            r1["Name"] = "New";
            r1["Extra"] = "Ignored";

            // Act
            IObservableMergeOptions options = new ObservableMergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: user schema is preserved (system columns allowed)
            Assert.True(MergeTestingHelpers.HasColumn(currentTable, "Id"));
            Assert.True(MergeTestingHelpers.HasColumn(currentTable, "Name"));
            Assert.False(MergeTestingHelpers.HasColumn(currentTable, "Extra"));
            Assert.Equal(2, MergeTestingHelpers.CountUserColumns(currentTable));

            // Assert: rows replaced
            Assert.Single(currentTable.Rows);
            Assert.Equal(10, (int)currentTable.Rows[0]["Id"]!);
            Assert.Equal("New", (string)currentTable.Rows[0]["Name"]!);
        }
    }
}
