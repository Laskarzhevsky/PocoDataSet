using System;
using System.Collections.Generic;
using System.Reflection;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public partial class CreateChangesetDeletedRowsTests2
    {
        [Fact]
        public void CreateChangeset_ModifiedRow_SkipsColumn_WhenTryGetValueReturnsFalse()
        {
            // Arrange: create a fake dataset/table/row where
            // - schema contains "Description"
            // - but row.TryGetValue("Description", out ..) returns false

            List<IColumnMetadata> columns = new List<IColumnMetadata>();
            columns.Add(FakeColumnMetadataProxy.Create("Id", DataTypeNames.INT32, false, true));
            columns.Add(FakeColumnMetadataProxy.Create("Name", DataTypeNames.STRING, true, false));
            columns.Add(FakeColumnMetadataProxy.Create("Description", DataTypeNames.STRING, true, false));

            List<string> primaryKeys = new List<string>();
            primaryKeys.Add("Id");

            Dictionary<string, object?> values = new Dictionary<string, object?>();
            values["Id"] = 1;
            values["Name"] = "Sales Updated";
            // Intentionally do NOT place "Description" into values.

            Dictionary<string, object?> originals = new Dictionary<string, object?>();
            originals["Id"] = 1;
            originals["Name"] = "Sales";
            originals["Description"] = "Original Description";

            // Make TryGetValue fail for "Description"
            HashSet<string> tryGetValueShouldFailFor = new HashSet<string>(StringComparer.Ordinal);
            tryGetValueShouldFailFor.Add("Description");

            IDataRow fakeRow = FakeDataRowProxy.Create(
                state: DataRowState.Modified,
                values: values,
                originals: originals,
                tryGetValueFailColumns: tryGetValueShouldFailFor);

            List<IDataRow> rows = new List<IDataRow>();
            rows.Add(fakeRow);

            IDataTable fakeTable = FakeDataTableProxy.Create(
                columns: columns,
                rows: rows,
                primaryKeys: primaryKeys);

            Dictionary<string, IDataTable> tables = new Dictionary<string, IDataTable>(StringComparer.Ordinal);
            tables["Department"] = fakeTable;

            IDataSet fakeDataSet = FakeDataSetProxy.Create(tables);

            // Act
            IDataSet? changeset = fakeDataSet.CreateChangeset();

            // Assert
            Assert.NotNull(changeset);
            Assert.True(changeset!.Tables.ContainsKey("Department"));

            IDataTable csTable = changeset.Tables["Department"];
            Assert.Equal(1, csTable.Rows.Count);

            IDataRow csRow = csTable.Rows[0];
            Assert.Equal(DataRowState.Modified, csRow.DataRowState);

            // PK copied
            Assert.Equal(1, (int)csRow["Id"]!);

            // Changed column copied
            Assert.Equal("Sales Updated", (string)csRow["Name"]!);

            // "Description" exists in schema (target table has full schema),
            // but since TryGetValue failed, it must not be copied => null.
            Assert.False(csRow.ContainsKey("Description"));
}

        private  class FakeDataSetProxy : DispatchProxy
        {
            Dictionary<string, IDataTable>? _tables;

            public static IDataSet Create(Dictionary<string, IDataTable> tables)
            {
                IDataSet ds = DispatchProxy.Create<IDataSet, FakeDataSetProxy>();
                ((FakeDataSetProxy)(object)ds)._tables = tables;
                return ds;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                if (targetMethod.Name == "get_Tables")
                {
                    return _tables!;
                }

                throw new NotImplementedException("FakeDataSetProxy does not implement: " + targetMethod.Name);
            }
        }

        private  class FakeDataTableProxy : DispatchProxy
        {
            List<IColumnMetadata>? _columns;
            List<IDataRow>? _rows;
            List<string>? _primaryKeys;

            public static IDataTable Create(List<IColumnMetadata> columns, List<IDataRow> rows, List<string> primaryKeys)
            {
                IDataTable table = DispatchProxy.Create<IDataTable, FakeDataTableProxy>();
                FakeDataTableProxy proxy = (FakeDataTableProxy)(object)table;
                proxy._columns = columns;
                proxy._rows = rows;
                proxy._primaryKeys = primaryKeys;
                return table;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                if (targetMethod.Name == "get_Columns")
                {
                    return _columns!;
                }

                if (targetMethod.Name == "get_Rows")
                {
                    return _rows!;
                }

                if (targetMethod.Name == "get_PrimaryKeys")
                {
                    return _primaryKeys!;
                }

                if (targetMethod.Name == "set_PrimaryKeys")
                {
                    _primaryKeys = (List<string>?)args![0];
                    return null;
                }

                throw new NotImplementedException("FakeDataTableProxy does not implement: " + targetMethod.Name);
            }
        }

        private  class FakeDataRowProxy : DispatchProxy
        {
            DataRowState _state;
            Dictionary<string, object?>? _values;
            Dictionary<string, object?>? _originals;
            HashSet<string>? _tryGetValueFailColumns;

            public static IDataRow Create(
                DataRowState state,
                Dictionary<string, object?> values,
                Dictionary<string, object?> originals,
                HashSet<string> tryGetValueFailColumns)
            {
                IDataRow row = DispatchProxy.Create<IDataRow, FakeDataRowProxy>();
                FakeDataRowProxy proxy = (FakeDataRowProxy)(object)row;
                proxy._state = state;
                proxy._values = values;
                proxy._originals = originals;
                proxy._tryGetValueFailColumns = tryGetValueFailColumns;
                return row;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                // DataRowState
                if (targetMethod.Name == "get_DataRowState")
                {
                    return _state;
                }

                // SetDataRowState(DataRowState)
                if (targetMethod.Name == "SetDataRowState")
                {
                    _state = (DataRowState)args![0]!;
                    return null;
                }

                // ContainsKey(string)
                if (targetMethod.Name == "ContainsKey")
                {
                    string key = (string)args![0]!;
                    return _values!.ContainsKey(key);
                }

                // TryGetValue(string, out object?)
                if (targetMethod.Name == "TryGetValue")
                {
                    string key = (string)args![0]!;
                    if (_tryGetValueFailColumns!.Contains(key))
                    {
                        args![1] = null;
                        return false;
                    }

                    object? val = null;
                    bool ok = _values!.TryGetValue(key, out val);
                    args![1] = val;
                    return ok;
                }

                // TryGetOriginalValue(string, out object?)
                if (targetMethod.Name == "TryGetOriginalValue")
                {
                    string key = (string)args![0]!;
                    object? val = null;
                    bool ok = _originals!.TryGetValue(key, out val);
                    args![1] = val;
                    return ok;
                }

                // indexer: object? this[string columnName]
                if (targetMethod.Name == "get_Item" && args != null && args.Length == 1)
                {
                    string key = (string)args[0]!;
                    return _values![key];
                }

                if (targetMethod.Name == "set_Item" && args != null && args.Length == 2)
                {
                    string key = (string)args[0]!;
                    _values![key] = args[1];
                    return null;
                }

                throw new NotImplementedException("FakeDataRowProxy does not implement: " + targetMethod.Name);
            }
        }

        private class FakeColumnMetadataProxy : DispatchProxy
        {
            string? _columnName;
            string? _dataTypeName;
            bool _isNullable;
            bool _isPrimaryKey;
            bool _isIdentity;
            int? _maxLength;

            public static IColumnMetadata Create(
                string columnName,
                string dataTypeName,
                bool isNullable,
                bool isPrimaryKey)
            {
                IColumnMetadata col = DispatchProxy.Create<IColumnMetadata, FakeColumnMetadataProxy>();
                FakeColumnMetadataProxy proxy = (FakeColumnMetadataProxy)(object)col;

                proxy._columnName = columnName;
                proxy._dataTypeName = dataTypeName;
                proxy._isNullable = isNullable;
                proxy._isPrimaryKey = isPrimaryKey;

                proxy._isIdentity = false;
                proxy._maxLength = null;

                return col;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                // Required by AddColumns/AddColumn (based on your exception and typical usage)
                if (targetMethod.Name == "get_ColumnName")
                    return _columnName!;
                if (targetMethod.Name == "get_DataTypeName")
                    return _dataTypeName!;
                if (targetMethod.Name == "get_IsNullable")
                    return _isNullable;
                if (targetMethod.Name == "get_IsPrimaryKey")
                    return _isPrimaryKey;

                // Common optional metadata properties (safe defaults)
                if (targetMethod.Name == "get_IsIdentity")
                    return _isIdentity;
                if (targetMethod.Name == "get_MaxLength")
                    return _maxLength;

                // Some codebases use alternate naming; return safe defaults if asked.
                if (targetMethod.Name == "get_AllowDBNull")
                    return _isNullable;
                if (targetMethod.Name == "get_IsAutoIncrement")
                    return _isIdentity;
                if (targetMethod.Name == "get_AutoIncrement")
                    return _isIdentity;

                // If your interface has other members AddColumns touches, add them here as needed.
                throw new NotImplementedException("FakeColumnMetadataProxy does not implement: " + targetMethod.Name);
            }
        }
    }
}
