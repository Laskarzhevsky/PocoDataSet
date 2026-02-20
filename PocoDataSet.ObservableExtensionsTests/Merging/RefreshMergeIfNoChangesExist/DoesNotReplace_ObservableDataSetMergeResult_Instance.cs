using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Options propagation coverage for RefreshIfNoChangesExist (observable).
    /// Locks that merge does not replace ObservableMergeOptions.ObservableDataSetMergeResult instance.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void DoesNotReplace_ObservableDataSetMergeResult_Instance()
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

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr1["Id"] = 1;
            rr1["Name"] = "One_Refreshed";
            rt.AddLoadedRow(rr1);

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult expectedResultInstance = options.ObservableDataSetMergeResult;

            // Act
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.ObservableDataSetMergeResult);
            Assert.Equal("One_Refreshed", (string)current.Tables["T"].Rows[0]["Name"]!);
        }
    }
}
