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
        /// Deletes row at index (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="rowIndex">Row index</param>
        public static void DeleteRowAt(this IObservableDataTable? observableDataTable, int rowIndex)
        {
            if (observableDataTable == null)
            {
                return;
            }

            observableDataTable.InnerDataTable.DeleteRowAt(rowIndex);
        }
        #endregion
    }
}
