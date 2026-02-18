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
        public void Throws_WhenCurrentHasDeletedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            IDataRow row = current.Tables["T"].Rows[0];
            current.Tables["T"].DeleteRow(row);
            Assert.Equal(DataRowState.Deleted, row.DataRowState);

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }
    }
}
