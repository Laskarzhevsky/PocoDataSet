using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Removes row from table
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="rowIndex">Row index</param>
        /// <exception cref="KeyNotFoundException">Exception is thrown if dataset does not contains a table with specified name</exception>
        /// <exception cref="ArgumentOutOfRangeException">Exception is thrown if table does not have row with specified index</exception>
        public static void RemoveRow(this IDataSet? dataSet, string tableName, int rowIndex)
        {
            if (dataSet == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            if (rowIndex < 0 || rowIndex >= dataTable.Rows.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex));
            }

            IDataRow dataRow = dataTable.Rows[rowIndex];
            RemoveRow(dataSet, tableName, dataRow);
        }

        /// <summary>
        /// Removes row from table using state-aware semantics.
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row to remove</param>
        /// <exception cref="KeyNotFoundException">Thrown if dataset does not contain a table with specified name</exception>
        public static void RemoveRow(this IDataSet? dataSet, string tableName, IDataRow? dataRow)
        {
            if (dataSet == null)
            {
                return;
            }

            if (dataRow == null)
            {
                return;
            }

            if (!dataSet.Tables.TryGetValue(tableName, out IDataTable? dataTable))
            {
                throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }

            if (!dataTable.Rows.Contains(dataRow))
            {
                return;
            }

            // New row → undo creation (physical removal)
            if (dataRow.DataRowState == DataRowState.Added)
            {
                dataTable.Rows.Remove(dataRow);
                dataRow.DataRowState = DataRowState.Detached;
                return;
            }

            // Existing row → soft delete (undoable)
            if (dataRow.DataRowState == DataRowState.Unchanged ||
                dataRow.DataRowState == DataRowState.Modified)
            {
                dataRow.Delete();
                return;
            }

            // Deleted / Detached → nothing to do
        }
        #endregion
    }
}
