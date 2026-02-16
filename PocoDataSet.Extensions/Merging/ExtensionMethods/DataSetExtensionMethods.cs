using PocoDataSet.Data;
using PocoDataSet.IData;

using System.Collections.Generic;

namespace PocoDataSet.Extensions
{
    internal static class DataSetExtensionMethods
    {
        /// <summary>
        /// Adds non-existing tables from the refreshed dataset to the current dataset.
        /// This method is used during the merging process to ensure that any tables that are present in the refreshed dataset
        /// but not in the current dataset are added to the current dataset.
        /// The method also updates the merge result with the added tables and their rows.
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="mergedTableNames">Meerged table names</param>
        public static void AddNonExistingTables(this IDataSet? currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            if (currentDataSet == null) {
                return;
            }

            foreach (IDataTable table in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(table.TableName))
                {
                    continue;
                }

                if (mergeOptions.ExcludeTablesFromMerge.Contains(table.TableName))
                {
                    continue;
                }

                IDataTable? cloned = table.Clone();
                if (cloned == null)
                {
                    continue;
                }

                currentDataSet.AddTable(cloned);

                for (int i = 0; i < cloned.Rows.Count; i++)
                {
                    mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(cloned.TableName, cloned.Rows[i]));
                }
            }
        }
    }
}
