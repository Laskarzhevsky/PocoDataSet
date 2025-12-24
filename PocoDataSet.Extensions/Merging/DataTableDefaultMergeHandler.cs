using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data table default merge handler functionality
    /// </summary>
    public class DataTableDefaultMergeHandler : ITableMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current table with refreshed table.
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public void Merge(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            List<string> currentDataTablePrimaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentDataTable);
            if (currentDataTablePrimaryKeyColumnNames.Count == 0)
            {
                // Current table has no primary key
                MergeDataRowsWithoutPrimaryKeys(currentDataTable, refreshedDataTable, mergeOptions);
            }
            else
            {
                // Current table has primary key
                Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildDataRowIndex(currentDataTablePrimaryKeyColumnNames);
                HashSet<string> primaryKeysOfMergedDataRows = new HashSet<string>();

                MergeCurrentDataTableRows(currentDataTable, refreshedDataTable, mergeOptions, currentDataTablePrimaryKeyColumnNames, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
                MergeNonProcessedDataRowsFromRefreshedDataTable(currentDataTable, refreshedDataTable, mergeOptions, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds new data row with default values to data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>New row with default values added to data table</returns>
        private IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable, IMergeOptions mergeOptions)
        {
            // Create a detached row
            IDataRow newDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);

            // Add to table (still Detached)
            dataTable.Rows.Add(newDataRow);

            // This row represents server data, not a user "Added" row
            newDataRow.DataRowState = DataRowState.Unchanged;

            return newDataRow;
        }
        
        /// <summary>
        /// Merges current data table rows
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="currentDataTablePrimaryKeyColumnNames">Current data table primary key column names</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeCurrentDataTableRows(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions, List<string> currentDataTablePrimaryKeyColumnNames, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            // 1) Merge or delete existing rows
            for (int i = currentDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow currentDataRow = currentDataTable.Rows[i];
                string primaryKeyValue = currentDataRow.CompilePrimaryKeyValue(currentDataTablePrimaryKeyColumnNames);

                IDataRow? refreshedDataRow;
                refreshedDataTableDataRowIndex.TryGetValue(primaryKeyValue, out refreshedDataRow);
                if (refreshedDataRow == null)
                {
                    if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(currentDataTable.TableName))
                    {
                        // Keep current row intact
                    }
                    else
                    {
                        currentDataTable.Rows.RemoveAt(i);
                        mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataRow));
                    }
                }
                else
                {
                    bool changed = MergeDataRowFromRefreshedDataRow(currentDataRow, refreshedDataRow, currentDataTable.Columns);
                    if (changed)
                    {
                        mergeOptions.DataSetMergeResult.UpdatedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataRow));
                    }

                    primaryKeysOfMergedDataRows.Add(primaryKeyValue);
                }
            }
        }

        /// <summary>
        /// Merges data row from refreshed data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        private static bool MergeDataRowFromRefreshedDataRow(IDataRow currentDataRow, IDataRow refreshedDataRow, IList<IColumnMetadata> listOfColumnMetadata)
        {
            bool changed = false;
            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;
                object? oldValue = currentDataRow[columnName];
                object? newValue = refreshedDataRow[columnName];
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentDataRow[columnName] = newValue;
                changed = true;
            }

            if (changed)
            {
                currentDataRow.AcceptChanges(); // refreshed values become baseline
            }

            return changed;
        }


        /// <summary>
        /// Merges data rows without primary keys
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        void MergeDataRowsWithoutPrimaryKeys(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
        {
            // Track deletions BEFORE clearing the table
            for (int i = 0; i < currentDataTable.Rows.Count; i++)
            {
                mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataTable.Rows[i]));
            }

            // Clear current rows
            currentDataTable.Rows.Clear();

            // Add all refreshed rows as new rows
            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedDataTable.Rows[i];
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);

                MergeDataRowFromRefreshedDataRow(newDataRow, refreshedRow, currentDataTable.Columns);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newDataRow));
            }
        }

        /// <summary>
        /// Merges non-processed data rows from refreshed data table
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeNonProcessedDataRowsFromRefreshedDataTable(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            foreach (KeyValuePair<string, IDataRow> keyValuePair in refreshedDataTableDataRowIndex)
            {
                string primaryKeyValue = keyValuePair.Key;
                if (primaryKeysOfMergedDataRows.Contains(primaryKeyValue))
                {
                    continue;
                }

                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);
                IDataRow refreshedDataRow = keyValuePair.Value;

                newDataRow.MergeWith(refreshedDataRow, currentDataTable.TableName, currentDataTable.Columns, mergeOptions);
                mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newDataRow));
            }
        }
        #endregion
    }
}
