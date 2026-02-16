using System;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    /// <summary>
    /// Composite primary key null-part policy matrix (Observable).
    ///
    /// Current behavior (locked by these tests):
    /// - Current observable rows MAY contain null/DBNull in a composite PK part.
    /// - When that happens, the current row is treated as invalid for correlation and is effectively replaced by refreshed data.
    /// - The merge must not throw.
    /// </summary>
    public class ObservableCompositePrimaryKeyNullPartPolicyTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsNull_StringPart_AndRefreshedHasNonNullPk()
        {
            IObservableDataSet current = CreateCurrentObservableWithCompositePkRow(1, null);
            IDataSet refreshed = CreateRefreshedCompositePkSnapshot(1, "X", "Refreshed");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IObservableDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsDBNull_StringPart_AndRefreshedHasNonNullPk()
        {
            IObservableDataSet current = CreateCurrentObservableWithCompositePkRow(1, DBNull.Value);
            IDataSet refreshed = CreateRefreshedCompositePkSnapshot(1, "X", "Refreshed");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IObservableDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_DoesNotThrow_WhenCurrentCompositePkContainsDBNull_IntPart_AndOtherPartNonNull_AndRefreshedHasNonNullPk()
        {
            IObservableDataSet current = CreateCurrentObservableWithCompositePkRow(DBNull.Value, "X");
            IDataSet refreshed = CreateRefreshedCompositePkSnapshot(1, "X", "Refreshed");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            IObservableDataTable result = current.Tables["T"];
            Assert.Equal(1, result.Rows.Count);

            IObservableDataRow row = result.Rows[0];
            Assert.Equal("Refreshed", row["Name"]);
        }

        private static IObservableDataSet CreateCurrentObservableWithCompositePkRow(object a, object? b)
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("T");

            // Make PK parts nullable so we can construct the invalid cases.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = "Current";
            t.AddLoadedRow(row);

            return new ObservableDataSet(inner);
        }

        private static IDataSet CreateRefreshedCompositePkSnapshot(int a, string b, string name)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return dataSet;
        }
    }
}
