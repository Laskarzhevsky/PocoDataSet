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
  
        [Fact]
        public void DoesNotOverwrite_Nullability_And_MaxLength()
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
