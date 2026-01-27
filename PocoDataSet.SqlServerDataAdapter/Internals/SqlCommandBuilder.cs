using System;
using System.Collections.Generic;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    internal static class SqlCommandBuilder
    {
        #region Public Methods
        /// <summary>
        /// Builds DELETE command
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <returns>Built DELETE command</returns>
        internal static SqlCommand BuildDeleteCommand(IDataTable table, TableWriteMetadata tableWriteMetadata, IDataRow dataRow)
        {
            PrimaryKeyProcessor.ValidatePrimaryKeysExistence(tableWriteMetadata, table.TableName);

            List<string> whereClauses = new List<string>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            for (int i = 0; i < tableWriteMetadata.PrimaryKeyColumns.Count; i++)
            {
                string pkColumnName = tableWriteMetadata.PrimaryKeyColumns[i];
                string parameterName = "@pk" + i;

                whereClauses.Add(SqlCommandBuilder.EscapeIdentifier(pkColumnName) + " = " + parameterName);

                object? pkValue;
                if (!dataRow.TryGetValue(pkColumnName, out pkValue))
                {
                    dataRow.TryGetOriginalValue(pkColumnName, out pkValue);
                }

                sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, pkValue));
            }

            HashSet<string> excludedColumns = ColumnsBuilder.BuildExcludedColumns(table, tableWriteMetadata);

            if (tableWriteMetadata.RowVersionColumns.Count == 1)
            {
                ServiceParametersProvider.AddRowVersionConcurrencyIfPossible(tableWriteMetadata, dataRow, whereClauses, sqlParameters);
            }
            else
            {
                ServiceParametersProvider.AddOriginalValuesConcurrencyClauses(table, tableWriteMetadata, dataRow, whereClauses, sqlParameters);
            }

            string sql = "DELETE FROM " + SqlCommandBuilder.EscapeIdentifier(table.TableName) + " WHERE " + string.Join(" AND ", whereClauses);
            SqlCommand sqlCommand = new SqlCommand(sql);

            for (int i = 0; i < sqlParameters.Count; i++)
            {
                sqlCommand.Parameters.Add(sqlParameters[i]);
            }

            return sqlCommand;
        }

        /// <summary>
        /// Builds INSERT command
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <param name="outputColumns">Output columns</param>
        /// <returns>Built INSERT command</returns>
        internal static SqlCommand BuildInsertCommand(IDataTable dataTable, TableWriteMetadata tableWriteMetadata, IDataRow dataRow, List<string> outputColumns)
        {
            List<string> columnNames = new List<string>();
            List<string> parameterNames = new List<string>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();

            HashSet<string> excludedColumns = ColumnsBuilder.BuildExcludedColumns(dataTable, tableWriteMetadata);
            int parameterIndex = 0;

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                string columnName = dataTable.Columns[i].ColumnName;
                if (!tableWriteMetadata.ColumnNames.Contains(columnName))
                {
                    // Column does not exist in server schema
                    continue;
                }
                if (excludedColumns.Contains(columnName))
                {
                    continue;
                }

                object? value;
                if (!dataRow.TryGetValue(columnName, out value))
                {
                    // Floating/delta rows: missing field means "not provided" -> omit it from INSERT.
                    continue;
                }

                string parameterName = "@p" + parameterIndex;
                parameterIndex++;

                columnNames.Add(EscapeIdentifier(columnName));
                parameterNames.Add(parameterName);
                sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, value));
            }

            if (columnNames.Count == 0)
            {
                throw new InvalidOperationException("Cannot INSERT into table '" + dataTable.TableName + "': no writable columns found.");
            }

            string outputClause = string.Empty;
            if (outputColumns != null && outputColumns.Count > 0)
            {
                List<string> outputParts = new List<string>();
                for (int i = 0; i < outputColumns.Count; i++)
                {
                    outputParts.Add("INSERTED." + EscapeIdentifier(outputColumns[i]));
                }
                outputClause = " OUTPUT " + string.Join(", ", outputParts) + " ";
            }

            string sql = "INSERT INTO " + EscapeIdentifier(dataTable.TableName) + " (" + string.Join(",", columnNames) + ")" + outputClause + "VALUES (" + string.Join(",", parameterNames) + ")";
            SqlCommand sqlCommand = new SqlCommand(sql);

            for (int i = 0; i < sqlParameters.Count; i++)
            {
                sqlCommand.Parameters.Add(sqlParameters[i]);
            }

            return sqlCommand;
        }

        internal static string EscapeIdentifier(string identifier)
        {
            // Simple SQL Server escaping.
            return "[" + identifier.Replace("]", "]]") + "]";
        }

        /// <summary>
        /// Builds UPDATE command
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <param name="outputColumns">Output columns</param>
        /// <returns>Built UPDATE command</returns>
        internal static SqlCommand BuildUpdateCommand(IDataTable table, TableWriteMetadata tableWriteMetadata, IDataRow dataRow, List<string> outputColumns)
        {
            PrimaryKeyProcessor.ValidatePrimaryKeysExistence(tableWriteMetadata, table.TableName);

            HashSet<string> excludedColumns = ColumnsBuilder.BuildExcludedColumns(table, tableWriteMetadata);

            List<string> setClauses = new List<string>();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            int parameterIndex = 0;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string columnName = table.Columns[i].ColumnName;
                if (!tableWriteMetadata.ColumnNames.Contains(columnName))
                {
                    continue;
                }
                if (tableWriteMetadata.PrimaryKeys.Contains(columnName))
                {
                    continue;
                }
                if (excludedColumns.Contains(columnName))
                {
                    continue;
                }

                object? value;
                if (!dataRow.TryGetValue(columnName, out value))
                {
                    // Floating/delta rows: missing field means "not provided" -> do not update it.
                    continue;
                }

                string parameterName = "@p" + parameterIndex;
                parameterIndex++;

                setClauses.Add(SqlCommandBuilder.EscapeIdentifier(columnName) + " = " + parameterName);
                sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, value));
            }

            if (setClauses.Count == 0)
            {
                return new SqlCommand(string.Empty);
            }

            List<string> whereClauses = new List<string>();
            for (int i = 0; i < tableWriteMetadata.PrimaryKeyColumns.Count; i++)
            {
                string pkColumnName = tableWriteMetadata.PrimaryKeyColumns[i];
                string parameterName = "@pk" + i;

                whereClauses.Add(SqlCommandBuilder.EscapeIdentifier(pkColumnName) + " = " + parameterName);

                object? pkValue;
                if (!dataRow.TryGetValue(pkColumnName, out pkValue))
                {
                    dataRow.TryGetOriginalValue(pkColumnName, out pkValue);
                }
                sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, pkValue));
            }

            if (tableWriteMetadata.RowVersionColumns.Count == 1)
            {
                ServiceParametersProvider.AddRowVersionConcurrencyIfPossible(tableWriteMetadata, dataRow, whereClauses, sqlParameters);
            }
            else
            {
                ServiceParametersProvider.AddOriginalValuesConcurrencyClauses(table, tableWriteMetadata, dataRow, whereClauses, sqlParameters);
            }

            string outputClause = string.Empty;
            if (outputColumns != null && outputColumns.Count > 0)
            {
                List<string> outputParts = new List<string>();
                for (int i = 0; i < outputColumns.Count; i++)
                {
                    outputParts.Add("INSERTED." + SqlCommandBuilder.EscapeIdentifier(outputColumns[i]));
                }
                outputClause = " OUTPUT " + string.Join(", ", outputParts) + " ";
            }

            string sql = "UPDATE " + SqlCommandBuilder.EscapeIdentifier(table.TableName) + " SET " + string.Join(", ", setClauses) + outputClause + "WHERE " + string.Join(" AND ", whereClauses);
            SqlCommand sqlCommand = new SqlCommand(sql);

            for (int i = 0; i < sqlParameters.Count; i++)
            {
                sqlCommand.Parameters.Add(sqlParameters[i]);
            }

            return sqlCommand;
        }
        #endregion
    }
}