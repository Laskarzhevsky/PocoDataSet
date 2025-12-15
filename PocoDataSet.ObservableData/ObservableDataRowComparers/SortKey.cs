namespace PocoDataSet.ObservableData
{
    /// <summary>
    /// Provides sort key fuctionality
    /// </summary>
    internal class SortKey
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets column name
        /// </summary>
        public string ColumnName
        {
            get; set;
        } = default!;

        /// <summary>
        /// Gets or sets Flag indicating whether sort shoud be done in ascending order
        /// </summary>
        public bool Ascending
        {
            get; set;
        }
        #endregion
    }
}
