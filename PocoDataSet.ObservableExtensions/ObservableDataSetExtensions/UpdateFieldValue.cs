using System;
using System.Collections.Generic;

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
        /// Updates field value in an observable data set by writing through the observable row.
        /// This ensures that observable events (row / cell notifications) are raised and views stay consistent.
        /// </summary>
        /// <typeparam name="T">Value type</typeparam>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <param name="columnName">Column name</param>
        /// <param name="fieldValue">Field value</param>
        /// <exception cref="ArgumentNullException">Exception is thrown if <paramref name="tableName"/> or <paramref name="columnName"/> is null</exception>
        /// <exception cref="KeyNotFoundException">Exception is thrown if data set does not contain a table with specified name</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if <paramref name="rowIndex"/> is out of range</exception>
        public static void UpdateFieldValue<T>(this IObservableDataSet? observableDataSet, string tableName, int rowIndex, string columnName, T? fieldValue)
        {
            if (observableDataSet == null)
            {
                return;
            }

            IObservableDataTable? observableDataTable;
            if (!observableDataSet.Tables.TryGetValue(tableName, out observableDataTable))
            {
                throw new KeyNotFoundException($"ObservableDataSet does not contain table with name {tableName}.");
            }

            if (rowIndex < 0 || rowIndex >= observableDataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            IObservableDataRow observableDataRow = observableDataTable.Rows[rowIndex];

            // IMPORTANT: write through observable row to ensure notifications are raised.
            observableDataRow[columnName] = fieldValue;
        }
        #endregion
    }
}
