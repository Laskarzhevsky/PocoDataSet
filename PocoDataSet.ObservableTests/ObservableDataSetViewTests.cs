using System;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableDataSetViewTests
    {
        [Fact]
        public void GetObservableDataView_UsesUnambiguousCacheKey_PreventsNameCollisions()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            DataTable tableAB = new DataTable();
            tableAB.TableName = "AB";
            dataSet.AddTable(tableAB);

            DataTable tableA = new DataTable();
            tableA.TableName = "A";
            dataSet.AddTable(tableA);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            // Act
            IObservableDataView? view1 = observableDataSet.GetObservableDataView("AB", null, false, null, "C");
            IObservableDataView? view2 = observableDataSet.GetObservableDataView("A", null, false, null, "BC");

            // Assert
            Assert.NotNull(view1);
            Assert.NotNull(view2);
            Assert.False(object.ReferenceEquals(view1, view2));
        }

        [Fact]
        public void RemoveObservableDataView_DisposesAndUnsubscribes_ViewStopsReceivingRowEvents()
        {
            // Arrange
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            table.TableName = "Department";
            dataSet.AddTable(table);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "Screen1");
            Assert.NotNull(view);

            RowsChangedCounter rowsAddedCounter = new RowsChangedCounter();
            view!.RowsAdded += rowsAddedCounter.Handler;

            // Sanity: adding a row should notify the view
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            observableTable.AddRow(new DataRow());
            Assert.Equal(1, rowsAddedCounter.Count);

            // Act
            bool removed = observableDataSet.RemoveObservableDataView("Department", "Screen1");

            // Assert
            Assert.True(removed);

            // Add another row - disposed view should no longer receive the event
            observableTable.AddRow(new DataRow());
            Assert.Equal(1, rowsAddedCounter.Count);

            // Asking for the same view should create a new instance
            IObservableDataView? newView = observableDataSet.GetObservableDataView("Department", null, false, null, "Screen1");
            Assert.NotNull(newView);
            Assert.False(object.ReferenceEquals(view, newView));
        }

        [Fact]
        public void RemoveObservableDataView_IsIdempotent_SecondCallReturnsFalse_AndDoesNotThrow()
        {
            // Arrange
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            table.TableName = "Department";
            dataSet.AddTable(table);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenX");
            Assert.NotNull(view);

            // Act
            bool removed1 = observableDataSet.RemoveObservableDataView("Department", "ScreenX");
            bool removed2 = observableDataSet.RemoveObservableDataView("Department", "ScreenX");

            // Assert
            Assert.True(removed1);
            Assert.False(removed2);
        }

        [Fact]
        public void RemoveObservableDataViewsForRequestor_WhenNoneExist_ReturnsZero()
        {
            // Arrange
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            table.TableName = "T1";
            dataSet.AddTable(table);
            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            // Act
            int removed = observableDataSet.RemoveObservableDataViewsForRequestor("NoSuchRequestor");

            // Assert
            Assert.Equal(0, removed);
        }

        [Fact]
        public void Dispose_CanBeCalledTwice_OnObservableDataView_DoesNotThrow()
        {
            // Arrange
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            table.TableName = "Department";
            dataSet.AddTable(table);
            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenDispose");
            Assert.NotNull(view);

            // Act
            view!.Dispose();
            view.Dispose();

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void RepeatedCreateRemoveCycle_DoesNotLeaveDanglingSubscriptions_NoDuplicateEventsOnNewView()
        {
            // Arrange
            DataSet dataSet = new DataSet();
            DataTable table = new DataTable();
            table.TableName = "Department";
            dataSet.AddTable(table);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            observableTable.AddRow(new DataRow());
            IObservableDataRow row = observableTable.Rows[0];

            DataFieldValueChangedCounter counter = new DataFieldValueChangedCounter();

            // Act + Assert (3 cycles)
            for (int i = 0; i < 3; i++)
            {
                IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenCycle");
                Assert.NotNull(view);

                view!.DataFieldValueChanged += counter.Handler;

                // Update should be observed once by the current view.
                row["Any"] = i;
                Assert.Equal(i + 1, counter.Count);

                // Remove/dispose the view but DO NOT unsubscribe the handler.
                // If the view did not unsubscribe from row events during disposal, the handler would
                // keep firing (dangling subscription), causing unexpected extra increments.
                bool removed = observableDataSet.RemoveObservableDataView("Department", "ScreenCycle");
                Assert.True(removed);

                // Another update after disposal must NOT increment counter.
                row["Any"] = i + 1000;
                Assert.Equal(i + 1, counter.Count);
            }
        }

        [Fact]
        public void RemoveObservableDataViewsForRequestor_RemovesAllViewsForRequestor()
        {
            // Arrange
            DataSet dataSet = new DataSet();

            DataTable t1 = new DataTable();
            t1.TableName = "T1";
            dataSet.AddTable(t1);

            DataTable t2 = new DataTable();
            t2.TableName = "T2";
            dataSet.AddTable(t2);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            IObservableDataView? v1 = observableDataSet.GetObservableDataView("T1", null, false, null, "R1");
            IObservableDataView? v2 = observableDataSet.GetObservableDataView("T2", null, false, null, "R1");
            IObservableDataView? v3 = observableDataSet.GetObservableDataView("T1", null, false, null, "Other");

            Assert.NotNull(v1);
            Assert.NotNull(v2);
            Assert.NotNull(v3);

            // Act
            int removedCount = observableDataSet.RemoveObservableDataViewsForRequestor("R1");

            // Assert
            Assert.Equal(2, removedCount);

            // Views for R1 should be recreated (not returned from cache)
            IObservableDataView? nv1 = observableDataSet.GetObservableDataView("T1", null, false, null, "R1");
            IObservableDataView? nv2 = observableDataSet.GetObservableDataView("T2", null, false, null, "R1");

            Assert.NotNull(nv1);
            Assert.NotNull(nv2);
            Assert.False(object.ReferenceEquals(v1, nv1));
            Assert.False(object.ReferenceEquals(v2, nv2));

            // View for Other should still be cached
            IObservableDataView? sameV3 = observableDataSet.GetObservableDataView("T1", null, false, null, "Other");
            Assert.True(object.ReferenceEquals(v3, sameV3));
        }
    }
}
