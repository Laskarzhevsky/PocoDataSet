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
        /// Adds a new observable table to observable data set
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <returns>New observable table</returns>
        public static IObservableDataTable AddNewTable(this IObservableDataSet? observableDataSet, string tableName)
        {
            if (observableDataSet == null)
            {
                return default!;
            }

            IDataTable newDataTable = observableDataSet.InnerDataSet.AddNewTable(tableName);
            IObservableDataTable observableDataTable =  observableDataSet.AddObservableTable(newDataTable);

            return observableDataTable;
        }
        #endregion
    }
}
