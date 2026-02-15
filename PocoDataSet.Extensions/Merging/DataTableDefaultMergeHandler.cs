using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
/// <summary>
/// Provides data table default merge handler functionality
/// </summary>
public partial class DataTableDefaultMergeHandler : ITableMergeHandler
{
    /// <summary>
    /// Adds new data row with default values to data table
    /// </summary>
    private IDataRow AddNewDataRowWithDefaultValuesToDataTable(IDataTable dataTable, IMergeOptions mergeOptions)
    {
                    // Create a detached row
                    IDataRow newDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);

                    // Add to table (still Detached)
                    dataTable.AddRow(newDataRow);

                    // This row represents server data, not a user "Added" row
                    newDataRow.SetDataRowState(DataRowState.Unchanged);

                    return newDataRow;

    }

    /// <summary>
    /// Merges data row from refreshed data row
    /// </summary>
    private static bool MergeDataRowFromRefreshedDataRow(
        IDataRow currentDataRow,
        IDataRow refreshedDataRow,
        IReadOnlyList<IColumnMetadata> listOfColumnMetadata,
        IMergeOptions mergeOptions)
    {
                    // Refresh must never overwrite local edits.
                    if (mergeOptions != null && mergeOptions.MergeMode == MergeMode.RefreshPreservingLocalChanges)
                    {
                        if (currentDataRow.DataRowState != DataRowState.Unchanged)
                        {
                            return false;
                        }
                    }

                    if (currentDataRow.DataRowState == DataRowState.Deleted)
                    {
                        // Keep local delete during Refresh merge; do not attempt to overwrite or AcceptChanges.
                        return false;
                    }

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
}
}
