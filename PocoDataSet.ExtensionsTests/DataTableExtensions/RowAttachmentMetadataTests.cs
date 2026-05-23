using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void RemoveRow_ClearsTableOwnedRowMetadata()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow row = employeeTable.AddNewRow();
            row["EmployeeId"] = 1;
            row["FirstName"] = "John";

            DataRow concreteRow = (DataRow)row;
            Assert.Single(concreteRow.PrimaryKeyColumns);

            employeeTable.RemoveRow(row);

            Assert.Empty(employeeTable.Rows);
            Assert.Equal(DataRowState.Detached, row.DataRowState);
            Assert.Empty(concreteRow.PrimaryKeyColumns);
        }

        [Fact]
        public void RemoveRowAt_ClearsTableOwnedRowMetadata()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow row = employeeTable.AddNewRow();
            row["EmployeeId"] = 1;
            row["FirstName"] = "John";

            DataRow concreteRow = (DataRow)row;
            Assert.Single(concreteRow.PrimaryKeyColumns);

            employeeTable.RemoveRowAt(0);

            Assert.Empty(employeeTable.Rows);
            Assert.Equal(DataRowState.Detached, row.DataRowState);
            Assert.Empty(concreteRow.PrimaryKeyColumns);
        }

        [Fact]
        public void RemoveAllRows_ClearsTableOwnedRowMetadataForEveryRow()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow firstRow = employeeTable.AddNewRow();
            firstRow["EmployeeId"] = 1;
            firstRow["FirstName"] = "John";

            IDataRow secondRow = employeeTable.AddNewRow();
            secondRow["EmployeeId"] = 2;
            secondRow["FirstName"] = "Sara";

            DataRow firstConcreteRow = (DataRow)firstRow;
            DataRow secondConcreteRow = (DataRow)secondRow;

            Assert.Single(firstConcreteRow.PrimaryKeyColumns);
            Assert.Single(secondConcreteRow.PrimaryKeyColumns);

            employeeTable.RemoveAllRows();

            Assert.Empty(employeeTable.Rows);
            Assert.Equal(DataRowState.Detached, firstRow.DataRowState);
            Assert.Equal(DataRowState.Detached, secondRow.DataRowState);
            Assert.Empty(firstConcreteRow.PrimaryKeyColumns);
            Assert.Empty(secondConcreteRow.PrimaryKeyColumns);
        }

        [Fact]
        public void DeleteRow_ForAddedRow_ClearsTableOwnedRowMetadata()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow row = employeeTable.AddNewRow();
            row["EmployeeId"] = 1;
            row["FirstName"] = "John";

            DataRow concreteRow = (DataRow)row;
            Assert.Single(concreteRow.PrimaryKeyColumns);

            employeeTable.DeleteRow(row);

            Assert.Empty(employeeTable.Rows);
            Assert.Equal(DataRowState.Detached, row.DataRowState);
            Assert.Empty(concreteRow.PrimaryKeyColumns);
        }

        [Fact]
        public void AcceptChanges_ForDeletedRow_ClearsTableOwnedRowMetadata()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow row = employeeTable.AddNewRow();
            row["EmployeeId"] = 1;
            row["FirstName"] = "John";
            row.AcceptChanges();
            row.Delete();

            DataRow concreteRow = (DataRow)row;
            Assert.Single(concreteRow.PrimaryKeyColumns);

            employeeTable.AcceptChanges();

            Assert.Empty(employeeTable.Rows);
            Assert.Equal(DataRowState.Detached, row.DataRowState);
            Assert.Empty(concreteRow.PrimaryKeyColumns);
        }

        [Fact]
        public void RemovedRow_CanBeAddedToAnotherTableWithoutKeepingOldPrimaryKeys()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable employeeTable = dataSet.AddNewTable("Employee");
            employeeTable.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING);

            IDataRow row = employeeTable.AddNewRow();
            row["EmployeeId"] = 1;
            row["FirstName"] = "John";

            employeeTable.RemoveRow(row);

            IDataTable departmentTable = dataSet.AddNewTable("Department");
            departmentTable.AddColumn("DepartmentId", DataTypeNames.INT32, false, true);
            departmentTable.AddColumn("Name", DataTypeNames.STRING);

            departmentTable.AddRow(row);

            DataRow concreteRow = (DataRow)row;

            Assert.Single(departmentTable.Rows);
            Assert.Equal(DataRowState.Added, row.DataRowState);
            Assert.Single(concreteRow.PrimaryKeyColumns);
            Assert.Equal("DepartmentId", concreteRow.PrimaryKeyColumns[0]);
        }
    }
}
