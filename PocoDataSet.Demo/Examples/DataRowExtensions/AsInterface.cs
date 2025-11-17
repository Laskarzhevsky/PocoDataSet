using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataRowExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// DataRowExtensions.AsInterface method example
        /// Gets "live" data row as an interface
        /// </summary>
        /// <typeparam name="TInterface">POCO interface type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <returns>"Live" data row as an interface</returns>
        public static TInterface AsInterface<TInterface>(IDataRow dataRow) where TInterface : class
        {
            // DataRowExtensions.AsInterface method call example
            return dataRow.AsInterface<TInterface>();
        }
        #endregion
    }
}
