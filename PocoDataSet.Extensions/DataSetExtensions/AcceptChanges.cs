using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Accepts changes
        /// </summary>
        /// <param name="dataSet">Data set</param>
        public static void AcceptChanges(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            foreach (KeyValuePair<string, IDataTable> keyValuePair in dataSet.Tables)
            {
                IDataTable dataTable = keyValuePair.Value;
                dataTable.AcceptChanges();
            }
        }
        #endregion
    }
}
