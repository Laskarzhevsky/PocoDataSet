using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationRoundTripTests
    {
        [Fact]
        public void JsonRoundTrip_PreservesRowsValuesAndStates()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row1 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row1["Id"] = 1;
            row1["Name"] = "Customer Service";
            table.AddLoadedRow(row1);

            IDataRow row2 = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row2["Id"] = 2;
            row2["Name"] = "Financial";
            table.AddLoadedRow(row2);

            // Modify second row to ensure OriginalValues + Modified state survive round-trip
            row2["Name"] = "Reception";
            Assert.Equal(DataRowState.Modified, row2.DataRowState);
            Assert.True(row2.HasOriginalValues);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            Assert.NotNull(restored);

            IDataTable restoredTable = restored!.Tables["Department"];
            Assert.Equal(2, restoredTable.Rows.Count);

            IDataRow restoredRow1 = restoredTable.Rows[0];
            IDataRow restoredRow2 = restoredTable.Rows[1];

            Assert.Equal(1, restoredRow1["Id"]);
            Assert.Equal("Customer Service", restoredRow1["Name"]);
            Assert.Equal(DataRowState.Unchanged, restoredRow1.DataRowState);

            Assert.Equal(2, restoredRow2["Id"]);
            Assert.Equal("Reception", restoredRow2["Name"]);
            Assert.Equal(DataRowState.Modified, restoredRow2.DataRowState);
            Assert.True(restoredRow2.HasOriginalValues);
            Assert.Equal("Financial", restoredRow2.OriginalValues["Name"]);
        }
    }
}
