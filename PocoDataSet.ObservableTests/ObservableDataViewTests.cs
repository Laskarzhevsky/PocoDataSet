using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableDataViewTests
    {
        #region Private Helpers
        static ObservableDataSet CreateObservableDepartmentDataSetWithTwoLoadedRows()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys.Add("Id");

            DataRow loadedRow1 = new DataRow();
            loadedRow1["Id"] = 1;
            loadedRow1["Name"] = "Sales";

            DataRow loadedRow2 = new DataRow();
            loadedRow2["Id"] = 2;
            loadedRow2["Name"] = "HR";

            ((DataTable)table).AddLoadedRow(loadedRow1);
            ((DataTable)table).AddLoadedRow(loadedRow2);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            return observableDataSet;
        }
        #endregion

        [Fact]
        public void RowsRemoved_WhenTableRowIsRemoved_ViewUpdatesAndRaisesEvent()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSetWithTwoLoadedRows();
            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");

            RowsChangedCounter removedCounter = new RowsChangedCounter();
            view.RowsRemoved += removedCounter.Handler;

            Assert.Equal(2, view.Rows.Count);

            // Act
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            observableTable.RemoveRow(0);

            // Assert
            Assert.Equal(1, removedCounter.Count);
            Assert.Equal(1, view.Rows.Count);
            Assert.Equal(2, (int)view.Rows[0]["Id"]);
        }

        [Fact]
        public void MultipleViewsOverSameTable_UpdateAndRemove_AffectBothViews_AndDisposingOneDoesNotBreakTheOther()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSetWithTwoLoadedRows();

            IObservableDataView? viewA = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");
            IObservableDataView? viewB = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenB");

            Assert.NotNull(viewA);
            Assert.NotNull(viewB);

            DataFieldValueChangedCounter viewAChanged = new DataFieldValueChangedCounter();
            DataFieldValueChangedCounter viewBChanged = new DataFieldValueChangedCounter();
            RowsChangedCounter viewARemoved = new RowsChangedCounter();
            RowsChangedCounter viewBRemoved = new RowsChangedCounter();

            viewA!.DataFieldValueChanged += viewAChanged.Handler;
            viewB!.DataFieldValueChanged += viewBChanged.Handler;
            viewA.RowsRemoved += viewARemoved.Handler;
            viewB.RowsRemoved += viewBRemoved.Handler;

            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            Assert.Equal(2, viewA.Rows.Count);
            Assert.Equal(2, viewB.Rows.Count);

            // Act 1: update a row value
            IObservableDataRow row0 = observableTable.Rows[0];
            row0["Name"] = "Marketing";

            // Assert 1: both views forward the row event
            Assert.Equal(1, viewAChanged.Count);
            Assert.Equal(1, viewBChanged.Count);

            // Act 2: remove a row from table
            observableTable.RemoveRow(0);

            // Assert 2: both views update and raise RowsRemoved once
            Assert.Equal(1, viewARemoved.Count);
            Assert.Equal(1, viewBRemoved.Count);
            Assert.Equal(1, viewA.Rows.Count);
            Assert.Equal(1, viewB.Rows.Count);

            // Act 3: dispose only viewA, then update remaining row
            viewA.Dispose();
            IObservableDataRow remainingRow = observableTable.Rows[0];
            remainingRow["Name"] = "Finance";

            // Assert 3: viewA no longer receives row events, viewB still works
            Assert.Equal(1, viewAChanged.Count);
            Assert.Equal(2, viewBChanged.Count);
        }

        [Fact]
        public void Dispose_WhenViewDisposed_StopsReceivingRowEvents()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys.Add("Id");

            DataRow loadedRow = new DataRow();
            loadedRow["Id"] = 1;
            loadedRow["Name"] = "Sales";
            ((DataTable)table).AddLoadedRow(loadedRow);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenB");

            DataFieldValueChangedCounter counter = new DataFieldValueChangedCounter();
            view.DataFieldValueChanged += counter.Handler;

            // Act
            view.Dispose();

            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            IObservableDataRow row = observableTable.Rows[0];
            row["Name"] = "Marketing";

            // Assert
            Assert.Equal(0, counter.Count);
        }

        [Fact]
        public void Filter_NameEquals_ReturnsOnlyMatchingRows_AndRowsAddedThatMatchAreIncluded()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSetWithTwoLoadedRows();

            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", "Name = 'Sales'", false, null, "ScreenFilter");
            Assert.NotNull(view);
            Assert.Single(view!.Rows);
            Assert.Equal(1, (int)view.Rows[0]["Id"]);

            RowsChangedCounter addedCounter = new RowsChangedCounter();
            view.RowsAdded += addedCounter.Handler;

            // Act: add a new matching row
            DataRow newRow = new DataRow();
            newRow["Id"] = 3;
            newRow["Name"] = "Sales";
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            observableTable.AddRow(newRow);

            // Assert
            Assert.Equal(1, addedCounter.Count);
            Assert.Equal(2, view.Rows.Count);
        }

        [Fact]
        public void Filter_CaseSensitivity_AffectsMatching()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSetWithTwoLoadedRows();

            // Act
            IObservableDataView? insensitive = observableDataSet.GetObservableDataView("Department", "Name = 'sales'", false, null, "CaseInsensitive");
            IObservableDataView? sensitive = observableDataSet.GetObservableDataView("Department", "Name = 'sales'", true, null, "CaseSensitive");

            // Assert
            Assert.NotNull(insensitive);
            Assert.NotNull(sensitive);

            Assert.Single(insensitive!.Rows);
            Assert.Empty(sensitive!.Rows);
        }

        [Fact]
        public void SortExpression_ChangesOrderDeterministically()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys.Add("Id");

            DataRow r1 = new DataRow();
            r1["Id"] = 1;
            r1["Name"] = "B";

            DataRow r2 = new DataRow();
            r2["Id"] = 2;
            r2["Name"] = "A";

            DataRow r3 = new DataRow();
            r3["Id"] = 3;
            r3["Name"] = "C";

            ((DataTable)table).AddLoadedRow(r1);
            ((DataTable)table).AddLoadedRow(r2);
            ((DataTable)table).AddLoadedRow(r3);

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            // Act
            IObservableDataView? asc = observableDataSet.GetObservableDataView("Department", null, false, "Name ASC", "SortAsc");
            IObservableDataView? desc = observableDataSet.GetObservableDataView("Department", null, false, "Name DESC", "SortDesc");

            // Assert
            Assert.NotNull(asc);
            Assert.NotNull(desc);

            Assert.Equal("A", (string)asc!.Rows[0]["Name"]);
            Assert.Equal("B", (string)asc.Rows[1]["Name"]);
            Assert.Equal("C", (string)asc.Rows[2]["Name"]);

            Assert.Equal("C", (string)desc!.Rows[0]["Name"]);
            Assert.Equal("B", (string)desc.Rows[1]["Name"]);
            Assert.Equal("A", (string)desc.Rows[2]["Name"]);
        }

        [Fact]
        public void Filter_DoesNotAutoReevaluateOnRowValueChange_CurrentBehavior()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSetWithTwoLoadedRows();
            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", "Name = 'Sales'", false, null, "ScreenFilterBehavior");

            Assert.NotNull(view);
            Assert.Single(view!.Rows);

            RowsChangedCounter addedCounter = new RowsChangedCounter();
            view.RowsAdded += addedCounter.Handler;

            // Act: update a row so it would match the filter if re-evaluated
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];
            IObservableDataRow hrRow = observableTable.Rows[1];
            hrRow["Name"] = "Sales";

            // Assert: view still has only the original matching row (no automatic re-evaluation)
            Assert.Single(view.Rows);
            Assert.Equal(0, addedCounter.Count);
        }

        [Fact]
        public void RemoveObservableDataViewsForRequestor_RemovesAllCachedViewsForThatRequestor()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable department = dataSet.AddNewTable("Department");
            department.AddColumn("Id", DataTypeNames.INT32);
            department.AddColumn("Name", DataTypeNames.STRING);
            department.PrimaryKeys.Add("Id");

            IDataTable employee = dataSet.AddNewTable("Employee");
            employee.AddColumn("Id", DataTypeNames.INT32);
            employee.AddColumn("DepartmentId", DataTypeNames.INT32);
            employee.AddColumn("Name", DataTypeNames.STRING);
            employee.PrimaryKeys.Add("Id");

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);

            IObservableDataView? deptView1 = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenC");
            IObservableDataView? empView1 = observableDataSet.GetObservableDataView("Employee", null, false, null, "ScreenC");

            // Act
            int removedCount = observableDataSet.RemoveObservableDataViewsForRequestor("ScreenC");

            // Assert
            Assert.True(removedCount > 0);

            IObservableDataView? deptView2 = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenC");
            IObservableDataView? empView2 = observableDataSet.GetObservableDataView("Employee", null, false, null, "ScreenC");

            Assert.False(ReferenceEquals(deptView1, deptView2));
            Assert.False(ReferenceEquals(empView1, empView2));
        }
    }
}
