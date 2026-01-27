using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using PocoDataSet.IData;

using PocoDataRowState = PocoDataSet.IData.DataRowState;

namespace PocoDataSet.SqlServerDataAdapter
{
    internal sealed class CommandApplier
    {
        
        private readonly SqlDataAdapter _adapter;

        public CommandApplier(SqlDataAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }
#region Public Methods
        /// <summary>
        /// Apply deletes async
        /// </summary>
        /// <param name="orderedTables">Ordered tables</param>
        /// <param name="tableWriteMetadataCache">Table write metadata cache</param>
        public async Task<int> ApplyDeletesAsync(IReadOnlyList<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> tableWriteMetadataCache)
        {
            int affected = 0;

            for (int t = orderedTables.Count - 1; t >= 0; t--)
            {
                IDataTable table = orderedTables[t];
                TableWriteMetadata metadata = GetMetadataOrThrow(tableWriteMetadataCache, table.TableName);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    IDataRow row = table.Rows[i];
                    if (row.DataRowState == PocoDataRowState.Detached)
                    {
                        continue;
                    }
                    if (row.DataRowState != PocoDataRowState.Deleted)
                    {
                        continue;
                    }

                    using SqlCommand sqlCommand = SqlCommandBuilder.BuildDeleteCommand(table, metadata, row);
                    int localAffected = await _adapter.ExecuteNonQueryAsync(sqlCommand).ConfigureAwait(false);
                    if (localAffected == 0)
                    {
                        throw new PocoConcurrencyException(table.TableName, "DELETE", PrimaryKeyProcessor.BuildPrimaryKeyText(metadata, row));
                    }

                    affected += localAffected;
                }
            }

            return affected;
        }

        /// <summary>
        /// Applies inserts async
        /// </summary>
        /// <param name="orderedTables">Ordered tables</param>
        /// <param name="tableWriteMetadataCache">Table write metadata cache</param>
        /// <returns>Number of affected rows</returns>
        public async Task<int> ApplyInsertsAsync(IReadOnlyList<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> tableWriteMetadataCache)
        {
            int affected = 0;

            for (int t = 0; t < orderedTables.Count; t++)
            {
                IDataTable table = orderedTables[t];
                TableWriteMetadata metadata = GetMetadataOrThrow(tableWriteMetadataCache, table.TableName);

                List<string> outputColumnsForTable = ColumnsBuilder.BuildInsertOutputColumns(metadata);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    IDataRow row = table.Rows[i];
                    if (row.DataRowState == PocoDataRowState.Detached)
                    {
                        continue;
                    }
                    if (row.DataRowState != PocoDataRowState.Added)
                    {
                        continue;
                    }

                    using SqlCommand sqlCommand = SqlCommandBuilder.BuildInsertCommand(table, metadata, row, outputColumnsForTable);

                    if (outputColumnsForTable.Count == 0)
                    {
                        affected += await _adapter.ExecuteNonQueryAsync(sqlCommand).ConfigureAwait(false);
                        continue;
                    }

                    using (SqlDataReader reader = await _adapter.ExecuteReaderAsync(sqlCommand).ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            ApplyOutputValuesToRow(row, outputColumnsForTable, reader);
                            affected += 1;
                        }
                        else
                        {
                            throw new InvalidOperationException("INSERT did not return output values for table '" + table.TableName + "'.");
                        }
                    }
                }
            }

            return affected;
        }

        /// <summary>
        /// Applies updates async
        /// </summary>
        /// <param name="orderedTables">Ordered tables</param>
        /// <param name="tableWriteMetadataCache">Table write metadata cache</param>
        /// <returns>Number of affected rows</returns>
        public async Task<int> ApplyUpdatesAsync(IReadOnlyList<IDataTable> orderedTables, Dictionary<string, TableWriteMetadata> tableWriteMetadataCache)
        {
            int affected = 0;

            for (int t = 0; t < orderedTables.Count; t++)
            {
                IDataTable table = orderedTables[t];
                TableWriteMetadata metadata = GetMetadataOrThrow(tableWriteMetadataCache, table.TableName);

                List<string> outputColumnsForTable = ColumnsBuilder.BuildUpdateOutputColumns(metadata);

                for (int i = 0; i < table.Rows.Count; i++)
                {
                    IDataRow row = table.Rows[i];
                    if (row.DataRowState == PocoDataRowState.Detached)
                    {
                        continue;
                    }
                    if (row.DataRowState != PocoDataRowState.Modified)
                    {
                        continue;
                    }

                    using SqlCommand sqlCommand = SqlCommandBuilder.BuildUpdateCommand(table, metadata, row, outputColumnsForTable);
                    if (sqlCommand.CommandText.Length == 0)
                    {
                        continue;
                    }

                    if (outputColumnsForTable.Count == 0)
                    {
                        int localAffected = await _adapter.ExecuteNonQueryAsync(sqlCommand).ConfigureAwait(false);
                        if (localAffected == 0)
                        {
                            throw new PocoConcurrencyException(table.TableName, "UPDATE", PrimaryKeyProcessor.BuildPrimaryKeyText(metadata, row));
                        }

                        affected += localAffected;
                        continue;
                    }

                    using (SqlDataReader reader = await _adapter.ExecuteReaderAsync(sqlCommand).ConfigureAwait(false))
                    {
                        if (await reader.ReadAsync().ConfigureAwait(false))
                        {
                            ApplyOutputValuesToRow(row, outputColumnsForTable, reader);
                            affected += 1;
                        }
                        else
                        {
                            throw new PocoConcurrencyException(table.TableName, "UPDATE", PrimaryKeyProcessor.BuildPrimaryKeyText(metadata, row));
                        }
                    }
                }
            }

            return affected;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Applies output values to row
        /// </summary>
        /// <param name="dataRow">Data row</param>
        /// <param name="outputColumns">Output columns</param>
        /// <param name="sqlDataReader">SQL data reader</param>
        static void ApplyOutputValuesToRow(IDataRow dataRow, List<string> outputColumns, SqlDataReader sqlDataReader)
        {
            for (int i = 0; i < outputColumns.Count; i++)
            {
                object value = sqlDataReader.GetValue(i);
                if (value == DBNull.Value)
                {
                    dataRow[outputColumns[i]] = null;
                }
                else
                {
                    dataRow[outputColumns[i]] = value;
                }
            }
        }

        /// <summary>
        /// Gets metadata for a table from the cache or throws a clear error.
        /// </summary>
        /// <param name="tableWriteMetadataCache">Table metadata cache</param>
        /// <param name="tableName">Table name</param>
        static TableWriteMetadata GetMetadataOrThrow(Dictionary<string, TableWriteMetadata> tableWriteMetadataCache, string tableName)
        {
            if (tableWriteMetadataCache == null)
            {
                throw new ArgumentNullException(nameof(tableWriteMetadataCache));
            }

            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name is not specified.", nameof(tableName));
            }

            if (!tableWriteMetadataCache.TryGetValue(tableName, out TableWriteMetadata? metadata) || metadata == null)
            {
                throw new InvalidOperationException("Missing metadata for table '" + tableName + "'. Ensure MetadataLoader has populated the cache before applying commands.");
            }

            return metadata;
        }
        #endregion
    }
}