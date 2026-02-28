using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents foreign key data
    /// </summary>
    public class ForeignKeyData : IForeignKeyData
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets foreign key name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ForeignKeyName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets principal column name (name of the column that holds the foreign key value)
        /// IForeignKeyData interface implementation
        /// </summary>
        public string PrincipalColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets principal table name (name of the table that contains the foreign key column
        /// IForeignKeyData interface implementation
        /// </summary>
        public string PrincipalTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets referenced column name (name of the column that holds the primary key value)
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ReferencedColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets referenced table name (table that contains the primary key column)
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ReferencedTableName
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}
