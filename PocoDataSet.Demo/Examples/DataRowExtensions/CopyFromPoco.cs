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
        /// DataRowExtensions.CopyFromPoco method example
        /// Copies public readable properties from POCO object into data row
        /// </summary>
        /// <typeparam name="T">POCO type</typeparam>
        /// <param name="dataRow">Data row</param>
        /// <param name="poco">POCO</param>
        public static void CopyFromPoco<T>(IDataRow dataRow, T poco)
        {
            // DataRowExtensions.CopyFromPoco method call example
            dataRow.CopyFromPoco<T>(poco);
        }
        #endregion
    }
}
