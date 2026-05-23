using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void RemoveTable_RemovesTableFromTablesAndTablesJson()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            DataSet concreteDataSet = (DataSet)dataSet;

            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);

            // Act
            dataSet.RemoveTable("Department");

            // Assert
            Assert.False(dataSet.Tables.ContainsKey("Department"));
            Assert.False(concreteDataSet.TablesJson.ContainsKey("Department"));
        }

        [Fact]
        public void RemoveTable_RemovesRelationsWhereTableIsParent()
        {
            // Arrange
            IDataSet dataSet = CreateDataSetWithDepartmentEmployeeAndLocationTables();
            dataSet.AddRelation("DepartmentEmployees", "Department", "Id", "Employee", "DepartmentId");
            dataSet.AddRelation("LocationDepartments", "Location", "Id", "Department", "LocationId");

            // Act
            dataSet.RemoveTable("Department");

            // Assert
            Assert.Empty(dataSet.Relations);
        }

        [Fact]
        public void RemoveTable_RemovesRelationsWhereTableIsChild()
        {
            // Arrange
            IDataSet dataSet = CreateDataSetWithDepartmentEmployeeAndLocationTables();
            dataSet.AddRelation("DepartmentEmployees", "Department", "Id", "Employee", "DepartmentId");

            // Act
            dataSet.RemoveTable("Employee");

            // Assert
            Assert.Empty(dataSet.Relations);
        }

        [Fact]
        public void RemoveTable_DoesNotRemoveUnrelatedRelations()
        {
            // Arrange
            IDataSet dataSet = CreateDataSetWithDepartmentEmployeeAndLocationTables();
            dataSet.AddRelation("DepartmentEmployees", "Department", "Id", "Employee", "DepartmentId");
            dataSet.AddRelation("LocationEmployees", "Location", "Id", "Employee", "LocationId");
            dataSet.AddRelation("LocationDepartments", "Location", "Id", "Department", "LocationId");

            // Act
            dataSet.RemoveTable("Department");

            // Assert
            Assert.Single(dataSet.Relations);
            Assert.True(ContainsRelationByName(dataSet, "LocationEmployees"));
        }

        [Fact]
        public void RemoveTable_AllowsAddingTableWithSameNameAgainAfterRemoval()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable firstDepartmentDataTable = dataSet.AddNewTable("Department");
            firstDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);

            // Act
            dataSet.RemoveTable("Department");

            IDataTable secondDepartmentDataTable = dataSet.AddNewTable("Department");
            secondDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            secondDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // Assert
            Assert.Single(dataSet.Tables);
            Assert.True(dataSet.Tables.ContainsKey("Department"));
            Assert.Equal(2, secondDepartmentDataTable.Columns.Count);
        }

        private static IDataSet CreateDataSetWithDepartmentEmployeeAndLocationTables()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("LocationId", DataTypeNames.INT32);

            IDataTable employeeDataTable = dataSet.AddNewTable("Employee");
            employeeDataTable.AddColumn("Id", DataTypeNames.INT32);
            employeeDataTable.AddColumn("DepartmentId", DataTypeNames.INT32);
            employeeDataTable.AddColumn("LocationId", DataTypeNames.INT32);

            IDataTable locationDataTable = dataSet.AddNewTable("Location");
            locationDataTable.AddColumn("Id", DataTypeNames.INT32);

            return dataSet;
        }

        private static bool ContainsRelationByName(IDataSet dataSet, string relationName)
        {
            for (int i = 0; i < dataSet.Relations.Count; i++)
            {
                IDataRelation relation = dataSet.Relations[i];
                if (relation.RelationName == relationName)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
