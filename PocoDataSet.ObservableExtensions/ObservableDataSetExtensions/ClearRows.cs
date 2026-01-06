using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    public static partial class ObservableDataSetExtensions
    {
        /// <summary>
        /// Alias for Clear method
        /// Clears all rows from all tables in the data set.
        /// Keeps schema (tables and columns) intact.
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        public static void ClearRows(this IObservableDataSet? observableDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableDataSet.Clear();
        }
    }
}
