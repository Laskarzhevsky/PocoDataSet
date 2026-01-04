using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationBinaryRoundTripTests
    {
        [Fact]
        public void JsonRoundTrip_Preserves_BinaryByteArray_Content()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("Bin");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("RowVersion", DataTypeNames.BINARY);

            byte[] original = new byte[] { 1, 2, 3, 4, 250, 251 };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["RowVersion"] = original;
            t.AddLoadedRow(row);

            // Act
            string? json = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? roundTrip = DataSetSerializer.FromJsonString(json);

            // Assert
            Assert.NotNull(roundTrip);
            Assert.True(roundTrip!.Tables.ContainsKey("Bin"));

            IDataTable rt = roundTrip.Tables["Bin"];
            Assert.Single(rt.Rows);

            IDataRow r = rt.Rows[0];

            object? value;
            r.TryGetValue("RowVersion", out value);

            Assert.NotNull(value);

            Assert.IsType<byte[]>(value);

            byte[] restored = (byte[])value!;

            Assert.Equal(original.Length, restored.Length);

            for (int i = 0; i < original.Length; i++)
            {
                Assert.Equal(original[i], restored[i]);
            }
        }
    }
}
