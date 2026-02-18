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
        public void Throws_WhenCurrentHasModifiedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();

            // Modify a loaded row to make the table dirty.
            current.Tables["T"].Rows[0]["Name"] = "One_Modified";
            Assert.Equal(DataRowState.Modified, current.Tables["T"].Rows[0].DataRowState);

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }
    }
}
