using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        [Fact]
        public void Throws_WhenNoPrimaryKeyDefined()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            // IMPORTANT: no "Id" column, because "Id" implies PK by convention
            IObservableDataTable currentTable = currentObservableDataSet.AddNewTable("Department");
            currentTable.AddColumn("Code", DataTypeNames.INT32); // NOT PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow row = currentTable.AddNewRow();
            row["Code"] = 100;
            row["Name"] = "Engineering";

            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshedDataSet.AddNewTable("Department");
            refreshedTable.AddColumn("Code", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = refreshedTable.AddNewRow();
            refreshedRow["Code"] = 100;
            refreshedRow["Name"] = "Engineering";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options));
        }
    }
}
