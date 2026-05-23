namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Defines row operation values sent to SQL Server table-valued parameters.
    /// These values are intentionally independent from ADO.NET DataRowState and
    /// PocoDataSet.IData.DataRowState enum numeric values.
    /// </summary>
    public enum SqlServerChangeState
    {
        /// <summary>
        /// Row has no pending save operation.
        /// </summary>
        Unchanged = 0,

        /// <summary>
        /// Row must be inserted.
        /// </summary>
        Added = 1,

        /// <summary>
        /// Row must be updated.
        /// </summary>
        Modified = 2,

        /// <summary>
        /// Row must be deleted or soft-deleted by the generated save procedure.
        /// </summary>
        Deleted = 3
    }
}
