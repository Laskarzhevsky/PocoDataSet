using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Additional Replace schema contract tests:
    /// - Replace preserves current column metadata (PK flag, nullability, max length).
    /// - Refreshed column metadata does not overwrite current schema.
    /// </summary>
    public sealed class ReplaceSchemaPolicyAdditionalTests
    {
        [Fact]
        public void Replace_PreservesPrimaryKeyFlag_WhenRefreshedPrimaryKeyFlagDiffers()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            IColumnMetadata currentId = ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow existing = ct.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            // Refreshed "Id" is NOT marked as PK (different from current)
            IColumnMetadata refreshedId = rt.AddColumn("Id", DataTypeNames.INT32, false, false);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 10;
            r1["Name"] = "New";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: current schema stays authoritative
            IColumnMetadata actualId = GetColumn(ct, "Id");
            Assert.True(actualId.IsPrimaryKey);

            // Sanity: refreshed differs (to ensure test is meaningful)
            Assert.False(refreshedId.IsPrimaryKey);

            // And rows were replaced
            Assert.Single(ct.Rows);
            Assert.Equal(10, (int)ct.Rows[0]["Id"]!);
        }

        [Fact]
        public void Replace_PreservesNullabilityAndMaxLength_WhenRefreshedMetadataDiffers()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);

            IColumnMetadata currentName = ct.AddColumn("Name", DataTypeNames.STRING);
            currentName.IsNullable = false;
            currentName.MaxLength = 10;

            IDataRow existing = ct.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);

            IColumnMetadata refreshedName = rt.AddColumn("Name", DataTypeNames.STRING);
            refreshedName.IsNullable = true;
            refreshedName.MaxLength = 100;

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 2;
            r1["Name"] = "New";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: current schema metadata preserved
            IColumnMetadata actualName = GetColumn(ct, "Name");
            Assert.False(actualName.IsNullable);
            Assert.Equal(10, actualName.MaxLength);

            // Sanity: refreshed differs
            Assert.True(refreshedName.IsNullable);
            Assert.Equal(100, refreshedName.MaxLength);

            // And rows were replaced
            Assert.Single(ct.Rows);
            Assert.Equal(2, (int)ct.Rows[0]["Id"]!);
            Assert.Equal("New", (string)ct.Rows[0]["Name"]!);
        }

        private static IColumnMetadata GetColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == columnName)
                {
                    return table.Columns[i];
                }
            }

            throw new Xunit.Sdk.XunitException("Column not found: " + columnName);
        }
    }
}
