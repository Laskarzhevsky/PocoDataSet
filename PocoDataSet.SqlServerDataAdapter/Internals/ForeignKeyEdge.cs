namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides foreign key edge functionality
    /// </summary>
    internal class ForeignKeyEdge
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets child table name
        /// </summary>
        public string ChildTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets foreign key name
        /// </summary>
        public string ForeignKeyName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets parent table name
        /// </summary>
        public string ParentTableName
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}
