using System.Collections.Generic;

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
        /// Merges current data row with refreshed data row by copying refreshed values into current data row
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="currentObservableDataTableName">Observable table name</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        public static bool MergeWith(this IDataRow currentDataRow, IDataRow refreshedDataRow, string currentObservableDataTableName, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            IObservableDataRowMergeHandler observableDataRowMergeHandler = observableMergeOptions.GetObservableRowMergeHandler(currentObservableDataTableName);
            return observableDataRowMergeHandler.Merge(currentObservableDataTableName, currentDataRow, refreshedDataRow, listOfColumnMetadata, observableMergeOptions);
        }

        /// <summary>
        /// Merges current observable data row with refreshed data row by copying refreshed values into current observable data row
        /// </summary>
        /// <param name="currentObservableDataRow">Current observable data row</param>
        /// <param name="refreshedDataRow">Refreshed data row</param>
        /// <param name="currentObservableDataTableName">Observable table name current observable data row belongs to</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <returns>True if any value of the current data row changed, otherwise false</returns>
        public static bool MergeWith(this IObservableDataRow currentObservableDataRow, IDataRow refreshedDataRow, string currentObservableDataTableName, IReadOnlyList<IColumnMetadata> listOfColumnMetadata, IObservableMergeOptions observableMergeOptions)
        {
            IObservableDataRowMergeHandler observableDataRowMergeHandler = observableMergeOptions.GetObservableRowMergeHandler(currentObservableDataTableName);
            return observableDataRowMergeHandler.Merge(currentObservableDataTableName, currentObservableDataRow, refreshedDataRow, listOfColumnMetadata, observableMergeOptions);
        }
        #endregion
    }
}
