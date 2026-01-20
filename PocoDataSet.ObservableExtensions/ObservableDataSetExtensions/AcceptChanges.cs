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
        /// Accepts changes for observable data set (delegates to inner data set)
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        public static void AcceptChanges(this IObservableDataSet? observableDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            foreach (IObservableDataTable observableDataTable in observableDataSet.Tables.Values)
            {
                observableDataTable.AcceptChanges();
            }
        }
        #endregion
    }
}
