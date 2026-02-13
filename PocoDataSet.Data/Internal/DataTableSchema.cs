using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using PocoDataSet.IData;

namespace PocoDataSet.Data.Internal
{
    class DataTableSchema
    {
        #region Data Fields
        /// <summary>
        /// ClientKeyColumn property data field
        /// </summary>
        IColumnMetadata? _clientKeyColumn;

        /// <summary>
        /// Columns property data field
        /// </summary>
        List<IColumnMetadata> _columns = new List<IColumnMetadata>();

        /// <summary>
        /// PrimaryKeys property data field
        /// </summary>
        readonly PrimaryKeys _primaryKeys = new();
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DataTableSchema()
        {
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Creates column
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a table contains a column with specified name already</exception>
        public IColumnMetadata CreateColumn(string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;

            // Primary key
            if (isPrimaryKey.HasValue)
            {
                columnMetadata.IsPrimaryKey = isPrimaryKey.Value;
            }
            else
            {
                if (columnName == "Id")
                {
                    columnMetadata.IsPrimaryKey = true;
                }
            }

            // Nullability
            if (isNullable.HasValue)
            {
                columnMetadata.IsNullable = isNullable.Value;
            }
            else
            {
                if (columnMetadata.IsPrimaryKey)
                {
                    columnMetadata.IsNullable = false;
                }
                else
                {
                    if (columnName == "Id")
                    {
                        columnMetadata.IsNullable = false;
                    }
                    else
                    {
                        columnMetadata.IsNullable = true;
                    }
                }
            }

            // Foreign key
            if (isForeignKey.HasValue)
            {
                columnMetadata.IsForeignKey = isForeignKey.Value;
            }
            else
            {
                // Do not infer FK for primary keys
                if (!columnMetadata.IsPrimaryKey)
                {
                    if (columnName != "Id" && columnName.EndsWith("Id", StringComparison.Ordinal))
                    {
                        columnMetadata.IsForeignKey = true;
                    }
                }
            }

            // Referenced table/column
            if (columnMetadata.IsForeignKey)
            {
                if (columnName != "Id" && columnName.EndsWith("Id", StringComparison.Ordinal))
                {
                    columnMetadata.ReferencedColumnName = "Id";
                    columnMetadata.ReferencedTableName = columnName.Substring(0, columnName.Length - 2);
                }
            }

            return columnMetadata;
        }

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="columnMetadata">Column metadata</param>
        public void AddColumn(IColumnMetadata columnMetadata)
        {
            // Keep table.PrimaryKeys as a single source of truth
            if (columnMetadata.IsPrimaryKey)
            {
                _primaryKeys.AddPrimaryKey(columnMetadata.ColumnName);
            }

            _columns.Add(columnMetadata);
        }

        /// <summary>
        /// Gets flag indicating whether data table contains column with specified name
        /// </summary>
        /// <param name="columnName">Column name</param>
        /// <returns>Flag indicating whether data table contains column with specified name</returns>
        public bool ContainsColumn(string columnName)
        {
            string columnNameToLowerInvariant = columnName.ToLowerInvariant();
            for (int i = 0; i < _columns.Count; i++)
            {
                IColumnMetadata columnMetadata = _columns[i];
                if (columnMetadata.ColumnName.ToLowerInvariant() == columnNameToLowerInvariant)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Ensures  that ClientKey column exists
        /// </summary>
        public void EnsureClientKeyColumnExists()
        {
            _clientKeyColumn = new ColumnMetadata();
            _clientKeyColumn.ColumnName = SpecialColumnNames.CLIENT_KEY;
            _clientKeyColumn.DataType = DataTypeNames.GUID;
            _clientKeyColumn.IsNullable = false;
            _clientKeyColumn.IsPrimaryKey = false;
            _clientKeyColumn.DisplayName = null;
            _clientKeyColumn.Description = "Client-only key for changeset correlation.";
            _columns.Add(_clientKeyColumn);
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets client key column
        /// </summary>
        public IColumnMetadata? ClientKeyColumn
        {
            get
            {
                return _clientKeyColumn;
            }
        }

        /// <summary>
        /// Gets data table columns
        /// </summary>
        public IReadOnlyList<IColumnMetadata> Items
        {
            get
            {
                return _columns;
            }
        }

        /// <summary>
        /// Gets primary keys
        /// </summary>
        public PrimaryKeys PrimaryKeys
        {
            get
            {
                return _primaryKeys;
            }
        }
        #endregion
    }
}
