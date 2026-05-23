namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Defines how <see cref="SqlDataAdapter"/> persists a POCO DataSet changeset.
    /// </summary>
    public enum SqlSaveMode
    {
        /// <summary>
        /// Uses the existing relation-aware row-by-row command pipeline.
        /// This remains the default mode for backward compatibility.
        /// </summary>
        RowByRow = 0,

        /// <summary>
        /// Uses generated Save stored procedures with SQL Server table-valued parameters.
        /// TVPs must include the __ChangeState metadata column.
        /// </summary>
        BatchStoredProcedure = 1
    }
}
