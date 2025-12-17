namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines merge manager functionality
    /// </summary>
    public interface IMergeManager
    {
        #region Methods
        /// <summary>
        /// Handles merge
        /// </summary>
        /// <param name="observableDataSet">Observable dataset</param>
        void HandleMerge(IObservableDataSet observableDataSet);

        /// <summary>
        /// Handles merge
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        void HandleMerge(IObservableDataTable observableDataTable);

        /// <summary>
        /// Handles merge
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="observableDataRow">Observable data row</param>
        void HandleMerge(string tableName, IObservableDataRow observableDataRow);
        #endregion
    }
}
