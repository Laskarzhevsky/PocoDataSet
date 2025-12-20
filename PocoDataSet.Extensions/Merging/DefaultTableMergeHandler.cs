using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides default table merge handler functionality
    /// </summary>
    public class DefaultTableMergeHandler : ITableMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current table with refreshed table.
        /// </summary>
        /// <param name="currentTable">Current table</param>
        /// <param name="refreshedTable">Refreshed table</param>
        /// <param name="mergeOptions">Merge options.</param>
        public void Merge(IDataTable currentTable, IDataTable refreshedTable, IMergeOptions mergeOptions)
        {
            if (currentTable == null || refreshedTable == null)
            {
                return;
            }

            List<string> primaryKeyColumnNames = mergeOptions.GetPrimaryKeyColumnNames(currentTable);
            if (primaryKeyColumnNames == null)
            {
                primaryKeyColumnNames = new List<string>();
            }

            // If no primary key, optionally replace all rows.
            if (primaryKeyColumnNames.Count == 0)
            {
                if (mergeOptions.ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined)
                {
                    currentTable.ReplaceAllRowsByRowsFrom(refreshedTable, mergeOptions);
                }
                else
                {
                    // No PK and no replace policy: fall back to "append missing" based on refreshed rows count.
                    // We will add all refreshed rows as new rows.
                    for (int i = 0; i < refreshedTable.Rows.Count; i++)
                    {
                        IDataRow refreshedRow = refreshedTable.Rows[i];
                        IDataRow newRow = currentTable.AddNewRow();
                        foreach (IColumnMetadata columnMetadata in currentTable.Columns)
                        {
                            newRow[columnMetadata.ColumnName] = mergeOptions.DataTypeDefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                        }

                        DataRowExtensions.MergeWith(newRow, refreshedRow, currentTable.TableName, currentTable.Columns, mergeOptions);
                        mergeOptions.DataSetMergeResult.ListOfAddedDataRows.Add(newRow);
                    }
                }

                return;
            }

            Dictionary<string, IDataRow> refreshedIndex = refreshedTable.BuildDataRowIndex(primaryKeyColumnNames);
            HashSet<string> matchedPrimaryKeys = new HashSet<string>();

            // 1) Merge or delete existing rows
            for (int i = currentTable.Rows.Count - 1; i >= 0; i--)
            {
                IDataRow currentRow = currentTable.Rows[i];
                string pkValue = currentRow.CompilePrimaryKeyValue(primaryKeyColumnNames);

                IDataRow? refreshedRow;
                refreshedIndex.TryGetValue(pkValue, out refreshedRow);

                if (refreshedRow == null)
                {
                    bool allowDeletion = true;
                    if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(currentTable.TableName))
                    {
                        allowDeletion = false;
                    }

                    if (allowDeletion)
                    {
                        currentTable.Rows.RemoveAt(i);
                        mergeOptions.DataSetMergeResult.ListOfDeletedDataRows.Add(currentRow);
                    }
                }
                else
                {
                    IRowMergeHandler rowHandler = mergeOptions.GetRowMergeHandler(currentTable.TableName);
                    bool changed = rowHandler.MergeRow(currentTable.TableName, currentRow, refreshedRow, currentTable.Columns, mergeOptions);
                    if (changed)
                    {
                        mergeOptions.DataSetMergeResult.ListOfUpdatedDataRows.Add(currentRow);
                    }

                    matchedPrimaryKeys.Add(pkValue);
                }
            }

            // 2) Add new rows from refreshed
            for (int i = 0; i < refreshedTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedTable.Rows[i];
                string pkValue = refreshedRow.CompilePrimaryKeyValue(primaryKeyColumnNames);

                if (matchedPrimaryKeys.Contains(pkValue))
                {
                    continue;
                }

                IDataRow newDataRow = currentTable.AddNewRow();
                foreach (IColumnMetadata columnMetadata in currentTable.Columns)
                {
                    newDataRow[columnMetadata.ColumnName] = mergeOptions.DataTypeDefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                }

                DataRowExtensions.MergeWith(newDataRow, refreshedRow, currentTable.TableName, currentTable.Columns, mergeOptions);
                mergeOptions.DataSetMergeResult.ListOfAddedDataRows.Add(newDataRow);
            }
        }
        #endregion
    }
}
