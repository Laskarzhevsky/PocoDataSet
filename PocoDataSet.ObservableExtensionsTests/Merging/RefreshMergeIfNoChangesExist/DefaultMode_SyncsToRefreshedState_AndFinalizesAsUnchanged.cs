using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void DefaultMode_SyncsToRefreshedState_AndFinalizesAsUnchanged()
        {
            // Arrange
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            IObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentDept = current.AddNewTable("Department");
            currentDept.AddColumn("Id", DataTypeNames.INT32, false, true);
            currentDept.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = currentDept.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1.AcceptChanges();

            // Refreshed dataset: update Id=1, add Id=2, omit Id=99 (deletion)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedDept = refreshed.AddNewTable("Department");
            refreshedDept.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedDept.AddColumn("Name", DataTypeNames.STRING);

            IDataRow s1 = refreshedDept.AddNewRow();
            s1["Id"] = 1;
            s1["Name"] = "Sales - Server";

            IDataRow s2 = refreshedDept.AddNewRow();
            s2["Id"] = 2;
            s2["Name"] = "Engineering";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            currentDept.DoRefreshMergeIfNoChangesExist(refreshedDept, options);

            // Assert
            Assert.Equal(2, currentDept.Rows.Count);

            IObservableDataRow found1 = MergeTestingHelpers.FindById(currentDept, 1);
            Assert.Equal("Sales - Server", (string)found1["Name"]!);
            Assert.Equal(DataRowState.Unchanged, found1.InnerDataRow.DataRowState);

            IObservableDataRow found2 = MergeTestingHelpers.FindById(currentDept, 2);
            Assert.Equal("Engineering", (string)found2["Name"]!);
            Assert.Equal(DataRowState.Unchanged, found2.InnerDataRow.DataRowState);
        }
    }
}
