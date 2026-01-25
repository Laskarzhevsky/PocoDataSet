using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public class SerializationNestedObjectGraphTests
    {
        [Fact]
        public void JsonRoundTrip_ObjectColumn_Preserves_NestedDictionaryAndArrayGraph_AsPrimitives()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Payloads");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Payload", DataTypeNames.OBJECT);

            Dictionary<string, object?> payload = new Dictionary<string, object?>();
            payload["a"] = 1;
            payload["b"] = new List<object?> { true, null, "x" };

            Dictionary<string, object?> inner = new Dictionary<string, object?>();
            inner["d"] = 2.5; // number (may come back as decimal or double)
            payload["c"] = inner;

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Payload"] = payload;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            Assert.NotNull(restored);

            IDataRow restoredRow = restored!.Tables["Payloads"].Rows[0];

            Assert.IsType<Dictionary<string, object?>>(restoredRow["Payload"]!);

            Dictionary<string, object?> restoredPayload = (Dictionary<string, object?>)restoredRow["Payload"]!;

            Assert.True(restoredPayload.ContainsKey("a"));
            Assert.True(restoredPayload.ContainsKey("b"));
            Assert.True(restoredPayload.ContainsKey("c"));

            // "a" should be a primitive numeric (usually Int64 from JSON).
            Assert.NotNull(restoredPayload["a"]);
            Assert.True(restoredPayload["a"] is long || restoredPayload["a"] is int);

            // "b" should be List<object?>
            Assert.IsType<List<object?>>(restoredPayload["b"]!);
            List<object?> restoredList = (List<object?>)restoredPayload["b"]!;
            Assert.Equal(3, restoredList.Count);
            Assert.IsType<bool>(restoredList[0]!);
            Assert.Null(restoredList[1]);
            Assert.IsType<string>(restoredList[2]!);

            // "c" should be Dictionary<string, object?>
            Assert.IsType<Dictionary<string, object?>>(restoredPayload["c"]!);
            Dictionary<string, object?> restoredInner = (Dictionary<string, object?>)restoredPayload["c"]!;
            Assert.True(restoredInner.ContainsKey("d"));
            Assert.NotNull(restoredInner["d"]);

            // "d" may be decimal or double depending on JsonElement parsing.
            object dVal = restoredInner["d"]!;
            Assert.True(dVal is decimal || dVal is double);

            double dAsDouble;
            if (dVal is decimal dec)
            {
                dAsDouble = (double)dec;
            }
            else
            {
                dAsDouble = (double)dVal;
            }

            Assert.Equal(2.5d, dAsDouble);
        }
    }
}
