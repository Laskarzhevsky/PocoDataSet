using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests.ObservableDataSetExtensions
{
    public partial class ObservableDataSetExtensionsTests
    {
        [Fact]
        public void MergeWith_RefreshMode()
        {
            // Arrange
            // 1. Create a current POCO data set and wrap it into an observable data set
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");
            currentDepartment.AddColumn("Id", DataTypeNames.INT32);
            currentDepartment.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow currentRow = currentDepartment.AddNewRow();
            currentRow["Id"] = 1;
            currentRow["Name"] = "Sales";
            currentRow.AcceptChanges(); // put row into Unchanged state

            // 2. Create a refreshed data set returned from database / service (non-observable)
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedDepartment = refreshedDataSet.AddNewTable("Department");
            refreshedDepartment.AddColumn("Id", DataTypeNames.INT32);
            refreshedDepartment.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedDepartment.AddNewRow();
            refreshedRow["Id"] = 1;
            refreshedRow["Name"] = "Sales and Marketing";

            // Act
            // 3. Merge refreshed values into the current observable data set (Refresh mode)
            IObservableMergeOptions observableMergeOptions = new ObservableMergeOptions();
            observableMergeOptions.MergeMode = MergeMode.Refresh;

            IObservableDataSetMergeResult mergeResult = currentObservableDataSet.MergeWith(refreshedDataSet, observableMergeOptions);

            // Assert
            // 4. Current observable row now has refreshed values "Sales and Marketing"
            string? name = (string?)currentDepartment.Rows[0]["Name"];

            Assert.Equal("Sales and Marketing", name);
        }

        [Fact]
        public void MergeWith_PostSaveMode_PropagatesIdentity_UsingClientKeyCorrelation()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentDepartment = currentObservableDataSet.AddNewTable("Department");

            // Id is the PK (design requirement)
            currentDepartment.AddColumn("Id", DataTypeNames.INT32, false, true);

            // Correlation only (not PK)
            currentDepartment.AddColumn("__ClientKey", DataTypeNames.GUID);

            currentDepartment.AddColumn("Name", DataTypeNames.STRING);
            currentDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            Guid clientKey = Guid.NewGuid();

            IObservableDataRow newRow = currentDepartment.AddNewRow();
            newRow["__ClientKey"] = clientKey;
            newRow["Name"] = "Engineering";
            // newRow["Id"] is 0 here

            // Post-save dataset
            IDataSet postSaveDataSet = DataSetFactory.CreateDataSet();
            IDataTable postSaveDepartment = postSaveDataSet.AddNewTable("Department");

            // Same PK
            postSaveDepartment.AddColumn("Id", DataTypeNames.INT32, false, true);

            // Same correlation column
            postSaveDepartment.AddColumn("__ClientKey", DataTypeNames.GUID);

            postSaveDepartment.AddColumn("Name", DataTypeNames.STRING);
            postSaveDepartment.AddColumn("RowVersion", DataTypeNames.BINARY);

            IDataRow savedRow = postSaveDepartment.AddNewRow();
            savedRow["Id"] = 10;
            savedRow["__ClientKey"] = clientKey;
            savedRow["Name"] = "Engineering";
            savedRow["RowVersion"] = new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 };

            // Act
            IObservableMergeOptions options = new ObservableMergeOptions();
            options.MergeMode = MergeMode.PostSave;

            currentObservableDataSet.MergeWith(postSaveDataSet, options);

            // Assert
            // PostSave merge must NOT add a second row; it must correlate and update.
            Assert.Equal(1, currentObservableDataSet.Tables["Department"].Rows.Count);

            IObservableDataRow? matched = null;

            foreach (IObservableDataRow row in currentObservableDataSet.Tables["Department"].Rows)
            {
                object? keyValue = row["__ClientKey"];
                if (keyValue is Guid g && g == clientKey)
                {
                    matched = row;
                    break;
                }
            }

            Assert.NotNull(matched);
            Assert.Equal(10, (int)matched!["Id"]!);

            byte[] rv = (byte[])matched["RowVersion"]!;
            Assert.Equal(new byte[] { 1, 0, 0, 0, 0, 0, 0, 0 }, rv);
        }
    }
}
