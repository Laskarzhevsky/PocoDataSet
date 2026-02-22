using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Locks the PostSave behavior for an **uncorrelatable** server changeset row.
    ///
    /// PostSave correlates by PK-first (when PK is present and non-empty), then by __ClientKey.
    /// If the server sends a row that cannot be matched to any CURRENT row (PK does not exist in current
    /// index and __ClientKey is absent), current implementation adds a new row to CURRENT and applies
    /// the server values to that newly created row.
    ///
    /// This is a critical behavior to lock because it is easy to "optimize" later and accidentally:
    /// - overwrite an unrelated CURRENT Added row, or
    /// - silently drop the server row.
    ///
    /// Scenario:
    /// - CURRENT has an Added row with a temporary PK (-1) and no __ClientKey column at all.
    /// - SERVER changeset contains an Added row with a real PK (100) but no __ClientKey.
    /// - There is no safe way to correlate the rows (PK differs, no client key).
    ///
    /// Expected behavior (current contract):
    /// - PostSave does NOT modify the existing CURRENT Added row.
    /// - PostSave adds a NEW row to CURRENT for the server row, applies values, and accepts it.
    /// - MergeResult records the new row under AddedDataRows.
    ///
    /// How this test proves it:
    /// - Arrange builds CURRENT with exactly one Added row.
    /// - Arrange builds a SERVER changeset with a different PK and Added state.
    /// - Act runs PostSave merge.
    /// - Assert verifies:
    ///   (1) CURRENT now has 2 rows,
    ///   (2) the original Added row is still there and still Added,
    ///   (3) the new server row exists and is Unchanged,
    ///   (4) MergeResult.AddedDataRows contains exactly one entry for the new row.
    /// </summary>
    public partial class PostSaveMerge
    {
        [Fact]
        public void UnmatchedServerRow_Added()
        {
            // Arrange: CURRENT contains a local Added row that has NOT been correlated (no __ClientKey column).
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow localRow = new DataRow();
            localRow["Id"] = -1;          // temporary client identity
            localRow["Name"] = "Local";
            t.AddRow(localRow);

            Assert.Equal(1, t.Rows.Count);
            Assert.Equal(DataRowState.Added, localRow.DataRowState);

            // Arrange: SERVER changeset returns a different row (no __ClientKey, different PK).
            IDataSet changeset = DataSetFactory.CreateDataSet();
            IDataTable ct = changeset.AddNewTable("Department");
            ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow serverRow = new DataRow();
            serverRow["Id"] = 100;
            serverRow["Name"] = "Server";
            ct.AddRow(serverRow);

            // Server row must be Added (PostSave processes Added/Modified/Deleted only).
            Assert.Equal(DataRowState.Added, serverRow.DataRowState);

            MergeOptions options = new MergeOptions();

            // Act: apply server PostSave changeset onto CURRENT.
            current.DoPostSaveMerge(changeset, options);

            // Assert: because there was no match, a NEW row was added.
            Assert.Equal(2, t.Rows.Count);

            // Assert: the original local row is untouched.
            Assert.Equal(-1, localRow["Id"]);
            Assert.Equal("Local", localRow["Name"]);
            Assert.Equal(DataRowState.Added, localRow.DataRowState);

            // Assert: the server row exists in CURRENT as a new accepted row.
            // Find the row with PK=100.
            IDataRow? persisted = null;
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["Id"] == 100)
                {
                    persisted = t.Rows[i];
                    break;
                }
            }

            Assert.NotNull(persisted);
            Assert.Equal("Server", persisted!["Name"]);
            Assert.Equal(DataRowState.Unchanged, persisted!.DataRowState);

            // Assert: MergeResult records the added row.
            Assert.Equal(1, options.DataSetMergeResult.AddedDataRows.Count);
            Assert.Equal("Department", options.DataSetMergeResult.AddedDataRows[0].TableName);
            Assert.Same(persisted, options.DataSetMergeResult.AddedDataRows[0].DataRow);
        }
    }
}
