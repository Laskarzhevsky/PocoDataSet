using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataSetExtensionsTests
    {
        [Fact]
        public void DoPostSaveMerge_MergesIdentityAndRowVersion_IntoCurrentDataSet()
        {
            // Arrange
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();

            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow newRow = currentDepartment.AddNewRow();
            newRow["Name"] = "Engineering";

            Guid clientKey = newRow.GetDataFieldValue<Guid>(SpecialColumnNames.CLIENT_KEY);

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow savedRow = postSaveDepartment.AddNewRow();
            savedRow["Id"] = 10;
            savedRow["Name"] = "Engineering";
            savedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            savedRow[SpecialColumnNames.CLIENT_KEY] = clientKey;

            IMergeOptions options = new MergeOptions();

            // Act
            currentDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            int id = currentDepartment.Rows[0].GetDataFieldValue<int>("Id");
            Assert.Equal(10, id);

            byte[]? rowVersion = currentDepartment.Rows[0].GetDataFieldValue<byte[]>("RowVersion");
            Assert.NotNull(rowVersion);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rowVersion);
        }
    }
}
