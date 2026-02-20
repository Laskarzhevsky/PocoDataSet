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
    /// Observable counterpart of MergeOptionsPropagationTests.
    /// Ensures the same ObservableMergeOptions instance (and result instance) flows through the merge chain.
    /// </summary>
    public partial class ReplaceMerge
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

            IDataRow rr2 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr2["Id"] = 2;
            rr2["Name"] = "Two";
            rt.AddLoadedRow(rr2);

            ObservableMergeOptions options = new ObservableMergeOptions();
            IObservableDataSetMergeResult expectedResultInstance = options.ObservableDataSetMergeResult;

            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.ObservableDataSetMergeResult);
            Assert.Single(current.Tables["T"].Rows);
            Assert.Equal(2, current.Tables["T"].Rows[0]["Id"]);
        }
    }
}
