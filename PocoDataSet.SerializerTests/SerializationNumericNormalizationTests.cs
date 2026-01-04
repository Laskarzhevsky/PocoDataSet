using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationNumericNormalizationTests
    {
        [Fact]
        public void JsonRoundTrip_Preserves_Int16_Int64_Byte_Single_And_Double()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Numbers");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Small", DataTypeNames.INT16);
            table.AddColumn("Big", DataTypeNames.INT64);
            table.AddColumn("Byte", DataTypeNames.BYTE);
            table.AddColumn("Float", DataTypeNames.SINGLE);
            table.AddColumn("Double", DataTypeNames.DOUBLE);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Small"] = (short)-12345;
            row["Big"] = 9223372036854770000L;
            row["Byte"] = (byte)250;
            row["Float"] = 123.5f;
            row["Double"] = 123.5d;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            Assert.NotNull(restored);

            IDataTable restoredTable = restored!.Tables["Numbers"];
            IDataRow restoredRow = restoredTable.Rows[0];

            Assert.IsType<short>(restoredRow["Small"]!);
            Assert.IsType<long>(restoredRow["Big"]!);
            Assert.IsType<byte>(restoredRow["Byte"]!);
            Assert.IsType<float>(restoredRow["Float"]!);
            Assert.IsType<double>(restoredRow["Double"]!);

            Assert.Equal((short)-12345, (short)restoredRow["Small"]!);
            Assert.Equal(9223372036854770000L, (long)restoredRow["Big"]!);
            Assert.Equal((byte)250, (byte)restoredRow["Byte"]!);
            Assert.Equal(123.5f, (float)restoredRow["Float"]!);
            Assert.Equal(123.5d, (double)restoredRow["Double"]!);
        }

        [Fact]
        public void FromJsonString_ThrowsOverflowException_WhenNumberExceeds_Int16_Range()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Numbers");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Small", DataTypeNames.INT16);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Small"] = (short)1;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            // Replace only the "Small" value in JSON with an out-of-range number.
            // This triggers ValueNormalization.ConvertValueToColumnType -> checked((short)l).
            string mutatedJson = ReplacePropertyNumericValue(json!, "Small", "70000");

            Assert.Throws<OverflowException>(() => DataSetSerializer.FromJsonString(mutatedJson));
        }

        private static string ReplacePropertyNumericValue(string json, string propertyName, string newNumericLiteral)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (newNumericLiteral == null)
            {
                throw new ArgumentNullException(nameof(newNumericLiteral));
            }

            string pattern = "\"" + RegexEscape(propertyName) + "\"\\s*:\\s*-?\\d+";

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(pattern);

            if (!regex.IsMatch(json))
            {
                throw new InvalidOperationException(
                    "Unable to locate numeric property '" + propertyName + "' in JSON.");
            }

            string replacement = "\"" + propertyName + "\":" + newNumericLiteral;

            // Replace only the first occurrence
            string mutated = regex.Replace(json, replacement, 1);
            return mutated;
        }

        private static string RegexEscape(string value)
        {
            return System.Text.RegularExpressions.Regex.Escape(value);
        }
    }
}
