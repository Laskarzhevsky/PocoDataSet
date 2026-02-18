using PocoDataSet.Extensions;
using PocoDataSet.IData;
using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        [Fact]
        public void ReplacesAllTables_InDataSet()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();

            IDataTable t1 = current.AddNewTable("T1");
            t1.AddColumn("Id", DataTypeNames.INT32, false, true);
            t1.AddColumn("Name", DataTypeNames.STRING);
            t1.AddNewRow()["Id"] = 1;
            t1.Rows[0]["Name"] = "Old1";

            IDataTable t2 = current.AddNewTable("T2");
            t2.AddColumn("Id", DataTypeNames.INT32, false, true);
            t2.AddColumn("Name", DataTypeNames.STRING);
            t2.AddNewRow()["Id"] = 1;
            t2.Rows[0]["Name"] = "Old2";

            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();

            IDataTable r1 = refreshed.AddNewTable("T1");
            r1.AddColumn("Id", DataTypeNames.INT32, false, true);
            r1.AddColumn("Name", DataTypeNames.STRING);
            r1.AddNewRow()["Id"] = 10;
            r1.Rows[0]["Name"] = "New1";

            IDataTable r2 = refreshed.AddNewTable("T2");
            r2.AddColumn("Id", DataTypeNames.INT32, false, true);
            r2.AddColumn("Name", DataTypeNames.STRING);
            r2.AddNewRow()["Id"] = 20;
            r2.Rows[0]["Name"] = "New2";

            MergeOptions mergeOptions = new MergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, mergeOptions);

            // Assert
            Assert.Equal(10, (int)current.Tables["T1"].Rows[0]["Id"]!);
            Assert.Equal("New1", (string)current.Tables["T1"].Rows[0]["Name"]!);

            Assert.Equal(20, (int)current.Tables["T2"].Rows[0]["Id"]!);
            Assert.Equal("New2", (string)current.Tables["T2"].Rows[0]["Name"]!);
        }
    }
}
