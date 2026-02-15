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
        public void MergeWithTest_PostSaveMode()
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
            IMergeOptions options = new MergeOptions();
            options.MergeMode = MergeMode.PostSave;

            // ✅ Explicit entry point
            currentDepartment.DoPostSaveMerge(postSaveDepartment, options);

            // Assert
            int id = currentDepartment.Rows[0].GetDataFieldValue<int>("Id"); // 10
            Assert.Equal(10, id);

            byte[]? rowVersion = currentDepartment.Rows[0].GetDataFieldValue<byte[]>("RowVersion");
            Assert.NotNull(rowVersion);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rowVersion);
        }
    }
}
