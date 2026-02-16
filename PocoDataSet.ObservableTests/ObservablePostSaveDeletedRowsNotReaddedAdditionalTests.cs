using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservablePostSaveDeletedRowsNotReaddedAdditionalTests
    {
        [Fact]
        public void PostSave_DoesNotReAdd_DeletedRow_EvenIfPresentInRefreshedChangeset()
        {
            // Arrange: current observable data set with 2 loaded rows.
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable table = inner.AddNewTable("Department");
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid clientKey2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            DataRow row1 = new DataRow();
            row1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            row1["Id"] = 1;
            row1["Name"] = "Sales";
            table.AddRow(row1);

            DataRow row2 = new DataRow();
            row2[SpecialColumnNames.CLIENT_KEY] = clientKey2;
            row2["Id"] = 2;
            row2["Name"] = "HR";
            table.AddRow(row2);

            table.AcceptChanges();

            ObservableDataSet current = new ObservableDataSet(inner);
            IObservableDataTable currentTable = current.Tables["Department"];

            // Mark the second row as Deleted.
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[1].DataRowState);
            currentTable.Rows[1].Delete();
            Assert.Equal(DataRowState.Deleted, currentTable.Rows[1].DataRowState);

            _rowsRemovedEventCount = 0;
            currentTable.RowsRemoved += this.CurrentTable_RowsRemoved;

            // Arrange: refreshed post-save changeset still contains the deleted row (server echoed it back).
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable cs = changeset.AddNewTable("Department");
            cs.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            cs.AddColumn("Id", DataTypeNames.INT32);
            cs.AddColumn("Name", DataTypeNames.STRING);

            // unchanged row
            DataRow cs1 = new DataRow();
            cs1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            cs1["Id"] = 1;
            cs1["Name"] = "Sales";
            cs.AddLoadedRow(cs1);

            // deleted row still present in refreshed snapshot
            DataRow cs2 = new DataRow();
            cs2[SpecialColumnNames.CLIENT_KEY] = clientKey2;
            cs2["Id"] = 2;
            cs2["Name"] = "HR";
            cs.AddLoadedRow(cs2);

            ObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert: deleted row must be removed and MUST NOT be re-added.
            Assert.Equal(1, currentTable.Rows.Count);
            Assert.Equal(1, (int)currentTable.Rows[0]["Id"]!);
            Assert.Equal("Sales", (string)currentTable.Rows[0]["Name"]!);

            // RowsRemoved must be raised exactly once for this single-row deletion.
            Assert.Equal(1, _rowsRemovedEventCount);
        }

        private int _rowsRemovedEventCount;

        private void CurrentTable_RowsRemoved(object? sender, RowsChangedEventArgs e)
        {
            _rowsRemovedEventCount++;
        }
    }
}
