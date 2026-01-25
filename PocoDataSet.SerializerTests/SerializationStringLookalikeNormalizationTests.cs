using System;
using System.Globalization;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public class SerializationStringLookalikeNormalizationTests
    {
        [Fact]
        public void JsonRoundTrip_StringColumn_GuidLikeString_RemainsString_AndIsSemanticallySameGuid()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Strings");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Text", DataTypeNames.STRING);

            Guid guid = Guid.NewGuid();

            // Uppercase "D" format to ensure we detect any normalization changes.
            string guidText = guid.ToString("D").ToUpperInvariant();

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Text"] = guidText;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            Assert.NotNull(restored);

            IDataRow restoredRow = restored!.Tables["Strings"].Rows[0];

            Assert.IsType<string>(restoredRow["Text"]!);

            string restoredText = (string)restoredRow["Text"]!;
            Assert.Equal(guidText, restoredText);
        }

        [Fact]
        public void JsonRoundTrip_StringColumn_DateTimeLikeString_RemainsString_AndParsesToSameInstant()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Strings");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Text", DataTypeNames.STRING);

            DateTime dt = new DateTime(2025, 12, 29, 13, 45, 59, DateTimeKind.Utc);

            // Use ISO-8601 Zulu form to encourage Json parsing in ObjectToPrimitivesConverter.
            string dateText = dt.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", CultureInfo.InvariantCulture);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Text"] = dateText;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            Assert.NotNull(restored);

            IDataRow restoredRow = restored!.Tables["Strings"].Rows[0];

            Assert.IsType<string>(restoredRow["Text"]!);

            string restoredText = (string)restoredRow["Text"]!;
            Assert.Equal(dateText, restoredText);
        }
    }
}
