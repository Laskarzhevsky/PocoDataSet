using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IDataMerge;

namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Default table merge handler.
    /// </summary>
    public class DefaultTableMergeHandler : ITableMergeHandler
    {
        /// <inheritdoc/>
        public void Merge(IDataTable currentTable, IDataTable refreshedTable, IMergeContext dataSetContext)
        {
            if (currentTable == null || refreshedTable == null)
            {
                return;
            }

            List<string> primaryKeyColumnNames = dataSetContext.GetPrimaryKeyColumnNames(currentTable);
            if (primaryKeyColumnNames == null)
            {
                primaryKeyColumnNames = new List<string>();
            }

            // If no primary key, optionally replace all rows.
            if (primaryKeyColumnNames.Count == 0)
            {
                if (dataSetContext.Configuration.ReplaceAllRowsInTableWhenTableHasNoPrimaryKeyDefined)
                {
                    currentTable.ReplaceAllRowsByRowsFrom(refreshedTable);
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
                            newRow[columnMetadata.ColumnName] = dataSetContext.Configuration.DefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                        }

                        dataSetContext.Configuration.DefaultRowMergeHandler.MergeRow(currentTable.TableName, newRow, refreshedRow, currentTable.Columns, dataSetContext);
                        dataSetContext.Result.ListOfAddedDataRows.Add(newRow);
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
                    if (dataSetContext.Configuration.ExcludeTablesFromRowDeletion.Contains(currentTable.TableName))
                    {
                        allowDeletion = false;
                    }

                    if (allowDeletion)
                    {
                        currentTable.Rows.RemoveAt(i);
                        dataSetContext.Result.ListOfDeletedDataRows.Add(currentRow);
                    }
                }
                else
                {
                    IRowMergeHandler rowHandler = dataSetContext.Configuration.DefaultRowMergeHandler;
                    IRowMergeHandler? specificRowHandler;
                    if (dataSetContext.Configuration.RowHandlersByTableName.TryGetValue(currentTable.TableName, out specificRowHandler))
                    {
                        if (specificRowHandler != null)
                        {
                            rowHandler = specificRowHandler;
                        }
                    }

                    bool changed = rowHandler.MergeRow(currentTable.TableName, currentRow, refreshedRow, currentTable.Columns, dataSetContext);
                    if (changed)
                    {
                        dataSetContext.Result.ListOfUpdatedDataRows.Add(currentRow);
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
                    newDataRow[columnMetadata.ColumnName] = dataSetContext.Configuration.DefaultValueProvider.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                }

                IRowMergeHandler rowHandler = dataSetContext.Configuration.DefaultRowMergeHandler;
                IRowMergeHandler? specificRowHandler;
                if (dataSetContext.Configuration.RowHandlersByTableName.TryGetValue(currentTable.TableName, out specificRowHandler))
                {
                    if (specificRowHandler != null)
                    {
                        rowHandler = specificRowHandler;
                    }
                }

                rowHandler.MergeRow(currentTable.TableName, newDataRow, refreshedRow, currentTable.Columns, dataSetContext);
                dataSetContext.Result.ListOfAddedDataRows.Add(newDataRow);
            }
        }
    }
}
