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
        /// <param name="table">Observable table</param>
        /// <returns>Array of observable rows</returns>
        public static IObservableDataRow[] ToArray(this IObservableDataTable table)
        {
            List<IObservableDataRow> list = new List<IObservableDataRow>(table.Rows);
            return list.ToArray();
        }
        #endregion
    }
}
