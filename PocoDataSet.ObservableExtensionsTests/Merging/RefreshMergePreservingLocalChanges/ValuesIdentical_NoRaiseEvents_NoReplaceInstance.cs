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
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies WhenValuesIdentical DoesNotRaiseEvents AndDoesNotReplaceRowInstance in RefreshMergePreservingLocalChanges merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void ValuesIdentical_NoRaiseEvents_NoReplaceInstance()
        {
            // Arrange: current observable dataset with one row.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IDataTable currentTable = currentInner.AddNewTable("Department");
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            DataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentTable.AddLoadedRow(currentRow);

            IObservableDataSet currentObservable = new ObservableDataSet(currentInner);
            IObservableDataTable observableTable = currentObservable.Tables["Department"];
            IObservableDataRow observableRow = observableTable.Rows[0];

            DataFieldValueChangedCounter counter = new DataFieldValueChangedCounter();
            observableRow.DataFieldValueChanged += counter.Handler;

            // Refreshed snapshot has identical values.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            DataRow refreshedRow = new DataRow();
            refreshedRow[SpecialColumnNames.CLIENT_KEY] = (Guid)currentRow[SpecialColumnNames.CLIENT_KEY]!;
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales";
            refreshedTable.AddLoadedRow(refreshedRow);

            ObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            currentObservable.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert: no churn
            Assert.Equal(0, counter.Count);
            Assert.Same(observableRow, observableTable.Rows[0]);
            Assert.Equal(DataRowState.Unchanged, observableTable.Rows[0].DataRowState);
        }
    }
}