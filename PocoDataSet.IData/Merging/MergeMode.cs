namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines merge mode
    /// </summary>
    public enum MergeMode
    {
        /// <summary>
        /// Default "refresh" merge (server data replaces local baseline).
        /// </summary>
        Refresh = 0,

        /// <summary>
        /// Post-save merge (server-confirmed changeset is applied back to the UI-bound dataset).
        /// Identity/rowversion propagation + AcceptChanges are performed for affected rows.
        /// </summary>
        PostSave = 1
    }
}
