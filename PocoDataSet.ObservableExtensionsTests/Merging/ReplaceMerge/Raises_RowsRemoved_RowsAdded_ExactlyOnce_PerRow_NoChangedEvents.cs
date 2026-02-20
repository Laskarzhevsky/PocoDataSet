using System.Collections.Generic;

using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.IData;
using PocoDataSet.Extensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
        /// <summary>
        /// Verifies Raises RowsRemoved And RowsAdded ExactlyOnce PerRow And NoChangedEvents in ReplaceMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void Raises_RowsRemoved_RowsAdded_ExactlyOnce_PerRow_NoChangedEvents()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddAcceptedIdNameRow(t, 1, "One");
            MergeTestingHelpers.AddAcceptedIdNameRow(t, 2, "Two");
            MergeTestingHelpers.AddAcceptedIdNameRow(t, 3, "Three");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            MergeTestingHelpers.AddLoadedIdNameRow(rt, 10, "Ten");
            MergeTestingHelpers.AddLoadedIdNameRow(rt, 11, "Eleven");

            ReplaceEventRecorder recorder = new ReplaceEventRecorder();
            recorder.Attach(t);

            IObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert: no changed events
            Assert.Equal(0, recorder.TableDataFieldValueChangedCount);
            Assert.Equal(0, recorder.TableRowStateChangedCount);

            // Assert: exactly-once structural events (per row)
            Assert.Equal(3, recorder.RemovedIds.Count);
            Assert.Contains(1, recorder.RemovedIds);
            Assert.Contains(2, recorder.RemovedIds);
            Assert.Contains(3, recorder.RemovedIds);

            Assert.Equal(2, recorder.AddedIds.Count);
            Assert.Contains(10, recorder.AddedIds);
            Assert.Contains(11, recorder.AddedIds);

            Assert.Equal(2, t.Rows.Count);
        }

        private sealed class ReplaceEventRecorder
        {
            public int TableDataFieldValueChangedCount;
            public int TableRowStateChangedCount;

            public HashSet<int> RemovedIds = new HashSet<int>();
            public HashSet<int> AddedIds = new HashSet<int>();

            public void Attach(IObservableDataTable table)
            {
                table.DataFieldValueChanged += HandleTableDataFieldValueChanged;
                table.RowStateChanged += HandleTableRowStateChanged;
                table.RowsRemoved += HandleRowsRemoved;
                table.RowsAdded += HandleRowsAdded;
            }

            private void HandleTableDataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
            {
                TableDataFieldValueChangedCount++;
            }

            private void HandleTableRowStateChanged(object? sender, RowStateChangedEventArgs e)
            {
                TableRowStateChangedCount++;
            }

            private void HandleRowsRemoved(object? sender, RowsChangedEventArgs e)
            {
                if (e.ObservableDataRow == null)
                {
                    return;
                }

                RemovedIds.Add((int)e.ObservableDataRow["Id"]!);
            }

            private void HandleRowsAdded(object? sender, RowsChangedEventArgs e)
            {
                if (e.ObservableDataRow == null)
                {
                    return;
                }

                AddedIds.Add((int)e.ObservableDataRow["Id"]!);
            }
        }
    }
}