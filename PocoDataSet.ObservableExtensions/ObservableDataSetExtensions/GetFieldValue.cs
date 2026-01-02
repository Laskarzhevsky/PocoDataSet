using PocoDataSet.Extensions;
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
        /// Gets field value
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <returns>Value</returns>
        public static T? GetFieldValue<T>(this IObservableDataSet? observableDataSet, string tableName, int rowIndex, string columnName)
        {
            if (observableDataSet == null)
            {
                return default;
            }

            return observableDataSet.InnerDataSet.GetFieldValue<T>(tableName, rowIndex, columnName);
        }
        #endregion
    }
}
