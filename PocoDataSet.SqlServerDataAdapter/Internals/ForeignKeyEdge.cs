namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides foreign key edge functionality
    /// </summary>
    internal class ForeignKeyEdge
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        public string ReferencedTableName
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
        /// Gets or sets principal table name
        /// </summary>
        public string PrincipalTableName
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}
