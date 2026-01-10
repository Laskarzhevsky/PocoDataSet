using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.Tests
{
    public class SchemaEvolutionTests
    {
        [Fact]
        public void Deserialize_OldJsonMissingNewColumn_AddsMissingColumnAsNull()
        {
            // Old JSON: Department table has Id + Name only (no Description)
            string oldJson =
@"{
  ""Name"": null,
  ""Relations"": [],
  ""Tables"": {
    ""Department"": {
      ""Columns"": [
        { ""ColumnName"": ""Id"", ""DataType"": ""Int32"", ""IsNullable"": false, ""IsPrimaryKey"": true },
        { ""ColumnName"": ""Name"", ""DataType"": ""String"", ""IsNullable"": true, ""IsPrimaryKey"": false }
      ],
      ""Rows"": [
        { ""DataRowState"": 0, ""HasOriginalValues"": false, ""OriginalValues"": {}, ""Selected"": false,
          ""Values"": { ""Id"": 1, ""Name"": ""Customer Service"" }
        }
      ],
      ""TableName"": ""Department"",
      ""PrimaryKeys"": []
    }
  }
}";

            // Act: deserialize old JSON
            IDataSet? dataSet = DataSetSerializer.FromJsonString(oldJson);

            Assert.NotNull(dataSet);
            Assert.True(dataSet!.Tables.ContainsKey("Department"));

            IDataTable table = dataSet.Tables["Department"];
            Assert.Single(table.Rows);

            // New code expects schema evolution support:
            // We'll "evolve" the schema by adding Description AFTER deserialization.
            table.AddColumn("Description", DataTypeNames.STRING);

            // Ensure invariant: row has the new column key with null
            IDataRow row = table.Rows[0];
            Assert.True(row.ContainsKey("Description"));
            Assert.Null(row["Description"]);
        }

        [Fact]
        public void Deserialize_JsonHasExtraColumn_NotInSchema_DoesNotBreakAccess()
        {
            // JSON includes Ghost column, schema contains only Id + Name.
            string jsonWithExtra =
@"{
  ""Name"": null,
  ""Relations"": [],
  ""Tables"": {
    ""Department"": {
      ""Columns"": [
        { ""ColumnName"": ""Id"", ""DataType"": ""Int32"", ""IsNullable"": false, ""IsPrimaryKey"": true },
        { ""ColumnName"": ""Name"", ""DataType"": ""String"", ""IsNullable"": true, ""IsPrimaryKey"": false }
      ],
      ""Rows"": [
        { ""DataRowState"": 0, ""HasOriginalValues"": false, ""OriginalValues"": {}, ""Selected"": false,
          ""Values"": { ""Id"": 1, ""Name"": ""Customer Service"", ""Ghost"": ""X"" }
        }
      ],
      ""TableName"": ""Department"",
      ""PrimaryKeys"": []
    }
  }
}";

            IDataSet? dataSet = DataSetSerializer.FromJsonString(jsonWithExtra);

            Assert.NotNull(dataSet);
            IDataTable table = dataSet!.Tables["Department"];
            IDataRow row = table.Rows[0];

            // Core columns must be accessible
            Assert.Equal(1, row["Id"]);
            Assert.Equal("Customer Service", row["Name"]);

            // Extra field behavior: you choose semantics.
            // This asserts it is preserved (common for forward compatibility).
            Assert.True(row.ContainsKey("Ghost"));
            Assert.Equal("X", row["Ghost"]);
        }
    }
}
