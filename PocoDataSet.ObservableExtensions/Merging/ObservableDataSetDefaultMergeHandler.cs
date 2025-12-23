using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Provides observable data set default merge handler 
    /// </summary>
    public class ObservableDataSetDefaultMergeHandler : IObservableDataSetMergeHandler
    {
        #region Public Methods
        /// <summary>
        /// Merges current observable data set with refreshed data set
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        public void Merge(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions)
        {
            HashSet<string> mergedTableNames = new HashSet<string>();
            MergeExistingTables(currentObservableDataSet, refreshedDataSet, observableMergeOptions, mergedTableNames);
            AddNonExistingTables(currentObservableDataSet, refreshedDataSet, observableMergeOptions, mergedTableNames);
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds non-existing tables to current data set from refreshed data set
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="mergedTableNames">Merged table names</param>
        static void AddNonExistingTables(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IDataTable dataTable in refreshedDataSet.Tables.Values)
            {
                if (mergedTableNames.Contains(dataTable.TableName))
                {
                    continue;
                }

                if (observableMergeOptions.ExcludeTablesFromMerge.Contains(dataTable.TableName))
                {
                    continue;
                }

                IDataTable? clonedDataTable = dataTable.Clone();
                if (clonedDataTable == null)
                {
                    continue;
                }

                IObservableDataTable observableDataTable = currentObservableDataSet.AddObservableTable(clonedDataTable);
                for (int i = 0; i < observableDataTable.Rows.Count; i++)
                {
                    observableMergeOptions.ObservableDataSetMergeResult.AddedObservableDataRows.Add(new ObservableDataSetMergeResultEntry(clonedDataTable.TableName, observableDataTable.Rows[i]));
                }
            }
        }

        /// <summary>
        /// Merges existing tables of current data set with tables of refreshed data set
        /// </summary>
        /// <param name="currentObservableDataSet">Current observable data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="observableMergeOptions">Observable merge options</param>
        /// <param name="mergedTableNames">Merged table names</param>
        static void MergeExistingTables(IObservableDataSet currentObservableDataSet, IDataSet refreshedDataSet, IObservableMergeOptions observableMergeOptions, HashSet<string> mergedTableNames)
        {
            foreach (IObservableDataTable observableDataTable in currentObservableDataSet.Tables.Values)
            {
                mergedTableNames.Add(observableDataTable.TableName);
                if (observableMergeOptions != null && observableMergeOptions.ExcludeTablesFromMerge.Contains(observableDataTable.TableName))
                {
                    continue;
                }

                IDataTable? refreshedDataTable = null;
                refreshedDataSet.TryGetTable(observableDataTable.TableName, out refreshedDataTable);
                if (refreshedDataTable != null)
                {
                    observableDataTable.MergeWith(refreshedDataTable, observableMergeOptions);
                }
            }
        }
        #endregion
    }
}
