using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Observable Replace merge schema contract (as implemented today):
    /// - Current schema remains authoritative.
    /// - Refreshed rows replace current rows.
    /// - Extra columns present only in refreshed are ignored.
    /// - Columns missing from refreshed remain in current schema (values become default/null).
    /// - System columns (e.g. __ClientKey) may be auto-added by infrastructure and are not treated as user schema.
    /// </summary>
    public class ObservableReplaceSchemaPolicyTests
    {
        [Fact]
        public void Replace_PreservesCurrentSchema_AndIgnoresExtraColumnsInRefreshed()
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
            Assert.True(HasColumn(currentTable, "Id"));
            Assert.True(HasColumn(currentTable, "Name"));
            Assert.False(HasColumn(currentTable, "Extra"));
            Assert.Equal(2, CountUserColumns(currentTable));

            // Assert: rows replaced
            Assert.Single(currentTable.Rows);
            Assert.Equal(10, (int)currentTable.Rows[0]["Id"]!);
            Assert.Equal("New", (string)currentTable.Rows[0]["Name"]!);
        }

        [Fact]
        public void Replace_PreservesCurrentSchema_WhenRefreshedIsMissingColumns()
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
            // NOTE: "Name" intentionally missing

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 10;

            // Act
            IObservableMergeOptions options = new ObservableMergeOptions();
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

        static int CountUserColumns(IObservableDataTable table)
        {
            int count = 0;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string name = table.Columns[i].ColumnName;

                if (name == SpecialColumnNames.CLIENT_KEY)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        static bool HasColumn(IObservableDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
