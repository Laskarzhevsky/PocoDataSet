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
        public void DoesNotThrow_WhenCurrentHasNoPendingChanges()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();
            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One_Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Equal(1, current.Tables["T"].Rows.Count);
            Assert.Equal("One_Refreshed", current.Tables["T"].Rows[0]["Name"]);
            Assert.Equal(DataRowState.Unchanged, current.Tables["T"].Rows[0].DataRowState);
        }
    }
}
