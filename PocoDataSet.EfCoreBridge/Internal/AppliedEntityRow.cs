using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// INTERNAL USE ONLY.
    /// Tracks a single changeset row that was applied to a tracked EF Core entity instance,
    /// so we can build a minimal post-save response dataset (PK / client key / concurrency tokens).
    /// </summary>
    internal sealed class AppliedEntityRow
    {
        /// <summary>
        /// Gets or sets the POCO DataSet table name being applied.
        /// </summary>
        public string TableName
        {
            get;
            set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets the source changeset row that was applied.
        /// </summary>
        public IDataRow? SourceRow
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the EF Core entity instance that was created/updated/deleted.
        /// </summary>
        public object? Entity
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the original state of the source row (Added/Modified/Deleted).
        /// </summary>
        public DataRowState SourceState
        {
            get;
            set;
        }
    }
}
