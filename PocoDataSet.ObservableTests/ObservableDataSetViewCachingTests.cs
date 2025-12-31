using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableDataSetViewCachingTests
    {
        #region Public Methods
        [Fact]
        public void GetObservableDataView_SameTableAndRequestor_ReturnsSameInstance_EvenIfFilterOrSortDiffers()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSet();

            // Act
            IObservableDataView? view1 = observableDataSet.GetObservableDataView("Department", "Name = 'Sales'", false, "Name ASC", "ScreenA");
            IObservableDataView? view2 = observableDataSet.GetObservableDataView("Department", "Name = 'HR'", false, "Name DESC", "ScreenA");

            // Assert
            Assert.NotNull(view1);
            Assert.NotNull(view2);
            Assert.Same(view1, view2);
        }

        [Fact]
        public void GetObservableDataView_DifferentRequestors_ReturnsDifferentInstances()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSet();

            // Act
            IObservableDataView? viewA = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");
            IObservableDataView? viewB = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenB");

            // Assert
            Assert.NotNull(viewA);
            Assert.NotNull(viewB);
            Assert.NotSame(viewA, viewB);
        }

        [Fact]
        public void RemoveObservableDataView_AfterRemoval_NewCallReturnsNewInstance()
        {
            // Arrange
            ObservableDataSet observableDataSet = CreateObservableDepartmentDataSet();

            IObservableDataView? oldView = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");
            Assert.NotNull(oldView);

            // Act
            bool removed = observableDataSet.RemoveObservableDataView("Department", "ScreenA");
            IObservableDataView? newView = observableDataSet.GetObservableDataView("Department", null, false, null, "ScreenA");

            // Assert
            Assert.True(removed);
            Assert.NotNull(newView);
            Assert.NotSame(oldView, newView);
        }
        #endregion

        #region Private Helpers
        static ObservableDataSet CreateObservableDepartmentDataSet()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys.Add("Id");

            DataRow row1 = new DataRow();
            row1["Id"] = 1;
            row1["Name"] = "Sales";
            table.AddRow(row1);

            DataRow row2 = new DataRow();
            row2["Id"] = 2;
            row2["Name"] = "HR";
            table.AddRow(row2);

            return new ObservableDataSet(dataSet);
        }
        #endregion
    }
}
