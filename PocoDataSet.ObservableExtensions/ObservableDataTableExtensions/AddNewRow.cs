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
        /// Adds a new row to the inner table and returns the corresponding observable row (if available)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <returns>New observable row, or null if it cannot be resolved</returns>
        public static IObservableDataRow? AddNewRow(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return null;
            }

            int oldCount = observableDataTable.Rows.Count;
            observableDataTable.InnerDataTable.AddNewRow();

            if (observableDataTable.Rows.Count > oldCount)
            {
                return observableDataTable.Rows[oldCount];
            }

            return null;
        }
        #endregion
    }
}
