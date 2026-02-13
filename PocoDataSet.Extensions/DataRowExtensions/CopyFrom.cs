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
        /// Copies column values from the source data row into the current data row using list of column metadata.
        /// If source data row doesn't have filed specified in the list of column metadata then method does nothing.
        /// If current data row doesn't have filed specified in the list of column metadata but source data row contains it
        /// then method adds it to the current row supporting schema-evolution
        /// </summary>
        /// <param name="currentDataRow">Current data row</param>
        /// <param name="sourceDataRow">Source data row</param>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        public static void CopyFrom(this IDataRow? currentDataRow, IDataRow sourceDataRow, IReadOnlyList<IColumnMetadata> listOfColumnMetadata)
        {
            if (currentDataRow == null)
            {
                return;
            }

            for (int i = 0; i < listOfColumnMetadata.Count; i++)
            {
                string columnName = listOfColumnMetadata[i].ColumnName;

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
