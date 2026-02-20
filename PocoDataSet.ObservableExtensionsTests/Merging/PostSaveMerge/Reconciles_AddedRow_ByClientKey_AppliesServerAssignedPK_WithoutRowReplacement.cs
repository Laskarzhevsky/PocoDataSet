using System;

using PocoDataSet.Data;
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
        /// Verifies Reconciles AddedRow ByClientKey AndAppliesServerAssignedPrimaryKey WithoutRowReplacement in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Reconciles_AddedRow_ByClientKey_AppliesServerAssignedPK_WithoutRowReplacement()
        {
            // Arrange
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            DataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["Id"] = -1;
            currentRow["Name"] = "Customer Service";
            t.AddRow(currentRow);

            IObservableDataSet currentObservable = new ObservableDataSet(inner);
            IObservableDataTable currentTable = currentObservable.Tables["Department"];

            Assert.Equal(1, currentTable.Rows.Count);
            IObservableDataRow beforeMergeRow = currentTable.Rows[0];

            // Arrange: server post-save response with server-assigned identity.
            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable pt = postSave.AddNewTable("Department");
            pt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            pt.AddColumn("Id", DataTypeNames.INT32, false, true);
            pt.AddColumn("Name", DataTypeNames.STRING);

            DataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Id"] = 10;
            serverRow["Name"] = "Customer Service";
            pt.AddRow(serverRow);

            // Ensure this is a proper changeset row (Added).
            serverRow.SetDataRowState(DataRowState.Added);

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult result = options.ObservableDataSetMergeResult;

            // Act
            currentObservable.DoPostSaveMerge(postSave, options);

            // Assert: row instance preserved
            Assert.Equal(1, currentTable.Rows.Count);
            Assert.Same(beforeMergeRow, currentTable.Rows[0]);

            // Assert: PK updated
            Assert.Equal(10, (int)currentTable.Rows[0]["Id"]!);
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);

            // Assert: merge result contains the reconciled row entry
            Assert.True(MergeTestingHelpers.ContainsRow(result, "Department", beforeMergeRow));
        }
    }
}