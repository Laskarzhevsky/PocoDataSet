using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataSetExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// Merges current data set with refreshed data set by copying all changes from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static DataSetsMergeResult MergeWith(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            // MergeWith extension method call example
            return currentDataSet.MergeWith(refreshedDataSet, mergeOptions);
        }
        #endregion
    }
}
