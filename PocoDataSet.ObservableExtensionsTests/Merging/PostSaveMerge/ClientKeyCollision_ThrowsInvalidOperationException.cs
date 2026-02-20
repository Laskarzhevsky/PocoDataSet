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
        public void ClientKeyCollision_ThrowsInvalidOperationException()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);      // correlation-only
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid collisionKey = Guid.NewGuid();

            IObservableDataRow first = currentDepartment.AddNewRow();
            first[SpecialColumnNames.CLIENT_KEY] = collisionKey;
            first["Name"] = "First";

            IObservableDataRow second = currentDepartment.AddNewRow();
            second[SpecialColumnNames.CLIENT_KEY] = collisionKey; // collision
            second["Name"] = "Second";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved[SpecialColumnNames.CLIENT_KEY] = collisionKey;
            saved["Name"] = "Second";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options));
        }
    }
}
