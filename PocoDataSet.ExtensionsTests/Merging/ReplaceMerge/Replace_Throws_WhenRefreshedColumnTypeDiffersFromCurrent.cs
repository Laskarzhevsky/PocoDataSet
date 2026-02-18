using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace schema contract matrix (Policy A - industry common):
    /// - Current schema is authoritative.
    /// - Refreshed extra columns are ignored.
    /// - Refreshed missing columns do not remove current columns; values for missing columns become null/default on replaced rows.
    /// - Incompatible column type changes are rejected (throw) to avoid silent corruption.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Replace_Throws_WhenRefreshedColumnTypeDiffersFromCurrent()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Amount", DataTypeNames.INT32);

            IDataRow c = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c["Id"] = 1;
            c["Amount"] = 10;
            t.AddLoadedRow(c);

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32, false, true);

            // Refreshed column type differs: string instead of int
            rt.AddColumn("Amount", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Amount"] = "NOT_AN_INT";
            rt.AddLoadedRow(r);

            MergeOptions options = new MergeOptions();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => current.DoReplaceMerge(refreshed, options));
        }
    }
}
