namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines table merge handler functionality
    /// </summary>
    public interface ITableMergeHandler
    {
        #region Methods
        /// <summary>
        /// Merges current table with refreshed table.
        /// </summary>
        /// <param name="currentTable">Current table</param>
        /// <param name="refreshedTable">Refreshed table</param>
        /// <param name="mergeOptions">Merge options</param>
        void Merge(IDataTable currentTable, IDataTable refreshedTable, IMergeOptions mergeOptions);
        #endregion
    }
}
