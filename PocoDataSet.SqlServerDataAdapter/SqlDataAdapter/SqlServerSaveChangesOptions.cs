using System.Collections.Generic;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Defines options used by <see cref="SqlDataAdapter.SaveChangesAsync"/>
    /// </summary>
    public class SqlServerSaveChangesOptions
    {
        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether a database transaction should be used.
        /// </summary>
        public bool UseTransaction
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets or sets per-table list of identity/computed columns that must be excluded from INSERT/UPDATE.
        /// Key is table name.
        /// </summary>
        public Dictionary<string, HashSet<string>> ExcludedColumnsByTable
        {
            get; set;
        } = new Dictionary<string, HashSet<string>>();

        /// <summary>
        /// Gets or sets explicit table save order. If provided, only the listed tables are ordered,
        /// remaining tables are processed afterwards in the incoming order.
        /// </summary>
        public List<string> TableSaveOrder
        {
            get; set;
        } = new List<string>();
        #endregion
    }
}
