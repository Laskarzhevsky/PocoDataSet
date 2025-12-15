using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges observable data set with data from refreshed data set
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeDataFrom(this IObservableDataRow currentDataRow, IDataRow refreshedDataRow, List<IColumnMetadata> listOfColumnMetadata, IMergeOptions? mergeOptions = null)
        {
            foreach (IColumnMetadata columnMetadata in listOfColumnMetadata)
            {
                string columnName = columnMetadata.ColumnName;
                object? oldValue = currentDataRow.GetDataFieldValue<object?>(columnName);
                object? newValue = refreshedDataRow.GetDataFieldValue<object?>(columnName);
                if (DataFieldValuesComparer.FieldValuesEqual(oldValue, newValue))
                {
                    continue;
                }

                currentDataRow.UpdateDataFieldValue(columnName, newValue, null);
            }
        }
        #endregion
    }
}
