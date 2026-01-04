using System;
using System.Collections;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationNoJsonElementTests
    {
        [Fact]
        public void FromJsonString_DoesNotProduce_JsonElement_InNestedObjectGraph()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Payloads");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Payload", DataTypeNames.OBJECT);

            var payload = new Dictionary<string, object?>
            {
                ["a"] = 1,
                ["b"] = new List<object?> { true, 2, "x" },
                ["c"] = new Dictionary<string, object?>
                {
                    ["d"] = 3.14
                }
            };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Payload"] = payload;
            table.AddLoadedRow(row);

            string json = DataSetSerializer.ToJsonString(dataSet)!;
            IDataSet? restored = DataSetSerializer.FromJsonString(json);

            object restoredPayload = restored!.Tables["Payloads"].Rows[0]["Payload"]!;
            AssertNoJsonElement(restoredPayload);
        }

        private static void AssertNoJsonElement(object? value)
        {
            if (value == null)
            {
                return;
            }

            Type t = value.GetType();
            Assert.NotEqual("JsonElement", t.Name);

            if (value is IDictionary dict)
            {
                foreach (object? v in dict.Values)
                {
                    AssertNoJsonElement(v);
                }
            }
            else if (value is IEnumerable enumerable && value is not string)
            {
                foreach (object? v in enumerable)
                {
                    AssertNoJsonElement(v);
                }
            }
        }
    }
}
