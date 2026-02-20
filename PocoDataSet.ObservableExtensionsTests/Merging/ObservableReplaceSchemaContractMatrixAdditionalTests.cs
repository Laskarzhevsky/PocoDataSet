using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests
{
    /// <summary>
    /// Replace schema contract matrix (Observable, Policy A - industry common):
    /// - Current schema is authoritative.
    /// - Refreshed extra columns are ignored (current must not gain them).
    /// - Refreshed missing columns do not remove current columns; values become null/default on replaced rows.
    /// - Incompatible column type changes are rejected (throw).
    ///
    /// NOTE: Observable tables may contain internal/special columns in addition to user-defined columns,
    /// so these tests do NOT assert total column counts.
    /// </summary>
    public sealed class ObservableReplaceSchemaContractMatrixAdditionalTests
    {
        [Fact]
        public void Replace_IgnoresExtraColumnsInRefreshed_AndKeepsCurrentSchema()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Note", DataTypeNames.STRING);

            IObservableDataRow c = t.AddNewRow();
            c["Id"] = 1;
            c["Name"] = "Old";
            c["Note"] = "OldNote";
            c.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            rt.AddColumn("Note", DataTypeNames.STRING);
            rt.AddColumn("Extra", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Name"] = "New";
            r["Note"] = "NewNote";
            r["Extra"] = "SHOULD_NOT_APPEAR";
            rt.AddLoadedRow(r);

            IObservableMergeOptions options = new ObservableMergeOptions();

            current.DoReplaceMerge(refreshed, options);

            Assert.True(HasColumn(t, "Id"));
            Assert.True(HasColumn(t, "Name"));
            Assert.True(HasColumn(t, "Note"));
            Assert.False(HasColumn(t, "Extra"));

            Assert.Single(t.Rows);
            Assert.Equal(2, (int)t.Rows[0]["Id"]!);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Equal("NewNote", (string)t.Rows[0]["Note"]!);
        }

        [Fact]
        public void Replace_WhenRefreshedMissingColumn_CurrentKeepsColumn_AndValuesBecomeNull()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Note", DataTypeNames.STRING);

            IObservableDataRow c = t.AddNewRow();
            c["Id"] = 1;
            c["Name"] = "Old";
            c["Note"] = "OldNote";
            c.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Name", DataTypeNames.STRING);
            // Missing "Note"

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Name"] = "New";
            rt.AddLoadedRow(r);

            IObservableMergeOptions options = new ObservableMergeOptions();

            current.DoReplaceMerge(refreshed, options);

            Assert.True(HasColumn(t, "Id"));
            Assert.True(HasColumn(t, "Name"));
            Assert.True(HasColumn(t, "Note"));

            Assert.Single(t.Rows);
            Assert.Equal(2, (int)t.Rows[0]["Id"]!);
            Assert.Equal("New", (string)t.Rows[0]["Name"]!);
            Assert.Null(t.Rows[0]["Note"]);
        }

        [Fact]
        public void Replace_Throws_WhenRefreshedColumnTypeDiffersFromCurrent()
        {
            IObservableDataSet current = new ObservableDataSet();
            IObservableDataTable t = current.AddNewTable("T");

            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Amount", DataTypeNames.INT32);

            IObservableDataRow c = t.AddNewRow();
            c["Id"] = 1;
            c["Amount"] = 10;
            c.AcceptChanges();

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable rt = refreshed.AddNewTable("T");

            rt.AddColumn("Id", DataTypeNames.INT32, false, true);
            rt.AddColumn("Amount", DataTypeNames.STRING);

            IDataRow r = DataRowExtensions.CreateRowFromColumns(rt.Columns);
            r["Id"] = 2;
            r["Amount"] = "NOT_AN_INT";
            rt.AddLoadedRow(r);

            IObservableMergeOptions options = new ObservableMergeOptions();

            Assert.Throws<InvalidOperationException>(() => current.DoReplaceMerge(refreshed, options));
        }

        private static bool HasColumn(IObservableDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, columnName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
