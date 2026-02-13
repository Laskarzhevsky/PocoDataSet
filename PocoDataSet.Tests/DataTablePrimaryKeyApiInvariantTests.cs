using System;
using System.Collections.Generic;
using System.Linq;

using Xunit;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Tests that lock the "PrimaryKeys is the single source of truth" strategy.
    /// These tests intentionally focus on externally observable behavior rather than implementation details.
    /// </summary>
    public class DataTablePrimaryKeyApiInvariantTests
    {
        [Fact]
        public void SetPrimaryKeys_WhenCalled_UpdatesPrimaryKeysAndColumnFlags()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");

            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Code", DataTypeNames.STRING, false, true);
            table.AddColumn("Name", DataTypeNames.STRING, true, false);

            // Assert (table-level truth)
            Assert.NotNull(table.PrimaryKeys);
            Assert.Equal(2, table.PrimaryKeys.Count);
            Assert.Equal("Id", table.PrimaryKeys[0]);
            Assert.Equal("Code", table.PrimaryKeys[1]);

            // Assert (column-level flags must reflect the table-level PK contract)
            Assert.True(GetColumn(table, "Id").IsPrimaryKey);
            Assert.True(GetColumn(table, "Code").IsPrimaryKey);
            Assert.False(GetColumn(table, "Name").IsPrimaryKey);
        }

        [Fact]
        public void ClearPrimaryKeys_WhenCalled_ClearsPrimaryKeysAndColumnFlags()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");

            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);

            // Sanity check
            Assert.Single(table.PrimaryKeys);
            Assert.True(GetColumn(table, "Id").IsPrimaryKey);

            // Act
            table.ClearPrimaryKeys();

            // Assert
            Assert.NotNull(table.PrimaryKeys);
            Assert.Empty(table.PrimaryKeys);
            Assert.False(GetColumn(table, "Id").IsPrimaryKey);
            Assert.False(GetColumn(table, "Name").IsPrimaryKey);
        }

        [Fact]
        public void AddPrimaryKey_WhenCalledTwice_DoesNotCreateDuplicates()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");

            table.AddColumn("Id", DataTypeNames.INT32, false, true);

            // Second call is allowed to be idempotent OR to throw,
            // but it must not result in duplicate key entries.
            try
            {
                table.AddColumn("Id", DataTypeNames.INT32, false, true);
            }
            catch (Exception)
            {
                // Acceptable: the implementation may enforce strictness by throwing on duplicates.
            }

            // Assert
            Assert.NotNull(table.PrimaryKeys);

            int idCount = 0;
            for (int i = 0; i < table.PrimaryKeys.Count; i++)
            {
                if (string.Equals(table.PrimaryKeys[i], "Id", StringComparison.OrdinalIgnoreCase))
                {
                    idCount++;
                }
            }

            Assert.Equal(1, idCount);
            Assert.True(GetColumn(table, "Id").IsPrimaryKey);
        }

        private static IColumnMetadata GetColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                IColumnMetadata column = table.Columns[i];
                if (column != null && string.Equals(column.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return column;
                }
            }

            throw new InvalidOperationException("Column not found: " + columnName);
        }
    }
}
