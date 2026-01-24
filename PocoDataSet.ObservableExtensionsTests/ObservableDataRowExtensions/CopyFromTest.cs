using System;
using System.Collections.Generic;
using System.Linq;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void CopyFromTest()
        {
            // Arrange
            // 1. Create a new observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create observable table and row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow departmentObservableDataRow = departmentObservableDataTable.AddNewRow();

            // 3. Create refreshed DataSet / DataTable / DataRow
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();

            IDataTable refreshedDepartmentDataTable = refreshedDataSet.AddNewTable("Department");
            refreshedDepartmentDataTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedDepartmentDataRow = refreshedDataSet.Tables["Department"].AddNewRow();
            refreshedDepartmentDataRow["Id"] = 1;
            refreshedDepartmentDataRow["Name"] = "Sales";
            refreshedDepartmentDataRow.AcceptChanges();

            // Act
            // 5. Copy values from the refreshedDepartmentDataRow row into observableDepartmentDataRow
            // (use departmentTable.Columns as the column metadata list)
            departmentObservableDataRow.CopyFrom(refreshedDepartmentDataRow, departmentObservableDataTable.Columns);

            // Assert
            Dictionary<string, object?> dictionary = departmentObservableDataRow.EnumerateValues().ToDictionary(p => p.Key, p => p.Value);
            foreach (KeyValuePair<string, object?> keyValuePair in dictionary)
            {
                object? refreshedDepartmentDataRowFieldValue;
                refreshedDepartmentDataRow.TryGetValue(keyValuePair.Key, out refreshedDepartmentDataRowFieldValue);

                Assert.Equal(keyValuePair.Value, refreshedDepartmentDataRowFieldValue);
            }
        }
    }
}
