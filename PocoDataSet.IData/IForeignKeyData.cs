namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines foreign key data functionality
    /// </summary>
    public interface IForeignKeyData
    {
        #region Properties
        /// <summary>
        /// Gets or sets foreign key name
        /// </summary>
        string ForeignKeyName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets principal column name
        /// </summary>
        string PrincipalColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets principal table name
        /// </summary>
        string PrincipalTableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced column name
        /// </summary>
        string ReferencedColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        string ReferencedTableName
        {
            get; set;
        }
        #endregion
    }
}
