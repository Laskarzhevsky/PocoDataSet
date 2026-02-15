namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines how refreshed data is reconciled with the current table.
    /// </summary>
    public enum MergeMode
    {
        /// <summary>
        /// Reconciles refreshed data after a successful save operation.
        /// Used to synchronize client-side temporary keys (e.g., client-generated IDs) with server-generated keys
        /// and to update row values returned by the server.
        /// Deleted rows are permanently removed from the table and all remaining rows transition to the Unchanged state.
        /// </summary>
        PostSave,

        /// <summary>
        /// Performs a refresh merge only if the current table contains no pending changes.
        /// If any rows are in Added, Modified, or Deleted state, an exception is thrown
        /// and the merge is not performed.
        /// Intended for read-only, polling, or search-result scenarios where the current table is expected to be clean.
        /// </summary>
        RefreshIfNoChangesExist,

        /// <summary>
        /// Reconciles refreshed data with the current table using primary keys, while preserving local pending changes.
        /// Existing rows are matched by primary key and updated only if their state allows overwrite according to merge rules.
        /// Rows present in the refreshed data but missing locally are added.
        /// Rows missing from the refreshed data may be removed if they are in the Unchanged state.
        /// Rows with local changes (Added, Modified, Deleted) are preserved.
        /// </summary>
        RefreshPreservingLocalChanges,

        /// <summary>
        /// Replaces the entire local table with refreshed data.
        /// No row-level reconciliation is performed.
        /// All existing rows are cleared and replaced by the refreshed rows.
        /// This is a destructive operation and does not attempt to preserve local changes.
        /// </summary>
        Replace
    }
}
