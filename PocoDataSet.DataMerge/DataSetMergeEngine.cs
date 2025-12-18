using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IDataMerge;

namespace PocoDataSet.DataMerge
{
    /// <summary>
    /// Povides data set merge engine.
    /// </summary>
    public class DataSetMergeEngine : IDataSetMergeEngine
    {
        /// <summary>
        /// Merges current data set with refreshed data set using provided configuration.
        /// IDataSetMergeEngine interface implementation
        /// </summary>
        /// <param name="currentDataSet">Current data set to be updated.</param>
        /// <param name="refreshedDataSet">Refreshed data set providing changes.</param>
        /// <param name="dataSetMergeConfiguration">Data set merge configuration.</param>
        /// <returns>Merge result.</returns>
        public IDataSetsMergeResult Merge(IDataSet currentDataSet, IDataSet refreshedDataSet, IDataSetMergeConfiguration dataSetMergeConfiguration)
        {
            List<IDataRow> added = new List<IDataRow>();
            List<IDataRow> deleted = new List<IDataRow>();
            List<IDataRow> updated = new List<IDataRow>();

            IDataSetsMergeResult result = new DataSetsMergeResult(added, deleted, updated);
            MergeContext context = new MergeContext(dataSetMergeConfiguration, result);

            HashSet<string> mergedTableNames = new HashSet<string>();

            // 1) Merge existing tables
            foreach (IDataTable currentTable in currentDataSet.Tables.Values)
            {
                if (currentTable == null)
                {
                    continue;
                }

                mergedTableNames.Add(currentTable.TableName);

                if (dataSetMergeConfiguration.ExcludeTablesFromMerge.Contains(currentTable.TableName))
                {
                    continue;
                }

                IDataTable? refreshedTable;
                refreshedDataSet.TryGetTable(currentTable.TableName, out refreshedTable);

                if (refreshedTable == null)
                {
                    continue;
                }

                ITableMergeHandler tableHandler = dataSetMergeConfiguration.DefaultTableMergeHandler;
                ITableMergeHandler? specificHandler;
                if (dataSetMergeConfiguration.TableHandlersByName.TryGetValue(currentTable.TableName, out specificHandler))
                {
                    if (specificHandler != null)
                    {
                        tableHandler = specificHandler;
                    }
                }

                tableHandler.Merge(currentTable, refreshedTable, context);
            }

            // 2) Add new tables that exist only in refreshed data set
            foreach (IDataTable refreshedTable in refreshedDataSet.Tables.Values)
            {
                if (refreshedTable == null)
                {
                    continue;
                }

                if (mergedTableNames.Contains(refreshedTable.TableName))
                {
                    continue;
                }

                if (dataSetMergeConfiguration.ExcludeTablesFromMerge.Contains(refreshedTable.TableName))
                {
                    continue;
                }

                IDataTable? cloned = refreshedTable.Clone();
                if (cloned == null)
                {
                    continue;
                }

                currentDataSet.AddTable(cloned);

                // Treat all rows of added table as added rows.
                for (int i = 0; i < cloned.Rows.Count; i++)
                {
                    IDataRow r = cloned.Rows[i];
                    result.ListOfAddedDataRows.Add(r);
                }
            }

            return result;
        }
    }
}
