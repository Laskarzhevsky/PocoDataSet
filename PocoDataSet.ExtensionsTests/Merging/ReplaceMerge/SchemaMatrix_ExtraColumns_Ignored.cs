using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace schema contract matrix (Policy A - industry common): - Current schema is
    /// authoritative. - Refreshed extra columns are ignored. - Refreshed missing columns do not
    /// remove current columns; values for missing columns become null/default on replaced rows. -
    /// Incompatible column type changes are rejected (throw) to avoid silent corruption.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute Replace merge (replace rows while enforcing current schema contract).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: SchemaMatrix_ExtraColumns_Ignored.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void SchemaMatrix_ExtraColumns_Ignored()
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
