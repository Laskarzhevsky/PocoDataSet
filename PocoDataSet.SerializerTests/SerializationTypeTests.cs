using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.SerializerTests
{
    public class SerializationTypeTests
    {
        [Fact]
        public void JsonRoundTrip_PreservesGuidType()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.GUID);

            Guid g = Guid.NewGuid();
            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = g;
            table.AddLoadedRow(row);

            string json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? rt = DataSetSerializer.FromJsonString(json);

            Assert.NotNull(rt);
            object? v = rt!.Tables["T"].Rows[0]["Id"];
            Assert.IsType<Guid>(v);
            Assert.Equal(g, (Guid)v!);
        }

        [Fact]
        public void JsonRoundTrip_PreservesDateTimeType()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Created", DataTypeNames.DATE_TIME);

            DateTime dt = new DateTime(2025, 12, 29, 13, 45, 10, DateTimeKind.Utc);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Created"] = dt;
            table.AddLoadedRow(row);

            string json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? rt = DataSetSerializer.FromJsonString(json);

            Assert.NotNull(rt);
            object? v = rt!.Tables["T"].Rows[0]["Created"];
            Assert.IsType<DateTime>(v);
            Assert.Equal(dt, (DateTime)v!);
        }

        [Fact]
        public void JsonRoundTrip_PreservesDecimalType()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Amount", DataTypeNames.DECIMAL);

            decimal amount = 1234567890.12345m;

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Amount"] = amount;
            table.AddLoadedRow(row);

            string json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? rt = DataSetSerializer.FromJsonString(json);

            Assert.NotNull(rt);
            object? v = rt!.Tables["T"].Rows[0]["Amount"];
            Assert.IsType<decimal>(v);
            Assert.Equal(amount, (decimal)v!);
        }
    }
}
