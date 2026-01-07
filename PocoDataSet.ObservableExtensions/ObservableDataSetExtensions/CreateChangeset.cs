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
        /// Creates a "changeset" dataset that contains only rows in Added / Modified / Deleted states.
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <returns>Dataset containing changed rows</returns>
        public static IDataSet? CreateChangeset(this IObservableDataSet? currentObservableDataSet)
        {
            if (currentObservableDataSet == null)
            {
                return null;
            }

            // The observable layer is only a wrapper.
            // The source of truth for state is the inner dataset.
            return currentObservableDataSet.InnerDataSet.CreateChangeset();
        }
    }
}
