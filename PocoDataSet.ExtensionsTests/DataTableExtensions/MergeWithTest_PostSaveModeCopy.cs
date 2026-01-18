using System;
using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void MergeWithTest_PostSaveModeCopy()
        {
            // Arrange
            // 1. Create a current table with a newly added row (client-side)
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow newRow = currentDepartment.AddNewRow();
            newRow["Name"] = "Engineering";

            // Correlation key (client-side) is typically used to match added rows with server-returned rows
            Guid clientKey = newRow.GetDataFieldValue<Guid>(SpecialColumnNames.CLIENT_KEY);

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
            IMergeOptions options = new MergeOptions();
            options.MergeMode = MergeMode.PostSave;
            currentDepartment.MergeWith(postSaveDepartment, options);

            // Assert
            // 4. Current row now has server-generated values (identity/rowversion) and is typically accepted as Unchanged by the merge handlers
            int id = currentDepartment.Rows[0].GetDataFieldValue<int>("Id"); // 10
            Assert.Equal(10, id);

            byte[]? rowVersion = currentDepartment.Rows[0].GetDataFieldValue<byte[]>("RowVersion");

            Assert.NotNull(rowVersion);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rowVersion);
        }
    }
}
