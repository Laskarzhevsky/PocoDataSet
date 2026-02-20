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
        /// Verifies CorrelatesByClientKey WhenCompositePrimaryKeyDiffers AndPreservesRowInstance in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void ClientKeyCorrelation_CompositePKDiffers_KeepsInstance()
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("T");

            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("A", DataTypeNames.INT32, false, true);
            t.AddColumn("B", DataTypeNames.STRING, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            DataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["A"] = -1;
            currentRow["B"] = "TEMP";
            currentRow["Name"] = "Row";
            t.AddRow(currentRow);

            IObservableDataSet currentObservable = new ObservableDataSet(inner);
            IObservableDataTable currentTable = currentObservable.Tables["T"];

            Assert.Equal(1, currentTable.Rows.Count);
            IObservableDataRow before = currentTable.Rows[0];

            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable pt = postSave.AddNewTable("T");

            pt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            pt.AddColumn("A", DataTypeNames.INT32, false, true);
            pt.AddColumn("B", DataTypeNames.STRING, false, true);
            pt.AddColumn("Name", DataTypeNames.STRING);

            DataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["A"] = 1;
            serverRow["B"] = "X";
            serverRow["Name"] = "Row";
            pt.AddRow(serverRow);

            serverRow.SetDataRowState(DataRowState.Added);

            ObservableMergeOptions options = new ObservableMergeOptions();

            currentObservable.DoPostSaveMerge(postSave, options);

            Assert.Equal(1, currentTable.Rows.Count);
            Assert.Same(before, currentTable.Rows[0]);

            Assert.Equal(1, (int)currentTable.Rows[0]["A"]!);
            Assert.Equal("X", (string)currentTable.Rows[0]["B"]!);
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);
        }
    }
}