using System;
using System.Collections.Generic;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class DataSetReloadFromTests
    {
        [Fact]
        public void ReloadFrom_ReplacesRows_AndAssignsClientKey_AndLeavesRowsUnchanged()
        {
            // Arrange
            // 1. Create current dataset with a table and a tracking column already present
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable departmentDataTable = currentDataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);
            departmentDataTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID, false, false, false);
            departmentDataTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow departmentDataRow = DataRowExtensions.CreateRowFromColumns(departmentDataTable.Columns);
            departmentDataRow["Id"] = 99;
            departmentDataRow["Name"] = "Reception";
            departmentDataRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
            departmentDataTable.AddLoadedRow(departmentDataRow);

            // 2. Create refreshed snapshot
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedDepartmentDataTable.PrimaryKeys = new List<string> { "Id" };

            IDataRow refreshedDepartmentDataRow1 = DataRowExtensions.CreateRowFromColumns(refreshedDepartmentDataTable.Columns);
            refreshedDepartmentDataRow1["Id"] = 1;
            refreshedDepartmentDataRow1["Name"] = "Finance";
            refreshedDepartmentDataTable.AddLoadedRow(refreshedDepartmentDataRow1);

            IDataRow refreshedDepartmentDataRow2 = DataRowExtensions.CreateRowFromColumns(refreshedDepartmentDataTable.Columns);
            refreshedDepartmentDataRow2["Id"] = 2;
            refreshedDepartmentDataRow2["Name"] = "Emergency";
            refreshedDepartmentDataTable.AddLoadedRow(refreshedDepartmentDataRow2);

            // Act
            currentDataSet.ReloadFrom(refreshedDataSet);

            // Assert
            Assert.Equal(2, departmentDataTable.Rows.Count);

            for (int i = 0; i < departmentDataTable.Rows.Count; i++)
            {
                IDataRow row = departmentDataTable.Rows[i];

                Assert.Equal(DataRowState.Unchanged, row.DataRowState);

                object? idObj;
                row.TryGetValue("Id", out idObj);
                Assert.NotNull(idObj);

                object? ckObj;
                row.TryGetValue(SpecialColumnNames.CLIENT_KEY, out ckObj);
                Assert.NotNull(ckObj);
            }
        }
    }
}
