namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides SQL Server adapter metadata column names.
    /// </summary>
    public static class SqlServerChangeStateColumn
    {
        /// <summary>
        /// Metadata column added to generated table-valued parameters so stored procedures
        /// can classify INSERT / UPDATE / DELETE by row state instead of primary-key sign.
        /// </summary>
        public const string ColumnName = "__ChangeState";
    }
}
