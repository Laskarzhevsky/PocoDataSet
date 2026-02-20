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
        public void DoesNotOverwrite_PrimaryKeyFlag()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable table = current.AddNewTable("T");

            table.AddColumn("Id", DataTypeNames.INT32, false, true);
            table.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r = table.AddNewRow();
            r["Id"] = 1;
            r["Name"] = "Current";
            r.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, false);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr["Id"] = 2;
            rr["Name"] = "Refreshed";
            rt.AddLoadedRow(rr);

            IObservableMergeOptions options = new ObservableMergeOptions();

            current.DoReplaceMerge(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.True(result.Columns[0].IsPrimaryKey);
        }
    }
}
