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
        /// Verifies Ignores UnchangedRows InChangeset in PostSaveMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Ignores_UnchangedRows_InChanges()
        {
            // ------------------------------------------------------------
            // Arrange
            // ------------------------------------------------------------
            Guid clientKey = Guid.NewGuid();

            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentTable.AddColumn("Name", DataTypeNames.STRING);
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IObservableDataRow baseline = currentTable.AddNewRow();
            baseline[SpecialColumnNames.CLIENT_KEY] = clientKey;
            baseline["Id"] = 10;
            baseline["Name"] = "Before";
            baseline.AcceptChanges();

            // Local pending change
            baseline["Name"] = "Edited Locally";
            Assert.Equal(DataRowState.Modified, baseline.InnerDataRow.DataRowState);
            Assert.True(baseline.InnerDataRow.HasOriginalValues);

            // Changeset row with same PK but Unchanged (must be ignored)
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable changesetTable = changeset.AddNewTable("Department");
            changesetTable.AddColumn("Id", DataTypeNames.INT32, false, true);
            changesetTable.AddColumn("Name", DataTypeNames.STRING);
            changesetTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataRow serverRowUnchanged = DataRowExtensions.CreateRowFromColumns(changesetTable.Columns);
            serverRowUnchanged[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRowUnchanged["Id"] = 10;
            serverRowUnchanged["Name"] = "Server Value That Must Be Ignored";
            changesetTable.AddLoadedRow(serverRowUnchanged);
            Assert.Equal(DataRowState.Unchanged, serverRowUnchanged.DataRowState);

            // ------------------------------------------------------------
            // Act
            // ------------------------------------------------------------
            IObservableMergeOptions options = new ObservableMergeOptions();
            current.DoPostSaveMerge(changeset, options);

            // ------------------------------------------------------------
            // Assert
            // ------------------------------------------------------------
            Assert.Single(currentTable.Rows);
            Assert.Equal("Edited Locally", (string)baseline["Name"]!);
            Assert.Equal(DataRowState.Modified, baseline.InnerDataRow.DataRowState);
            Assert.True(baseline.InnerDataRow.HasOriginalValues);
        }
    }
}