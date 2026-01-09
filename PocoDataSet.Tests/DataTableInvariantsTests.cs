using PocoDataSet.Extensions;
using PocoDataSet.IData;
using Xunit;

namespace PocoDataSet.Tests
{
    public  class DataTableInvariantsTests
    {
        [Fact]
        public void AddRow_ThenContainsRow_AndRemoveRow_Works()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "Customer Service";

            table.AddRow(row);

            Assert.True(table.ContainsRow(row));
            Assert.Equal(1, table.Rows.Count);

            bool removed = table.RemoveRow(row);

            Assert.True(removed);
            Assert.False(table.ContainsRow(row));
            Assert.Equal(0, table.Rows.Count);
        }

        [Fact]
        public void RemoveRowAt_RemovesCorrectRow()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            table.AddRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            table.AddRow(r2);

            table.RemoveRowAt(0);

            Assert.Equal(1, table.Rows.Count);
            Assert.Equal(2, table.Rows[0]["Id"]);
        }

        [Fact]
        public void RemoveAllRows_ClearsTable()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r1["Id"] = 1;
            r1["Name"] = "A";
            table.AddRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            r2["Id"] = 2;
            r2["Name"] = "B";
            table.AddRow(r2);

            table.RemoveAllRows();

            Assert.Equal(0, table.Rows.Count);
        }
    }
}
