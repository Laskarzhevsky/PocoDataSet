using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

using Xunit;

namespace PocoDataSet.SerializerTests
{
    public class SerializationFloatingRowSemanticsTests
    {
        [Fact]
        public void JsonRoundTrip_FloatingRow_PreservesMissingFieldDistinctFromExplicitNull()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);

            // Row1: FLOATING row → Description is truly missing
            FloatingDataRow row1 = new FloatingDataRow();
            row1["Id"] = 1;
            row1["Name"] = "Sales";
            table.AddRow(row1);

            Assert.False(row1.TryGetValue("Description", out _)); // critical precondition

            // Row2: dense row with explicit null
            IDataRow row2 = table.AddNewRow();
            row2["Id"] = 2;
            row2["Name"] = "Support";
            row2["Description"] = null;
            row2.AcceptChanges();

            // Act
            string json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? restored = DataSetSerializer.FromJsonString(json);

            // Assert
            IDataTable restoredTable = restored!.Tables["Department"];

            IDataRow restoredRow1 = restoredTable.Rows[0];
            IDataRow restoredRow2 = restoredTable.Rows[1];

            Assert.False(restoredRow1.TryGetValue("Description", out _)); // missing stays missing

            Assert.True(restoredRow2.TryGetValue("Description", out object? desc2));
            Assert.Null(desc2); // explicit null stays explicit null
        }
    }
}
