using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column</returns>
        /// <exception cref="KeyDuplicationException">Exception is thrown if a table contains a column with specified name already</exception>
        public static IColumnMetadata AddColumn(this IDataTable? dataTable, string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            if (dataTable == null)
            {
                return default!;
            }

            if (string.IsNullOrEmpty(columnName))
            {
                throw new ArgumentNullException(nameof(columnName));
            }

            if (string.IsNullOrEmpty(dataType))
            {
                throw new ArgumentNullException(nameof(dataType));
            }

            for (int i = 0; i < dataTable.Columns.Count; i++)
            {
                if (dataTable.Columns[i].ColumnName == columnName)
                {
                    throw new KeyDuplicationException($"DataTable contains column with name {columnName} already");
                }
            }

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

            dataTable.Columns.Add(columnMetadata);

            // Keep table.PrimaryKeys in sync with column metadata
            if (columnMetadata.IsPrimaryKey)
            {
                if (dataTable.PrimaryKeys == null)
                {
                    dataTable.PrimaryKeys = new List<string>();
                }

                bool exists = false;
                for (int i = 0; i < dataTable.PrimaryKeys.Count; i++)
                {
                    if (string.Equals(dataTable.PrimaryKeys[i], columnName, StringComparison.OrdinalIgnoreCase))
                    {
                        exists = true;
                        break;
                    }
                }

                if (!exists)
                {
                    dataTable.PrimaryKeys.Add(columnName);
                }
            }

            EnsureExistingRowsHaveColumn(dataTable, columnName);

            return columnMetadata;
        }
        #endregion
    }
}
