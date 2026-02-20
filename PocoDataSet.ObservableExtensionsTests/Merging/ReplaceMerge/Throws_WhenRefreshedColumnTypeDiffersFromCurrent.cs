using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    /// <summary>
    /// Replace schema contract matrix (Observable, Policy A - industry common):
    /// - Current schema is authoritative.
    /// - Refreshed extra columns are ignored (current must not gain them).
    /// - Refreshed missing columns do not remove current columns; values become null/default on replaced rows.
    /// - Incompatible column type changes are rejected (throw).
    ///
    /// NOTE: Observable tables may contain internal/special columns in addition to user-defined columns,
    /// so these tests do NOT assert total column counts.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Throws_WhenRefreshedColumnTypeDiffersFromCurrent()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Amount", DataTypeNames.INT32);

            IObservableDataRow c = t.AddNewRow();
            c["Id"] = 1;
            c["Amount"] = 10;
            c.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Amount", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Amount"] = "NOT_AN_INT";
            rt.AddLoadedRow(r);

            IObservableMergeOptions options = new ObservableMergeOptions();

            Assert.Throws<InvalidOperationException>(() => current.DoReplaceMerge(refreshed, options));
        }
    }
}
