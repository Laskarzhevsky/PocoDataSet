using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable data table default merge handler functionality
    /// </summary>
    public class ObservableDataTableDefaultMergeHandler : IObservableDataTableMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current observable data table with refreshed data table
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public void Merge(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            List<string> currentObservableDataTablePrimaryKeyColumnNames = currentObservableDataTable.GetPrimaryKeyColumnNames(observableMergeOptions);
            if (currentObservableDataTablePrimaryKeyColumnNames.Count == 0)
            {
                // Current table has no primary key (treat as reload/replace).
                MergeObservableDataRowsWithoutPrimaryKeys(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
            }
            else
            {
                // Current table has primary key
                Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildDataRowIndex(currentObservableDataTablePrimaryKeyColumnNames);
                HashSet<string> primaryKeysOfMergedDataRows = new HashSet<string>();

                MergeCurrentObservableDataTableRows(currentObservableDataTable, refreshedDataTable, observableMergeOptions, currentObservableDataTablePrimaryKeyColumnNames, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
                MergeNonProcessedDataRowsFromRefreshedDataTable(currentObservableDataTable, refreshedDataTable, observableMergeOptions, refreshedDataTableDataRowIndex, primaryKeysOfMergedDataRows);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds new data row with default values to the inner data table and returns the row.
        /// The row is immediately put into Unchanged state, because it represents server-origin data.
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>New row with default values added to data table</returns>
        private IDataRow AddNewDataRowWithDefaultValuesToInnerTable(IObservableDataTable observableDataTable, IObservableMergeOptions observableMergeOptions)
        {
            IDataRow newDataRow = observableDataTable.InnerDataTable.AddNewRow();
            foreach (IColumnMetadata columnMetadata in observableDataTable.Columns)
            {
                newDataRow[columnMetadata.ColumnName] = observableMergeOptions.DataTypeDefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
            }

            // This row is being created from refreshed/server data, not from user entry.
            newDataRow.AcceptChanges();

            return newDataRow;
        }

        /// <summary>
        /// Merges current observable data table rows
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="currentObservableDataTablePrimaryKeyColumnNames">Current observable data table primary key column names</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeCurrentObservableDataTableRows(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, List<string> currentObservableDataTablePrimaryKeyColumnNames, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                IObservableDataRow observableDataRow = currentObservableDataTable.Rows[i];
                string observableDataRowPrimaryKeyValue = observableDataRow.CompilePrimaryKeyValue(currentObservableDataTablePrimaryKeyColumnNames);

                IDataRow? refreshedDataRow = null;
                refreshedDataTableDataRowIndex.TryGetValue(observableDataRowPrimaryKeyValue, out refreshedDataRow);
                if (refreshedDataRow == null)
                {
                    bool shouldPreserveRow = false;

                    // Refresh must preserve local pending changes (Added/Modified/Deleted).
                    if (observableMergeOptions.MergeMode == MergeMode.Refresh)
                    {
                        if (observableDataRow.InnerDataRow.DataRowState != DataRowState.Unchanged)
                        {
                            shouldPreserveRow = true;
                        }
                    }

                    // PostSave should never treat "missing from refreshed" as deletion unless you explicitly opt into it.
                    if (observableMergeOptions.MergeMode == MergeMode.PostSave)
                    {
                        shouldPreserveRow = true;
                    }

                    if (shouldPreserveRow)
                    {
                        // Keep row intact
                    }
                    else if (observableMergeOptions.ExcludeTablesFromRowDeletion.Contains(currentObservableDataTable.TableName))
                    {
                        // Keep row intact
                    }
                    else
                    {
                        IObservableDataRow removedObservableDataRow = currentObservableDataTable.RemoveRow(i);
                        observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, removedObservableDataRow));
                    }
                }
                else
                {
                    bool changed = observableDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.InnerDataTable.Columns, observableMergeOptions);
                    if (changed)
                    {
                        observableMergeOptions.ObservableDataSetMergeResult.UpdatedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
                    }

                    primaryKeysOfMergedDataRows.Add(observableDataRowPrimaryKeyValue);
                }
            }
        }

        /// <summary>
        /// Reload semantics for tables without primary keys:
        /// remove all current rows and add all refreshed rows.
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        void MergeObservableDataRowsWithoutPrimaryKeys(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            // Track deletions BEFORE clearing the table
            for (int i = 0; i < currentObservableDataTable.Rows.Count; i++)
            {
                observableMergeOptions.ObservableDataSetMergeResult.DeletedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, currentObservableDataTable.Rows[i]));
            }

            // Clear current rows
            for (int i = currentObservableDataTable.Rows.Count - 1; i >= 0; i--)
            {
                currentObservableDataTable.RemoveRow(i);
            }

            // Add all refreshed rows as new rows
            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedDataRow = refreshedDataTable.Rows[i];
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);

                // Copy refreshed values (this will AcceptChanges in Refresh/PostSave via the row handler).
                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);
                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }

        /// <summary>
        /// Adds rows from refreshed table which had not been merged already
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="refreshedDataTableDataRowIndex">Refreshed data table data row index</param>
        /// <param name="primaryKeysOfMergedDataRows">Primary keys of merged data rows</param>
        void MergeNonProcessedDataRowsFromRefreshedDataTable(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions, Dictionary<string, IDataRow> refreshedDataTableDataRowIndex, HashSet<string> primaryKeysOfMergedDataRows)
        {
            foreach (KeyValuePair<string, IDataRow> keyValuePair in refreshedDataTableDataRowIndex)
            {
                string primaryKeyValue = keyValuePair.Key;
                if (primaryKeysOfMergedDataRows.Contains(primaryKeyValue))
                {
                    continue;
                }

                IDataRow refreshedDataRow = keyValuePair.Value;
                IDataRow newDataRow = AddNewDataRowWithDefaultValuesToInnerTable(currentObservableDataTable, observableMergeOptions);

                // Copy refreshed values (row handler will commit baseline).
                newDataRow.MergeWith(refreshedDataRow, currentObservableDataTable.TableName, currentObservableDataTable.Columns, observableMergeOptions);
                IObservableDataRow observableDataRow = currentObservableDataTable.AddRow(newDataRow);
                observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(currentObservableDataTable.TableName, observableDataRow));
            }
        }
        #endregion
    }
}
