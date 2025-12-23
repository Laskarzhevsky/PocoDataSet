using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data table merge handler functionality
    /// </summary>
    public interface IObservableDataTableMergeHandler
    {
        #region Methods
        /// <summary>
        /// Merges current observable data table with refreshed data table
        /// </summary>
        /// <param name="currentObservableDataTable">Current observable data table</param>
        /// <param name="refreshedDataTable">Refreshed data table</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        void Merge(IObservableDataTable currentObservableDataTable, IDataTable refreshedDataTable, IObservableMergeOptions observableMergeOptions);
        #endregion
    }
}
