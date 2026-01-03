using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Clears all rows from all tables in the observable data set.
        /// Keeps schema (tables and columns) intact.
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        public static void Clear(this IObservableDataSet? observableDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            foreach (IObservableDataTable table in observableDataSet.Tables.Values)
            {
                if (table == null)
                {
                    continue;
                }

                for (int i = table.Rows.Count - 1; i >= 0; i--)
                {
                    table.RemoveRow(i);
                }
            }
        }
        #endregion
    }
}
