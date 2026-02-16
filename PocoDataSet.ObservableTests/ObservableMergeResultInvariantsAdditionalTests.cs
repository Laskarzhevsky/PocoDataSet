
using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    /// <summary>
    /// Observable merge result invariants: Added/Updated entries must refer to rows present in the final table,
    /// Deleted entries must refer to PKs not present in the final table.
    /// </summary>
    public sealed class ObservableMergeResultInvariantsAdditionalTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_ResultEntries_MatchFinalTableState()
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
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert - counts
            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            int addedId = (int)options.ObservableDataSetMergeResult.AddedObservableDataRows[0].ObservableDataRow["Id"]!;
            int updatedId = (int)options.ObservableDataSetMergeResult.UpdatedObservableDataRows[0].ObservableDataRow["Id"]!;
            int deletedId = (int)options.ObservableDataSetMergeResult.DeletedObservableDataRows[0].ObservableDataRow["Id"]!;

            Assert.True(RowExistsById(t, addedId));
            Assert.True(RowExistsById(t, updatedId));
            Assert.False(RowExistsById(t, deletedId));

            Assert.Equal(2, t.Rows.Count);
            Assert.True(RowExistsById(t, 2));
            Assert.True(RowExistsById(t, 3));
        }

        [Fact]
        public void RefreshIfNoChangesExist_ResultEntries_MatchFinalTableState()
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

            Assert.True(RowExistsById(t, addedId));
            Assert.True(RowExistsById(t, updatedId));
            Assert.False(RowExistsById(t, deletedId));

            Assert.Equal(2, t.Rows.Count);
            Assert.True(RowExistsById(t, 2));
            Assert.True(RowExistsById(t, 3));
        }

        private static bool RowExistsById(IObservableDataTable t, int id)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["Id"]! == id)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
