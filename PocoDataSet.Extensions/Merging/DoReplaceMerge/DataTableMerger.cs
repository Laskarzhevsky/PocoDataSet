using PocoDataSet.Data;
using PocoDataSet.IData;

using System;

namespace PocoDataSet.Extensions.Merging.DoReplaceMerge
{
    /// <summary>
    /// Replace table merge: destructive reload (clear all rows and re-add refreshed rows as Unchanged).
    /// </summary>
    public sealed class DataTableMerger
    {
        #region Public Methods
        /// <summary>
        /// Merges the refreshed data table into the current data table using the "DoReplaceMerge" strategy,
        /// which clears all existing rows in the current data table and adds all rows from the refreshed data table as Unchanged.
        /// This method does not consider any existing data in the current data table and simply replaces it with the refreshed data.
        /// The merge options can be used to track added rows and specify any additional merge behavior.
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            ValidateSchemaCompatibilityForReplace(currentDataTable, refreshedDataTable);
            currentDataTable.RemoveAllRows();

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];

                IDataRow newRow = DataRowFactory.CreateEmpty(refreshedRow.Values.Count);
//                newRow.DoReplaceMerge(refreshedRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                DataRowMerger merger = new DataRowMerger();
                merger.Merge(newRow, refreshedRow, currentDataTable.Columns);

                currentDataTable.AddLoadedRow(newRow);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newRow));
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// FindColumnByName
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnName"></param>
        /// <returns></returns>
        private static IColumnMetadata? FindColumnByName(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, columnName, StringComparison.Ordinal))
                {
                    return table.Columns[i];
                }
            }

            return null;
        }

        private static void ValidateSchemaCompatibilityForReplace(IDataTable currentDataTable, IDataTable refreshedDataTable)
        {
            // Policy A: current schema is authoritative.
            // Extra columns in refreshed are ignored.
            // If a column exists in BOTH and DataType differs, reject to avoid silent corruption.

            for (int i = 0; i < currentDataTable.Columns.Count; i++)
            {
                IColumnMetadata currentColumn = currentDataTable.Columns[i];
                IColumnMetadata? refreshedColumn = FindColumnByName(refreshedDataTable, currentColumn.ColumnName);

                if (refreshedColumn == null)
                {
                    continue; // refreshed missing column => allowed
                }

                if (!string.Equals(currentColumn.DataType, refreshedColumn.DataType, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(
                        "Replace merge schema mismatch for table '" + currentDataTable.TableName +
                        "': column '" + currentColumn.ColumnName +
                        "' has current type '" + currentColumn.DataType +
                        "' but refreshed type '" + refreshedColumn.DataType + "'.");
                }
            }
        }
        #endregion
    }
}
