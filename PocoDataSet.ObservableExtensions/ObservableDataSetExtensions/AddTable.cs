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
        /// Adds data table to observable data set by wrapping it into observable data table
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="dataTable">Data table</param>
        /// <returns>Observable table added to observable data set</returns>
        public static IObservableDataTable AddTable(this IObservableDataSet? observableDataSet, IDataTable dataTable)
        {
            if (observableDataSet == null)
            {
                return default!;
            }

            observableDataSet.InnerDataSet.AddTable(dataTable);
            IObservableDataTable observableDataTable = observableDataSet.AddObservableTable(dataTable);
            return observableDataTable;
        }
        #endregion
    }
}
