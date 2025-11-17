using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataSetFactoryExamples
    {
        #region Public Methods
        /// <summary>
        /// DataSetFactory.CreateDataSet method example
        /// Creates DataSet
        /// </summary>
        /// <returns>Created DataSet</returns>
        public static IDataSet CreateDataSet()
        {
            // DataSetFactory.CreateDataSet method call example
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            return dataSet;
        }
        #endregion
    }
}
