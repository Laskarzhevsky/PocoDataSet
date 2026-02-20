
using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergeIfNoChangesExist
    {
        /// <summary>
        /// Verifies ResultEntries MatchFinalTableState in RefreshMergeIfNoChangesExist merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void ResultEntries_MatchFinalTableState()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1.AcceptChanges();

            IObservableDataRow c2 = t.AddNewRow();
            c2["Id"] = 2;
            c2["Name"] = "Two";
            c2.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r2["Id"] = 2;
            r2["Name"] = "Two (updated)";
            rt.AddLoadedRow(r2);

            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r3["Id"] = 3;
            r3["Name"] = "Three";
            rt.AddLoadedRow(r3);

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert - counts
            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            int addedId = (int)options.ObservableDataSetMergeResult.AddedObservableDataRows[0].ObservableDataRow["Id"]!;
            int updatedId = (int)options.ObservableDataSetMergeResult.UpdatedObservableDataRows[0].ObservableDataRow["Id"]!;
            int deletedId = (int)options.ObservableDataSetMergeResult.DeletedObservableDataRows[0].ObservableDataRow["Id"]!;

            Assert.True(MergeTestingHelpers.RowExistsById(t, addedId));
            Assert.True(MergeTestingHelpers.RowExistsById(t, updatedId));
            Assert.False(MergeTestingHelpers.RowExistsById(t, deletedId));

            Assert.Equal(2, t.Rows.Count);
            Assert.True(MergeTestingHelpers.RowExistsById(t, 2));
            Assert.True(MergeTestingHelpers.RowExistsById(t, 3));
        }
    }
}