using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using Xunit;

namespace PocoDataSet.ObservableExtensionsTests.ObservableDataRowExtensions
{
    public partial class ObservableDataRowExtensionsTests
    {
        [Fact]
        public void CompilePrimaryKeyValue_ReturnsEmptyString_WhenRowIsNull()
        {
            bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                row: null,
                primaryKeyColumnNames: new List<string> { "Id" },
                primaryKeyValue: out string value);

            Assert.False(ok);
            Assert.Equal(string.Empty, value);
        }

        [Fact]
        public void CompilePrimaryKeyValue_ReturnsEmptyString_WhenPrimaryKeyColumnNamesEmpty()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", PocoDataSet.IData.DataTypeNames.INT32, false, true);

            IObservableDataRow r = t.AddNewRow();
            r["Id"] = 5;
            r.AcceptChanges();

            bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                row: r.InnerDataRow,
                primaryKeyColumnNames: new List<string>(),
                primaryKeyValue: out string value);

            Assert.False(ok);
            Assert.Equal(string.Empty, value);
        }

        [Fact]
        public void CompilePrimaryKeyValue_FormatsSingleKey_AsLengthHashValue()
        {
            IObservableDataRow r = CreateRowWithValues(("Id", 5));

            bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                row: r.InnerDataRow,
                primaryKeyColumnNames: new List<string> { "Id" },
                primaryKeyValue: out string value);

            Assert.True(ok);
            Assert.Equal("1#5", value);
        }

        [Fact]
        public void CompilePrimaryKeyValue_FormatsCompositeKey_WithDelimiter_AndLengthPrefixes()
        {
            IObservableDataRow r = CreateRowWithValues(("TenantId", 7), ("Code", "AB"));

            bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                row: r.InnerDataRow,
                primaryKeyColumnNames: new List<string> { "TenantId", "Code" },
                primaryKeyValue: out string value);

            Assert.True(ok);
            Assert.Equal("1#7|2#AB", value);
        }

        [Fact]
        public void CompilePrimaryKeyValue_UsesEmptyString_ForNullAndDBNull()
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");
            t.AddColumn("A", PocoDataSet.IData.DataTypeNames.STRING, true, true);
            t.AddColumn("B", PocoDataSet.IData.DataTypeNames.STRING, true, true);

            IObservableDataRow r = t.AddNewRow();
            r["A"] = null;
            r["B"] = DBNull.Value;
            r.AcceptChanges();

            bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                row: r.InnerDataRow,
                primaryKeyColumnNames: new List<string> { "A", "B" },
                primaryKeyValue: out string value);

            Assert.True(ok);
            Assert.Equal("0#|0#", value);
        }

        [Fact]
        public void CompilePrimaryKeyValue_UsesInvariantCulture_ForFormattableTypes()
        {
            CultureInfo originalCulture = Thread.CurrentThread.CurrentCulture;
            CultureInfo originalUiCulture = Thread.CurrentThread.CurrentUICulture;

            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("fr-FR");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("fr-FR");

                IObservableDataRow r = CreateRowWithValues(("Amount", 12.5m));

                bool ok = RowIdentityResolver.TryGetPrimaryKeyValue(
                    row: r.InnerDataRow,
                    primaryKeyColumnNames: new List<string> { "Amount" },
                    primaryKeyValue: out string value);

                Assert.True(ok);
                Assert.Equal("4#12.5", value);
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = originalCulture;
                Thread.CurrentThread.CurrentUICulture = originalUiCulture;
            }
        }

        private static IObservableDataRow CreateRowWithValues(params (string Name, object? Value)[] values)
        {
            IObservableDataSet ds = new ObservableDataSet();
            IObservableDataTable t = ds.AddNewTable("T");

            for (int i = 0; i < values.Length; i++)
            {
                t.AddColumn(values[i].Name, PocoDataSet.IData.DataTypeNames.OBJECT, true, true);
            }

            IObservableDataRow r = t.AddNewRow();

            for (int i = 0; i < values.Length; i++)
            {
                r[values[i].Name] = values[i].Value;
            }

            r.AcceptChanges();
            return r;
        }
    }
}
