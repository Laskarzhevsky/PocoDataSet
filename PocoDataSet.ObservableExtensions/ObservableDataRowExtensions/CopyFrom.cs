using System;
using System.Collections.Generic;

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
        /// Copies column values from the source data row into the target observable data row
        /// using the provided column metadata list.
        /// Missing columns are ignored; extra target columns are left untouched.
        /// </summary>
        /// <param name="observableDataRow">Observable data row</param>
        /// <param name="dataRow">Source data row</param>
        /// <param name="sourceColumns">Source column metadata</param>
        public static void CopyFrom(this IObservableDataRow? observableDataRow, IDataRow dataRow, IList<IColumnMetadata> sourceColumns)
        {
            if (observableDataRow == null)
            {
                return;
            }

            for (int i = 0; i < sourceColumns.Count; i++)
            {
                string columnName = sourceColumns[i].ColumnName;

                object? value;
                if (dataRow.TryGetValue(columnName, out value))
                {
                    // Use observable indexer so events fire
                    observableDataRow[columnName] = value;
                }
            }
        }
        #endregion
    }
}
