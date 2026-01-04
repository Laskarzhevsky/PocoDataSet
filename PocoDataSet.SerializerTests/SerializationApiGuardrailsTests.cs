using System;
using System.Text.Json;

using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationApiGuardrailsTests
    {
        [Fact]
        public void FromJsonString_ReturnsNull_WhenInputIsNull()
        {
            IDataSet? restored = DataSetSerializer.FromJsonString(null);
            Assert.Null(restored);
        }

        [Fact]
        public void FromJsonString_ReturnsNull_WhenInputIsEmpty()
        {
            IDataSet? restored = DataSetSerializer.FromJsonString(string.Empty);
            Assert.Null(restored);
        }

        [Fact]
        public void FromJsonString_ThrowsJsonException_WhenJsonIsInvalid()
        {
            Assert.Throws<JsonException>(() => DataSetSerializer.FromJsonString("{"));
        }
    }
}
