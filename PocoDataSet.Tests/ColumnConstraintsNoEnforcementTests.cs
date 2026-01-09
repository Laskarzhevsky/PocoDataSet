using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class ColumnConstraintsNoEnforcementTests
    {
        [Fact]
        public void IsNullableFalse_DoesNotRejectNullAssignment()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING, isNullable: false);

            IDataRow row = table.AddNewRow();
            row["Id"] = 1;

            // Act (no throw expected)
            row["Name"] = null;

            // Assert
            Assert.Null(row["Name"]);
        }

        [Fact]
        public void MaxLength_DoesNotTruncateOrThrow()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Code", DataTypeNames.STRING);

            // Set MaxLength metadata (no enforcement currently)
            IColumnMetadata? codeColumn = null;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                IColumnMetadata column = table.Columns[i];
                if (column.ColumnName == "Code")
                {
                    codeColumn = column;
                    break;
                }
            }

            Assert.NotNull(codeColumn);
            codeColumn!.MaxLength = 3;
            IDataRow row = table.AddNewRow();
            row["Id"] = 1;

            // Act
            row["Code"] = "ABCDEFG";

            // Assert
            Assert.Equal("ABCDEFG", row["Code"]);
        }
    }
}
