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
        /// When CURRENT already matches the refreshed snapshot exactly, RefreshIfNoChangesExist should be a no-op: no rows are added/removed/updated and the merge result may record a merge-result entry even when no data changes occur.
        /// </summary>
        [Fact]
        public void NoOp_WhenIdentical()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow c1 = t.AddNewRow();
            c1["Id"] = 1;
            c1["Name"] = "One";
            c1.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One";

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: still one row, and merge result has no entries.
            Assert.Single(t.Rows);
            Assert.Equal("One", t.Rows[0]["Name"]);

            Assert.Empty(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            // NOTE: Observable merge reports the processed row in UpdatedObservableDataRows even when the row remains Unchanged.
            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);

            IObservableDataSetMergeResultEntry entry = options.ObservableDataSetMergeResult.UpdatedObservableDataRows[0];
            Assert.Equal("T", entry.TableName);
            Assert.Equal(DataRowState.Unchanged, entry.ObservableDataRow.InnerDataRow.DataRowState);
        }
    }
}
