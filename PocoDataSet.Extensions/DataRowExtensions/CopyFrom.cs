using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data row extension methods
    /// </summary>
    public static partial class DataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Copies column values from the source data row into the target data row
        /// using the provided column metadata list.
        /// Missing columns are ignored; extra target columns are left untouched.
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="sourceDataRow">Source data row</param>
        /// <param name="sourceColumns">Source column metadata</param>
        public static void CopyFrom(this IDataRow? currentDataRow, IDataRow sourceDataRow, IList<IColumnMetadata> sourceColumns)
        {
            if (currentDataRow == null)
            {
                return;
            }

            for (int i = 0; i < sourceColumns.Count; i++)
            {
                string columnName = sourceColumns[i].ColumnName;

                object? value;
                if (sourceDataRow.TryGetValue(columnName, out value))
                {
                    currentDataRow[columnName] = value;
                }
            }
        }
        #endregion
    }
}
