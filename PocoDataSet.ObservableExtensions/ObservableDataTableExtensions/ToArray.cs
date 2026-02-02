using System;
using System.Collections.Generic;

using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Returns a shallow copy of the table's observable rows as array
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <returns>Array of observable rows</returns>
        public static IObservableDataRow[] ToArray(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null || observableDataTable.Rows.Count == 0)
            {
                return Array.Empty<IObservableDataRow>();
            }

            List<IObservableDataRow> list = new List<IObservableDataRow>(observableDataTable.Rows);
            return list.ToArray();
        }
        #endregion
    }
}
