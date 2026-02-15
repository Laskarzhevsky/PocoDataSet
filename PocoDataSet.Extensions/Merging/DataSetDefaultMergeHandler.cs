using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.Extensions.Merging.Modes;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data set default merge handler 
    /// </summary>
    public class DataSetDefaultMergeHandler : IDataSetMergeHandler
    {
        private static readonly PostSaveDataTableMerger _postSaveMerger = new PostSaveDataTableMerger();
        private static readonly RefreshIfNoChangesExistDataTableMerger _refreshIfCleanMerger = new RefreshIfNoChangesExistDataTableMerger();
        private static readonly RefreshPreservingLocalChangesDataTableMerger _refreshPreservingMerger = new RefreshPreservingLocalChangesDataTableMerger();
        private static readonly ReplaceDataTableMerger _replaceMerger = new ReplaceDataTableMerger();

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

                MergeMode mode = mergeOptions.MergeMode;
                switch (mode)
                {
                    case MergeMode.PostSave:
                        _postSaveMerger.MergePostSave(currentTable, refreshedTable, mergeOptions);
                        break;

                    case MergeMode.RefreshIfNoChangesExist:
                        _refreshIfCleanMerger.MergeRefreshIfNoChangesExist(currentTable, refreshedTable, mergeOptions);
                        break;

                    case MergeMode.RefreshPreservingLocalChanges:
                        _refreshPreservingMerger.MergeRefreshPreservingLocalChanges(currentTable, refreshedTable, mergeOptions);
                        break;

                    case MergeMode.Replace:
                        _replaceMerger.Replace(currentTable, refreshedTable, mergeOptions);
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(nameof(mergeOptions.MergeMode), mergeOptions.MergeMode, "Unknown merge mode.");
                }
            }
        }
        #endregion
    }
}
