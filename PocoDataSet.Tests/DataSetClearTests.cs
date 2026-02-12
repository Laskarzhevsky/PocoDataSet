using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class DataSetClearTests
    {
        [Fact]
        public void Clear_RemovesAllRows_ButKeepsTablesAndColumns()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();

            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            t.AddLoadedRow(r1);

            IDataRow r2 = t.AddNewRow();
            r2["Id"] = 2;
            r2["Name"] = "B";

            Assert.Equal(2, t.Rows.Count);

            // Act
            int before = t.Columns.Count;
            ds.Clear();
            int after = t.Columns.Count;

            // Assert
            Assert.Equal(before, after);
            Assert.Equal(0, t.Rows.Count);
        }
    }
}
