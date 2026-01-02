using PocoDataSet.Extensions;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Updates data field value of the inner row
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="columnName">Column name</param>
        /// <param name="fieldValue">Field value</param>
        /// <returns>True if value changed, otherwise false</returns>
        public static bool UpdateDataFieldValue<T>(this IObservableDataRow? observableDataRow, string columnName, T? fieldValue)
        {
            if (observableDataRow == null)
            {
                return false;
            }

            return observableDataRow.InnerDataRow.UpdateDataFieldValue(columnName, fieldValue);
        }
        #endregion
    }
}
