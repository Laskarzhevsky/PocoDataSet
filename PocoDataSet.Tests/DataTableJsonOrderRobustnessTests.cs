using System;
using System.Text.Json;
using Xunit;
using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.Extensions;

namespace PocoDataSet.Tests
{
    public class DataTableJsonOrderRobustnessTests
    {
        [Fact]
        public void JsonRoundTrip_WhenRowsAppearBeforeColumns_DoesNotLoseSchemaOrRowValues()
        {
            DataTable table = new DataTable();
            table.TableName = "Department";
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = table.AddNewRow();
            row["Id"] = 1;
            row["Name"] = "HR";

            // Serialize normally, then rewrite JSON to put Rows before Columns.
            string json = JsonSerializer.Serialize(table);

            using JsonDocument doc = JsonDocument.Parse(json);
            JsonElement root = doc.RootElement;

            // Build JSON manually with order: TableName, Rows, Columns, PrimaryKeys (if present), then the rest ignored for this test.
            string reordered =
                "{"
                + "\"TableName\":" + JsonSerializer.Serialize(root.GetProperty("TableName").GetString())
                + ",\"Rows\":" + root.GetProperty("Rows").GetRawText()
                + ",\"Columns\":" + root.GetProperty("Columns").GetRawText()
                + (root.TryGetProperty("PrimaryKeys", out JsonElement pk) ? ",\"PrimaryKeys\":" + pk.GetRawText() : string.Empty)
                + "}";

            DataTable? roundTrip = JsonSerializer.Deserialize<DataTable>(reordered);

            Assert.NotNull(roundTrip);
            Assert.Equal("Department", roundTrip!.TableName);
            // AddNewRow() ensures __ClientKey exists, so the table has 3 columns after the row is created.
            Assert.Equal(3, roundTrip.Columns.Count);
            Assert.Equal(1, roundTrip.Rows.Count);

            // Client correlation key column must survive round-trip.
            Assert.True(roundTrip.ContainsColumn(SpecialColumnNames.CLIENT_KEY));

            object? id = null;
            object? name = null;

            Assert.True(roundTrip.Rows[0].TryGetValue("Id", out id));
            Assert.True(roundTrip.Rows[0].TryGetValue("Name", out name));

            Assert.Equal(1, id);
            Assert.Equal("HR", name);
        }

        [Fact]
        public void JsonDeserialize_WhenPrimaryKeysAppearBeforeColumns_DoesNotThrow_AndKeepsFlagsConsistent()
        {
            // This is a robustness test: PrimaryKeysJson setter may run before ColumnsJson setter.
            // We want a stable outcome with PK flags consistent after full deserialization.
            string json =
                "{"
                + "\"TableName\":\"Employee\","
                + "\"PrimaryKeys\":[\"Id\"],"
                + "\"Columns\":["
                    + "{\"ColumnName\":\"Id\",\"DataType\":\"Int32\",\"IsNullable\":false,\"IsPrimaryKey\":true,\"IsForeignKey\":false},"
                    + "{\"ColumnName\":\"Name\",\"DataType\":\"String\",\"IsNullable\":true,\"IsPrimaryKey\":false,\"IsForeignKey\":false}"
                + "],"
                + "\"Rows\":["
                    + "{\"DataRowState\":1,\"Values\":{\"Id\":1,\"Name\":\"Sara\"}}"
                + "]"
                + "}";

            DataTable? table = JsonSerializer.Deserialize<DataTable>(json);

            Assert.NotNull(table);
            Assert.Equal(2, table!.Columns.Count);
            Assert.Equal(1, table.PrimaryKeys.Count);
            Assert.Equal("Id", table.PrimaryKeys[0]);

            // Column flag must still be true
            bool idIsPk = false;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (table.Columns[i].ColumnName == "Id")
                {
                    idIsPk = table.Columns[i].IsPrimaryKey;
                    break;
                }
            }

            Assert.True(idIsPk);
        }
    }
}
