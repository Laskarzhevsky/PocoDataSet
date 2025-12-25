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
        /// Merges observable table set with data from refreshed data table
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public static void MergeWith(this IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            IObservableDataTableMergeHandler observableDataTableMergeHandler = observableMergeOptions!.GetObservableTableMergeHandler(currentObservableDataTable.TableName);
            observableDataTableMergeHandler.Merge(currentObservableDataTable, refreshedDataTable, observableMergeOptions);
        }
        #endregion
    }
}
