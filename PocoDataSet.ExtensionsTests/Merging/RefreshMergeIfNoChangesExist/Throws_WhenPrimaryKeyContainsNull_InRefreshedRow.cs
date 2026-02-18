using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional coverage for RefreshIfNoChangesExist after the "no MergeMode / no policies" refactor.
    /// Focus: dirty-detection matrix + PK-null behavior lock-in.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void Throws_WhenPrimaryKeyContainsNull_InRefreshedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow refreshedRow = DataRowExtensions.CreateRowFromColumns(t.Columns);
            refreshedRow["Id"] = null;
            refreshedRow["Name"] = "Bad";
            t.AddLoadedRow(refreshedRow);

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
