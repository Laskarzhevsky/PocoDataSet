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
        /// Adds data row to observable data table in observable data set by wrapping it into observable data row
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row</param>
        /// <returns>Added observable data row</returns>
        public static IObservableDataRow AddRow(this IObservableDataSet? observableDataSet, string tableName, IDataRow dataRow)
        {
            if (observableDataSet == null)
            {
                return default!;
            }

            return observableDataSet.Tables[tableName].AddRow(dataRow);
        }
        #endregion
    }
}
