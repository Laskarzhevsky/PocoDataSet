using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.Extensions;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        /// <summary>
        /// Merges changes from observable dataset into provided dataset
        /// Useful if you already created a dataset instance for a service request
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="targetDataSet">Target dataset to receive changes</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeChangesetInto(this IObservableDataSet? observableDataSet, IDataSet? targetDataSet, IMergeOptions? mergeOptions = null)
        {
            if (observableDataSet == null)
            {
                return;
            }

            if (targetDataSet == null)
            {
                return;
            }

            IDataSet? outboundChangeset = observableDataSet.CreateChangeset();
            if (outboundChangeset == null)
            {
                return;
            }

            if (mergeOptions == null)
            {
                mergeOptions = new MergeOptions();
            }

            targetDataSet.DoReplaceMerge(outboundChangeset, mergeOptions);
        }
    }
}
