namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data row state
    /// </summary>
    public enum DataRowState
    {
        /// <summary>
        /// Added to data table
        /// </summary>
        Added,

        /// <summary>
        /// Marked as deleted
        /// </summary>
        Deleted,

        /// <summary>
        /// Not part of data table
        /// </summary>
        Detached,

        /// <summary>
        /// With modified data
        /// </summary>
        Modified,

        /// <summary>
        /// Without modified data
        /// </summary>
        Uchanged
    }
}
