using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests
{
    public partial class DataTableExtensionsTests
    {
        [Fact]
        public void DoRefreshMergePreservingLocalChanges_DoesNotOverwriteModifiedRow()
        {
            // Arrange
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            // Current row (locally edited)
            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Engineering";
            currentRow.AcceptChanges();

            // Local change
            currentRow["Name"] = "Engineering (local edit)";

            // Refreshed data row (server)
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Engineering (server)";

            IMergeOptions options = new MergeOptions();

            // Act
            currentDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options);

            // Assert
            string name = currentDepartment.Rows[0].GetDataFieldValue<string>("Name");
            Assert.Equal("Engineering (local edit)", name);
            Assert.Equal(DataRowState.Modified, currentDepartment.Rows[0].DataRowState);
        }

        [Fact]
        public void DoRefreshMergeIfNoChangesExist_Throws_WhenTableIsDirty()
        {
            // Arrange
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Engineering";
            currentRow.AcceptChanges();

            // Make table dirty
            currentRow["Name"] = "Engineering (local edit)";

            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Engineering (server)";

            IMergeOptions options = new MergeOptions();

            // Act + Assert
            Assert.Throws<System.InvalidOperationException>(
                delegate
                {
                    currentDataSet.DoRefreshMergeIfNoChangesExist(refreshedDataSet, options);
                });
        }

        [Fact]
        public void DoReplaceMerge_ClearsAndReloadsRows()
        {
            // Arrange
            IDataSet currentDataSet = DataSetFactory.CreateDataSet();
            IDataTable currentDepartment = currentDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Old";
            currentRow.AcceptChanges();

            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 2;
            refreshedRow["Name"] = "New";
            refreshedRow.AcceptChanges();

            IMergeOptions options = new MergeOptions();

            // Act
            currentDataSet.DoReplaceMerge(refreshedDataSet, options);

            // Assert
            Assert.Single(currentDepartment.Rows);
            Assert.Equal(2, currentDepartment.Rows[0].GetDataFieldValue<int>("Id"));
            Assert.Equal("New", currentDepartment.Rows[0].GetDataFieldValue<string>("Name"));
            Assert.Equal(DataRowState.Unchanged, currentDepartment.Rows[0].DataRowState);
        }
    }
}
