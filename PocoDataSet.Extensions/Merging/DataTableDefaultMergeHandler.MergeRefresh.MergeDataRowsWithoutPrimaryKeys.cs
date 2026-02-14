using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
public partial class DataTableDefaultMergeHandler
{
    void MergeDataRowsWithoutPrimaryKeys(IDataTable currentDataTable, IDataTable refreshedDataTable, IMergeOptions mergeOptions)
            {
                // Track deletions BEFORE clearing the table
                for (int i = 0; i < currentDataTable.Rows.Count; i++)
                {
                    mergeOptions.DataSetMergeResult.DeletedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, currentDataTable.Rows[i]));
                }

                // Clear current rows
                currentDataTable.RemoveAllRows();

                // Add all refreshed rows as new rows
                for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
                {
                    IDataRow refreshedRow = refreshedDataTable.Rows[i];
                    IDataRow newDataRow = AddNewDataRowWithDefaultValuesToDataTable(currentDataTable, mergeOptions);

                    MergeDataRowFromRefreshedDataRow(newDataRow, refreshedRow, currentDataTable.Columns, mergeOptions);
                    mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(currentDataTable.TableName, newDataRow));
                }
            }
}
}
