using System;
using System.Globalization;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

using Xunit;

namespace PocoDataSet.SerializerTests
{
    public class SerializationFloatingChangesetSparsityTests
    {
        [Fact]
        public void JsonRoundTrip_FloatingRow_PreservesMissingKeys()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.AddColumn("Description", DataTypeNames.STRING);
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            // Create a FLOATING (sparse) row explicitly:
            // We intentionally do NOT set Description at all.
            FloatingDataRow floatingRow = new FloatingDataRow();
            floatingRow["Id"] = 1;
            floatingRow["Name"] = "Sales";
            floatingRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();

            table.AddRow(floatingRow);

            Assert.True(table.Rows[0].TryGetValue("Id", out _));
            Assert.True(table.Rows[0].TryGetValue("Name", out _));
            Assert.False(table.Rows[0].TryGetValue("Description", out _)); // precondition

            // Act
            string? json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? restored = DataSetSerializer.FromJsonString(json);

            // Assert
            Assert.NotNull(restored);

            IDataTable restoredTable = restored!.Tables["Department"];
            Assert.Single(restoredTable.Rows);

            IDataRow restoredRow = restoredTable.Rows[0];

            Assert.True(restoredRow.TryGetValue("Id", out _));
            Assert.True(restoredRow.TryGetValue("Name", out _));

            // The missing key must remain missing after round-trip.
            Assert.False(restoredRow.TryGetValue("Description", out _));
        }
    }
}
