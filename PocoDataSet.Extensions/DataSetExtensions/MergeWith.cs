using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges current data set with refreshed data set by copying all changes from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static IDataSetMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            if (mergeOptions == null)
            {
                mergeOptions = new MergeOptions();
            }

            if (currentDataSet == null || refreshedDataSet == null)
            {
                return mergeOptions.DataSetMergeResult;
            }

            HashSet<string> mergedTableNames = new HashSet<string>();
            MergeExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);
            AddNonExistingTables(currentDataSet, refreshedDataSet, mergeOptions, mergedTableNames);

            return mergeOptions.DataSetMergeResult;
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

                if (mergeOptions != null && mergeOptions.ExcludeTablesFromMerge.Contains(dataTable.TableName))
                {
                    continue;
                }

                IDataTable? clonedDataTable = dataTable.Clone();
                if (clonedDataTable == null)
                {
                    continue;
                }

                currentDataSet.AddTable(clonedDataTable);
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
                if (currentTable == null)
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

                ITableMergeHandler tableHandler = mergeOptions!.GetTableMergeHandler(currentTable.TableName);
                tableHandler.Merge(currentTable, refreshedTable, mergeOptions);
            }
        }
        #endregion
    }
}
