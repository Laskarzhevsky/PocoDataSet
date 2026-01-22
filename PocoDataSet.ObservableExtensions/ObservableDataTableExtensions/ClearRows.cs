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
        /// Deletes observable row from observable table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        public static void ClearRows(this IObservableDataTable? observableDataTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            for (int i = observableDataTable.InnerDataTable.Rows.Count - 1; i >= 0; i--)
            {
                observableDataTable.RemoveRowAt(i);
            }
        }
        #endregion
    }
}
