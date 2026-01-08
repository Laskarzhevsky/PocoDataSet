namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines merge mode
    /// </summary>
    public enum MergeMode
    {
        /// <summary>
        /// Post-save merge (server-confirmed changeset is applied back to the UI-bound dataset).
        /// Identity/rowversion propagation + AcceptChanges are performed for affected rows.
        /// </summary>
        PostSave = 1,

        /// <summary>
        /// Default "refresh" merge (server data replaces local baseline).
        /// </summary>
        Refresh = 0,

        /// <summary>
        /// Allow Replace to overwrite local values and commit as Unchanged.
        /// </summary>
        Replace = 2
    }
}
