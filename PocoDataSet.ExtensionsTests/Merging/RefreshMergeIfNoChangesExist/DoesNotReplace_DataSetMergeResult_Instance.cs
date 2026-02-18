using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Options propagation coverage for RefreshIfNoChangesExist.
    /// Locks that merge does not replace MergeOptions.DataSetMergeResult instance.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void DoesNotReplace_DataSetMergeResult_Instance()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One_Refreshed";
            rt.AddLoadedRow(r1);

            MergeOptions options = new MergeOptions();
            IDataSetMergeResult expectedResultInstance = options.DataSetMergeResult;

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.DataSetMergeResult);
            Assert.Equal("One_Refreshed", (string)current.Tables["T"].Rows[0]["Name"]!);
        }
    }
}
