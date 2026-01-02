using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Clears selection for all rows in the table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        public static void ClearSelection(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            observableDataTable.InnerDataTable.ClearSelection();
        }
        #endregion
    }
}
