using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.SerializerTests
{
    public class SerializationNormalizationEdgeCasesTests
    {
        [Fact]
        public void JsonRoundTrip_Distinguishes_EmptyString_From_Null()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Strings");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("S1", DataTypeNames.STRING);
            table.AddColumn("S2", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["S1"] = "";
            row["S2"] = null;

            table.AddLoadedRow(row);

            // Act
            string? json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? rt = DataSetSerializer.FromJsonString(json);

            // Assert
            Assert.NotNull(rt);

            IDataTable strings = rt!.Tables["Strings"];
            Assert.Single(strings.Rows);

            IDataRow r = strings.Rows[0];

            Assert.True(r.ContainsKey("S1"));
            Assert.True(r.ContainsKey("S2"));

            Assert.IsType<string>(r["S1"]);
            Assert.Equal("", (string)r["S1"]!);

            Assert.Null(r["S2"]);
        }

        [Fact]
        public void JsonRoundTrip_Preserves_Bool_And_NullableBool()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Bools");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Flag", DataTypeNames.BOOL);
            table.AddColumn("OptFlag", DataTypeNames.BOOL);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Flag"] = true;
            row["OptFlag"] = null;

            table.AddLoadedRow(row);

            // Act
            string? json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? rt = DataSetSerializer.FromJsonString(json);

            // Assert
            Assert.NotNull(rt);

            IDataTable bools = rt!.Tables["Bools"];
            Assert.Single(bools.Rows);

            IDataRow r = bools.Rows[0];

            Assert.IsType<bool>(r["Flag"]);
            Assert.True((bool)r["Flag"]!);

            Assert.True(r.ContainsKey("OptFlag"));
            Assert.Null(r["OptFlag"]);
        }
    }
}
