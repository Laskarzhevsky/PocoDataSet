using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Composite primary key matrix (POCO).
    /// NOTE: Your current merge contract differs by mode:
    /// - RefreshPreservingLocalChanges rejects refreshed composite PK rows containing null parts (throws).
    /// - RefreshIfNoChangesExist currently allows refreshed composite PK rows containing null parts and treats them as non-correlatable.
    ///
    /// These tests lock the CURRENT observed behavior to prevent future regressions.
    /// </summary>
    public sealed class CompositePrimaryKeyMatrixAdditionalTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_Throws_WhenRefreshedCompositePkContainsNullPart()
        {
            IDataSet current = CreateCurrentCompositePk(1, "X", "Current");

            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            MergeOptions options = new MergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }

        [Fact]
        public void RefreshIfNoChangesExist_Allows_NullCompositePkPart_InRefreshed_AndTreatsAsNonCorrelatable()
        {
            // Arrange: current has a valid composite PK row (1, X)
            IDataSet current = CreateCurrentCompositePk(1, "X", "Current");

            // Refreshed uses (1, null) -> cannot correlate to (1, X) on full composite PK
            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            MergeOptions options = new MergeOptions();

            // Act (should NOT throw)
            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            // Assert: old row deleted, new row added, no update
            Assert.Single(options.DataSetMergeResult.AddedDataRows);
            Assert.Empty(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Single(result.Rows);

            Assert.Equal(1, (int)result.Rows[0]["A"]!);
            Assert.Null(result.Rows[0]["B"]);
            Assert.Equal("Bad", (string)result.Rows[0]["Name"]!);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_CorrelatesOnlyWhenAllCompositePkPartsMatch()
        {
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(current);

            AddLoadedRow(t, 1, "X", "One-X");
            AddLoadedRow(t, 1, "Y", "One-Y");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = AddCompositePkTable(refreshed);

            AddLoadedRow(rt, 1, "X", "One-X (updated)");
            AddLoadedRow(rt, 1, "Z", "One-Z");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Equal(2, result.Rows.Count);

            Assert.True(ContainsCompositePk(result, 1, "X"));
            Assert.True(ContainsCompositePk(result, 1, "Z"));
            Assert.False(ContainsCompositePk(result, 1, "Y"));

            IDataRow rowX = FindByCompositePk(result, 1, "X");
            Assert.Equal("One-X (updated)", (string)rowX["Name"]!);
        }

        [Fact]
        public void RefreshIfNoChangesExist_CorrelatesOnlyWhenAllCompositePkPartsMatch()
        {
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(current);

            AddLoadedRow(t, 1, "X", "One-X");
            AddLoadedRow(t, 1, "Y", "One-Y");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = AddCompositePkTable(refreshed);

            AddLoadedRow(rt, 1, "X", "One-X (updated)");
            AddLoadedRow(rt, 1, "Z", "One-Z");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Single(options.DataSetMergeResult.UpdatedDataRows);
            Assert.Single(options.DataSetMergeResult.DeletedDataRows);
            Assert.Single(options.DataSetMergeResult.AddedDataRows);

            IDataTable result = current.Tables["T"];
            Assert.Equal(2, result.Rows.Count);

            Assert.True(ContainsCompositePk(result, 1, "X"));
            Assert.True(ContainsCompositePk(result, 1, "Z"));
            Assert.False(ContainsCompositePk(result, 1, "Y"));
        }

        private static IDataTable AddCompositePkTable(IDataSet ds)
        {
            IDataTable t = ds.AddNewTable("T");

            // Mark both columns as PK parts.
            // Kept nullable=true so we can construct null-part refreshed rows for contract tests.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            return t;
        }

        private static void AddLoadedRow(IDataTable t, object a, object? b, string name)
        {
            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);
        }

        private static IDataSet CreateCurrentCompositePk(int a, string b, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, a, b, name);

            return ds;
        }

        private static IDataSet CreateRefreshedCompositePk(object objectA, object? objectB, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, objectA, objectB, name);

            return ds;
        }

        private static bool ContainsCompositePk(IDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow r = t.Rows[i];
                if ((int)r["A"]! == a && (string)r["B"]! == b)
                {
                    return true;
                }
            }

            return false;
        }

        private static IDataRow FindByCompositePk(IDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow r = t.Rows[i];
                if ((int)r["A"]! == a && (string)r["B"]! == b)
                {
                    return r;
                }
            }

            throw new InvalidOperationException("Row not found.");
        }
    }
}
