using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

using Npgsql;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.PostgreSqlDataAdapter
{
    /// <summary>
    /// Creates POCO DataTables from an NpgsqlDataReader.
    /// </summary>
    internal class PostgreSqlDataTableCreator
    {
        #region Public Methods
        /// <summary>
        /// Adds all result sets from the current data reader to the configured data set.
        /// </summary>
        public async Task AddTablesToDataSetAsync()
        {
            VerifyDataSetExistence();
            if (ListOfTableNames == null)
            {
                await GetListOfTableNamesFromDataReaderAsync().ConfigureAwait(false);
            }
            else
            {
                CreateNewDataTable();
                AddColumnsToDataTable();
                await AddRowsToDataTableAsync().ConfigureAwait(false);
                AddDataTableToDataSet();
            }

            while (await DataReader!.NextResultAsync().ConfigureAwait(false))
            {
                CreateNewDataTable();
                AddColumnsToDataTable();
                await AddRowsToDataTableAsync().ConfigureAwait(false);
                AddDataTableToDataSet();
            }

            ReleaseResources();
        }
        #endregion

        #region Public Properties
        public IDataSet? DataSet
        {
            get; set;
        }

        public PocoDataSet.Data.DataTable? DataTable
        {
            get; set;
        }

        public NpgsqlDataReader? DataReader
        {
            get; set;
        }

        public List<string>? ListOfTableNames
        {
            get; set;
        }
        #endregion

        #region Methods
        void AddColumnsToDataTable()
        {
            System.Data.DataTable? schemaTable = DataReader!.GetSchemaTable();
            for (int i = 0; i < DataReader.FieldCount; i++)
            {
                string columnName = DataReader.GetName(i);
                string dataType = DataReader.GetDataTypeName(i);
                Type fieldType = DataReader.GetFieldType(i);

                ColumnMetadata columnMetadata = new ColumnMetadata();
                columnMetadata.ColumnName = columnName;
                columnMetadata.DataType = dataType;

                int maxLength = GetMaxLength(schemaTable, i, dataType);
                if (maxLength > 0)
                {
                    columnMetadata.MaxLength = maxLength;
                }

                bool? isNullable = GetNullability(schemaTable, i);
                if (isNullable.HasValue)
                {
                    columnMetadata.IsNullable = isNullable.Value;
                }
                else if (fieldType.IsValueType && Nullable.GetUnderlyingType(fieldType) == null)
                {
                    columnMetadata.IsNullable = false;
                }

                DataTable!.AddColumn(columnMetadata);
            }
        }

        void AddDataTableToDataSet()
        {
            DataSet!.AddTable(DataTable!);
            DataTableIndex++;
        }

        async Task AddRowsToDataTableAsync()
        {
            while (await DataReader!.ReadAsync().ConfigureAwait(false))
            {
                IDataRow row = DataRowFactory.CreateEmpty(DataReader.FieldCount);
                for (int i = 0; i < DataReader.FieldCount; i++)
                {
                    string columnName = DataReader.GetName(i);

                    object? value;
                    if (await DataReader.IsDBNullAsync(i).ConfigureAwait(false))
                    {
                        value = null;
                    }
                    else
                    {
                        value = DataReader.GetValue(i);
                    }

                    row[columnName] = value;
                }

                DataTable!.AddLoadedRow(row);
            }
        }

        void CreateNewDataTable()
        {
            if (ListOfTableNames == null)
            {
                throw new InvalidOperationException("ListOfTableNames is not set.");
            }

            if (DataTableIndex >= ListOfTableNames.Count)
            {
                throw new InvalidOperationException("The query returned more result sets than there are table names in ListOfTableNames.");
            }

            DataTable = new PocoDataSet.Data.DataTable();
            DataTable.TableName = ListOfTableNames[DataTableIndex];
        }

        async Task GetListOfTableNamesFromDataReaderAsync()
        {
            ListOfTableNames = new List<string>();
            while (await DataReader!.ReadAsync().ConfigureAwait(false))
            {
                ListOfTableNames.Add(DataReader.GetString(0));
            }
        }

        int GetMaxLength(System.Data.DataTable? schemaTable, int columnIndex, string dataType)
        {
            int maxLength = -1;
            if (dataType.Equals("character varying", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("character", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("char", StringComparison.OrdinalIgnoreCase) ||
                dataType.Equals("text", StringComparison.OrdinalIgnoreCase))
            {
                if (schemaTable != null && schemaTable.Rows.Count > columnIndex)
                {
                    System.Data.DataRow schemaRow = schemaTable.Rows[columnIndex];
                    if (schemaRow.Table.Columns.Contains("ColumnSize"))
                    {
                        object? columnSize = schemaRow["ColumnSize"];
                        if (columnSize != DBNull.Value && columnSize is int size)
                        {
                            maxLength = size;
                        }
                    }
                }
            }

            return maxLength;
        }

        bool? GetNullability(System.Data.DataTable? schemaTable, int columnIndex)
        {
            bool? isNullable = null;
            if (schemaTable != null && schemaTable.Rows.Count > columnIndex)
            {
                System.Data.DataRow schemaRow = schemaTable.Rows[columnIndex];
                if (schemaRow.Table.Columns.Contains("AllowDBNull"))
                {
                    object? allowDBNull = schemaRow["AllowDBNull"];
                    if (allowDBNull != DBNull.Value && allowDBNull is bool allow)
                    {
                        isNullable = allow;
                    }
                }
                else if (schemaRow.Table.Columns.Contains("IsNullable"))
                {
                    object? nullableValue = schemaRow["IsNullable"];
                    if (nullableValue != DBNull.Value && nullableValue is bool nullable)
                    {
                        isNullable = nullable;
                    }
                }
            }

            return isNullable;
        }

        void ReleaseResources()
        {
            DataTable = null;
            ListOfTableNames = null;
        }

        void VerifyDataSetExistence()
        {
            if (DataSet == null)
            {
                DataSet = new PocoDataSet.Data.DataSet();
            }
        }
        #endregion

        #region Properties
        int DataTableIndex
        {
            get; set;
        }
        #endregion
    }
}
