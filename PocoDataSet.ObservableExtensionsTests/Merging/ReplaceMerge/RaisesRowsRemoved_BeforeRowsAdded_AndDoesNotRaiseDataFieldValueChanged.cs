using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Locks observable "Replace" event ordering: rows must be cleared (RowsRemoved) before new rows are added (RowsAdded).
    /// This guards against future regressions where the replace pipeline becomes a row-by-row update.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void RaisesRowsRemoved_BeforeRowsAdded_AndDoesNotRaiseDataFieldValueChanged()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();

            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = t.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "One";
            r1.AcceptChanges();

            IObservableDataRow r2 = t.AddNewRow();
            r2["Id"] = 2;
            r2["Name"] = "Two";
            r2.AcceptChanges();

            List<string> eventOrder = new List<string>();
            int firstRemovedIndex = -1;
            int firstAddedIndex = -1;
            int sequence = 0;
            int valueChangedCount = 0;

            t.RowsRemoved += delegate(object? sender, RowsChangedEventArgs e)
            {
                eventOrder.Add("Removed");
                if (firstRemovedIndex < 0)
                {
                    firstRemovedIndex = sequence;
                }

                sequence++;
            };

            t.RowsAdded += delegate(object? sender, RowsChangedEventArgs e)
            {
                eventOrder.Add("Added");
                if (firstAddedIndex < 0)
                {
                    firstAddedIndex = sequence;
                }

                sequence++;
            };

            t.DataFieldValueChanged += delegate(object? sender, DataFieldValueChangedEventArgs e)
            {
                valueChangedCount++;
            };

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr1["Id"] = 10;
            rr1["Name"] = "Ten";
            rt.AddLoadedRow(rr1);

            IDataRow rr2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr2["Id"] = 11;
            rr2["Name"] = "Eleven";
            rt.AddLoadedRow(rr2);

            ObservableMergeOptions options = new ObservableMergeOptions();

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(0, valueChangedCount);

            Assert.True(firstRemovedIndex >= 0);
            Assert.True(firstAddedIndex >= 0);
            Assert.True(firstRemovedIndex < firstAddedIndex);

            // We expect at least one "Removed" and one "Added", and the first must be "Removed".
            Assert.True(eventOrder.Count >= 2);
            Assert.Equal("Removed", eventOrder[0]);
            Assert.Equal("Added", eventOrder[eventOrder.Count - 1]);

            Assert.Equal(2, current.Tables["T"].Rows.Count);
            Assert.Equal(10, current.Tables["T"].Rows[0]["Id"]);
            Assert.Equal(11, current.Tables["T"].Rows[1]["Id"]);
        }
    }
}
