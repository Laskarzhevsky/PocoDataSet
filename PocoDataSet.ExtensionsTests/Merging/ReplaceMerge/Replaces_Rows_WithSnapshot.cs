using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge data contract: rows are replaced by the REFRESHED snapshot.
    ///
    /// Scenario:
    /// - CURRENT rows: A(Id=1), B(Id=2)
    /// - REFRESHED rows: B(Id=2), C(Id=3)
    ///
    /// How the test proves the contract:
    /// - After DoReplaceMerge, CURRENT must contain only the REFRESHED rows (Ids 2 and 3).
    /// - The CURRENT-only row (Id 1) must be removed.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Replaces_Rows_WithSnapshot()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            GetColumn(t, "Id").IsPrimaryKey = true;

            IDataRow a = t.AddNewRow();
            a["Id"] = 1;
            a["Name"] = "A";

            IDataRow b = t.AddNewRow();
            b["Id"] = 2;
            b["Name"] = "B";

            IDataSet refreshed = new DataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);
            GetColumn(rt, "Id").IsPrimaryKey = true;

            IDataRow b2 = rt.AddNewRow();
            b2["Id"] = 2;
            b2["Name"] = "B (server)";

            IDataRow c = rt.AddNewRow();
            c["Id"] = 3;
            c["Name"] = "C";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: only snapshot rows remain.
            Assert.Equal(2, t.Rows.Count);

            bool has2 = false;
            bool has3 = false;

            for (int i = 0; i < t.Rows.Count; i++)
            {
                int id = (int)t.Rows[i]["Id"]!;
                if (id == 2) has2 = true;
                if (id == 3) has3 = true;

                // Also verify values come from snapshot, not current.
                if (id == 2)
                {
                    Assert.Equal("B (server)", (string)t.Rows[i]["Name"]!);
                }
            }

            Assert.True(has2);
            Assert.True(has3);
        }
    }
}
