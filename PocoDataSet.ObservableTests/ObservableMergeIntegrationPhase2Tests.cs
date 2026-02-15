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
    public class ObservableMergeIntegrationPhase2Tests
    {
        #region Public Methods
        [Fact]
        public void MergeWith_PostSaveMode_MultipleAddedRows_PropagatesIdentities_AndDoesNotCreateDuplicates()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid key1 = Guid.NewGuid();
            Guid key2 = Guid.NewGuid();

            IObservableDataRow row1 = currentDepartment.AddNewRow();
            row1[SpecialColumnNames.CLIENT_KEY] = key1;
            row1["Name"] = "Engineering";

            IObservableDataRow row2 = currentDepartment.AddNewRow();
            row2[SpecialColumnNames.CLIENT_KEY] = key2;
            row2["Name"] = "Sales";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved1 = postSaveDepartment.AddNewRow();
            saved1["Id"] = 10;
            saved1[SpecialColumnNames.CLIENT_KEY] = key1;
            saved1["Name"] = "Engineering";
            saved1["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IDataRow saved2 = postSaveDepartment.AddNewRow();
            saved2["Id"] = 11;
            saved2[SpecialColumnNames.CLIENT_KEY] = key2;
            saved2["Name"] = "Sales";
            saved2["RowVersion"] = new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? merged1 = FindRowByClientKey(currentObservableDataSet.Tables["Department"], key1);
            IObservableDataRow? merged2 = FindRowByClientKey(currentObservableDataSet.Tables["Department"], key2);

            Assert.NotNull(merged1);
            Assert.NotNull(merged2);

            Assert.Equal(10, (int)merged1!["Id"]!);
            Assert.Equal(11, (int)merged2!["Id"]!);

            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged1["RowVersion"]!);
            Assert.Equal(new byte[] { 2, 0, 0, 0, 0, 0, 0, 0 }, (byte[])merged2["RowVersion"]!);
        }

        [Fact]
        public void MergeWith_PostSaveMode_MixedAddedAndModified_PropagatesBothCorrectly_AndDoesNotCrossPollinate()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Existing row (Modified scenario)
            Guid existingKey = Guid.NewGuid();
            IObservableDataRow existing = currentDepartment.AddNewRow();
            existing["Id"] = 5;
            existing[SpecialColumnNames.CLIENT_KEY] = existingKey;
            existing["Name"] = "HR";
            existing["RowVersion"] = new byte[] { 9, 0, 0, 0, 0, 0, 0, 0 };

            // Put it into Unchanged baseline, then modify
            existing.AcceptChanges();
            existing["Name"] = "HR Updated"; // now Modified

            // New row (Added scenario)
            Guid newKey = Guid.NewGuid();
            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = newKey;
            added["Name"] = "Engineering";

            // Post-save dataset
            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Returned for existing modified row (PK match)
            IDataRow savedExisting = postSaveDepartment.AddNewRow();
            savedExisting["Id"] = 5;
            savedExisting[SpecialColumnNames.CLIENT_KEY] = existingKey;
            savedExisting["Name"] = "HR Updated";
            savedExisting["RowVersion"] = new byte[] { 10, 0, 0, 0, 0, 0, 0, 0 };

            // Returned for added row (correlation by client key, PK differs because identity assigned)
            IDataRow savedAdded = postSaveDepartment.AddNewRow();
            savedAdded["Id"] = 10;
            savedAdded[SpecialColumnNames.CLIENT_KEY] = newKey;
            savedAdded["Name"] = "Engineering";
            savedAdded["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? mergedExisting = FindRowByClientKey(currentObservableDataSet.Tables["Department"], existingKey);
            IObservableDataRow? mergedAdded = FindRowByClientKey(currentObservableDataSet.Tables["Department"], newKey);

            Assert.NotNull(mergedExisting);
            Assert.NotNull(mergedAdded);

            // Existing row must remain Id=5 and get its new rowversion
            Assert.Equal(5, (int)mergedExisting!["Id"]!);
            Assert.Equal(new byte[] { 10, 0, 0, 0, 0, 0, 0, 0 }, (byte[])mergedExisting["RowVersion"]!);

            // Added row must receive identity Id=10 and its rowversion
            Assert.Equal(10, (int)mergedAdded!["Id"]!);
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, (byte[])mergedAdded["RowVersion"]!);

            // No cross-pollination (sanity)
            Assert.NotEqual((int)mergedExisting["Id"]!, (int)mergedAdded["Id"]!);
            Assert.NotEqual(((byte[])mergedExisting["RowVersion"]!)[0], ((byte[])mergedAdded["RowVersion"]!)[0]);
        }

        [Fact]
        public void MergeWith_PostSaveMode_MissingClientKeyInPostSaveRow_AddsNewRow_AndDoesNotUpdateExistingAddedRow()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();
            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = clientKey;
            added["Name"] = "Engineering";
            // added["Id"] is 0

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Missing client key: correlation is impossible.
            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved["Name"] = "Engineering";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options);

            // Assert
            // Documented defensive behavior (given current implementation): row is added as a new row.
            Assert.Equal(2, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? stillOriginal = FindRowByClientKey(currentObservableDataSet.Tables["Department"], clientKey);
            Assert.NotNull(stillOriginal);

            // Original added row is not updated (no correlation key)
            Assert.Equal(0, (int)stillOriginal!["Id"]!);

            // And a new row exists with Id=10
            Assert.True(ContainsRowWithId(currentObservableDataSet.Tables["Department"], 10));
        }

        [Fact]
        public void MergeWith_PostSaveMode_ClientKeyCollision_ThrowsInvalidOperationException()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);      // correlation-only
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid collisionKey = Guid.NewGuid();

            IObservableDataRow first = currentDepartment.AddNewRow();
            first[SpecialColumnNames.CLIENT_KEY] = collisionKey;
            first["Name"] = "First";

            IObservableDataRow second = currentDepartment.AddNewRow();
            second[SpecialColumnNames.CLIENT_KEY] = collisionKey; // collision
            second["Name"] = "Second";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow saved = postSaveDepartment.AddNewRow();
            saved["Id"] = 10;
            saved[SpecialColumnNames.CLIENT_KEY] = collisionKey;
            saved["Name"] = "Second";
            saved["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options));
        }

        [Fact]
        public void MergeWith_PostSaveMode_Throws_WhenRefreshedRowClientKeyIsEmptyGuid()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = clientKey;
            added["Name"] = "Engineering";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            // Create a refreshed row WITHOUT using AddNewRow (it auto-generates client key).
            IDataRow refreshedRow = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumnsWithDefaultValues(postSaveDepartment.Columns);
            refreshedRow["Id"] = 10;
            refreshedRow[SpecialColumnNames.CLIENT_KEY] = Guid.Empty; // invalid in PostSave
            refreshedRow["Name"] = "Engineering";
            refreshedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            refreshedRow.SetDataRowState(DataRowState.Unchanged);
            postSaveDepartment.AddRow(refreshedRow);

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options));
        }

        [Fact]
        public void MergeWith_PostSaveMode_Throws_WhenRefreshedRowClientKeyIsNull()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow added = currentDepartment.AddNewRow();
            added[SpecialColumnNames.CLIENT_KEY] = clientKey;
            added["Name"] = "Engineering";

            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            postSaveDepartment.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow refreshedRow = PocoDataSet.Extensions.DataRowExtensions.CreateRowFromColumnsWithDefaultValues(postSaveDepartment.Columns);
            refreshedRow["Id"] = 10;

            // Intentionally set to null (if your IDataRow supports null values)
            refreshedRow[SpecialColumnNames.CLIENT_KEY] = null;

            refreshedRow["Name"] = "Engineering";
            refreshedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };
            refreshedRow.SetDataRowState(DataRowState.Unchanged);
            postSaveDepartment.AddRow(refreshedRow);

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoPostSaveMerge(postSaveDataSet, options));
        }

        [Fact]
        public void MergeWith_Refresh_Throws_WhenNoPrimaryKeyDefined()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            // IMPORTANT: no "Id" column, because "Id" implies PK by convention
            IObservableDataTable currentTable = currentObservableDataSet.AddNewTable("Department");
            currentTable.AddColumn("Code", DataTypeNames.INT32); // NOT PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow row = currentTable.AddNewRow();
            row["Code"] = 100;
            row["Name"] = "Engineering";

            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshedDataSet.AddNewTable("Department");
            refreshedTable.AddColumn("Code", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedTable.AddNewRow();
            refreshedRow["Code"] = 100;
            refreshedRow["Name"] = "Engineering";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options));
        }

        [Fact]
        public void MergeWith_Refresh_Throws_WhenRefreshedTableHasDuplicatePrimaryKeys()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentTable = currentObservableDataSet.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow row = currentTable.AddNewRow();
            row["Id"] = 1;
            row["Name"] = "Engineering";

            // Refreshed dataset with DUPLICATE PK rows (Id=1 appears twice)
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshedDataSet.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Engineering";

            IDataRow r2 = refreshedTable.AddNewRow();
            r2["Id"] = 1; // duplicate
            r2["Name"] = "Engineering Duplicate";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options));
        }


        static IObservableDataRow? FindRowByClientKey(IObservableDataTable table, Guid clientKey)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IObservableDataRow row = table.Rows[i];

                object? value;
                row.InnerDataRow.TryGetValue(SpecialColumnNames.CLIENT_KEY, out value);

                if (value is Guid g && g == clientKey)
                {
                    return row;
                }
            }

            return null;
        }

        static bool ContainsRowWithId(IObservableDataTable table, int id)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                object? value = table.Rows[i]["Id"];
                if (value is int v && v == id)
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
