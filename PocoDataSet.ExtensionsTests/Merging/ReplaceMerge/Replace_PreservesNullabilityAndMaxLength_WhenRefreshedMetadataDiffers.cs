using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional Replace schema contract tests:
    /// - Replace preserves current column metadata (PK flag, nullability, max length).
    /// - Refreshed column metadata does not overwrite current schema.
    /// </summary>
    public partial class ReplaceMerge
    {
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
    }
}
