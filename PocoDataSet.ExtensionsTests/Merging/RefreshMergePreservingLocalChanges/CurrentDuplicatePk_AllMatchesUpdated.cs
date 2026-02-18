using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies the CURRENT observed behavior when the CURRENT table already contains **duplicate primary keys**.
        ///
        /// Why this matters:
        /// - While duplicate PKs are generally invalid, corrupted client state can happen.
        /// - The merge must be deterministic (not crash, not randomly pick different rows on different runs).
        ///
        /// Observed contract locked by this test:
        /// - RefreshPreservingLocalChanges does **not throw** when CURRENT contains duplicate PKs.
        /// - Correlation is deterministic: the refresh applies to **all** CURRENT rows that match the refreshed PK.
        ///   (Both duplicates are updated to the snapshot values.)
        ///
        /// How the test proves the contract:
        /// 1) CURRENT contains two loaded rows with the same PK (Id=1) but different values.
        /// 2) REFRESHED contains a single row with Id=1 and a new Name.
        /// 3) Act: merge.
        /// 4) Assert: merge does not throw; both CURRENT duplicates are updated to the snapshot value.
        /// </summary>
        [Fact]
        public void CurrentDuplicatePk_AllMatchesUpdated()
        {
            // Arrange (CURRENT) - duplicate PK rows
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r1["Id"] = 1;
            r1["Name"] = "FIRST";
            t.AddLoadedRow(r1);

            IDataRow r2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r2["Id"] = 1;
            r2["Name"] = "SECOND";
            t.AddLoadedRow(r2);

            // Arrange (REFRESHED)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow rr = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            rr["Id"] = 1;
            rr["Name"] = "SERVER";
            rt.AddLoadedRow(rr);

            // Act (must not throw)
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            // Assert
            Assert.Equal(2, t.Rows.Count);

            // Both duplicates updated to the snapshot value
            Assert.Equal("SERVER", t.Rows[0]["Name"]);

            Assert.Equal("SERVER", t.Rows[1]["Name"]);
        }
    }
}