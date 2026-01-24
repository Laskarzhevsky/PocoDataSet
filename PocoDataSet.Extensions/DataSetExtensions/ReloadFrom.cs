using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces rows in the current data set with rows from the refreshed data set.
        /// Keeps existing schema and ensures the internal tracking column _ClientKey exists.
        /// This is a destructive operation intended for search results and full reload scenarios.
        /// After completion, all loaded rows are in Unchanged state.
        /// </summary>
        /// <param name="currentDataSet">Current data set to reload</param>
        /// <param name="refreshedDataSet">Refreshed data set snapshot</param>
        public static void ReloadFrom(this IDataSet currentDataSet, IDataSet refreshedDataSet)
        {
            if (currentDataSet == null)
            {
                throw new ArgumentNullException(nameof(currentDataSet));
            }

            if (refreshedDataSet == null)
            {
                throw new ArgumentNullException(nameof(refreshedDataSet));
            }

            // 1) Clear rows from all existing tables (keep schema intact).
            foreach (IDataTable existingTable in currentDataSet.Tables.Values)
            {
                ClearTableRows(existingTable);
            }

            // 2) Ensure refreshed tables exist, ensure schema columns exist, then load rows.
            foreach (KeyValuePair<string, IDataTable> kvp in refreshedDataSet.Tables)
            {
                string tableName = kvp.Key;
                IDataTable refreshedTable = kvp.Value;

                IDataTable currentTable;
                if (currentDataSet.Tables.ContainsKey(tableName))
                {
                    currentTable = currentDataSet.Tables[tableName];
                }
                else
                {
                    currentTable = currentDataSet.AddNewTable(tableName);
                }

                EnsureColumnsExist(currentTable, refreshedTable.Columns);

                // Primary keys: mirror refreshed table PK list.
                if (refreshedTable.PrimaryKeys != null)
                {
                    List<string> pk = new List<string>();
                    for (int i = 0; i < refreshedTable.PrimaryKeys.Count; i++)
                    {
                        pk.Add(refreshedTable.PrimaryKeys[i]);
                    }
                    currentTable.SetPrimaryKeys(pk);
                }

                EnsureClientKeyColumnExists(currentTable);

                // Load rows
                for (int i = 0; i < refreshedTable.Rows.Count; i++)
                {
                    IDataRow refreshedRow = refreshedTable.Rows[i];

                    IDataRow newRow = DataRowExtensions.CreateRowFromColumns(currentTable.Columns);

                    // Copy refreshed values using refreshed schema columns (schema-evolution safe).
                    newRow.CopyFrom(refreshedRow, refreshedTable.Columns);

                    // Assign client key (always new identity for reloaded rows).
                    if (ContainsColumn(currentTable.Columns, SpecialColumnNames.CLIENT_KEY))
                    {
                        newRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
                    }

                    currentTable.AddLoadedRow(newRow);
                }
            }
        }
        #endregion

        #region Private Methods
        private static void ClearTableRows(IDataTable table)
        {
            if (table == null)
            {
                return;
            }

            // IDataTable.Rows is read-only list; remove rows via table-level operations.
            while (table.Rows.Count > 0)
            {
                table.RemoveRowAt(table.Rows.Count - 1);
            }
        }

        private static void EnsureColumnsExist(IDataTable targetTable, IList<IColumnMetadata> sourceColumns)
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

        private static void EnsureClientKeyColumnExists(IDataTable table)
        {
            if (!ContainsColumn(table.Columns, SpecialColumnNames.CLIENT_KEY))
            {
                // GUID not nullable, not PK, not FK
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
