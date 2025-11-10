namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines foreign key data functionality
    /// </summary>
    public interface IForeignKeyData
    {
        #region Properties
        /// <summary>
        /// Gets or sets 
        /// </summary>
        string ForeignKeyName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        string ParentColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        string ParentTableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        string ReferencedColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets 
        /// </summary>
        string ReferencedTableName
        {
            get; set;
        }
        #endregion
    }
}
