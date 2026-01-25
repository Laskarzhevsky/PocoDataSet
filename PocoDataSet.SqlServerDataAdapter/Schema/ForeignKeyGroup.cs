using System.Collections.Generic;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Represents a grouped foreign key definition (supports composite keys).
    /// Parent = dependent table, Referenced = principal table.
    /// </summary>
    internal sealed class ForeignKeyGroup
    {
        /// <summary>
        /// Gets or sets foreign key name
        /// </summary>
        public string ForeignKeyName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets dependent table name (parent table in sys.foreign_keys terminology)
        /// </summary>
        public string ParentTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets dependent column names (ordered)
        /// </summary>
        public List<string> ParentColumnNames
        {
            get;
        } = new List<string>();

        /// <summary>
        /// Gets or sets principal table name
        /// </summary>
        public string ReferencedTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets principal column names (ordered)
        /// </summary>
        public List<string> ReferencedColumnNames
        {
            get;
        } = new List<string>();
    }
}
