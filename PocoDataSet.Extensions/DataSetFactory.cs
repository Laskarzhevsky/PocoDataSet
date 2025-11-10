using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data set  factory functionality
    /// </summary>
    public static class DataSetFactory
    {
        #region Public Methods
        /// <summary>
        /// Creates data set
        /// </summary>
        /// <returns>Data set</returns>
        public static IDataSet CreateDataSet()
        {
            IDataSet dataSet = new DataSet();
            return dataSet;
        }
        #endregion
    }
}
