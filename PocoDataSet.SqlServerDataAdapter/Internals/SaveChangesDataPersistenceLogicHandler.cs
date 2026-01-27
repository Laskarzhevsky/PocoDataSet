using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using PocoDataSet.IData;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Encapsulates SaveChanges persistence pipeline.
    /// This type orchestrates ordering, validation, metadata loading and command application.
    /// Connection/transaction are owned privately by SqlDataAdapter; this handler never opens/closes them.
    /// </summary>
    internal sealed class SaveChangesDataPersistenceLogicHandler
    {
        private readonly SqlDataAdapter _adapter;

        public SaveChangesDataPersistenceLogicHandler(SqlDataAdapter adapter)
        {
            _adapter = adapter ?? throw new ArgumentNullException(nameof(adapter));
        }

        public async Task<int> SaveChangesAsync(IDataSet changeset)
        {
            if (changeset == null)
            {
                throw new ArgumentNullException(nameof(changeset));
            }

            List<IDataTable> tablesWithChanges = ChangesetProcessor.GetTablesWithChanges(changeset);
            if (tablesWithChanges.Count == 0)
            {
                return 0;
            }

            HashSet<string> namesOfTablesWithChanges = ChangesetProcessor.GetNamesOfTablesWithChanges(tablesWithChanges);

            Dictionary<string, TableWriteMetadata> metadataCache =
                new Dictionary<string, TableWriteMetadata>(StringComparer.OrdinalIgnoreCase);

            // FK edges are loaded once for the operation using adapter's private connection/transaction.
            List<ForeignKeyEdge> edges =
                await _adapter.LoadForeignKeyEdgesAsync(changeset, tablesWithChanges, namesOfTablesWithChanges).ConfigureAwait(false);

            // Order tables so deletes/inserts/updates can be applied safely.
            tablesWithChanges =
                TableSorter.BuildOrderedTablesWithChangesByForeignKeys(changeset, tablesWithChanges, namesOfTablesWithChanges, edges);

            // Validate and load table metadata once per table.
            for (int t = 0; t < tablesWithChanges.Count; t++)
            {
                IDataTable table = tablesWithChanges[t];

                DataTableValidator.ValidateTableForSave(table);

                TableWriteMetadata metadata =
                    await _adapter.GetOrLoadTableWriteMetadataAsync(table.TableName, metadataCache).ConfigureAwait(false);

                DataTableValidator.ValidateTableExistsInSqlServer(metadata, table.TableName);
            }

            CommandApplier applier = new CommandApplier(_adapter);

            int affectedRows = 0;
            affectedRows += await applier.ApplyDeletesAsync(tablesWithChanges, metadataCache).ConfigureAwait(false);
            affectedRows += await applier.ApplyInsertsAsync(tablesWithChanges, metadataCache).ConfigureAwait(false);
            affectedRows += await applier.ApplyUpdatesAsync(tablesWithChanges, metadataCache).ConfigureAwait(false);

            return affectedRows;
        }
    }
}
