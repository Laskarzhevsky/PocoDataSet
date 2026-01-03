using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Rejects changes for observable data row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        public static void RejectChanges(this IObservableDataRow? observableDataRow)
        {
            if (observableDataRow == null)
            {
                return;
            }

            observableDataRow.InnerDataRow.RejectChanges();
        }
        #endregion
    }
}
