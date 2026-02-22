using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.DataTests.DataTableTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void AddColumnTest()
        {
            // Arrange
            // 1) Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2) Create an empty data table
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");

            // Act
            // 3) Add columns
            // - "Id" becomes a primary key, non-nullable
            IColumnMetadata idColumnMetadata = departmentDataTable.AddColumn("Id", DataTypeNames.INT32);

            // - "Name" is nullable by default (unless overridden)
            IColumnMetadata nameColumnMetadata = departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // - "DepartmentId" becomes a foreign key by convention
            IColumnMetadata departmentIdColumnMetadata = departmentDataTable.AddColumn("DepartmentId", DataTypeNames.INT32);

            // Assert
            Assert.Equal(3, departmentDataTable.Columns.Count);

            Assert.True(idColumnMetadata.IsPrimaryKey);
            Assert.False(idColumnMetadata.IsNullable);
            Assert.False(idColumnMetadata.IsForeignKey);

            Assert.True(nameColumnMetadata.IsNullable);

            Assert.True(departmentIdColumnMetadata.IsForeignKey);
            Assert.True(departmentIdColumnMetadata.IsNullable);
            Assert.False(departmentIdColumnMetadata.IsPrimaryKey);
        }
    }
}
