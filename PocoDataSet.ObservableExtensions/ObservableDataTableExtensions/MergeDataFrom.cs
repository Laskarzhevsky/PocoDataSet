using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.DataMerge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges observable data set with data from refreshed data set
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeDataFrom(this IObservableDataTable observableDataTable, IDataTable refreshedDataTable, IMergeOptions? mergeOptions = null)
        {
            List<string> observableDataTablePrimaryKeyColumnNames = observableDataTable.InnerDataTable.GetPrimaryKeyColumnNames(mergeOptions);
            Dictionary<string, IDataRow> refreshedDataTableDataRowIndex = refreshedDataTable.BuildDataRowIndex(observableDataTablePrimaryKeyColumnNames);
            for (int i = observableDataTable.Rows.Count - 1; i >=0; i--)
            {
                IObservableDataRow observableDataRow = observableDataTable.Rows[i];
                string observableDataRowPrimaryKeyValue = observableDataRow.InnerDataRow.CompilePrimaryKeyValue(observableDataTablePrimaryKeyColumnNames);
                IDataRow? indexedDataRow = null;
                refreshedDataTableDataRowIndex.TryGetValue(observableDataRowPrimaryKeyValue, out indexedDataRow);
                if (indexedDataRow == null)
                {
                    if (mergeOptions == null)
                    {
                        observableDataTable.RemoveRow(i);
                    }
                    else
                    {
                        if (mergeOptions.ExcludeTablesFromRowDeletion.Contains(observableDataTable.TableName))
                        {
                            // Keep observable row intact
                        }
                        else
                        {
                            observableDataTable.RemoveRow(i);
                        }
                    }
                }
                else
                {
                    observableDataRow.MergeDataFrom(indexedDataRow, observableDataTable.InnerDataTable.Columns, mergeOptions);
                    indexedDataRow.Selected = true;
                }
            }

            for (int i = 0; i < refreshedDataTable.Rows.Count; i++)
            {
                IDataRow refreshedDataRow = refreshedDataTable.Rows[i];
                if (refreshedDataRow.Selected == false)
                {
                    IDataRow newDataRow = observableDataTable.InnerDataTable.AddNewRow();
                    foreach (IColumnMetadata columnMetadata in observableDataTable.Columns)
                    {
                        newDataRow[columnMetadata.ColumnName] = MetadataDefaults.GetDefaultValue(columnMetadata.DataType, columnMetadata.IsNullable);
                    }

                    newDataRow.MergeWith(refreshedDataRow, observableDataTable.Columns);
                    observableDataTable.AddRow(newDataRow, null);
                }
            }
        }
        #endregion
    }
}
