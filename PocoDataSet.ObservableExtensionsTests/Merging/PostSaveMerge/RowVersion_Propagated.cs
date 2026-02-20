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
        /// Verifies the observable PostSave contract for rowversion / concurrency tokens: server-returned RowVersion overwrites the local token so the client uses the latest value.
        /// </summary>
        [Fact]
        public void RowVersion_Propagated()
        {
            // Arrange CURRENT observable table with a RowVersion column and deterministic client key.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");

            IObservableDataRow currentRow = t.AddNewRow();
            currentRow["Id"] = 10;
            currentRow["Name"] = "Before";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["RowVersion"] = new byte[] { 1, 2, 3, 4 };
            currentRow.AcceptChanges();

            // Simulate a local edit before save.
            currentRow["Name"] = "Edited";
            Assert.Equal(DataRowState.Modified, currentRow.DataRowState);

            // SERVER PostSave changeset returns same logical row with updated RowVersion.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable cs = changeset.AddNewTable("Department");
            cs.AddColumn("Id", DataTypeNames.INT32);
            cs.AddColumn("Name", DataTypeNames.STRING);
            cs.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            cs.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow serverRow = DataRowExtensions.CreateRowFromColumns(cs.Columns);
            serverRow["Id"] = 10;
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["RowVersion"] = new byte[] { 9, 9, 9, 9 };

            // Mark as Modified in the changeset so PostSave applies it as an update.
            serverRow.AcceptChanges();
            serverRow["Name"] = "Edited";
            cs.AddRow(serverRow);

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert
            Assert.Equal("Edited", currentRow["Name"]);
            Assert.Equal(DataRowState.Unchanged, currentRow.DataRowState);

            byte[] rv = (byte[])currentRow["RowVersion"]!;
            Assert.Equal(4, rv.Length);
            Assert.Equal(9, rv[0]);
            Assert.Equal(9, rv[1]);
            Assert.Equal(9, rv[2]);
            Assert.Equal(9, rv[3]);

        }
    }
}
