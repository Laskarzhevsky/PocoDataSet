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
    /// Composite primary key matrix (Observable).
    /// NOTE: Based on observed behavior, RefreshIfNoChangesExist does not throw when refreshed composite PK contains null/DBNull,
    /// while RefreshPreservingLocalChanges does throw for null PK parts.
    /// </summary>
    public sealed class ObservableCompositePrimaryKeyMatrixAdditionalTests
    {
        [Fact]
        public void RefreshPreservingLocalChanges_Throws_WhenRefreshedCompositePkContainsNullPart()
        {
            IObservableDataSet current = CreateCurrentObservableCompositePk(1, "X", "Current");

            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            ObservableMergeOptions options = new ObservableMergeOptions();

            Assert.Throws<InvalidOperationException>(() =>
                current.DoRefreshMergePreservingLocalChanges(refreshed, options));
        }

        [Fact]
        public void RefreshIfNoChangesExist_Allows_NullCompositePkPart_InRefreshed_AndTreatsAsNonCorrelatable()
        {
            IObservableDataSet current = CreateCurrentObservableCompositePk(1, "X", "Current");

            IDataSet refreshed = CreateRefreshedCompositePk(objectA: 1, objectB: null, name: "Bad");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergeIfNoChangesExist(refreshed, options);

            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);
            Assert.Empty(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);

            IObservableDataTable result = current.Tables["T"];
            Assert.Single(result.Rows);

            Assert.Equal(1, (int)result.Rows[0]["A"]!);
            Assert.Null(result.Rows[0]["B"]);
            Assert.Equal("Bad", (string)result.Rows[0]["Name"]!);
        }

        [Fact]
        public void RefreshPreservingLocalChanges_CorrelatesOnlyWhenAllCompositePkPartsMatch()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            AddAcceptedRow(t, 1, "X", "One-X");
            AddAcceptedRow(t, 1, "Y", "One-Y");

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = AddCompositePkTable(refreshed);

            AddLoadedRow(rt, 1, "X", "One-X (updated)");
            AddLoadedRow(rt, 1, "Z", "One-Z");

            ObservableMergeOptions options = new ObservableMergeOptions();

            current.DoRefreshMergePreservingLocalChanges(refreshed, options);

            Assert.Single(options.ObservableDataSetMergeResult.UpdatedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.DeletedObservableDataRows);
            Assert.Single(options.ObservableDataSetMergeResult.AddedObservableDataRows);

            Assert.Equal(2, t.Rows.Count);

            Assert.True(ContainsCompositePk(t, 1, "X"));
            Assert.True(ContainsCompositePk(t, 1, "Z"));
            Assert.False(ContainsCompositePk(t, 1, "Y"));
        }

        [Fact]
        public void PostSave_CorrelatesByClientKey_WhenCompositePrimaryKeyDiffers_AndPreservesRowInstance()
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("T");

            t.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            t.AddColumn("A", DataTypeNames.INT32, false, true);
            t.AddColumn("B", DataTypeNames.STRING, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey = Guid.NewGuid();

            DataRow currentRow = new DataRow();
            currentRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            currentRow["A"] = -1;
            currentRow["B"] = "TEMP";
            currentRow["Name"] = "Row";
            t.AddRow(currentRow);

            IObservableDataSet currentObservable = new ObservableDataSet(inner);
            IObservableDataTable currentTable = currentObservable.Tables["T"];

            Assert.Equal(1, currentTable.Rows.Count);
            IObservableDataRow before = currentTable.Rows[0];

            IDataSet postSave = DataSetFactory.CreateDataSet();
            IDataTable pt = postSave.AddNewTable("T");

            pt.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            pt.AddColumn("A", DataTypeNames.INT32, false, true);
            pt.AddColumn("B", DataTypeNames.STRING, false, true);
            pt.AddColumn("Name", DataTypeNames.STRING);

            DataRow serverRow = new DataRow();
            serverRow[SpecialColumnNames.CLIENT_KEY] = clientKey;
            serverRow["A"] = 1;
            serverRow["B"] = "X";
            serverRow["Name"] = "Row";
            pt.AddRow(serverRow);

            serverRow.SetDataRowState(DataRowState.Added);

            ObservableMergeOptions options = new ObservableMergeOptions();

            currentObservable.DoPostSaveMerge(postSave, options);

            Assert.Equal(1, currentTable.Rows.Count);
            Assert.Same(before, currentTable.Rows[0]);

            Assert.Equal(1, (int)currentTable.Rows[0]["A"]!);
            Assert.Equal("X", (string)currentTable.Rows[0]["B"]!);
            Assert.Equal(DataRowState.Unchanged, currentTable.Rows[0].DataRowState);
        }

        private static IDataTable AddCompositePkTable(IDataSet ds)
        {
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            return t;
        }

        private static void AddLoadedRow(IDataTable t, object a, object? b, string name)
        {
            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);
        }

        private static IDataSet CreateRefreshedCompositePk(object objectA, object? objectB, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);
            AddLoadedRow(t, objectA, objectB, name);
            return ds;
        }

        private static IObservableDataSet CreateCurrentObservableCompositePk(int a, string b, string name)
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(inner);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return new ObservableDataSet(inner);
        }

        private static void AddAcceptedRow(IObservableDataTable t, int a, string b, string name)
        {
            IObservableDataRow r = t.AddNewRow();
            r["A"] = a;
            r["B"] = b;
            r["Name"] = name;
            r.AcceptChanges();
        }

        private static bool ContainsCompositePk(IObservableDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["A"]! == a && (string)t.Rows[i]["B"]! == b)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
