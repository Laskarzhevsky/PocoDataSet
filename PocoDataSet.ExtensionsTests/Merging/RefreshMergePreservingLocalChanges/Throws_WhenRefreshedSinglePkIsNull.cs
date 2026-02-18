using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that RefreshPreservingLocalChanges **rejects refreshed rows whose single-column primary key is null**.
        ///
        /// This mirrors the already-locked composite PK rule for this mode:
        /// - Refreshed rows with null PK parts are considered invalid and must not be merged silently, because they cannot
        ///   be correlated deterministically.
        ///
        /// How the test proves the contract:
        /// 1) CURRENT has a normal PK table.
        /// 2) REFRESHED snapshot contains a row with Id = null.
        /// 3) Act: merge.
        /// 4) Assert: merge throws (InvalidOperationException) rather than silently adding or ignoring an invalid row.
        /// </summary>
        [Fact]
        public void Throws_WhenRefreshedSinglePkIsNull()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            // Arrange (REFRESHED) - invalid PK (null)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow bad = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            bad["Id"] = null;
            bad["Name"] = "BAD";
            rt.AddLoadedRow(bad);

            // Act + Assert
            MergeOptions options = new MergeOptions();

            Assert.Throws<System.InvalidOperationException>(
                () => current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }
    }
}
