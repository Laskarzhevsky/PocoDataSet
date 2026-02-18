using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    public partial class RefreshMergePreservingLocalChanges
    {
        /// <summary>
        /// Verifies that RefreshPreservingLocalChanges produces a consistent MergeResult for a mixed scenario:
        /// - one row UPDATED (matching PK, snapshot differs),
        /// - one row DELETED (current row missing from snapshot AND is Unchanged),
        /// - one row ADDED (snapshot has extra row).
        ///
        /// This test focuses on **result accounting** (counts and membership), not just final row values.
        ///
        /// How the test proves the contract:
        /// 1) CURRENT has two Unchanged rows: Id=1 and Id=2.
        /// 2) REFRESHED contains Id=1 with different Name (update) and Id=3 (add); Id=2 is missing (delete).
        /// 3) Act: merge.
        /// 4) Assert: final rows are {1,3}, and MergeResult contains exactly:
        ///    - 1 Updated entry (Id=1)
        ///    - 1 Deleted entry (Id=2)
        ///    - 1 Added entry (Id=3)
        /// </summary>
        [Fact]
        public void MergeResult_Tracks_AddedUpdatedDeleted()
        {
            // Arrange (CURRENT)
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow c1 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c1["Id"] = 1;
            c1["Name"] = "One";
            t.AddLoadedRow(c1);

            IDataRow c2 = DataRowExtensions.CreateRowFromColumns(t.Columns);
            c2["Id"] = 2;
            c2["Name"] = "Two";
            t.AddLoadedRow(c2);

            Assert.Equal(DataRowState.Unchanged, t.Rows[0].DataRowState);
            Assert.Equal(DataRowState.Unchanged, t.Rows[1].DataRowState);

            // Arrange (REFRESHED)
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("Department");
            rt.AddColumn("Id", DataTypeNames.INT32, true);
            rt.AddColumn("Name", DataTypeNames.STRING);

            // Id=1 changed -> update
            IDataRow r1 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r1["Id"] = 1;
            r1["Name"] = "One (server)";
            rt.AddLoadedRow(r1);

            // Id=3 new -> add
            IDataRow r3 = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r3["Id"] = 3;
            r3["Name"] = "Three";
            rt.AddLoadedRow(r3);

            // Act
            MergeOptions options = new MergeOptions();
            current.DoRefreshMergePreservingLocalChanges(refreshed, options);
            IDataSetMergeResult result = options.DataSetMergeResult;

            // Assert (final rows)
            Assert.Equal(2, t.Rows.Count);

            bool has1 = false;
            bool has3 = false;

            for (int i = 0; i < t.Rows.Count; i++)
            {
                int id = (int)t.Rows[i]["Id"]!;
                if (id == 1) has1 = true;
                if (id == 3) has3 = true;
            }

            Assert.True(has1);
            Assert.True(has3);

            // Assert (result accounting)
            Assert.Single(result.UpdatedDataRows);
            Assert.Single(result.DeletedDataRows);
            Assert.Single(result.AddedDataRows);

            Assert.Equal(1, (int)result.UpdatedDataRows[0].DataRow["Id"]!);
            Assert.Equal(2, (int)result.DeletedDataRows[0].DataRow["Id"]!);
            Assert.Equal(3, (int)result.AddedDataRows[0].DataRow["Id"]!);
        }
    }
}
