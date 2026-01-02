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
        /// Removes observable row from observable table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="observableDataRow">Observable data row</param>
        public static void RemoveRow(this IObservableDataTable? observableDataTable, IObservableDataRow? observableDataRow)
        {
            if (observableDataTable == null || observableDataRow == null)
            {
                return;
            }

            observableDataTable.InnerDataTable.RemoveRow(observableDataRow.InnerDataRow);
        }
        #endregion
    }
}
