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
        /// Verifies RemovesDeletedRow AndRaisesRowsRemovedOnce in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void RemovesDeletedRow_RaisesRowsRemovedOnce()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable dept = current.AddNewTable("Department");
            dept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            dept.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            dept.AddColumn("Name", DataTypeNames.STRING);

            Guid key1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid key2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            IObservableDataRow r1 = dept.AddNewRow();
            r1["Id"] = 1;
            r1[SpecialColumnNames.CLIENT_KEY] = key1;
            r1["Name"] = "Engineering";

            IObservableDataRow r2 = dept.AddNewRow();
            r2["Id"] = 2;
            r2[SpecialColumnNames.CLIENT_KEY] = key2;
            r2["Name"] = "Sales";

            // Make initial rows baseline so we can mark one as Deleted.
            r1.InnerDataRow.AcceptChanges();
            r2.InnerDataRow.AcceptChanges();

            // Delete through inner row (observable surface does not expose Delete directly)
            r2.InnerDataRow.Delete();

            RowsChangedCounter removedCounter = new RowsChangedCounter();
            dept.RowsRemoved += removedCounter.Handler;

            // PostSave snapshot contains only the row that remains after save.
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable postSaveDept = postSave.AddNewTable("Department");
            postSaveDept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDept.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDept.AddColumn("Name", DataTypeNames.STRING);

            IDataRow saved1 = postSaveDept.AddNewRow();
            saved1["Id"] = 1;
            saved1[SpecialColumnNames.CLIENT_KEY] = key1;
            saved1["Name"] = "Engineering";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(postSave, options);

            // Assert
            Assert.Equal(1, removedCounter.Count);
            Assert.Equal(1, dept.Rows.Count);

            Assert.True(MergeTestingHelpers.ContainsRowWithId(dept, 1));
            Assert.False(MergeTestingHelpers.ContainsRowWithId(dept, 2));
        }
    }
}