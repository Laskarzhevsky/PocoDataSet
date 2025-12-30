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
        /// Reject changes
        /// </summary>
        /// <param name="dataSet">Data set</param>
        public static void RejectChanges(this IDataSet? dataSet)
        {
            if (dataSet == null)
            {
                return;
            }

            foreach (KeyValuePair<string, IDataTable> keyValuePair in dataSet.Tables)
            {
                IDataTable dataTable = keyValuePair.Value;
                dataTable.RejectChanges();
            }
        }
        #endregion
    }
}
