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
        /// <summary>
        /// Verifies MissingClientKeyInPostSaveRow AddsNewRow AndDoesNotUpdateExistingAddedRow in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void MissingClientKey_AddsRow_NoUpdateAddedRow()
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
            // added["Id"] is 0

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Missing client key: correlation is impossible.
            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved["Name"] = "Engineering";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            // Documented defensive behavior (given current implementation): row is added as a new row.
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? stillOriginal = MergeTestingHelpers.FindRowByClientKey(currentObservableDataSet.Tables["Department"], clientKey);
            Assert.NotNull(stillOriginal);

            // Original added row is not updated (no correlation key)
            Assert.Equal(0, (int)stillOriginal!["Id"]!);

            // And a new row exists with Id=10
            Assert.True(MergeTestingHelpers.ContainsRowWithId(currentObservableDataSet.Tables["Department"], 10));
        }
    }
}