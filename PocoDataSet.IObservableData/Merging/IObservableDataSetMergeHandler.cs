using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data set merge handler functionality
    /// </summary>
    public interface IObservableDataSetMergeHandler
    {
        #region Methods
        /// <summary>
        /// Merges current observable data set with refreshed data set
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        void Merge(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions);
        #endregion
    }
}
