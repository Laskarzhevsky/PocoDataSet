using System.Collections.Generic;

using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.IData;
using PocoDataSet.Extensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    /// <summary>
    /// Observable event contract hardening for Replace:
    /// - RowsRemoved fires once per removed row (args carries ObservableDataRow + RowIndex)
    /// - RowsAdded fires once per added row
    /// - Replace does not raise DataFieldValueChanged or RowStateChanged
    /// </summary>
    public sealed class ObservableEventExactlyOnceAdditionalTests
    {
        [Fact]
        public void Replace_Raises_RowsRemoved_And_RowsAdded_ExactlyOnce_PerRow_And_NoChangedEvents()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            AddAcceptedRow(t, 1, "One");
            AddAcceptedRow(t, 2, "Two");
            AddAcceptedRow(t, 3, "Three");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            AddLoadedRow(rt, 10, "Ten");
            AddLoadedRow(rt, 11, "Eleven");

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

        private static void AddAcceptedRow(IObservableDataTable t, int id, string name)
        {
            IObservableDataRow r = t.AddNewRow();
            r["Id"] = id;
            r["Name"] = name;
            r.AcceptChanges();
        }

        private static void AddLoadedRow(IDataTable t, int id, string name)
        {
            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Id"] = id;
            r["Name"] = name;
            t.AddLoadedRow(r);
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
