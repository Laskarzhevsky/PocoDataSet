using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataTableExtensions
{
    public partial class ObservableDataTableExtensionsTests
    {
        [Fact]
        public void AddColumnTest()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Employee observable table
            IObservableDataTable employeeObservableDataTable = observableDataSet.AddNewTable("Employee");

            // Act
            // 3) Add columns
            // - "Id" becomes a primary key, non-nullable
            IColumnMetadata idColumnMetadata = employeeObservableDataTable.AddColumn("Id", DataTypeNames.INT32);

            // - "FirstName" is nullable by default (unless overridden)
            IColumnMetadata FirstNameColumnMetadata = employeeObservableDataTable.AddColumn("FirstName", DataTypeNames.STRING);

            // - "EmployeeTypeId" becomes a foreign key by convention
            IColumnMetadata employeeTypeIdColumnMetadata = employeeObservableDataTable.AddColumn("EmployeeTypeId", DataTypeNames.INT32);

            // Adds a non-nullable foreign key column, "SecurityLevelId"
            IColumnMetadata securityLevelIdColumnMetadata = employeeObservableDataTable.AddColumn("SecurityLevelId", DataTypeNames.INT32, false, false, true);

            // Assert
            // - Employee table contins:
            // Non-nullable primary key "Id" column
            // Nullable "FirstName" column
            // Nullable foreign key "EmployeeTypeId" column
            // Non-nullbale foreign key "SecurityLevelId" column
            Assert.Equal(4, employeeObservableDataTable.Columns.Count);

            Assert.Equal("Id", idColumnMetadata.ColumnName);
            Assert.Equal(DataTypeNames.INT32, idColumnMetadata.DataType);
            Assert.False(idColumnMetadata.IsForeignKey);
            Assert.False(idColumnMetadata.IsNullable);
            Assert.True(idColumnMetadata.IsPrimaryKey);

            Assert.Equal("FirstName", FirstNameColumnMetadata.ColumnName);
            Assert.Equal(DataTypeNames.STRING, FirstNameColumnMetadata.DataType);
            Assert.False(FirstNameColumnMetadata.IsForeignKey);
            Assert.True(FirstNameColumnMetadata.IsNullable);
            Assert.False(FirstNameColumnMetadata.IsPrimaryKey);

            Assert.Equal("EmployeeTypeId", employeeTypeIdColumnMetadata.ColumnName);
            Assert.Equal(DataTypeNames.INT32, employeeTypeIdColumnMetadata.DataType);
            Assert.True(employeeTypeIdColumnMetadata.IsForeignKey);
            Assert.True(employeeTypeIdColumnMetadata.IsNullable);
            Assert.False(employeeTypeIdColumnMetadata.IsPrimaryKey);

            Assert.Equal("SecurityLevelId", securityLevelIdColumnMetadata.ColumnName);
            Assert.Equal(DataTypeNames.INT32, securityLevelIdColumnMetadata.DataType);
            Assert.True(securityLevelIdColumnMetadata.IsForeignKey);
            Assert.False(securityLevelIdColumnMetadata.IsNullable);
            Assert.False(securityLevelIdColumnMetadata.IsPrimaryKey);
        }
    }
}
