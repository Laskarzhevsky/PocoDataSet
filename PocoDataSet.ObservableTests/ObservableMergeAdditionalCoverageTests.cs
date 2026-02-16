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
    /// <summary>
    /// Additional high-value coverage for the observable merge pipeline after the "no MergeMode / no policies" refactor.
    /// These tests focus on invariants that are easy to regress during future edits.
    /// </summary>
    public class ObservableMergeAdditionalCoverageTests
    {
        #region RefreshIfNoChangesExist - Dirty matrix

        [Fact]
        public void DoRefreshMergeIfNoChangesExist_Throws_WhenCurrentHasAddedRow()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentDept = current.AddNewTable("Department");
            currentDept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDept.AddColumn("Name", DataTypeNames.STRING);

            // Added row (not accepted)
            IObservableDataRow added = currentDept.AddNewRow();
            added["Id"] = 1;
            added["Name"] = "Sales";

            IDataSet refreshed = CreateDepartmentRefreshedSnapshot(id1Name: "Sales");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                current.DoRefreshMergeIfNoChangesExist(refreshed, options);
            });
        }

        [Fact]
        public void DoRefreshMergeIfNoChangesExist_Throws_WhenCurrentHasDeletedRow()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentDept = current.AddNewTable("Department");
            currentDept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDept.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = currentDept.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1.AcceptChanges();

            // Delete through inner row (observable surface does not expose Delete directly)
            r1.InnerDataRow.Delete();

            IDataSet refreshed = CreateDepartmentRefreshedSnapshot(id1Name: "Sales");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                current.DoRefreshMergeIfNoChangesExist(refreshed, options);
            });
        }

        #endregion

        #region Replace - event invariants

        [Fact]
        public void DoReplaceMerge_RaisesRowsRemovedAndRowsAddedCounts()
        {
            // Arrange: current observable data set with 2 rows
            ObservableDataSet current = CreateCurrentObservableDepartmentDataSet();
            IObservableDataView? view = current.GetObservableDataView("Department", null, false, "Id ASC", "ScreenReplace");
            Assert.NotNull(view);

            RowsChangedCounter removedCounter = new RowsChangedCounter();
            RowsChangedCounter addedCounter = new RowsChangedCounter();

            view.RowsRemoved += removedCounter.Handler;
            view.RowsAdded += addedCounter.Handler;

            // Refreshed has 1 row (Id=1)
            IDataSet refreshed = CreateDepartmentRefreshedSnapshot(id1Name: "SalesRefreshed");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(1, view.Rows.Count);
            Assert.Equal(1, view.Rows[0].GetDataFieldValue<int>("Id"));

            // 2 removed, 1 added
            Assert.Equal(2, removedCounter.Count);
            Assert.Equal(1, addedCounter.Count);
        }

        #endregion

        #region Refresh - composite PK duplicate detection

        [Fact]
        public void DoRefreshMergePreservingLocalChanges_Throws_WhenRefreshedHasDuplicateCompositePrimaryKeys()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentT = current.AddNewTable("EmployeeRole");
            currentT.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            currentT.AddColumn("RoleId", DataTypeNames.INT32, false, true);
            currentT.AddColumn("Name", DataTypeNames.STRING);

            // Current can be empty; we just want refreshed validation to run.
            currentT.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedT = refreshed.AddNewTable("EmployeeRole");
            refreshedT.AddColumn("EmployeeId", DataTypeNames.INT32, false, true);
            refreshedT.AddColumn("RoleId", DataTypeNames.INT32, false, true);
            refreshedT.AddColumn("Name", DataTypeNames.STRING);

            DataRow a = new DataRow();
            a["EmployeeId"] = 1;
            a["RoleId"] = 10;
            a["Name"] = "A";
            refreshedT.AddRow(a);

            DataRow b = new DataRow();
            b["EmployeeId"] = 1;
            b["RoleId"] = 10; // duplicate composite PK
            b["Name"] = "B";
            refreshedT.AddRow(b);

            refreshedT.AcceptChanges();

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act / Assert
            Assert.Throws<InvalidOperationException>(delegate
            {
                current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            });
        }

        #endregion

        #region Options propagation / result stability

        [Fact]
        public void MergeOptions_ObservableDataSetMergeResultReferenceIsStable_AndCollectsEntries()
        {
            // Arrange
            ObservableDataSet current = CreateCurrentObservableDepartmentDataSet();

            // Refreshed adds Id=3, removes Id=2, updates Id=1
            IDataSet refreshed = CreateRefreshedDepartmentDataSet();

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult initialResult = options.ObservableDataSetMergeResult;

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert: same instance
            Assert.Same(initialResult, options.ObservableDataSetMergeResult);

            // And it collected something meaningful (added + deleted + updated should all be possible here)
            Assert.True(
                options.ObservableDataSetMergeResult.AddedObservableDataRows.Count > 0
                || options.ObservableDataSetMergeResult.DeletedObservableDataRows.Count > 0
                || options.ObservableDataSetMergeResult.UpdatedObservableDataRows.Count > 0);
        }

        #endregion

        

        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotRaiseDataFieldValueChanged_WhenRefreshedValuesAreIdentical()
        {
            // Arrange
            ObservableDataSet current = CreateCurrentObservableDepartmentDataSet();
            IObservableDataTable currentTable = current.Tables["Department"];

            IObservableDataRow row1 = FindById(currentTable, 1);
            DataFieldValueChangedCounter counter = new DataFieldValueChangedCounter();
            row1.DataFieldValueChanged += counter.Handler;

            // Refreshed snapshot has the exact same values as current for Id=1.
            IDataSet refreshed = CreateDepartmentRefreshedSnapshot("Sales");

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(0, counter.Count);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_Throws_WhenPrimaryKeyContainsNull_InRefreshedRow()
        {
            // Arrange
            ObservableDataSet current = CreateCurrentObservableDepartmentDataSet();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("Department");
            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("Id", DataTypeNames.INT32, true, true); // PK, allow null so the row can be constructed
            t.AddColumn("Name", DataTypeNames.STRING);

	            // NOTE:
	            // Assigning `null` to a value-type column may be normalized to the default value
	            // (e.g., 0 for Int32) depending on conversion rules. Use DBNull.Value to represent
	            // a "missing PK value" reliably.
	            DataRow bad = new DataRow();
	            bad[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
	            bad["Id"] = DBNull.Value;
	            bad["Name"] = "Bad";
            t.AddRow(bad);

            t.AcceptChanges();

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }

        private static IObservableDataRow FindById(IObservableDataTable table, int id)
        {
            foreach (IObservableDataRow row in table.Rows)
            {
                if (row.TryGetValue("Id", out object? value) && value is int i && i == id)
                {
                    return row;
                }
            }

            throw new InvalidOperationException("Row not found.");
        }


        private sealed class DataFieldValueChangedCounter
        {
            public int Count { get; private set; }

            public void Handler(object? sender, DataFieldValueChangedEventArgs e)
            {
                Count++;
            }
        }


#region Helpers

        private static ObservableDataSet CreateCurrentObservableDepartmentDataSet()
        {
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            ObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            // Deterministic client keys so tests are stable.
            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid clientKey2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            IObservableDataRow r1 = currentTable.AddNewRow();
            r1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1.AcceptChanges();

            IObservableDataRow r2 = currentTable.AddNewRow();
            r2[SpecialColumnNames.CLIENT_KEY] = clientKey2;
            r2["Id"] = 2;
            r2["Name"] = "HR";
            r2.AcceptChanges();

            currentTable.AcceptChanges();

            return current;
        }

        private static IDataSet CreateRefreshedDepartmentDataSet()
        {
            // Refreshed:
            // - Id=1 updated Name
            // - Id=2 missing (deleted)
            // - Id=3 added
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");

            DataRow refreshedRow1 = new DataRow();
            refreshedRow1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            refreshedRow1["Id"] = 1;
            refreshedRow1["Name"] = "SalesUpdated";
            refreshedTable.AddRow(refreshedRow1);

            DataRow refreshedRow3 = new DataRow();
            refreshedRow3[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("33333333-3333-3333-3333-333333333333");
            refreshedRow3["Id"] = 3;
            refreshedRow3["Name"] = "IT";
            refreshedTable.AddRow(refreshedRow3);

            refreshedTable.AcceptChanges();

            return refreshed;
        }

        private static IDataSet CreateDepartmentRefreshedSnapshot(string id1Name)
        {
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow r1 = new DataRow();
            r1["Id"] = 1;
            r1["Name"] = id1Name;
            t.AddRow(r1);

            t.AcceptChanges();
            return refreshed;
        }

        #endregion
    }
}
