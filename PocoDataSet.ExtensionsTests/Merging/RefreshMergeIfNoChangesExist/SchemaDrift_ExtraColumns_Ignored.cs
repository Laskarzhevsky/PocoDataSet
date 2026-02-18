using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks schema drift behavior for RefreshIfNoChangesExist when REFRESHED contains extra columns.
    ///
    /// Expected behavior (current observed contract):
    /// - Extra columns in REFRESHED are ignored (CURRENT schema is authoritative).
    /// - Merge still updates correlatable columns (e.g., Name).
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT schema has Id+Name. REFRESHED has Id+Name+Extra.
    /// - Act: merge.
    /// - Assert: CURRENT schema remains Id+Name (Extra is not added) and Name updates for correlated row.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void SchemaDrift_ExtraColumns_Ignored()
        {
            // Arrange CURRENT schema (authoritative).
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            // REFRESHED contains an extra column that does not exist in CURRENT.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One (updated)";
            r1["Extra"] = "Ignored";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: CURRENT schema not expanded.
            Assert.Equal(2, t.Columns.Count);
            Assert.True(HasColumn(t, "Id"));
            Assert.True(HasColumn(t, "Name"));
            Assert.False(HasColumn(t, "Extra"));

// Assert: data updated where schema overlaps.
            Assert.Equal("One (updated)", (string)t.Rows[0]["Name"]!);
            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
        }
    }
}
