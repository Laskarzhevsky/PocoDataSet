using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Locks the Observable PostSave contract around current Deleted rows.
    ///
    /// Observable PostSave semantics finalize deletes at the observable table level:
    /// - Deleted rows are removed from the current observable table.
    /// - A matching server row (by PK) must NOT resurrect the deleted row.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void RemovesDeletedRow_AndDoesNotResurrect_WhenChangesetContainsRowMatchingCurrentDeletedRow_ByPk()
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
