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
        /// Verifies RemovesDeletedRow AndDoesNotResurrect WhenChangesetContainsRowMatchingCurrentDeletedRow ByPk in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void RemovesDeletedRow_NoResurrect_ChangesContainsRowMatchingCurrentDeletedRow_ByPk()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow row = currentTable.AddNewRow();
            row["Id"] = 10;
            row["Name"] = "To Be Deleted";
            row.AcceptChanges();

            row.InnerDataRow.Delete();
            Assert.Equal(DataRowState.Deleted, row.InnerDataRow.DataRowState);

            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = changesetTable.AddNewRow();
            serverRow["Id"] = 10;
            serverRow["Name"] = "Server Says Exists";
            Assert.Equal(DataRowState.Added, serverRow.DataRowState);

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            IObservableMergeOptions options = new ObservableMergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            // Deleted row is finalized (removed) and not resurrected.
            Assert.Empty(currentTable.Rows);

            // Merge result records the deletion.
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);
        }
    }
}