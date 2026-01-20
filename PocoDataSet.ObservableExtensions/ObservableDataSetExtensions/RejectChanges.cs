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
        /// Rejects changes for an observable data set by delegating to each observable table.
        /// This ensures observable row collections stay consistent and observable events are raised.
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        public static void RejectChanges(this IObservableDataSet? observableDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            foreach (IObservableDataTable table in observableDataSet.Tables.Values)
            {
                table.RejectChanges();
            }
        }
        #endregion
    }
}
