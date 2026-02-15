using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableExtensions.Merging.Modes;

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
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public static void MergePostSaveWith(this IObservableDataTable observableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions)
        {
            ObservablePostSaveDataTableMerger observablePostSaveDataTableMerger = new ObservablePostSaveDataTableMerger();
            observablePostSaveDataTableMerger.MergePostSave(observableDataTable, refreshedDataTable, observableMergeOptions);
        }
        #endregion
    }
}
