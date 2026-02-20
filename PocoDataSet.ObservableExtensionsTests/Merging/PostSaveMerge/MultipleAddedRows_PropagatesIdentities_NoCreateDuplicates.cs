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
        /// Verifies MultipleAddedRows PropagatesIdentities AndDoesNotCreateDuplicates in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void MultipleAddedRows_PropagatesIdentities_NoCreateDuplicates()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid key1 = Guid.NewGuid();
            Guid key2 = Guid.NewGuid();

            IObservableDataRow row1 = currentDepartment.AddNewRow();
            row1[SpecialColumnNames.CLIENT_KEY] = key1;
            row1["Name"] = "Engineering";

            IObservableDataRow row2 = currentDepartment.AddNewRow();
            row2[SpecialColumnNames.CLIENT_KEY] = key2;
            row2["Name"] = "Sales";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved1 = postSaveDepartment.AddNewRow();
            saved1["Id"] = 10;
            saved1[SpecialColumnNames.CLIENT_KEY] = key1;
            saved1["Name"] = "Engineering";
            saved1["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IDataRow saved2 = postSaveDepartment.AddNewRow();
            saved2["Id"] = 11;
            saved2[SpecialColumnNames.CLIENT_KEY] = key2;
            saved2["Name"] = "Sales";
            saved2["RowVersion"] = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? merged1 = MergeTestingHelpers.FindRowByClientKey(currentObservableDataSet.Tables["Department"], key1);
            IObservableDataRow? merged2 = MergeTestingHelpers.FindRowByClientKey(currentObservableDataSet.Tables["Department"], key2);

            Assert.NotNull(merged1);
            Assert.NotNull(merged2);

            Assert.Equal(10, (int)merged1!["Id"]!);
            Assert.Equal(11, (int)merged2!["Id"]!);

            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged1["RowVersion"]!);
            Assert.Equal(new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged2["RowVersion"]!);
        }
    }
}