
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public sealed class ObservableReplaceSchemaPolicyAdditionalTests
    {
        [Fact]
        public void Replace_DoesNotOverwrite_PrimaryKeyFlag()
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

        [Fact]
        public void Replace_DoesNotOverwrite_Nullability_And_MaxLength()
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
