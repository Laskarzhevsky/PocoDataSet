using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class DataRowCopyFromTests
    {
        [Fact]
        public void CopyFrom_CopiesValues_ForMatchingColumns()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();

            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow src = DataRowExtensions.CreateRowFromColumns(t.Columns);
            src["Id"] = 1;
            src["Name"] = "A";
            t.AddLoadedRow(src);

            IDataRow dst = DataRowExtensions.CreateRowFromColumns(t.Columns);

            // Act
            dst.CopyFrom(src, t.Columns);

            // Assert
            Assert.Equal(1, dst["Id"]);
            Assert.Equal("A", dst["Name"]);
        }

        [Fact]
        public void CopyFrom_IgnoresMissingColumns_AndDoesNotThrow()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();

            IDataTable t1 = ds.AddNewTable("T1");
            t1.AddColumn("Id", DataTypeNames.INT32);

            IDataTable t2 = ds.AddNewTable("T2");
            t2.AddColumn("Id", DataTypeNames.INT32);
            t2.AddColumn("Name", DataTypeNames.STRING);

            IDataRow src = DataRowExtensions.CreateRowFromColumns(t2.Columns);
            src["Id"] = 5;
            src["Name"] = "X";

            IDataRow dst = DataRowExtensions.CreateRowFromColumns(t1.Columns);

            // Act
            dst.CopyFrom(src, t2.Columns);

            // Assert
            Assert.Equal(5, dst["Id"]);
        }
    }
}
