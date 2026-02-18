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
        public void Replace_WhenRefreshedMissingColumn_CurrentKeepsColumn_AndValuesBecomeNull()
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

            // Refreshed is missing "Note"
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Name"] = "New";
            rt.AddLoadedRow(r);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert: current schema preserved (still has Note)
            Assert.Equal(3, t.Columns.Count);
            Assert.Equal("Note", t.Columns[2].ColumnName);

            // Assert: replaced row has Note = null
            Assert.Single(t.Rows);
            Assert.Equal(2, (int)t.Rows[0]["Id"]!);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Null(t.Rows[0]["Note"]);
        }
    }
}
