using PocoDataSet.Extensions;
using PocoDataSet.IData;
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
        /// Adds a table to observable data set (delegates to inner data set)
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="dataTable">Data table</param>
        public static void AddTable(this IObservableDataSet? observableDataSet, IDataTable dataTable)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableDataSet.InnerDataSet.AddTable(dataTable);
        }
        #endregion
    }
}
