using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Refresh_Skips_ExcludedExistingTable()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();

            IDataTable t1 = current.AddNewTable("T1");
            t1.AddColumn("Id", DataTypeNames.INT32);
            t1.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(t1.Columns);
            r["Id"] = 1;
            r["Name"] = "Old";
            t1.AddLoadedRow(r);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt1 = refreshed.AddNewTable("T1");
            rt1.AddColumn("Id", DataTypeNames.INT32);
            rt1.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt1.Columns);
            rr["Id"] = 1;
            rr["Name"] = "New";
            rt1.AddLoadedRow(rr);

            IMergeOptions options = new MergeOptions();
            options.ExcludeTablesFromMerge.Add("T1");

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // unchanged because merge skipped
            Assert.Equal("Old", current.Tables["T1"].Rows[0]["Name"]);
        }
    }
}
