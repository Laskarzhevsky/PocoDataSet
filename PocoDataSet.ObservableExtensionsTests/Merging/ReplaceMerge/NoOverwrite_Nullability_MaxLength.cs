using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public partial class ReplaceMerge
    {
  
        /// <summary>
        /// Verifies DoesNotOverwrite Nullability And MaxLength in ReplaceMerge merge.
        ///
        /// How the test proves this:
        /// 1) Arrange: build a current observable table/data set and a refreshed (server) changeset that triggers the scenario.
        /// 2) Act: execute MergeWith(...) using the merge mode under test.
        /// 3) Assert: validate the observable row instances/state and that result semantics match the contract.
        /// </summary>
        [Fact]
        public void NoOverwrite_Nullability_MaxLength()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable table = current.AddNewTable("T");

            IColumnMetadata columnMetadata = table.AddColumn("Name", DataTypeNames.STRING, false, false);
            columnMetadata.MaxLength = 10;

            IObservableDataRow r = table.AddNewRow();
            r["Name"] = "Current";
            r.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            IColumnMetadata rtColumnMetadata = rt.AddColumn("Name", DataTypeNames.STRING, true, false);
            rtColumnMetadata.MaxLength = 100;

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr["Name"] = "Refreshed";
            rt.AddLoadedRow(rr);

            IObservableMergeOptions options = new ObservableMergeOptions();

            current.DoReplaceMerge(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.False(result.Columns[0].IsNullable);
            Assert.Equal(10, result.Columns[0].MaxLength);
        }
    }
}