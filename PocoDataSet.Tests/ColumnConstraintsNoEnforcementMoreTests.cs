using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class ColumnConstraintsNoEnforcementMoreTests
    {
        [Fact]
        public void TypeMetadata_DoesNotAutoConvertStringToInt()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("T");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Age", DataTypeNames.INT32);

            IDataRow row = table.AddNewRow();
            row["Id"] = 1;

            // If you later add type enforcement, this would start throwing.
            row["Age"] = "123";

            Assert.IsType<string>(row["Age"]);
            Assert.Equal("123", row["Age"]);
        }
    }
}
