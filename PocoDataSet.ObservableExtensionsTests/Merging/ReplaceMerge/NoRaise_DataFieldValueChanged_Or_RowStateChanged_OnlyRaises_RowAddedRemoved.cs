using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Verifies DoesNotRaise DataFieldValueChanged Or RowStateChanged AndOnlyRaises RowAddedRemoved in ReplaceMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void NoRaise_DataFieldValueChanged_Or_RowStateChanged_OnlyRaises_RowAddedRemoved()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow existing = t.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "One";
            existing.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr2["Id"] = 2;
            rr2["Name"] = "Two";
            rt.AddLoadedRow(rr2);

            ReplaceEventCounter counter = new ReplaceEventCounter();

            t.DataFieldValueChanged += counter.HandleTableDataFieldValueChanged;
            t.RowStateChanged += counter.HandleTableRowStateChanged;
            t.RowsAdded += counter.HandleRowsAdded;
            t.RowsRemoved += counter.HandleRowsRemoved;

            // Attach to an existing row to ensure Replace does not cause per-row "changed" events either.
            existing.DataFieldValueChanged += counter.HandleRowDataFieldValueChanged;

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(1, counter.RowsRemovedCount);
            Assert.Equal(1, counter.RowsAddedCount);

            Assert.Equal(0, counter.TableDataFieldValueChangedCount);
            Assert.Equal(0, counter.RowDataFieldValueChangedCount);
            Assert.Equal(0, counter.TableRowStateChangedCount);

            Assert.Single(current.Tables["T"].Rows);
            Assert.Equal(2, current.Tables["T"].Rows[0]["Id"]);
            Assert.Equal("Two", (string)current.Tables["T"].Rows[0]["Name"]!);
        }

        private sealed class ReplaceEventCounter
        {
            public int TableDataFieldValueChangedCount;
            public int RowDataFieldValueChangedCount;
            public int TableRowStateChangedCount;
            public int RowsAddedCount;
            public int RowsRemovedCount;

            public void HandleTableDataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
            {
                TableDataFieldValueChangedCount++;
            }

            public void HandleRowDataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
            {
                RowDataFieldValueChangedCount++;
            }

            public void HandleTableRowStateChanged(object? sender, RowStateChangedEventArgs e)
            {
                TableRowStateChangedCount++;
            }

            public void HandleRowsAdded(object? sender, RowsChangedEventArgs e)
            {
                RowsAddedCount++;
            }

            public void HandleRowsRemoved(object? sender, RowsChangedEventArgs e)
            {
                RowsRemovedCount++;
            }
        }
    }
}