using System;
using System.Collections.Generic;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides service parameters
    /// </summary>
    internal static class ServiceParametersProvider
    {
        #region Public Methods
        /// <summary>
        /// Adda original values concurrency clauses
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <param name="whereClauses">WHERE clauses</param>
        /// <param name="sqlParameters">SQL parameters</param>
        public static void AddOriginalValuesConcurrencyClauses(IDataTable dataTable, TableWriteMetadata tableWriteMetadata, IDataRow dataRow, List<string> whereClauses, List<SqlParameter> sqlParameters)
        {
            if (!dataRow.HasOriginalValues)
            {
                throw new InvalidOperationException("Optimistic concurrency requested but OriginalValues snapshot is missing.");
            }

            HashSet<string> excludedColumns = ColumnsBuilder.BuildExcludedColumns(dataTable, tableWriteMetadata);

            List<string> autoColumns = new List<string>();
            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                string columnName = dataTable.Columns[i].ColumnName;
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

                autoColumns.Add(columnName);
            }

            autoColumns.Sort(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < autoColumns.Count; i++)
            {
                AddSingleOriginalValueConcurrencyClause(autoColumns[i], dataRow, whereClauses, sqlParameters);
            }
        }

        /// <summary>
        /// Add row version concurrency if possible
        /// </summary>
        /// <param name="tableWriteMetadata">Table write metadata</param>
        /// <param name="dataRow">Data row</param>
        /// <param name="whereClauses">WHERE clauses</param>
        /// <param name="sqlParameters">SQL parameters</param>
        public static void AddRowVersionConcurrencyIfPossible(TableWriteMetadata tableWriteMetadata, IDataRow dataRow, List<string> whereClauses, List<SqlParameter> sqlParameters)
        {
            if (tableWriteMetadata.RowVersionColumns.Count != 1)
            {
                return;
            }

            string rvColumn = null!;
            foreach (string col in tableWriteMetadata.RowVersionColumns)
            {
                rvColumn = col;
                break;
            }

            object? rvValue = null;
            bool hasRv = false;

            // Prefer the OriginalValues snapshot when available (classic full changeset scenario)
            if (dataRow.HasOriginalValues && dataRow.OriginalValues != null)
            {
                hasRv = dataRow.OriginalValues.TryGetValue(rvColumn, out rvValue);
            }

            // Delta / floating rows may not have OriginalValues; rowversion should be provided directly.
            if (!hasRv)
            {
                if (dataRow.TryGetValue(rvColumn, out rvValue))
                {
                    hasRv = true;
                }
                else if (dataRow.TryGetOriginalValue(rvColumn, out rvValue))
                {
                    hasRv = true;
                }
            }

            if (!hasRv)
            {
                throw new InvalidOperationException("Optimistic concurrency requested but rowversion value is missing for column '" + rvColumn + "'.");
            }

            string parameterName = "@oc_rv";
            whereClauses.Add(SqlCommandBuilder.EscapeIdentifier(rvColumn) + " = " + parameterName);
            sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, rvValue));
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds single original value concurrency clause
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataRow">Data row</param>
        /// <param name="whereClauses">WHERE clauses</param>
        /// <param name="sqlParameters">SQL parameters</param>
        static void AddSingleOriginalValueConcurrencyClause(string columnName, IDataRow dataRow, List<string> whereClauses, List<SqlParameter> sqlParameters)
        {
            object? origValue;
            if (!dataRow.OriginalValues.TryGetValue(columnName, out origValue))
            {
                throw new InvalidOperationException("Optimistic concurrency requested but OriginalValues does not contain column '" + columnName + "'.");
            }

            string parameterName = "@oc" + sqlParameters.Count;
            string escaped = SqlCommandBuilder.EscapeIdentifier(columnName);
            whereClauses.Add("(" + escaped + " = " + parameterName + " OR (" + escaped + " IS NULL AND " + parameterName + " IS NULL))");
            sqlParameters.Add(ParametersProvider.CreateSqlParameter(parameterName, origValue));
        }
        #endregion
    }
}
