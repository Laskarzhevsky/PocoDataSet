using PocoDataSet.IData;

namespace PocoDataSet.IDataMerge
{
    /// <summary>
    /// Defines data set merge engine.
    /// </summary>
    public interface IDataSetMergeEngine
    {
        /// <summary>
        /// Merges current data set with refreshed data set using provided configuration.
        /// </summary>
        /// <param name="currentDataSet">Current data set to be updated.</param>
        /// <param name="refreshedDataSet">Refreshed data set providing changes.</param>
        /// <param name="dataSetMergeConfiguration">Data set merge configuration.</param>
        /// <returns>Merge result.</returns>
        IDataSetsMergeResult Merge(IDataSet currentDataSet, IDataSet refreshedDataSet, IDataSetMergeConfiguration dataSetMergeConfiguration);
    }
}
