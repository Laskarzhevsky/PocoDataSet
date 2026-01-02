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
        /// Updates field value in the inner data set
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <param name="fieldValue">Field value</param>
        public static void UpdateFieldValue<T>(this IObservableDataSet? observableDataSet, string tableName, int rowIndex, string columnName, T? fieldValue)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableDataSet.InnerDataSet.UpdateFieldValue(tableName, rowIndex, columnName, fieldValue);
        }
        #endregion
    }
}
