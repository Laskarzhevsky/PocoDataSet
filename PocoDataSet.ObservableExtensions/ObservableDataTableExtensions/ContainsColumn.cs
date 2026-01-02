using PocoDataSet.Extensions;
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
        /// Returns true if table contains column with specified name
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="columnName">Column name</param>
        /// <returns>True if exists, otherwise false</returns>
        public static bool ContainsColumn(this IObservableDataTable? observableDataTable, string columnName)
        {
            if (observableDataTable == null)
            {
                return false;
            }

            return observableDataTable.InnerDataTable.ContainsColumn(columnName);
        }
        #endregion
    }
}
