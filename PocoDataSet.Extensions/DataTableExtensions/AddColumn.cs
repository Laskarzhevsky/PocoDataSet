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
/*
        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        public static void AddColumn(this IDataTable? dataTable, string columnName, string dataType)
        {
            if (dataTable == null)
            {
                return;
            }

            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;
            dataTable.Columns.Add(columnMetadata);
        }

        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        public static void AddColumn(this IDataTable? dataTable, string columnName, string dataType, bool isPrimaryKey)
        {
            if (dataTable == null)
            {
                return;
            }

            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;
            columnMetadata.IsPrimaryKey = isPrimaryKey;
            dataTable.Columns.Add(columnMetadata);
        }
*/
        /// <summary>
        /// Adds column
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        public static void AddColumn(this IDataTable? dataTable, string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            if (dataTable == null)
            {
                return;
            }

            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = columnName;
            columnMetadata.DataType = dataType;

            // Set nullability
            if (isNullable.HasValue)
            {
                columnMetadata.IsNullable = isNullable.Value;
            }
            else
            {
                // By default, columns are nullable except primary keys
                if (isPrimaryKey.HasValue && isPrimaryKey.Value == true)
                {
                    columnMetadata.IsNullable = false;
                }
                else
                {
                    // By default, columns are nullable except primary keys
                    if (isPrimaryKey.HasValue && isPrimaryKey.Value == true)
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
            }

            // Set primary key
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

            // Set foreign key
            if (isForeignKey.HasValue)
            {
                columnMetadata.IsForeignKey = isForeignKey.Value;
            }
            else
            {
                if (columnName != "Id" && columnName.EndsWith("Id"))
                {
                    columnMetadata.IsForeignKey = true;
                }
            }

            // Set referenced table/column for foreign keys
            if (columnMetadata.IsForeignKey == true)
            {
                if (columnName != "Id" && columnName.EndsWith("Id"))
                {
                    columnMetadata.ReferencedColumnName = "Id";
                    columnMetadata.ReferencedTableName = columnName.Substring(0, columnName.Length - 2);
                }
            }

            dataTable.Columns.Add(columnMetadata);
        }
        #endregion
    }
}
