using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge schema contract: PRIMARY KEY set is owned by CURRENT and must not change even if REFRESHED disagrees.
    ///
    /// Meaning:
    /// - Replace enforces CURRENT schema metadata; REFRESHED is treated as a data carrier, not a schema authority.
    /// - If REFRESHED marks different columns as primary key, CURRENT must keep its own PK definition.
    ///
    /// Scenario:
    /// - CURRENT: Id is PK, Name is not PK.
    /// - REFRESHED: Id is NOT PK, Name IS PK (conflicting schema metadata).
    ///
    /// How the test proves the contract:
    /// - Run DoReplaceMerge.
    /// - Assert CURRENT PK flags are unchanged (Id remains PK, Name remains non-PK).
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void Keeps_PrimaryKeySet_WhenRefreshedDiffers()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable currentTable = current.AddNewTable("T");
            currentTable.AddColumn("Id", DataTypeNames.INT32);
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            GetColumn(currentTable, "Id").IsPrimaryKey = true;

            IDataRow cur = currentTable.AddNewRow();
            cur["Id"] = 1;
            cur["Name"] = "Old";

            IDataSet refreshed = new DataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("T");
            refreshedTable.AddColumn("Id", DataTypeNames.INT32);
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            // Conflicting PK definition in REFRESHED.
            GetColumn(refreshedTable, "Name").IsPrimaryKey = true;

            IDataRow r1 = refreshedTable.AddNewRow();
            r1["Id"] = 2;
            r1["Name"] = "New";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: CURRENT PK set is unchanged.
            Assert.True(GetColumn(currentTable, "Id").IsPrimaryKey);
            Assert.False(GetColumn(currentTable, "Name").IsPrimaryKey);
        }
    }
}
