using System;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional coverage for RefreshIfNoChangesExist after the "no MergeMode / no policies"
    /// refactor. Focus: dirty-detection matrix + PK-null behavior lock-in.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute RefreshIfNoChangesExist merge (throws if current has pending changes).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: Throws_WhenRefreshedPkContainsNull.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        [Fact]
        public void Throws_WhenRefreshedPkContainsNull()
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
            // Execute RefreshIfNoChangesExist merge: this mode only allows refresh when CURRENT has no pending local changes.
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
