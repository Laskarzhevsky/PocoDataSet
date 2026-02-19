using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Replace merge behavior on identical snapshot: Replace is destructive and always rebuilds rows.
    ///
    /// Meaning:
    /// - Even when REFRESHED contains exactly the same values as CURRENT, Replace clears rows and re-adds them.
    /// - Therefore it produces AddedDataRows entries (one per row) and recreates row instances.
    ///
    /// Scenario:
    /// - CURRENT and REFRESHED contain one identical row.
    ///
    /// How the test proves the contract:
    /// - Capture CURRENT row instance.
    /// - Run DoReplaceMerge with identical snapshot.
    /// - Assert row values are identical but the row instance has changed.
    /// - Assert MergeResult.AddedDataRows contains one entry.
    /// </summary>
    public partial class ReplaceMerge
    {
        [Fact]
        public void IdenticalSnapshot_RecreatesRows_TracksAdded()
        {
            // Arrange
            IDataSet current = new DataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow cur = t.AddNewRow();
            cur["Id"] = 1;
            cur["Name"] = "Same";

            IDataRow originalInstance = t.Rows[0];

            IDataSet refreshed = new DataSet();
            IDataTable rt = refreshed.AddNewTable("T");
            rt.AddColumn("Id", DataTypeNames.INT32);
            rt.AddColumn("Name", DataTypeNames.STRING);

            IDataRow r = rt.AddNewRow();
            r["Id"] = 1;
            r["Name"] = "Same";

            // Act
            MergeOptions options = new MergeOptions();
            current.DoReplaceMerge(refreshed, options);

            // Assert: values preserved, instance replaced
            Assert.Single(t.Rows);
            Assert.Equal(1, (int)t.Rows[0]["Id"]!);
            Assert.Equal("Same", (string)t.Rows[0]["Name"]!);
            Assert.NotSame(originalInstance, t.Rows[0]);

            // Assert: Replace tracks added rows even when identical
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
        }
    }
}
