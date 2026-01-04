using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationBinaryBase64NormalizationTests
    {
        [Fact]
        public void FromJsonString_Converts_Base64String_ToByteArray_ForBinaryColumn()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Bin");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("RowVersion", DataTypeNames.BINARY);

            byte[] original = new byte[] { 10, 20, 30, 40 };

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["RowVersion"] = original;

            table.AddLoadedRow(row);

            string json = DataSetSerializer.ToJsonString(dataSet)!;

            // Replace binary value with explicit base64 string
            string base64 = Convert.ToBase64String(original);
            string mutated = ReplacePropertyWithQuotedValue(json, "RowVersion", base64);

            IDataSet? restored = DataSetSerializer.FromJsonString(mutated);
            Assert.NotNull(restored);

            byte[] restoredBytes = (byte[])restored!.Tables["Bin"].Rows[0]["RowVersion"]!;
            Assert.Equal(original, restoredBytes);
        }

        private static string ReplacePropertyWithQuotedValue(
            string json,
            string propertyName,
            string value)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            string pattern = "\"" + RegexEscape(propertyName) + "\"\\s*:\\s*\"[^\"]*\"";

            System.Text.RegularExpressions.Regex regex =
                new System.Text.RegularExpressions.Regex(pattern);

            if (!regex.IsMatch(json))
            {
                throw new InvalidOperationException(
                    "Unable to locate property '" + propertyName + "' in JSON.");
            }

            string replacement = "\"" + propertyName + "\":\"" + value + "\"";

            // Replace only the first occurrence
            return regex.Replace(json, replacement, 1);
        }

        private static string RegexEscape(string value)
        {
            return System.Text.RegularExpressions.Regex.Escape(value);
        }
    }
}
