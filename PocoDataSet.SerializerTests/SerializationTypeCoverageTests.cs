using System;
using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.SerializerTests
{
    public class SerializationTypeCoverageTests
    {
        [Fact]
        public void JsonRoundTrip_Preserves_Guid_DateTime_Decimal_AndNullables()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Types");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("G", DataTypeNames.GUID);
            table.AddColumn("DT", DataTypeNames.DATE_TIME);
            table.AddColumn("Money", DataTypeNames.DECIMAL);
            table.AddColumn("OptInt", DataTypeNames.INT32);

            Guid g = Guid.NewGuid();
            DateTime dt = new DateTime(2025, 12, 29, 13, 45, 59, DateTimeKind.Utc);
            decimal money = 1234567890.0123456789m;

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["G"] = g;
            row["DT"] = dt;
            row["Money"] = money;
            row["OptInt"] = null;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? roundTrip = DataSetSerializer.FromJsonString(json);

            Assert.NotNull(roundTrip);
            IDataTable rt = roundTrip!.Tables["Types"];
            Assert.Single(rt.Rows);

            IDataRow rtRow = rt.Rows[0];

            Assert.Equal(1, rtRow["Id"]);
            Assert.IsType<Guid>(rtRow["G"]);
            Assert.Equal(g, (Guid)rtRow["G"]!);

            Assert.IsType<DateTime>(rtRow["DT"]);
            Assert.Equal(dt, (DateTime)rtRow["DT"]!);

            Assert.IsType<decimal>(rtRow["Money"]);
            Assert.Equal(money, (decimal)rtRow["Money"]!);

            Assert.True(rtRow.ContainsKey("OptInt"));
            Assert.Null(rtRow["OptInt"]);
        }
    }
}
