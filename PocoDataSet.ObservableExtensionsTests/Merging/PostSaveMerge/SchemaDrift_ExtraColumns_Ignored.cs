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
        /// Locks the observable PostSave schema contract: server-only extra columns in the changeset are ignored and the CURRENT observable table schema remains unchanged.
        /// </summary>
        [Fact]
        public void SchemaDrift_ExtraColumns_Ignored()
        {
            // Arrange CURRENT (observable) with Id+Name.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            Guid clientKey = Guid.NewGuid();


            IObservableDataRow currentRow = t.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Before";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow.AcceptChanges();

            // Simulate local modification before save.
            currentRow["Name"] = "Edited";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            Assert.Equal(DataRowState.Modified, currentRow.InnerDataRow.DataRowState);

            // SERVER changeset includes an extra column.
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable cs = changeset.AddNewTable("T");
            cs.AddColumn("Id", DataTypeNames.INT32, false, true);
            cs.AddColumn("Name", DataTypeNames.STRING);
            cs.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            cs.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow serverRow = cs.AddNewRow();
            serverRow["Id"] = 1;
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["Extra"] = "Ignored";
            serverRow.AcceptChanges();
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey; // mark Modified

            IObservableMergeOptions options = new ObservableMergeOptions();


            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert: value updated, schema unchanged (Extra not added).
            Assert.Equal("Edited", currentRow["Name"]);
            Assert.Equal(2, MergeTestingHelpers.CountUserColumns(t));

            // We intentionally do not assert DataRowState here: observable PostSave may preserve local Modified state
            // depending on the contract for reconciling pending changes.
        }
    }
}
