using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void PocoListToDataTableTest()
        {
            // Arrange
            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2. Prepare column metadata (data table schema)
            List<IColumnMetadata> listOfColumnMetadata = new List<IColumnMetadata>();

            ColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "Id";
            columnMetadata.DataType = DataTypeNames.INT32;
            columnMetadata.IsNullable = false;
            columnMetadata.IsPrimaryKey = true;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "Code";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "Description";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            // 3. Prepare POCO list
            IList<IEmploymentType> employmentTypes = new List<IEmploymentType>();
            IEmploymentType employmentType = new EmploymentType();
            employmentType.Id = 1;
            employmentType.Code = "ET01";
            employmentType.Description = "Full Time";
            employmentTypes.Add(employmentType);

            employmentType = new EmploymentType();
            employmentType.Id = 1;
            employmentType.Code = "ET02";
            employmentType.Description = "Part Time";
            employmentTypes.Add(employmentType);

            // Act
            // 4. Create table and populate it from the POCO list
            IDataTable employmentTypeDataTable = dataSet.PocoListToDataTable("EmploymentType", employmentTypes, listOfColumnMetadata);

            // Assert
            Assert.Equal(2, employmentTypeDataTable.Rows.Count);
        }
    }
}
