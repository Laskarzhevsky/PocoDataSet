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
        /// Gets or sets principal column name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string PrincipalColumnName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets principal table name
        /// IForeignKeyData interface implementation
        /// </summary>
        public string PrincipalTableName
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
