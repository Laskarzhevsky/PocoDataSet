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
        /// Gets data field value from inner data row
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Value</returns>
        public static T? GetDataFieldValue<T>(this IObservableDataRow? observableDataRow, string columnName)
        {
            if (observableDataRow == null)
            {
                return default;
            }

            return observableDataRow.InnerDataRow.GetDataFieldValue<T>(columnName);
        }
        #endregion
    }
}
