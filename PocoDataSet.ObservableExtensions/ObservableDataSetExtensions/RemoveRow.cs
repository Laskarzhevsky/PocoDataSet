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
        /// Removes row from a table in observable data set (delegates to inner data set)
        /// </summary>
        /// <param name="observableDataSet">Observable data set</param>
        /// <param name="tableName">Table name</param>
        /// <param name="dataRow">Data row</param>
        public static void RemoveRow(this IObservableDataSet? observableDataSet, string tableName, IDataRow dataRow)
        {
            if (observableDataSet == null)
            {
                return;
            }

            observableDataSet.InnerDataSet.RemoveRow(tableName, dataRow);
        }
        #endregion
    }
}
