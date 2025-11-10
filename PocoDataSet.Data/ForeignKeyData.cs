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
        /// Gets or sets parent column name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ParentColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets parent table name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ParentTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets referenced column name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ReferencedColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets referenced table name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string ReferencedTableName
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}
