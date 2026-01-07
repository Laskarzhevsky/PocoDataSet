using System;

using PocoDataSet.IData;
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
        /// Accepts changes for observable data row
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <exception cref="InvalidOperationException">
        /// Exception is thrown if AcceptChanges is called for Deleted row.
        /// Deleted rows must be accepted at table level because accepting a deletion removes the row from the table.
        /// </exception>
        public static void AcceptChanges(this IObservableDataRow? observableDataRow)
        {
            if (observableDataRow == null)
            {
                return;
            }

            if (observableDataRow.DataRowState == DataRowState.Deleted)
            {
                throw new InvalidOperationException("AcceptChanges for Deleted rows must be performed at table level, because accepting a deletion removes the row from the table.");
            }

            observableDataRow.InnerDataRow.AcceptChanges();
        }
        #endregion
    }
}
