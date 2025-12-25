using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges observable data set with data from refreshed data set
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public static IObservableDataSetMergeResult MergeWith(this IObservableDataSet? currentObservableDataSet, IDataSet? refreshedDataSet, IObservableMergeOptions? observableMergeOptions = null)
        {
            if (observableMergeOptions == null)
            {
                observableMergeOptions = new ObservableMergeOptions();
            }
            else
            {
                observableMergeOptions.ObservableDataSetMergeResult.Clear();
            }

            if (currentObservableDataSet == null || refreshedDataSet == null)
            {
                return observableMergeOptions.ObservableDataSetMergeResult;
            }

            IObservableDataSetMergeHandler observableDataSetMergeHandler = observableMergeOptions!.GetObservableDataSetMergeHandler(currentObservableDataSet.Name);
            observableDataSetMergeHandler.Merge(currentObservableDataSet, refreshedDataSet, observableMergeOptions);

            return observableMergeOptions.ObservableDataSetMergeResult;
        }
        #endregion
    }
}
