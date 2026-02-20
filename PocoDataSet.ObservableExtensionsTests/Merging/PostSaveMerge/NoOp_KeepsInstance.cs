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
        /// Locks the identity contract for Observable PostSave merge when the server changeset contains identical values: the merge should perform no replacement of the ObservableDataRow or its InnerDataRow, and the row should remain Unchanged.
        /// </summary>
        [Fact]
        public void NoOp_KeepsInstance()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.Parse("11111111-1111-1111-1111-111111111111");

            IObservableDataRow currentRow = t.AddNewRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["Id"] = 1;
            currentRow["Name"] = "A";
            currentRow.AcceptChanges();

            IDataRow currentInnerRow = currentRow.InnerDataRow;

            // Refreshed changeset contains the same values (no-op).
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = rt.AddNewRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Id"] = 1;
            serverRow["Name"] = "A";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(refreshed, options);

            // Assert
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            // PostSave merge should not replace the observable row on a no-op.
            Assert.Same(currentRow, afterRow);

            // Inner row should remain the same instance as well.
            Assert.Same(currentInnerRow, afterRow.InnerDataRow);

            // Row remains unchanged.
            Assert.Equal(DataRowState.Unchanged, afterRow.InnerDataRow.DataRowState);
        }
    }
}
