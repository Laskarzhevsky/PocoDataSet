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
        /// Locks the identity contract for Observable PostSave merge: updating an existing row must not replace the ObservableDataRow (and must not replace its InnerDataRow). The server changeset is correlated by __ClientKey and updates the 'Name' field, then the test asserts reference equality before/after the merge.
        /// </summary>
        [Fact]
        public void Update_KeepsInstance()
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

            // Refreshed changeset updates the row by matching __ClientKey.
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = rt.AddNewRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Id"] = 1;
            serverRow["Name"] = "A2";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(refreshed, options);

            // Assert
            IObservableDataRow afterRow = MergeTestingHelpers.GetObservableRowById(t, 1);

            // Observable identity must be stable for PostSave merge (update in place).
            Assert.Same(currentRow, afterRow);

            // Inner row identity should also be stable (no replacement).
            Assert.Same(currentInnerRow, afterRow.InnerDataRow);

            // And the value was updated.
            Assert.Equal("A2", afterRow["Name"]);
        }
    }
}
