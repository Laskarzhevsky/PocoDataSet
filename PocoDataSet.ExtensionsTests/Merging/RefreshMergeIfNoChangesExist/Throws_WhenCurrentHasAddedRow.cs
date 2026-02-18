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
        public void Throws_WhenCurrentHasAddedRow()
        {
            IDataSet current = BuildCurrentWithPrimaryKey();
            IDataRow added = current.Tables["T"].AddNewRow();
            added["Id"] = 2;
            added["Name"] = "Added";

            IDataSet refreshed = BuildRefreshedWithSameSchemaAndRow(1, "One");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergeIfNoChangesExist(refreshed, options));
        }
    }
}
