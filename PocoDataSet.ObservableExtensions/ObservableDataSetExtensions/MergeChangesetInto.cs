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
        /// Merges outbound changes from observable dataset into an existing target dataset.
        /// Useful if you already created a dataset instance for a service request.
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="targetDataSet">Target dataset to receive changes</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeChangesetInto(this IObservableDataSet? currentObservableDataSet, IDataSet? targetDataSet, IMergeOptions? mergeOptions = null)
        {
            if (currentObservableDataSet == null)
            {
                return;
            }

            if (targetDataSet == null)
            {
                return;
            }

            IDataSet? outboundChangeset = currentObservableDataSet.CreateChangeset();
            if (outboundChangeset == null)
            {
                return;
            }

            targetDataSet.MergeWith(outboundChangeset, mergeOptions);
        }
    }
}
