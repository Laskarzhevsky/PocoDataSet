namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides behavior options for SQL Server save operations.
    /// </summary>
    public sealed class SqlDataAdapterOptions
    {
        /// <summary>
        /// Gets or sets the save pipeline used by the adapter.
        /// Default is RowByRow to preserve existing behavior.
        /// </summary>
        public SqlSaveMode SaveMode
        {
            get; set;
        } = SqlSaveMode.RowByRow;
    }
}
