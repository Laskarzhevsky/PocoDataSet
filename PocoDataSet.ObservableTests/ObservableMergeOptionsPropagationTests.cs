using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    /// <summary>
    /// Observable counterpart of MergeOptionsPropagationTests.
    /// Ensures the same ObservableMergeOptions instance (and result instance) flows through the merge chain.
    /// </summary>
    public class ObservableMergeOptionsPropagationTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotReplace_ObservableDataSetMergeResult_Instance()
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
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Same(expectedResultInstance, options.ObservableDataSetMergeResult);
            Assert.Equal("One_Refreshed", (string)current.Tables["T"].Rows[0]["Name"]!);
        }

        [Fact]
        public void ReplaceMerge_DoesNotReplace_ObservableDataSetMergeResult_Instance()
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
