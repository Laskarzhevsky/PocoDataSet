using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data set default merge handler 
    /// </summary>
    public class DataSetDefaultMergeHandler : IDataSetMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current data set with refreshed data set
        /// IDataSetMergeHandler interface implementation
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public void Merge(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions)
        {
            HashSet<string> mergedTableNames = new HashSet<string>();
            MergeExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
            AddNonExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds non-existing tables to current data set from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="mergedTableNames">Merged table names</param>
        static void AddNonExistingTables(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable dataTable in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(dataTable.TableName))
                {
                    continue;
                }

                if (mergeOptions.ExcludeTablesFromMerge.Contains(dataTable.TableName))
                {
                    continue;
                }

                IDataTable? clonedDataTable = dataTable.Clone();
                if (clonedDataTable == null)
                {
                    continue;
                }

                currentDataSet.AddTable(clonedDataTable);
                for (int i = 0; i < clonedDataTable.Rows.Count; i++)
                {
                    mergeOptions.DataSetMergeResult.AddedDataRows.Add(new DataSetMergeResultEntry(clonedDataTable.TableName, clonedDataTable.Rows[i]));
                }
            }
        }

        /// <summary>
        /// Merges existing tables of current data set with tables of refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <param name="mergedTableNames">Merged table names</param>
        static void MergeExistingTables(IDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions mergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable currentTable in currentDataSet.Tables.Values)
            {
                if (mergeOptions.ExcludeTablesFromMerge.Contains(currentTable.TableName))
                {
                    continue;
                }

                mergedTableNames.Add(currentTable.TableName);

                IDataTable? refreshedTable;
                refreshedDataSet.TryGetTable(currentTable.TableName, out refreshedTable);
                if (refreshedTable == null)
                {
                    continue;
                }

                DataTableExtensions.MergeWith(currentTable, refreshedTable, mergeOptions);
            }
        }
        #endregion
    }
}
