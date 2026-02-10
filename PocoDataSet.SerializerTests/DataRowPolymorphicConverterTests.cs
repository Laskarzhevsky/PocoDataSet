using System;
using System.Text.Json;
using Xunit;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.Serializer.Tests
{
    public class DataRowPolymorphicConverterTests
    {
        private static JsonSerializerOptions CreateOptionsForRows()
        {
            JsonSerializerOptions options = new JsonSerializerOptions();
            options.PropertyNameCaseInsensitive = true;

            // Keep parity with DataSetSerializer: prevents JsonElement in Dictionary<string, object?>
            options.Converters.Add(new ObjectToPrimitivesConverter());

            // The converter under test
            options.Converters.Add(new DataRowPolymorphicConverter());

            return options;
        }

        [Fact]
        public void SerializeDeserialize_FloatingDataRow_RoundTripsAsFloating()
        {
            // Arrange
            IDataRow row = new FloatingDataRow();
            row["Id"] = 7;
            row["FirstName"] = "Sara";

            JsonSerializerOptions options = CreateOptionsForRows();

            // Act
            string json = JsonSerializer.Serialize<IDataRow>(row, options);
            IDataRow? restored = JsonSerializer.Deserialize<IDataRow>(json, options);

            // Assert
            Assert.NotNull(restored);
            Assert.IsType<FloatingDataRow>(restored);

            object? id;
            bool hasId = restored!.TryGetValue("Id", out id);
            Assert.True(hasId);
            Assert.Equal(7, Convert.ToInt32(id));

            object? firstName;
            bool hasFirstName = restored.TryGetValue("FirstName", out firstName);
            Assert.True(hasFirstName);
            Assert.Equal("Sara", firstName as string);

            // Also ensure discriminator is emitted
            Assert.Contains("\"$rowKind\"", json);
            Assert.Contains("\"floating\"", json);
        }

        [Fact]
        public void Deserialize_OldJsonWithoutRowKind_DefaultsToDataRow()
        {
            // Arrange
            // Simulates pre-discriminator JSON. Note: "Values" is the JSON bridge property.
            string oldJson = "{\"DataRowState\":0,\"Values\":{\"Id\":1}}";
            JsonSerializerOptions options = CreateOptionsForRows();

            // Act
            IDataRow? restored = JsonSerializer.Deserialize<IDataRow>(oldJson, options);

            // Assert
            Assert.NotNull(restored);
            Assert.IsType<DataRow>(restored);

            object? id;
            bool hasId = restored!.TryGetValue("Id", out id);
            Assert.True(hasId);
            Assert.Equal(1, Convert.ToInt32(id));
        }

        [Fact]
        public void SerializeDeserialize_RegularDataRow_RoundTripsAsDataRow()
        {
            // Arrange
            IDataRow row = new DataRow();
            row["Id"] = 2;
            row["LastName"] = "Connor";

            JsonSerializerOptions options = CreateOptionsForRows();

            // Act
            string json = JsonSerializer.Serialize<IDataRow>(row, options);
            IDataRow? restored = JsonSerializer.Deserialize<IDataRow>(json, options);

            // Assert
            Assert.NotNull(restored);
            Assert.IsType<DataRow>(restored);

            object? id;
            bool hasId = restored!.TryGetValue("Id", out id);
            Assert.True(hasId);
            Assert.Equal(2, Convert.ToInt32(id));

            object? lastName;
            bool hasLastName = restored.TryGetValue("LastName", out lastName);
            Assert.True(hasLastName);
            Assert.Equal("Connor", lastName as string);

            Assert.Contains("\"$rowKind\"", json);
            Assert.Contains("\"regular\"", json);
        }
    }
}
