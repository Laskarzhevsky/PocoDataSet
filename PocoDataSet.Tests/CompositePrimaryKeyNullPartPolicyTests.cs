using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.Tests
{
    /// <summary>
    /// Composite primary key null-part policy matrix (POCO).
    ///
    /// Current behavior (locked by these tests):
    /// - Current rows MAY contain null/DBNull in a composite PK part.
    /// - When that happens, the current row is treated as invalid for correlation and is effectively replaced by refreshed data.
    /// - The merge must not throw.
    /// </summary>
    public class CompositePrimaryKeyNullPartPolicyTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsNull_StringPart_AndRefreshedHasNonNullPk()
        {
            IDataSet current = CreateCompositePkDataSetWithCurrentRow(1, null);
            IDataSet refreshed = CreateCompositePkRefreshedSnapshot(1, "X", "Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsDBNull_StringPart_AndRefreshedHasNonNullPk()
        {
            IDataSet current = CreateCompositePkDataSetWithCurrentRow(1, DBNull.Value);
            IDataSet refreshed = CreateCompositePkRefreshedSnapshot(1, "X", "Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsDBNull_IntPart_AndOtherPartNonNull_AndRefreshedHasNonNullPk()
        {
            IDataSet current = CreateCompositePkDataSetWithCurrentRow(DBNull.Value, "X");
            IDataSet refreshed = CreateCompositePkRefreshedSnapshot(1, "X", "Refreshed");

            MergeOptions options = new MergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        private static IDataSet CreateCompositePkDataSetWithCurrentRow(object a, object? b)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            // Make PK parts nullable so we can construct the invalid cases.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = "Current";
            t.AddLoadedRow(row);

            return dataSet;
        }

        private static IDataSet CreateCompositePkRefreshedSnapshot(int a, string b, string name)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return dataSet;
        }
    }
}
