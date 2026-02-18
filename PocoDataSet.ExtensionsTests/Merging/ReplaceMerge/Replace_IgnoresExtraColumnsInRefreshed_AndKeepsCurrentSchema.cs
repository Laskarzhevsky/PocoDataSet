using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace schema contract matrix (Policy A - industry common):
    /// - Current schema is authoritative.
    /// - Refreshed extra columns are ignored.
    /// - Refreshed missing columns do not remove current columns; values for missing columns become null/default on replaced rows.
    /// - Incompatible column type changes are rejected (throw) to avoid silent corruption.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Replace_IgnoresExtraColumnsInRefreshed_AndKeepsCurrentSchema()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Note", DataTypeNames.STRING);

            IDataRow c = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c["Id"] = 1;
            c["Name"] = "Old";
            c["Note"] = "OldNote";
            t.AddLoadedRow(c);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.AddColumn("Note", DataTypeNames.STRING);
            rt.AddColumn("Extra", DataTypeNames.STRING); // should be ignored

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Name"] = "New";
            r["Note"] = "NewNote";
            r["Extra"] = "SHOULD_NOT_APPEAR";
            rt.AddLoadedRow(r);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert: schema unchanged (no Extra column)
            Assert.Equal(3, t.Columns.Count);
            Assert.Equal("Id", t.Columns[0].ColumnName);
            Assert.Equal("Name", t.Columns[1].ColumnName);
            Assert.Equal("Note", t.Columns[2].ColumnName);

            // Assert: row replaced and values copied for known columns only
            Assert.Single(t.Rows);
            Assert.Equal(2, (int)t.Rows[0]["Id"]!);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Equal("NewNote", (string)t.Rows[0]["Note"]!);
        }
    }
}
