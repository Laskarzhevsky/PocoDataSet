using PocoDataSet.Extensions;
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
        /// Adds a new table to observable data set (delegates to inner data set)
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        public static void AddNewTable(this IObservableDataSet? observableDataSet, string tableName)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableDataSet.InnerDataSet.AddNewTable(tableName);
        }
        #endregion
    }
}
