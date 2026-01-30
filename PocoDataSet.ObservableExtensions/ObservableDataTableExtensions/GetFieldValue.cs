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
        /// Gets field value from row at row with specified index
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Field value from row at row with specified index</returns>
        public static T? GetFieldValue<T>(this IObservableDataTable? observableDataTable, int rowIndex, string columnName)
        {
            if (observableDataTable == null)
            {
                return default;
            }

            return observableDataTable.InnerDataTable.GetFieldValue<T>(rowIndex, columnName);
        }
        #endregion
    }
}
