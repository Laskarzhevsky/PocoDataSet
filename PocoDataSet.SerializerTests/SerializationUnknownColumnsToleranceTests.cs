using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationUnknownColumnsToleranceTests
    {
        [Fact]
        public void FromJsonString_IgnoresUnknownRowFields_WhenPresentInJson()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = table.AddNewRow();
            row["Id"] = 1;
            row["Name"] = "Sales";
            row.AcceptChanges();

            string json = DataSetSerializer.ToJsonString(dataSet);

            // Mutate JSON: inject an unknown column into the first row's value object.
            string mutatedJson = InjectUnknownFieldIntoFirstRow(json, "SomeNewField", "X");

            // Act
            IDataSet? restored = DataSetSerializer.FromJsonString(mutatedJson);

            // Assert
            Assert.NotNull(restored);

            IDataTable restoredTable = restored!.Tables["Department"];
            Assert.Single(restoredTable.Rows);

            IDataRow restoredRow = restoredTable.Rows[0];

            Assert.Equal(1, restoredRow["Id"]);
            Assert.Equal("Sales", restoredRow["Name"]);

            Assert.True(restoredRow.TryGetValue("SomeNewField", out object? v));
            Assert.Equal("X", v);
        }

        private static string InjectUnknownFieldIntoFirstRow(string json, string fieldName, string fieldValue)
        {
            JsonNode? root = JsonNode.Parse(json);
            if (root == null)
            {
                throw new InvalidOperationException("Unable to parse JSON.");
            }

            JsonObject? rowValuesObject = FindFirstRowValuesObject(root);
            if (rowValuesObject == null)
            {
                throw new InvalidOperationException("Unable to locate a row values object in JSON.");
            }

            // Only inject if it doesn't already exist
            if (!rowValuesObject.ContainsKey(fieldName))
            {
                rowValuesObject[fieldName] = fieldValue;
            }

            return root.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
        }

        /// <summary>
        /// Attempts to find the JsonObject that holds the per-row field dictionary.
        /// This method is intentionally schema-agnostic and searches the JSON tree
        /// for the first object that appears to contain actual column values.
        /// </summary>
        private static JsonObject? FindFirstRowValuesObject(JsonNode root)
        {
            Stack<JsonNode> stack = new Stack<JsonNode>();
            stack.Push(root);

            while (stack.Count > 0)
            {
                JsonNode current = stack.Pop();

                JsonObject? currentObject = current as JsonObject;
                if (currentObject != null)
                {
                    // Heuristic 1: typical POCO DataRow JSON ends up containing a dictionary of column names.
                    // We look for the first object that contains both Id and Name.
                    if (currentObject.ContainsKey("Id") && currentObject.ContainsKey("Name"))
                    {
                        return currentObject;
                    }

                    foreach (KeyValuePair<string, JsonNode?> kvp in currentObject)
                    {
                        if (kvp.Value != null)
                        {
                            stack.Push(kvp.Value);
                        }
                    }
                }

                JsonArray? currentArray = current as JsonArray;
                if (currentArray != null)
                {
                    foreach (JsonNode? item in currentArray)
                    {
                        if (item != null)
                        {
                            stack.Push(item);
                        }
                    }
                }
            }

            return null;
        }
    }
}
