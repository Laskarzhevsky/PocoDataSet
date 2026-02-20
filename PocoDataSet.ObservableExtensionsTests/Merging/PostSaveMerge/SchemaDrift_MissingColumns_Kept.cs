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
        /// Locks the observable PostSave schema contract when the server changeset is missing a CURRENT column: CURRENT keeps its schema, and values for missing columns are preserved (not overwritten).
        /// </summary>
        [Fact]
        public void SchemaDrift_MissingColumns_Kept()
        {
            // Arrange CURRENT with an extra column that the server will not send back.
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Extra", DataTypeNames.STRING);
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            Guid clientKey = Guid.NewGuid();


            IObservableDataRow currentRow = t.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Before";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["Extra"] = "LocalExtra";
            currentRow.AcceptChanges();

            // Local modification before save.
            currentRow["Name"] = "Edited";
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            Assert.Equal(DataRowState.Modified, currentRow.InnerDataRow.DataRowState);

            // SERVER changeset lacks "Extra".
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable cs = changeset.AddNewTable("T");
            cs.AddColumn("Id", DataTypeNames.INT32, false, true);
            cs.AddColumn("Name", DataTypeNames.STRING);
            cs.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            IDataRow serverRow = cs.AddNewRow();
            serverRow["Id"] = 1;
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow.AcceptChanges();
            serverRow["Name"] = "Edited";
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey; // mark Modified

            IObservableMergeOptions options = new ObservableMergeOptions();


            // Act
            current.DoPostSaveMerge(changeset, options);

            // Assert: CURRENT schema is preserved (Extra column remains) and the value is NOT overwritten
            // when the server changeset does not include that column.
            Assert.Equal(3, MergeTestingHelpers.CountUserColumns(t));
            Assert.Equal("Edited", currentRow["Name"]);
            Assert.Equal("LocalExtra", currentRow["Extra"]);

            // Observable PostSave does not auto-Accept local modified rows in this scenario; it remains Modified.
            Assert.Equal(DataRowState.Modified, currentRow.InnerDataRow.DataRowState);
        }
    }
}
