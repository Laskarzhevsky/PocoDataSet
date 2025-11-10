using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents column metadata
    /// </summary>
    public class ColumnMetadata : IColumnMetadata
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets column name
        /// IColumnMetadata interface implementation
        /// </summary>
        public string ColumnName 
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets data type
        /// IColumnMetadata interface implementation
        /// </summary>
        public string DataType
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets description
        /// IColumnMetadata interface implementation
        /// </summary>
        public string? Description
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets display name
        /// IColumnMetadata interface implementation
        /// </summary>
        public string? DisplayName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets display order
        /// IColumnMetadata interface implementation
        /// </summary>
        public int? DisplayOrder
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether column is foreign key
        /// IColumnMetadata interface implementation
        /// </summary>
        public bool IsForeignKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets flag indicating whether column is nullable
        /// IColumnMetadata interface implementation
        /// </summary>
        public bool IsNullable
        {
            get; set;
        } = true;

        /// <summary>
        /// Gets or sets flag indicating whether column is primary key
        /// IColumnMetadata interface implementation
        /// </summary>
        public bool IsPrimaryKey
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets max length
        /// IColumnMetadata interface implementation
        /// </summary>
        public int? MaxLength
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets precision
        /// IColumnMetadata interface implementation
        /// </summary>
        public byte? Precision
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced column name
        /// IColumnMetadata interface implementation
        /// </summary>
        public string? ReferencedColumnName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets referenced table name
        /// IColumnMetadata interface implementation
        /// </summary>
        public string? ReferencedTableName
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets scale
        /// IColumnMetadata interface implementation
        /// </summary>
        public byte? Scale
        {
            get; set;
        }
        #endregion
    }
}
