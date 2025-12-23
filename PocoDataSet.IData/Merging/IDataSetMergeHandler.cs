namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set merge handler functionality
    /// </summary>
    public interface IDataSetMergeHandler
    {
        #region Methods
        /// <summary>
        /// Merges current data set with refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        void Merge(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions);
        #endregion
    }
}
