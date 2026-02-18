using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional Replace schema contract tests: - Replace preserves current column metadata (PK
    /// flag, nullability, max length). - Refreshed column metadata does not overwrite current
    /// schema.
    ///
    /// Scenario:
    /// - Build a CURRENT DataSet (the client-side truth before the merge).
    /// - Build a REFRESHED/CHANGESET DataSet (the server-side snapshot or post-save response).
    /// - Execute Replace merge (replace rows while enforcing current schema contract).
    /// How the test proves the contract:
    /// - Arrange sets up schema + row states to trigger the behavior.
    /// - Act runs the merge using MergeOptions.
    /// - Assert verifies final data and invariants (row instances, row state, and merge result entries).
    /// Notes:
    /// - This file contains exactly one test method: Keeps_PrimaryKeyFlag.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Keeps_PrimaryKeyFlag()
        {
            // Arrange
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable ct = current.AddNewTable("T");
            IColumnMetadata currentId = ct.AddColumn("Id", DataTypeNames.INT32, false, true);
            ct.AddColumn("Name", DataTypeNames.STRING);

            IDataRow existing = ct.AddNewRow();
            existing["Id"] = 1;
            existing["Name"] = "Old";
            current.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            // Refreshed "Id" is NOT marked as PK (different from current)
            IColumnMetadata refreshedId = rt.AddColumn("Id", DataTypeNames.INT32, false, false);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = rt.AddNewRow();
            r1["Id"] = 10;
            r1["Name"] = "New";

            // Act
            MergeOptions options = new MergeOptions();
            // Execute Replace merge: replace rows using the refreshed snapshot, while enforcing the current schema contract.
            current.DoReplaceMerge(refreshed, options);

            // Assert: current schema stays authoritative
            IColumnMetadata actualId = GetColumn(ct, "Id");
            Assert.True(actualId.IsPrimaryKey);

            // Sanity: refreshed differs (to ensure test is meaningful)
            Assert.False(refreshedId.IsPrimaryKey);

            // And rows were replaced
            Assert.Single(ct.Rows);
            Assert.Equal(10, (int)ct.Rows[0]["Id"]!);
        }
    }
}
