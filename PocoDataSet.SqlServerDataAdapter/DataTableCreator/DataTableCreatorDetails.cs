using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides data table creator functionality
    /// </summary>
    public partial class DataTableCreator
    {
        #region Methods
        /// <summary>
        /// Adds columns to data table
        /// </summary>
        void AddColumnsToDataTable()
        {
            System.Data.DataTable schemaTable = SqlDataReader!.GetSchemaTable();
            for (int i = 0; i < SqlDataReader.FieldCount; i++)
            {
                string columnName = SqlDataReader.GetName(i);
                string dataType = SqlDataReader.GetDataTypeName(i);
                Type fieldType = SqlDataReader.GetFieldType(i);

                int maxLength = -1;
                if (dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase) || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase) || dataType.Equals("char", StringComparison.OrdinalIgnoreCase))
                {
                    if (schemaTable != null && schemaTable.Rows.Count > i)
                    {
                        var schemaRow = schemaTable.Rows[i];
                        if (schemaRow["ColumnSize"] is int size)
                        {
                            maxLength = size;
                        }
                    }
                }

                ColumnMetadata columnMetadata = new ColumnMetadata();
                columnMetadata.ColumnName = columnName;
                columnMetadata.DataType = dataType;
                if (maxLength > 0)
                {
                    columnMetadata.MaxLength = maxLength;
                }

                // Populate IsNullable from schema when available.
                // GetSchemaTable includes an "AllowDBNull" column that indicates whether the column accepts nulls.
                if (schemaTable != null && schemaTable.Rows.Count > i)
                {
                    var schemaRow = schemaTable.Rows[i];
                    if (schemaRow.Table.Columns.Contains("AllowDBNull"))
                    {
                        object? allowObj = schemaRow["AllowDBNull"];
                        if (allowObj != DBNull.Value && allowObj is bool allow)
                        {
                            columnMetadata.IsNullable = allow;
                        }
                    }
                    else if (schemaRow.Table.Columns.Contains("IsNullable"))
                    {
                        // defensive: some providers/databases might expose "IsNullable"
                        object? isNullObj = schemaRow["IsNullable"];
                        if (isNullObj != DBNull.Value && isNullObj is bool isNull)
                        {
                            columnMetadata.IsNullable = isNull;
                        }
                    }
                }

                // If schema did not provide nullability, infer a sensible default:
                // value types are typically non-nullable unless they are Nullable<T>.
                // Keep behavior minimal: if fieldType is a non-nullable value type, mark as non-nullable.
                // (ColumnMetadata default is true, so only change when we can infer non-nullability.)
                if (!columnMetadata.IsNullable)
                {
                    // already set to false by schema or inference
                }
                else
                {
                    // If schema didn't set it and fieldType is a non-nullable value type, set IsNullable = false.
                    if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) == null)
                    {
                        columnMetadata.IsNullable = false;
                    }
                }

                if (ForeignKeysData != null && ForeignKeysData.TryGetValue(columnName, out IForeignKeyData? foreignKeyData))
                {
                    columnMetadata.IsForeignKey = true;
                    columnMetadata.ReferencedTableName = foreignKeyData.ReferencedTableName;
                    columnMetadata.ReferencedColumnName = foreignKeyData.ReferencedColumnName;
                }

                if (PrimaryKeyData != null && PrimaryKeyData.Contains(columnName))
                {
                    columnMetadata.IsPrimaryKey = true;
                }

                DataTable!.AddColumn(columnMetadata);
            }
        }

        /// <summary>
        /// Adds data table to data set
        /// </summary>
        void AddDataTableToDataSet()
        {
            DataSet!.Tables[DataTable!.TableName] = DataTable;
            DataTableIndex++;
        }

        /// <summary>
        /// Populate DataTable.PrimaryKey from PrimaryKeyData (preserving column order).
        /// Call after columns are created so we can preserve column ordering.
        /// </summary>
        void AddPrimaryKeys()
        {
            if (DataTable == null)
            {
                return;
            }

            // Primary keys are a table-level contract.
            // Use the DataTable primary key API so the table remains the single source of truth.
            DataTable.ClearPrimaryKeys();
            if (PrimaryKeyData == null || PrimaryKeyData.Count == 0)
            {
                return;
            }

            // Preserve table column order when building the primary key list
            List<string> orderedPrimaryKeys = new List<string>();
            foreach (IColumnMetadata columnMetadata in DataTable.Columns)
            {
                if (PrimaryKeyData.Contains(columnMetadata.ColumnName))
                {
                    orderedPrimaryKeys.Add(columnMetadata.ColumnName);
                }
            }

            DataTable.SetPrimaryKeys(orderedPrimaryKeys);
        }

        /// <summary>
        /// Adds rows to data table
        /// </summary>
        async Task AddRowsToDataTableAsync()
        {
            while (await SqlDataReader!.ReadAsync())
            {
                IDataRow row = DataRowFactory.CreateEmpty(SqlDataReader!.FieldCount);
                for (int i = 0; i < SqlDataReader.FieldCount; i++)
                {
                    string columnName = SqlDataReader.GetName(i);

                    object? value;
                    if (await SqlDataReader.IsDBNullAsync(i))
                    {
                        value = null;
                    }
                    else
                    {
                        value = SqlDataReader.GetValue(i);
                    }

                    row[columnName] = value;
                }

                DataTable!.AddLoadedRow(row);
            }
        }

        /// <summary>
        /// Creates new data table
        /// </summary>
        void CreateNewDataTable()
        {
            DataTable = new PocoDataSet.Data.DataTable();
            DataTable.TableName = ListOfTableNames![DataTableIndex];
        }

        /// <summary>
        /// Gets list of table names from SQL data reader
        /// </summary>
        async Task GetListOfTableNamesFromSqlDataReaderAsync()
        {
            ListOfTableNames = new List<string>();
            while (await SqlDataReader!.ReadAsync())
            {
                ListOfTableNames.Add(SqlDataReader.GetString(0));
            }
        }

        /// <summary>
        /// Loads data table keys information
        /// </summary>
        protected async Task LoadDataTableKeysInformationAsync()
        {
            ForeignKeysData = null;
            PrimaryKeyData = null;
            if (LoadDataTableKeysInformationRequest != null)
            {
                await LoadDataTableKeysInformationRequest(this, new EventArgs());
            }
        }

        /// <summary>
        /// Releases resources
        /// </summary>
        void ReleaseResources()
        {
            DataTable = null!;
            ForeignKeysData = null;
            ListOfTableNames = null!;
            PrimaryKeyData = null!;
        }

        /// <summary>
        /// Verifies data set existense
        /// </summary>
        void VerifyDataSetExistense()
        {
            if (DataSet == null)
            {
                DataSet = new PocoDataSet.Data.DataSet();
            }
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets data table index
        /// </summary>
        int DataTableIndex
        {
            get; set;
        }
        #endregion
    }
}
