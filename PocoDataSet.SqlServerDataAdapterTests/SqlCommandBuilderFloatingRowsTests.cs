using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.Data.SqlClient;
using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoSqlDataAdapter = PocoDataSet.SqlServerDataAdapter.SqlDataAdapter;

namespace PocoDataSet.SqlServerDataAdapterTests
{
    /// <summary>
    /// Tests command generation behavior needed for floating (sparse) delta rows,
    /// while remaining compatible with full (non-floating) rows.
    /// These tests do NOT hit a database; they use reflection to call internal builders.
    /// </summary>
    public sealed class SqlCommandBuilderFloatingRowsTests
    {
        [Fact]
        public void BuildUpdateCommand_ModifiedRow_SetsOnlyProvidedColumns_WhenRowIsSparse()
        {
            // Arrange
            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(null);

            string tableName = "Person";
            List<string> columns = new List<string> { "Id", "Name", "Description" };
            List<string> primaryKeys = new List<string> { "Id" };

            IDataTable table = FakeDataTableProxy.Create(tableName, columns);
            object metadata = CreateMetadata(adapter, tableName, columns, primaryKeys);

            // Sparse row: Id + Name only; Description is missing => TryGetValue returns false.
            Dictionary<string, object?> current = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            current["Id"] = 1;
            current["Name"] = "Alice";

            Dictionary<string, object?> original = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            original["Id"] = 1;

            original["Name"] = "Alice";
            original["Description"] = "Dev";
            IDataRow row = FakeDataRowProxy.Create(DataRowState.Modified, current, original, true);

            // Act
            SqlCommand cmd = InvokeBuildUpdateCommand(adapter, table, metadata, row, null, new List<string>());

            // Assert
            // Must generate a non-empty UPDATE statement.
            Assert.False(string.IsNullOrWhiteSpace(cmd.CommandText));

            // Must set Name, must NOT set Description.
            Assert.Contains("[Name]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // Description may appear in WHERE due to optimistic concurrency predicates.
            // We only require that Description is NOT included in the SET clause.
            string cmdText = cmd.CommandText;

            // SQL formatting can vary (newlines, multiple spaces). Find SET/WHERE tokens robustly.
            int setIndex = cmdText.IndexOf("SET", StringComparison.OrdinalIgnoreCase);
            Assert.True(setIndex >= 0);

            int whereIndex = cmdText.IndexOf("WHERE", StringComparison.OrdinalIgnoreCase);

            string setClause;
            if (whereIndex > setIndex)
            {
                setClause = cmdText.Substring(setIndex, whereIndex - setIndex);
            }
            else
            {
                // If WHERE is absent/unexpected, treat everything after SET as the SET clause.
                setClause = cmdText.Substring(setIndex);
            }

            Assert.DoesNotContain("[Description] =", setClause, StringComparison.OrdinalIgnoreCase);
// Parameters:
            // - One @p* parameter for Name
            // - One @pk0 parameter for Id
            Assert.Contains("@pk0", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // Ensure we did not create a parameter for Description
            Assert.Single(FindParameters(cmd, "@p"));
            Assert.True(cmd.Parameters.Count >= 2);

            List<SqlParameter> setParams = FindParameters(cmd, "@p");
            Assert.Equal("Alice", setParams[0].Value);
        }

        [Fact]
        public void BuildUpdateCommand_ModifiedRow_IncludesExplicitNull_WhenColumnIsProvidedAsNull()
        {
            // Arrange
            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(null);

            string tableName = "Person";
            List<string> columns = new List<string> { "Id", "Name", "Description" };
            List<string> primaryKeys = new List<string> { "Id" };

            IDataTable table = FakeDataTableProxy.Create(tableName, columns);
            object metadata = CreateMetadata(adapter, tableName, columns, primaryKeys);

            // Provided null means "set NULL" (must be included in SET).
            Dictionary<string, object?> current = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            current["Id"] = 1;
            current["Description"] = null; // explicitly provided null

            Dictionary<string, object?> original = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            original["Id"] = 1;

            original["Name"] = "Alice";
            original["Description"] = "Dev";
            IDataRow row = FakeDataRowProxy.Create(DataRowState.Modified, current, original, true);

            // Act
            SqlCommand cmd = InvokeBuildUpdateCommand(adapter, table, metadata, row, null, new List<string>());

            // Assert
            Assert.Contains("[Description]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // Must have a SET parameter for Description (even if NULL) and pk parameter.
            Assert.Single(FindParameters(cmd, "@p"));
            Assert.True(cmd.Parameters.Count >= 2);

            // Provider should translate null to DBNull.Value.
            List<SqlParameter> setParams = FindParameters(cmd, "@p");
            Assert.Equal(DBNull.Value, setParams[0].Value);
        }

        [Fact]
        public void BuildUpdateCommand_ModifiedRow_RemainsCompatible_WithFullRows()
        {
            // Arrange
            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(null);

            string tableName = "Person";
            List<string> columns = new List<string> { "Id", "Name", "Description" };
            List<string> primaryKeys = new List<string> { "Id" };

            IDataTable table = FakeDataTableProxy.Create(tableName, columns);
            object metadata = CreateMetadata(adapter, tableName, columns, primaryKeys);

            // Full row: all columns present => builder should include both non-PK columns.
            Dictionary<string, object?> current = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            current["Id"] = 1;
            current["Name"] = "Alice";
            current["Description"] = "Dev";

            Dictionary<string, object?> original = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            original["Id"] = 1;
            original["Name"] = "Alice";
            original["Description"] = "Dev";

            IDataRow row = FakeDataRowProxy.Create(DataRowState.Modified, current, original, true);

            // Act
            SqlCommand cmd = InvokeBuildUpdateCommand(adapter, table, metadata, row, null, new List<string>());

            // Assert
            Assert.Contains("[Name]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("[Description]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // Two SET parameters + one pk parameter.
            Assert.True(cmd.Parameters.Count >= 3);
            Assert.Equal(2, FindParameters(cmd, "@p").Count);
        }

        [Fact]
        public void BuildInsertCommand_AddedRow_InsertsOnlyProvidedColumns_WhenRowIsSparse()
        {
            // Arrange
            PocoSqlDataAdapter adapter = new PocoSqlDataAdapter(null);

            string tableName = "Person";
            List<string> columns = new List<string> { "Id", "Name", "Description" };
            List<string> primaryKeys = new List<string> { "Id" };

            IDataTable table = FakeDataTableProxy.Create(tableName, columns);
            object metadata = CreateMetadata(adapter, tableName, columns, primaryKeys);

            // Sparse insert payload: only Name provided.
            Dictionary<string, object?> current = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
            current["Name"] = "Alice";

            IDataRow row = FakeDataRowProxy.Create(DataRowState.Added, current, new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase), false);

            // Act
            SqlCommand cmd = InvokeBuildInsertCommand(adapter, table, metadata, row, null, new List<string>());

            // Assert
            Assert.False(string.IsNullOrWhiteSpace(cmd.CommandText));

            Assert.Contains("INSERT", cmd.CommandText, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("[Name]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // Must NOT include Description if missing.
            Assert.DoesNotContain("[Description]", cmd.CommandText, StringComparison.OrdinalIgnoreCase);

            // One parameter only
            Assert.Single(cmd.Parameters);
            List<SqlParameter> setParams = FindParameters(cmd, "@p");
            Assert.Equal("Alice", setParams[0].Value);
        }

        // -----------------------
        // Reflection helpers
        // -----------------------

        static SqlCommand InvokeBuildUpdateCommand(PocoSqlDataAdapter adapter, IDataTable table, object metadata, IDataRow row, SqlTransaction? tx, List<string> outputColumns)
        {
            MethodInfo? method = typeof(PocoSqlDataAdapter).GetMethod(
                "BuildUpdateCommand",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);

            object? result = method!.Invoke(adapter, new object?[] { table, metadata, row, tx, outputColumns });
            Assert.NotNull(result);

            return (SqlCommand)result!;
        }

        static SqlCommand InvokeBuildInsertCommand(PocoSqlDataAdapter adapter, IDataTable table, object metadata, IDataRow row, SqlTransaction? tx, List<string> outputColumns)
        {
            MethodInfo? method = typeof(PocoSqlDataAdapter).GetMethod(
                "BuildInsertCommand",
                BindingFlags.Instance | BindingFlags.NonPublic);

            Assert.NotNull(method);

            object? result = method!.Invoke(adapter, new object?[] { table, metadata, row, tx, outputColumns });
            Assert.NotNull(result);

            return (SqlCommand)result!;
        }

        static object CreateMetadata(PocoSqlDataAdapter adapter, string tableName, List<string> columnNames, List<string> primaryKeyColumns)
        {
            Type? metadataType = typeof(PocoSqlDataAdapter).GetNestedType("TableWriteMetadata", BindingFlags.NonPublic);
            Assert.NotNull(metadataType);

            object metadata = Activator.CreateInstance(metadataType!)!;
            // TableName
            metadataType!.GetProperty("TableName", BindingFlags.Instance | BindingFlags.Public)!.SetValue(metadata, tableName);

            // ColumnNames HashSet<string>
            HashSet<string> cols = (HashSet<string>)metadataType.GetProperty("ColumnNames")!.GetValue(metadata)!;
            for (int i = 0; i < columnNames.Count; i++)
            {
                cols.Add(columnNames[i]);
            }

            // PrimaryKeyColumns List<string>
            List<string> pkCols = (List<string>)metadataType.GetProperty("PrimaryKeyColumns")!.GetValue(metadata)!;
            for (int i = 0; i < primaryKeyColumns.Count; i++)
            {
                pkCols.Add(primaryKeyColumns[i]);
            }

            // PrimaryKeys HashSet<string>
            HashSet<string> pkSet = (HashSet<string>)metadataType.GetProperty("PrimaryKeys")!.GetValue(metadata)!;
            for (int i = 0; i < primaryKeyColumns.Count; i++)
            {
                pkSet.Add(primaryKeyColumns[i]);
            }

            // Identity / Computed / RowVersion are empty by default.

            return metadata;
        }

        static List<SqlParameter> FindParameters(SqlCommand cmd, string prefix)
        {
            List<SqlParameter> list = new List<SqlParameter>();
            for (int i = 0; i < cmd.Parameters.Count; i++)
            {
                SqlParameter p = cmd.Parameters[i];
                if (p.ParameterName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    // Exclude primary key parameters (@pk0, @pk1, ...) when searching for SET/INSERT params (@p0, @p1, ...)
                    if (prefix.Equals("@p", StringComparison.OrdinalIgnoreCase) &&
                        p.ParameterName.StartsWith("@pk", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    list.Add(p);
                }
            }
            return list;
        }

        // -----------------------
        // Minimal fakes for IDataTable / IDataRow / IColumnMetadata
        // -----------------------

        private class FakeDataTableProxy : DispatchProxy
        {
            string? _tableName;
            List<IColumnMetadata>? _columns;

            public static IDataTable Create(string tableName, List<string> columnNames)
            {
                IDataTable table = DispatchProxy.Create<IDataTable, FakeDataTableProxy>();
                FakeDataTableProxy proxy = (FakeDataTableProxy)(object)table;
                proxy._tableName = tableName;

                List<IColumnMetadata> cols = new List<IColumnMetadata>();
                for (int i = 0; i < columnNames.Count; i++)
                {
                    cols.Add(FakeColumnMetadataProxy.Create(columnNames[i]));
                }
                proxy._columns = cols;

                return table;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                if (targetMethod.Name == "get_TableName")
                {
                    return _tableName!;
                }

                if (targetMethod.Name == "get_Columns")
                {
                    return _columns!;
                }

                // Not needed for these tests
                throw new NotImplementedException("FakeDataTableProxy does not implement: " + targetMethod.Name);
            }
        }

        private class FakeColumnMetadataProxy : DispatchProxy
        {
            string? _columnName;

            public static IColumnMetadata Create(string columnName)
            {
                IColumnMetadata col = DispatchProxy.Create<IColumnMetadata, FakeColumnMetadataProxy>();
                ((FakeColumnMetadataProxy)(object)col)._columnName = columnName;
                return col;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                if (targetMethod.Name == "get_ColumnName")
                {
                    return _columnName!;
                }

                // Some implementations might ask for IsPrimaryKey during AddColumns, but in these tests we never call AddColumns.
                if (targetMethod.Name.StartsWith("get_", StringComparison.Ordinal))
                {
                    // Safe defaults for any other getters.
                    if (targetMethod.ReturnType == typeof(bool)) return false;
                    if (targetMethod.ReturnType == typeof(int)) return 0;
                    if (targetMethod.ReturnType == typeof(int?)) return (int?)null;
                    if (targetMethod.ReturnType == typeof(string)) return string.Empty;
                    return null;
                }

                throw new NotImplementedException("FakeColumnMetadataProxy does not implement: " + targetMethod.Name);
            }
        }

        private class FakeDataRowProxy : DispatchProxy
        {
            DataRowState _state;
            Dictionary<string, object?>? _current;
            Dictionary<string, object?>? _original;
            bool _hasOriginalValues;

            public static IDataRow Create(DataRowState state, Dictionary<string, object?> currentValues, Dictionary<string, object?> originalValues, bool hasOriginalValues)
            {
                IDataRow row = DispatchProxy.Create<IDataRow, FakeDataRowProxy>();
                FakeDataRowProxy proxy = (FakeDataRowProxy)(object)row;
                proxy._state = state;
                proxy._current = currentValues;
                proxy._original = originalValues;
                proxy._hasOriginalValues = hasOriginalValues;
                return row;
            }

            protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
            {
                if (targetMethod == null)
                {
                    throw new InvalidOperationException("Missing target method.");
                }

                if (targetMethod.Name == "get_DataRowState")
                {
                    return _state;
                }

                
                if (targetMethod.Name == "get_HasOriginalValues")
                {
                    // True if any original value exists. The adapter uses this to decide whether to add
                    // original-values based concurrency predicates.
                    return _hasOriginalValues;
                }


                if (targetMethod.Name == "get_OriginalValues")
                {
                    // Adapter uses OriginalValues to build optimistic concurrency predicates.
                    // Return our backing dictionary (matches interface type in your codebase).
                    return _original!;
                }

if (targetMethod.Name == "TryGetValue")
                {
                    string key = (string)args![0]!;
                    object? val;
                    bool ok = _current!.TryGetValue(key, out val);
                    args![1] = val;
                    return ok;
                }

                if (targetMethod.Name == "TryGetOriginalValue")
                {
                    string key = (string)args![0]!;
                    object? val;
                    bool ok = _original!.TryGetValue(key, out val);
                    args![1] = val;
                    return ok;
                }

                if (targetMethod.Name == "ContainsKey")
                {
                    string key = (string)args![0]!;
                    return _current!.ContainsKey(key);
                }

                if (targetMethod.Name == "get_Item")
                {
                    string key = (string)args![0]!;
                    return _current![key];
                }

                if (targetMethod.Name == "set_Item")
                {
                    string key = (string)args![0]!;
                    _current![key] = args![1];
                    return null;
                }

                if (targetMethod.Name == "SetDataRowState")
                {
                    _state = (DataRowState)args![0]!;
                    return null;
                }

                throw new NotImplementedException("FakeDataRowProxy does not implement: " + targetMethod.Name);
            }
        }
    }
}
