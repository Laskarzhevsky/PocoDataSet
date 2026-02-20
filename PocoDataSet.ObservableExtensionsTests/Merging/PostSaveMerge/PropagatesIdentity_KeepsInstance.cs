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
        /// Verifies PropagatesIdentity AndPreservesObservableRowInstance in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void PropagatesIdentity_KeepsInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservable = new ObservableDataSet(currentInner);

            IObservableDataTable department = currentObservable.AddNewTable("Department");
            department.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            department.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            department.AddColumn("Name", DataTypeNames.STRING);
            department.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow currentAdded = department.AddNewRow();
            currentAdded[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentAdded["Name"] = "Engineering";

            // Build post-save snapshot: server assigns identity + rowversion.
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSave.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved[SpecialColumnNames.CLIENT_KEY] = clientKey;
            saved["Name"] = "Engineering";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            currentObservable.DoPostSaveMerge(postSave, options);

            // Assert
            IObservableDataRow? merged = MergeTestingHelpers.FindRowByClientKey(department, clientKey);
            Assert.NotNull(merged);

            // Critical invariant: the same observable row instance remains (UI binding stability).
            Assert.Same(currentAdded, merged);

            Assert.Equal(10, (int)merged!["Id"]!);
            Assert.Equal("Engineering", (string)merged["Name"]!);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged["RowVersion"]!);
        }
    }
}
