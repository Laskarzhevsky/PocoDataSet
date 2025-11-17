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
        /// DataRowExtensions.ToPoco method example
        /// Converts data row into POCO
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <returns>POCO</returns>
        public static T ToPoco<T>(IDataRow dataRow) where T : new()
        {
            // DataRowExtensions.ToPoco method call example
            return dataRow.ToPoco<T>();
        }
        #endregion
    }
}
