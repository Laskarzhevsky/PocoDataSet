using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces rows in the current observable data set with rows from the refreshed data set.
        /// Keeps existing schema and ensures the internal tracking column _ClientKey exists.
        /// This is a destructive operation intended for search results and full reload scenarios.
        /// After completion, all loaded observable rows are in Unchanged state.
        /// </summary>
        /// <param name="observableDataSet">Observable data set to reload</param>
        /// <param name="refreshedDataSet">Refreshed data set snapshot</param>
        public static void ReloadFrom(this IObservableDataSet observableDataSet, IDataSet refreshedDataSet)
        {
            if (observableDataSet == null)
            {
                throw new ArgumentNullException(nameof(observableDataSet));
            }

            if (refreshedDataSet == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataSet));
            }

            // 1) Clear rows from all existing tables (keep schema intact).
            foreach (IObservableDataTable existingTable in observableDataSet.Tables.Values)
            {
                ClearObservableTableRows(existingTable);
            }

            // 2) Ensure refreshed tables exist, ensure schema columns exist, then load rows.
            foreach (KeyValuePair<string, IDataTable> kvp in refreshedDataSet.Tables)
            {
                string tableName = kvp.Key;
                IDataTable refreshedTable = kvp.Value;

                IObservableDataTable observableTable = GetOrCreateObservableTable(observableDataSet, tableName);

                EnsureObservableColumnsExist(observableTable, refreshedTable.Columns);

                // Primary keys: mirror refreshed table PK list on the inner table.
                if (refreshedTable.PrimaryKeys != null)
                {
                    List<string> pk = new List<string>();
                    for (int i = 0; i < refreshedTable.PrimaryKeys.Count; i++)
                    {
                        pk.Add(refreshedTable.PrimaryKeys[i]);
                    }
                    observableTable.InnerDataTable.PrimaryKeys = pk;
                }

                EnsureClientKeyColumnExists(observableTable);

                // Load rows
                for (int i = 0; i < refreshedTable.Rows.Count; i++)
                {
                    IDataRow refreshedRow = refreshedTable.Rows[i];

                    IObservableDataRow newRow = observableTable.AddNewRow();

                    // Copy refreshed values using refreshed schema columns (schema-evolution safe).
                    newRow.CopyFrom(refreshedRow, refreshedTable.Columns);

                    // Assign client key (always new identity for reloaded rows).
                    if (ContainsColumn(observableTable.Columns, SpecialColumnNames.CLIENT_KEY))
                    {
                        newRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
                    }

                    // Loaded baseline
                    newRow.AcceptChanges();
                }
            }
        }
        #endregion

        #region Private Methods
        private static IObservableDataTable GetOrCreateObservableTable(IObservableDataSet observableDataSet, string tableName)
        {
            // Do NOT rely only on dictionary key lookup:
            // some implementations may store a different key than TableName.
            // Always fall back to TableName scan to prevent false negatives.
            if (observableDataSet.Tables.ContainsKey(tableName))
            {
                return observableDataSet.Tables[tableName];
            }

            foreach (IObservableDataTable table in observableDataSet.Tables.Values)
            {
                if (table == null)
                {
                    continue;
                }

                if (string.Equals(table.TableName, tableName, StringComparison.OrdinalIgnoreCase))
                {
                    return table;
                }
            }

            return observableDataSet.AddNewTable(tableName);
        }

        private static void ClearObservableTableRows(IObservableDataTable table)
        {
            if (table == null)
            {
                return;
            }

            for (int i = table.Rows.Count - 1; i >= 0; i--)
            {
                table.RemoveRowAt(i);
            }
        }

        private static void EnsureObservableColumnsExist(IObservableDataTable targetTable, IList<IColumnMetadata> sourceColumns)
        {
            for (int i = 0; i < sourceColumns.Count; i++)
            {
                IColumnMetadata c = sourceColumns[i];

                if (!ContainsColumn(targetTable.Columns, c.ColumnName))
                {
                    targetTable.AddColumn(c.ColumnName, c.DataType, c.IsNullable, c.IsPrimaryKey, c.IsForeignKey);
                }
            }
        }

        private static void EnsureClientKeyColumnExists(IObservableDataTable table)
        {
            if (!ContainsColumn(table.Columns, SpecialColumnNames.CLIENT_KEY))
            {
                table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID, false, false, false);
            }
        }

        private static bool ContainsColumn(IList<IColumnMetadata> columns, string columnName)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                if (string.Equals(columns[i].ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
