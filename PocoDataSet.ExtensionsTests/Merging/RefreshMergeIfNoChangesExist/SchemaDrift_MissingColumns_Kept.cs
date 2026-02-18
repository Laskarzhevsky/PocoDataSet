using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks schema drift behavior for RefreshIfNoChangesExist when REFRESHED is missing columns.
    ///
    /// Expected behavior (current observed contract):
    /// - Missing columns in REFRESHED do not remove columns from CURRENT (CURRENT schema is authoritative).
    /// - For rows that are refreshed from REFRESHED, columns that are missing in REFRESHED are set to null
    ///   (i.e., the refresh snapshot defines the complete row payload it can provide).
    ///
    /// How it proves the contract:
    /// - Arrange: CURRENT has Id+Name+Extra (Extra populated). REFRESHED has Id+Name only.
    /// - Act: merge (updates Name for the matching row).
    /// - Assert: CURRENT still has Extra column, but because REFRESHED lacks it, the refreshed row's Extra becomes null.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void SchemaDrift_MissingColumns_Kept()
        {
            // Arrange CURRENT with an extra column that REFRESHED will not include.
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1["Extra"] = "KeepMe";
            t.AddLoadedRow(c1);

            // Arrange REFRESHED without the Extra column.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One (updated)";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: CURRENT schema is kept (schema drift does not remove columns).
            Assert.Equal(3, t.Columns.Count);
            Assert.NotNull(HasColumn(t, "Extra"));

            // Name updated from refreshed snapshot.
            Assert.Equal("One (updated)", (string)t.Rows[0]["Name"]!);

            // Extra was missing from REFRESHED, so the refreshed row's Extra becomes null.
            Assert.Null(t.Rows[0]["Extra"]);
        }
    }
}
