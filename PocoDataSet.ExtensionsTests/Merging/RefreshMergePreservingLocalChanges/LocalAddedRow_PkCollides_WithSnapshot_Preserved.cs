using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies the behavior when a **locally Added row has a PK that collides with a row in the refreshed snapshot**.
        ///
        /// Why this matters:
        /// - Clients sometimes assign temporary identifiers or reuse identifiers in disconnected workflows.
        /// - When the snapshot contains the same PK, the merge must be deterministic.
        ///
        /// Observed contract locked by this test:
        /// - In RefreshPreservingLocalChanges mode, **locally Added rows are preserved** and are not overwritten by the
        ///   snapshot, even if the snapshot contains a row with the same PK.
        ///
        /// How the test proves the contract:
        /// 1) CURRENT contains a single locally Added row (Id=1, Name='LOCAL').
        /// 2) REFRESHED contains a loaded row with the same PK (Id=1, Name='SERVER').
        /// 3) Act: merge.
        /// 4) Assert: CURRENT still has one row, it remains Added, and 'LOCAL' value is preserved.
        /// </summary>
        [Fact]
        public void LocalAddedRow_PkCollides_WithSnapshot_Preserved()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow local = DataRowExtensions.CreateRowFromColumns(t.Columns);
            local["Id"] = 1;
            local["Name"] = "LOCAL";
            t.AddRow(local);

            Assert.Equal(DataRowState.Added, t.Rows[0].DataRowState);

            // Arrange (REFRESHED) - same PK exists on server
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow server = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            server["Id"] = 1;
            server["Name"] = "SERVER";
            rt.AddLoadedRow(server);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            // Local Added row remains and is not overwritten.
            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Added, t.Rows[0].DataRowState);
            Assert.Equal("LOCAL", t.Rows[0]["Name"]);
        }
    }
}
