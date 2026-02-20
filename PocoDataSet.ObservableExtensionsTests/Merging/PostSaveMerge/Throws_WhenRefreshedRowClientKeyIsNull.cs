using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class PostSaveMerge
    {
        [Fact]
        public void Throws_WhenRefreshedRowClientKeyIsNull()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = clientKey;
            added["Name"] = "Engineering";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow refreshedRow = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumnsWithDefaultValues(postSaveDepartment.Columns);
            refreshedRow["Id"] = 10;

            // Intentionally set to null (if your IDataRow supports null values)
            refreshedRow[SpecialColumnNames.CLIENT_KEY] = null;

            refreshedRow["Name"] = "Engineering";
            refreshedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            refreshedRow.SetDataRowState(DataRowState.Unchanged);
            postSaveDepartment.AddRow(refreshedRow);

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options));
        }
    }
}
