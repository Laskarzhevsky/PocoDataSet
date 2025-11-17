using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges current data table with data from refreshed data set
        /// </summary>
        /// <param name="currentDataTable">Current data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeWith(this IDataTable? currentDataTable, IDataTable? refreshedDataTable, IMergeOptions? mergeOptions = null)
        {
            if (currentDataTable == null || refreshedDataTable == null)
            {
                return;
            }

            List<string> dataTablePrimaryKeyColumnNames = currentDataTable.GetPrimaryKeyColumnNames(mergeOptions);
            Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildDataRowIndex(dataTablePrimaryKeyColumnNames, null);
            for (int i = currentDataTable.Rows.Count - 1; i >=0; i--)
            {
                IDataRow dataRow = currentDataTable.Rows[i];
                string dataRowPrimaryKeyValue = dataRow.CompilePrimaryKeyValue(dataTablePrimaryKeyColumnNames);
                IDataRow? indexedDataRow = null;
                refreshedDataTableDataRowIndex.TryGetValue(dataRowPrimaryKeyValue, out indexedDataRow);
                if (indexedDataRow == null)
                {
                    if (mergeOptions == null)
                    {
                        currentDataTable.Rows.RemoveAt(i);
                    }
                    else
                    {
                        if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(currentDataTable.TableName))
                        {
                            // Keep row intact
                        }
                        else
                        {
                            currentDataTable.Rows.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    dataRow.MergeWith(indexedDataRow, currentDataTable.Columns);
                    indexedDataRow.Selected = true;
                }
            }

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedDataRow = refreshedDataTable.Rows[i];
                if (refreshedDataRow.Selected == false)
                {
                    IDataRow newDataRow = currentDataTable.AddNewRow();
                    foreach (IColumnMetadata columnMetadata in currentDataTable.Columns)
                    {
                        newDataRow[columnMetadata.ColumnName] = Defaults.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                    }

                    newDataRow.MergeWith(refreshedDataRow, currentDataTable.Columns);
                }
            }
        }
        #endregion
    }
}
