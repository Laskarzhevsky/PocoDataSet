using System;
using System.Collections.Generic;
using System.Data;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;

using PocoDataRowState = PocoDataSet.IData.DataRowState;
using AdoDataTable = System.Data.DataTable;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Converts POCO DataSet tables into ADO.NET DataTable instances suitable for
    /// SQL Server table-valued parameters used by generated stored procedures.
    /// </summary>
    public static class SqlServerTableValuedParameterBuilder
    {
        #region Public Methods
        /// <summary>
        /// Creates ADO.NET DataTables for every POCO table that contains saveable rows.
        /// </summary>
        /// <param name="dataSet">Source data set or changeset.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>ADO.NET DataTables keyed by POCO table name.</returns>
        public static Dictionary<string, AdoDataTable> CreateDataTables(IDataSet dataSet, bool changedRowsOnly = true)
        {
            if (dataSet == null)
            {
                throw new ArgumentNullException(nameof(dataSet));
            }

            Dictionary<string, AdoDataTable> result = new Dictionary<string, AdoDataTable>(StringComparer.OrdinalIgnoreCase);

            foreach (KeyValuePair<string, IDataTable> pair in dataSet.Tables)
            {
                IDataTable sourceTable = pair.Value;
                if (sourceTable == null)
                {
                    continue;
                }

                if (changedRowsOnly && !TableHasSaveableRows(sourceTable))
                {
                    continue;
                }

                AdoDataTable adoDataTable = CreateDataTable(sourceTable, changedRowsOnly);
                result[sourceTable.TableName] = adoDataTable;
            }

            return result;
        }

        /// <summary>
        /// Creates an ADO.NET DataTable from a POCO DataTable using the POCO table columns and appending adapter metadata columns.
        /// </summary>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>ADO.NET DataTable suitable for use as a SQL Server TVP value.</returns>
        public static AdoDataTable CreateDataTable(IDataTable dataTable, bool changedRowsOnly = false)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            AdoDataTable adoDataTable = new AdoDataTable(dataTable.TableName);
            AddBusinessColumns(adoDataTable, dataTable);
            AddClientKeyColumn(adoDataTable);
            AddChangeStateColumn(adoDataTable);
            AddRows(adoDataTable, dataTable, changedRowsOnly);

            return adoDataTable;
        }

        /// <summary>
        /// Creates an ADO.NET DataTable from a POCO DataTable using the supplied SQL Server TVP column schema.
        /// Missing POCO columns are sent as DBNull.Value. Extra POCO columns are ignored.
        /// </summary>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="columns">SQL Server TVP columns in exact SQL type order.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>ADO.NET DataTable suitable for use as a SQL Server TVP value.</returns>
        public static AdoDataTable CreateDataTable(IDataTable dataTable, IEnumerable<SqlServerTableValuedParameterColumn> columns, bool changedRowsOnly = false)
        {
            if (dataTable == null)
            {
                throw new ArgumentNullException(nameof(dataTable));
            }

            if (columns == null)
            {
                throw new ArgumentNullException(nameof(columns));
            }

            AdoDataTable adoDataTable = new AdoDataTable(dataTable.TableName);
            AddSchemaColumns(adoDataTable, columns);
            AddRows(adoDataTable, dataTable, changedRowsOnly);

            return adoDataTable;
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type using POCO columns plus adapter metadata columns.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public static SqlParameter CreateStructuredParameter(string parameterName, string typeName, IDataTable dataTable, bool changedRowsOnly = false)
        {
            ValidateParameterArguments(parameterName, typeName);

            SqlParameter sqlParameter = new SqlParameter(parameterName, SqlDbType.Structured);
            sqlParameter.TypeName = typeName;
            sqlParameter.Value = CreateDataTable(dataTable, changedRowsOnly);

            return sqlParameter;
        }

        /// <summary>
        /// Creates a structured SQL parameter for a generated table-valued parameter type using the supplied SQL Server TVP schema.
        /// Missing POCO columns are sent as DBNull.Value. Extra POCO columns are ignored.
        /// </summary>
        /// <param name="parameterName">SQL parameter name.</param>
        /// <param name="typeName">SQL Server table type name.</param>
        /// <param name="dataTable">Source POCO DataTable.</param>
        /// <param name="columns">SQL Server TVP columns in exact SQL type order.</param>
        /// <param name="changedRowsOnly">When true, only Added, Modified and Deleted rows are copied. For search/read procedures pass false or omit the argument.</param>
        /// <returns>Configured SQL structured parameter.</returns>
        public static SqlParameter CreateStructuredParameter(string parameterName, string typeName, IDataTable dataTable, IEnumerable<SqlServerTableValuedParameterColumn> columns, bool changedRowsOnly = false)
        {
            ValidateParameterArguments(parameterName, typeName);

            SqlParameter sqlParameter = new SqlParameter(parameterName, SqlDbType.Structured);
            sqlParameter.TypeName = typeName;
            sqlParameter.Value = CreateDataTable(dataTable, columns, changedRowsOnly);

            return sqlParameter;
        }

        /// <summary>
        /// Maps POCO row state to the stable SQL Server adapter change-state contract.
        /// </summary>
        /// <param name="dataRowState">POCO row state.</param>
        /// <returns>SQL Server adapter change-state value.</returns>
        public static SqlServerChangeState GetChangeState(PocoDataRowState dataRowState)
        {
            switch (dataRowState)
            {
                case PocoDataRowState.Added:
                    return SqlServerChangeState.Added;

                case PocoDataRowState.Modified:
                    return SqlServerChangeState.Modified;

                case PocoDataRowState.Deleted:
                    return SqlServerChangeState.Deleted;

                case PocoDataRowState.Unchanged:
                case PocoDataRowState.Detached:
                default:
                    return SqlServerChangeState.Unchanged;
            }
        }
        #endregion

        #region Private Methods
        static void ValidateParameterArguments(string parameterName, string typeName)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
            {
                throw new ArgumentException("Parameter name is required.", nameof(parameterName));
            }

            if (string.IsNullOrWhiteSpace(typeName))
            {
                throw new ArgumentException("SQL Server table type name is required.", nameof(typeName));
            }
        }

        static void AddBusinessColumns(AdoDataTable adoDataTable, IDataTable sourceTable)
        {
            for (int i = 0; i < sourceTable.Columns.Count; i++)
            {
                IColumnMetadata columnMetadata = sourceTable.Columns[i];
                AddColumn(adoDataTable, columnMetadata.ColumnName, columnMetadata.DataType);
            }
        }

        static void AddSchemaColumns(AdoDataTable adoDataTable, IEnumerable<SqlServerTableValuedParameterColumn> columns)
        {
            int columnCount = 0;

            foreach (SqlServerTableValuedParameterColumn column in columns)
            {
                if (column == null)
                {
                    continue;
                }

                AddColumn(adoDataTable, column.ColumnName, column.DataType);
                columnCount++;
            }

            if (columnCount == 0)
            {
                throw new ArgumentException("At least one SQL Server TVP column must be supplied.", nameof(columns));
            }
        }

        static void AddColumn(AdoDataTable adoDataTable, string columnName, string? dataType)
        {
            if (string.IsNullOrWhiteSpace(columnName))
            {
                throw new ArgumentException("Column name is required.", nameof(columnName));
            }

            if (adoDataTable.Columns.Contains(columnName))
            {
                throw new InvalidOperationException("Duplicate SQL Server TVP column name '" + columnName + "'.");
            }

            Type columnType = ResolveColumnType(dataType);
            DataColumn dataColumn = new DataColumn(columnName, columnType);
            dataColumn.AllowDBNull = true;
            adoDataTable.Columns.Add(dataColumn);
        }

        static void AddClientKeyColumn(AdoDataTable adoDataTable)
        {
            if (adoDataTable.Columns.Contains(SqlServerClientKeyColumn.ColumnName))
            {
                return;
            }

            DataColumn clientKeyColumn = new DataColumn(SqlServerClientKeyColumn.ColumnName, typeof(Guid));
            clientKeyColumn.AllowDBNull = true;
            adoDataTable.Columns.Add(clientKeyColumn);
        }

        static void AddChangeStateColumn(AdoDataTable adoDataTable)
        {
            if (adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName))
            {
                throw new InvalidOperationException(
                    "Source table already contains reserved SQL Server adapter metadata column '" +
                    SqlServerChangeStateColumn.ColumnName + "'.");
            }

            DataColumn changeStateColumn = new DataColumn(SqlServerChangeStateColumn.ColumnName, typeof(int));
            changeStateColumn.AllowDBNull = true;
            adoDataTable.Columns.Add(changeStateColumn);
        }

        static void AddRows(AdoDataTable adoDataTable, IDataTable sourceTable, bool changedRowsOnly)
        {
            for (int i = 0; i < sourceTable.Rows.Count; i++)
            {
                IDataRow sourceRow = sourceTable.Rows[i];
                SqlServerChangeState changeState = GetChangeState(sourceRow.DataRowState);

                if (changedRowsOnly && changeState == SqlServerChangeState.Unchanged)
                {
                    continue;
                }

                DataRow adoRow = adoDataTable.NewRow();

                for (int c = 0; c < adoDataTable.Columns.Count; c++)
                {
                    string columnName = adoDataTable.Columns[c].ColumnName;
                    object? value;
                    if (sourceRow.TryGetValue(columnName, out value))
                    {
                        adoRow[columnName] = ConvertNullToDbNull(value);
                    }
                    else
                    {
                        adoRow[columnName] = DBNull.Value;
                    }
                }

                if (adoDataTable.Columns.Contains(SqlServerClientKeyColumn.ColumnName) &&
                    adoRow[SqlServerClientKeyColumn.ColumnName] == DBNull.Value)
                {
                    adoRow[SqlServerClientKeyColumn.ColumnName] = Guid.NewGuid();
                }

                if (adoDataTable.Columns.Contains(SqlServerChangeStateColumn.ColumnName))
                {
                    adoRow[SqlServerChangeStateColumn.ColumnName] = (int)changeState;
                }

                adoDataTable.Rows.Add(adoRow);
            }
        }

        static object ConvertNullToDbNull(object? value)
        {
            if (value == null)
            {
                return DBNull.Value;
            }

            return value;
        }

        static bool TableHasSaveableRows(IDataTable dataTable)
        {
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                SqlServerChangeState changeState = GetChangeState(dataTable.Rows[i].DataRowState);
                if (changeState == SqlServerChangeState.Added ||
                    changeState == SqlServerChangeState.Modified ||
                    changeState == SqlServerChangeState.Deleted)
                {
                    return true;
                }
            }

            return false;
        }

        static Type ResolveColumnType(string? dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType))
            {
                return typeof(object);
            }

            string normalized = dataType.Trim();

            if (normalized.Equals("bit", StringComparison.OrdinalIgnoreCase) || normalized.Equals("bool", StringComparison.OrdinalIgnoreCase) || normalized.Equals("boolean", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Boolean", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(bool);
            }

            if (normalized.Equals("tinyint", StringComparison.OrdinalIgnoreCase) || normalized.Equals("byte", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Byte", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(byte);
            }

            if (normalized.Equals("smallint", StringComparison.OrdinalIgnoreCase) || normalized.Equals("short", StringComparison.OrdinalIgnoreCase) || normalized.Equals("int16", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Int16", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(short);
            }

            if (normalized.Equals("int", StringComparison.OrdinalIgnoreCase) || normalized.Equals("integer", StringComparison.OrdinalIgnoreCase) || normalized.Equals("int32", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Int32", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(int);
            }

            if (normalized.Equals("bigint", StringComparison.OrdinalIgnoreCase) || normalized.Equals("long", StringComparison.OrdinalIgnoreCase) || normalized.Equals("int64", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Int64", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(long);
            }

            if (normalized.Equals("real", StringComparison.OrdinalIgnoreCase) || normalized.Equals("single", StringComparison.OrdinalIgnoreCase) || normalized.Equals("float32", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Single", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(float);
            }

            if (normalized.Equals("float", StringComparison.OrdinalIgnoreCase) || normalized.Equals("double", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Double", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(double);
            }

            if (normalized.Equals("decimal", StringComparison.OrdinalIgnoreCase) || normalized.Equals("numeric", StringComparison.OrdinalIgnoreCase) || normalized.Equals("money", StringComparison.OrdinalIgnoreCase) || normalized.Equals("smallmoney", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(decimal);
            }

            if (normalized.Equals("System.Decimal", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(decimal);
            }

            if (normalized.Equals("date", StringComparison.OrdinalIgnoreCase) || normalized.Equals("datetime", StringComparison.OrdinalIgnoreCase) || normalized.Equals("datetime2", StringComparison.OrdinalIgnoreCase) || normalized.Equals("datetime2(7)", StringComparison.OrdinalIgnoreCase) || normalized.Equals("smalldatetime", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.DateTime", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(DateTime);
            }

            if (normalized.Equals("datetimeoffset", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.DateTimeOffset", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(DateTimeOffset);
            }

            if (normalized.Equals("time", StringComparison.OrdinalIgnoreCase) || normalized.Equals("timespan", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.TimeSpan", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(TimeSpan);
            }

            if (normalized.Equals("uniqueidentifier", StringComparison.OrdinalIgnoreCase) || normalized.Equals("guid", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Guid", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(Guid);
            }

            if (normalized.Equals("binary", StringComparison.OrdinalIgnoreCase) || normalized.Equals("varbinary", StringComparison.OrdinalIgnoreCase) || normalized.Equals("image", StringComparison.OrdinalIgnoreCase) || normalized.Equals("timestamp", StringComparison.OrdinalIgnoreCase) || normalized.Equals("rowversion", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.Byte[]", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(byte[]);
            }

            if (normalized.Equals("char", StringComparison.OrdinalIgnoreCase) || normalized.Equals("varchar", StringComparison.OrdinalIgnoreCase) || normalized.Equals("nchar", StringComparison.OrdinalIgnoreCase) || normalized.Equals("nvarchar", StringComparison.OrdinalIgnoreCase) || normalized.Equals("text", StringComparison.OrdinalIgnoreCase) || normalized.Equals("ntext", StringComparison.OrdinalIgnoreCase) || normalized.Equals("xml", StringComparison.OrdinalIgnoreCase) || normalized.Equals("string", StringComparison.OrdinalIgnoreCase) || normalized.Equals("System.String", StringComparison.OrdinalIgnoreCase))
            {
                return typeof(string);
            }

            Type? resolvedType = Type.GetType(normalized, false, true);
            if (resolvedType != null)
            {
                return resolvedType;
            }

            return typeof(object);
        }
        #endregion
    }
}
