namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines column metadata functionality
    /// </summary>
    public interface IColumnMetadata
    {
        #region Properties
        /// <summary>
        /// Gets or sets column name
        /// </summary>
        string ColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets data type
        /// </summary>
        string DataType
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets description
        /// </summary>
        string? Description
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets display name
        /// </summary>
        string? DisplayName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets display order
        /// </summary>
        int? DisplayOrder
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether column is foreign key
        /// </summary>
        bool IsForeignKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether column is nullable
        /// </summary>
        bool IsNullable
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets falg indicating whether this column participates in the table primary key.
        /// This property is the single source of truth for primary key membership.
        /// IDataTable.PrimaryKeys is derived from these flags.
        /// </summary>
        bool IsPrimaryKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets max length
        /// </summary>
        int? MaxLength
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets precision
        /// </summary>
        byte? Precision
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced column name
        /// </summary>
        string? ReferencedColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced table name
        /// </summary>
        string? ReferencedTableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets scale
        /// </summary>
        byte? Scale
        {
            get; set;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Clones column metadata
        /// </summary>
        /// <returns>Cloned column metadata</returns>
        IColumnMetadata Clone();
        #endregion
    }
}
