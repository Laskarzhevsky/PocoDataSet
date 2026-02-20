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
        public void Throws_WhenRefreshedTableHasDuplicatePrimaryKeys()
        {
            // Arrange
            IDataSet currentInnerDataSet = DataSetFactory.CreateDataSet();
            IObservableDataSet currentObservableDataSet = new ObservableDataSet(currentInnerDataSet);

            IObservableDataTable currentTable = currentObservableDataSet.AddNewTable("Department");
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow row = currentTable.AddNewRow();
            row["Id"] = 1;
            row["Name"] = "Engineering";

            // Refreshed dataset with DUPLICATE PK rows (Id=1 appears twice)
            IDataSet refreshedDataSet = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshedDataSet.AddNewTable("Department");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Engineering";

            IDataRow r2 = refreshedTable.AddNewRow();
            r2["Id"] = 1; // duplicate
            r2["Name"] = "Engineering Duplicate";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act + Assert
            Assert.Throws<InvalidOperationException>(() =>
                currentObservableDataSet.DoRefreshMergePreservingLocalChanges(refreshedDataSet, options));
        }
    }
}
