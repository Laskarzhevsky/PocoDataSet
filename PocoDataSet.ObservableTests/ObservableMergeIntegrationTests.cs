using System;
using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableMergeIntegrationTests
    {
        #region Public Methods
        [Fact]
        public void MergeWith_WhenRowUpdatedDeletedAndAdded_ViewStaysConsistent_AndRaisesExpectedEvents()
        {
            // Arrange: current observable data set with 2 rows (1=Sales, 2=HR)
            ObservableDataSet currentObservableDataSet = CreateCurrentObservableDepartmentDataSet();
            IObservableDataView? view = currentObservableDataSet.GetObservableDataView("Department", null, false, "Id ASC", "ScreenA");
            Assert.NotNull(view);

            RowsChangedCounter rowsAddedCounter = new RowsChangedCounter();
            RowsChangedCounter rowsRemovedCounter = new RowsChangedCounter();
            DataFieldValueChangedCounter fieldChangedCounter = new DataFieldValueChangedCounter();

            view.RowsAdded += rowsAddedCounter.Handler;
            view.RowsRemoved += rowsRemovedCounter.Handler;
            view.DataFieldValueChanged += fieldChangedCounter.Handler;

            // Refreshed data set:
            // - Id=1 updated Name
            // - Id=2 deleted (missing)
            // - Id=3 added
            IDataSet refreshed = CreateRefreshedDepartmentDataSet();

            // Act
            currentObservableDataSet.MergeWith(refreshed);

            // Assert: view sees 2 rows (Ids 1 and 3)
            Assert.Equal(2, view.Rows.Count);
            Assert.Equal(1, view.Rows[0].GetDataFieldValue<int>("Id"));
            Assert.Equal("SalesUpdated", view.Rows[0].GetDataFieldValue<string>("Name"));
            Assert.Equal(3, view.Rows[1].GetDataFieldValue<int>("Id"));

            // Events: 1 row removed, 1 row added, 1 field change (Name on Id=1)
            Assert.Equal(1, rowsRemovedCounter.Count);
            Assert.Equal(1, rowsAddedCounter.Count);
            Assert.Equal(1, fieldChangedCounter.Count);
        }
        #endregion

        #region Private Helpers
        static ObservableDataSet CreateCurrentObservableDepartmentDataSet()
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            // Observable merge relies on the client-only key column.
            table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);
            table.PrimaryKeys.Add("Id");

            // Use deterministic client keys so the refreshed data set can reference the same key.
            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid clientKey2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            DataRow row1 = new DataRow();
            row1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            row1["Id"] = 1;
            row1["Name"] = "Sales";
            table.AddRow(row1);

            DataRow row2 = new DataRow();
            row2[SpecialColumnNames.CLIENT_KEY] = clientKey2;
            row2["Id"] = 2;
            row2["Name"] = "HR";
            table.AddRow(row2);

            // Treat these as loaded rows
            table.AcceptChanges();

            return new ObservableDataSet(dataSet);
        }

        static IDataSet CreateRefreshedDepartmentDataSet()
        {
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);
            refreshedTable.PrimaryKeys.Add("Id");

            // Keep the same client key for Id=1 so merge treats it as an update.
            // The value is copied from the current data set row by deterministic generation in this test.
            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");

            DataRow refreshedRow1 = new DataRow();
            refreshedRow1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            refreshedRow1["Id"] = 1;
            refreshedRow1["Name"] = "SalesUpdated";
            refreshedTable.AddRow(refreshedRow1);

            DataRow refreshedRow3 = new DataRow();
            refreshedRow3[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("33333333-3333-3333-3333-333333333333");
            refreshedRow3["Id"] = 3;
            refreshedRow3["Name"] = "IT";
            refreshedTable.AddRow(refreshedRow3);

            refreshedTable.AcceptChanges();

            return refreshed;
        }
        #endregion
    }
}
