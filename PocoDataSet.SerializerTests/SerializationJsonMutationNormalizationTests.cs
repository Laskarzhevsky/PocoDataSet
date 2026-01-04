using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationJsonMutationNormalizationTests
    {
        [Fact]
        public void FromJsonString_Converts_NumberStrings_ToNumericTypes_BasedOnSchema()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = dataSet.AddNewTable("Mut");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Amount", DataTypeNames.DECIMAL);
            table.AddColumn("Rate", DataTypeNames.DOUBLE);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
            row["Id"] = 1;
            row["Amount"] = 12.50m;
            row["Rate"] = 0.125d;

            table.AddLoadedRow(row);

            string? json = DataSetSerializer.ToJsonString(dataSet);
            Assert.False(string.IsNullOrEmpty(json));

            // Mutate JSON so numeric values become quoted strings.
            string mutated = json!;
            mutated = ReplacePropertyNumericValueWithQuotedString(mutated, "Id", "1");
            mutated = ReplacePropertyNumericValueWithQuotedString(mutated, "Amount", "12.50");
            mutated = ReplacePropertyNumericValueWithQuotedString(mutated, "Rate", "0.125");

            // Act
            IDataSet? restored = DataSetSerializer.FromJsonString(mutated);

            // Assert
            Assert.NotNull(restored);

            IDataRow restoredRow = restored!.Tables["Mut"].Rows[0];

            Assert.IsType<int>(restoredRow["Id"]!);
            Assert.IsType<decimal>(restoredRow["Amount"]!);
            Assert.IsType<double>(restoredRow["Rate"]!);

            Assert.Equal(1, (int)restoredRow["Id"]!);
            Assert.Equal(12.50m, (decimal)restoredRow["Amount"]!);
            Assert.Equal(0.125d, (double)restoredRow["Rate"]!);
        }

        private static string ReplacePropertyNumericValueWithQuotedString(string json, string propertyName, string numericText)
        {
            if (json == null)
            {
                throw new ArgumentNullException(nameof(json));
            }

            if (propertyName == null)
            {
                throw new ArgumentNullException(nameof(propertyName));
            }

            if (numericText == null)
            {
                throw new ArgumentNullException(nameof(numericText));
            }

            // Matches: "Prop": 123   or  "Prop": 12.34   (with optional whitespace, optional negative sign)
            string pattern = "\"" + RegexEscape(propertyName) + "\"\\s*:\\s*-?\\d+(?:\\.\\d+)?";

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(pattern);

            if (!regex.IsMatch(json))
            {
                throw new InvalidOperationException("Unable to locate numeric property '" + propertyName + "' in JSON.");
            }

            string replacement = "\"" + propertyName + "\":\"" + numericText + "\"";

            // Replace only the first occurrence
            return regex.Replace(json, replacement, 1);
        }

        private static string RegexEscape(string value)
        {
            return System.Text.RegularExpressions.Regex.Escape(value);
        }
    }
}
