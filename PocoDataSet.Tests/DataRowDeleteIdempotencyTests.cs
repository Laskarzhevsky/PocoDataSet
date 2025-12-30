using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataRowDeleteIdempotencyTests
    {
        [Fact]
        public void DeleteRow_CalledTwice_DoesNotCorruptState()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Name"] = "A";
            table.AddLoadedRow(row);

            Assert.Equal(DataRowState.Unchanged, row.DataRowState);

            // Act
            table.DeleteRow(row);
            DataRowState first = row.DataRowState;

            table.DeleteRow(row);
            DataRowState second = row.DataRowState;

            // Assert
            Assert.Equal(DataRowState.Deleted, first);
            Assert.Equal(DataRowState.Deleted, second);

            // Row should still be in table until AcceptChanges (soft delete)
            Assert.Equal(1, table.Rows.Count);
            Assert.Same(row, table.Rows[0]);
        }
    }
}
