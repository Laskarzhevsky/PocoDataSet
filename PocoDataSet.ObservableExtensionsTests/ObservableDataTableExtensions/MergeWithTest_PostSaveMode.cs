using System;

using PocoDataSet.Extensions;
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
        public void MergeWithTest_PostSaveMode()
        {
            // Arrange
            // 1. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet();

            // 2. Create Department observable table with row
            IObservableDataTable departmentObservableDataTable = observableDataSet.AddNewTable("Department");
            departmentObservableDataTable.AddColumn("Id", DataTypeNames.INT32);
            departmentObservableDataTable.AddColumn("Name", DataTypeNames.STRING);
            departmentObservableDataTable.AddColumn("RowVersion", DataTypeNames.BINARY);

            IObservableDataRow departmentObservableDataRow1 = departmentObservableDataTable.AddNewRow();
            departmentObservableDataRow1["Name"] = "Engineering";

            // Correlation key (client-side) is typically used to match added rows with server-returned rows
            Guid clientKey = departmentObservableDataRow1.GetDataFieldValue<Guid>(SpecialColumnNames.CLIENT_KEY);

            // 2. Create a refreshed table containing server-generated values
            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow savedRow = postSaveDepartment.AddNewRow();
            savedRow["Id"] = 10;
            savedRow["Name"] = "Engineering";
            savedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            savedRow[SpecialColumnNames.CLIENT_KEY] = clientKey; // correlate rows

            // Act
            // 3. Merge server-returned values into current table using PostSave mode
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.PostSave;
            departmentObservableDataTable.MergeWith(postSaveDepartment, observableMergeOptions);

            // Assert
            // 4. Current row now has server-generated values (identity/rowversion) and is typically accepted as Unchanged by the merge handlers
            int id = departmentObservableDataTable.Rows[0].GetDataFieldValue<int>("Id"); // 10
            Assert.Equal(10, id);

            byte[]? rowVersion = departmentObservableDataTable.Rows[0].GetDataFieldValue<byte[]>("RowVersion");

            Assert.NotNull(rowVersion);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rowVersion);
        }
    }
}
