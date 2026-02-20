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
        public void MixedAddedAndModified_PropagatesBothCorrectly_AndDoesNotCrossPollinate()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Existing row (Modified scenario)
            Guid existingKey = Guid.NewGuid();
            IObservableDataRow existing = currentDepartment.AddNewRow();
            existing["Id"] = 5;
            existing[SpecialColumnNames.CLIENT_KEY] = existingKey;
            existing["Name"] = "HR";
            existing["RowVersion"] = new byte[] { 9, 0, 0, 0, 0, 0, 0, 0 };

            // Put it into Unchanged baseline, then modify
            existing.AcceptChanges();
            existing["Name"] = "HR Updated"; // now Modified

            // New row (Added scenario)
            Guid newKey = Guid.NewGuid();
            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = newKey;
            added["Name"] = "Engineering";

            // Post-save dataset
            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Returned for existing modified row (PK match)
            IDataRow savedExisting = postSaveDepartment.AddNewRow();
            savedExisting["Id"] = 5;
            savedExisting[SpecialColumnNames.CLIENT_KEY] = existingKey;
            savedExisting["Name"] = "HR Updated";
            savedExisting["RowVersion"] = new byte[] { 10, 0, 0, 0, 0, 0, 0, 0 };

            // Returned for added row (correlation by client key, PK differs because identity assigned)
            IDataRow savedAdded = postSaveDepartment.AddNewRow();
            savedAdded["Id"] = 10;
            savedAdded[SpecialColumnNames.CLIENT_KEY] = newKey;
            savedAdded["Name"] = "Engineering";
            savedAdded["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? mergedExisting = MergeTestingHelpers.FindRowByClientKey(currentObservableDataSet.Tables["Department"], existingKey);
            IObservableDataRow? mergedAdded = MergeTestingHelpers.FindRowByClientKey(currentObservableDataSet.Tables["Department"], newKey);

            Assert.NotNull(mergedExisting);
            Assert.NotNull(mergedAdded);

            // Existing row must remain Id=5 and get its new rowversion
            Assert.Equal(5, (int)mergedExisting!["Id"]!);
            Assert.Equal(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0 }, (byte[])mergedExisting["RowVersion"]!);

            // Added row must receive identity Id=10 and its rowversion
            Assert.Equal(10, (int)mergedAdded!["Id"]!);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])mergedAdded["RowVersion"]!);

            // No cross-pollination (sanity)
            Assert.NotEqual((int)mergedExisting["Id"]!, (int)mergedAdded["Id"]!);
            Assert.NotEqual(((byte[])mergedExisting["RowVersion"]!)[0], ((byte[])mergedAdded["RowVersion"]!)[0]);
        }
    }
}
