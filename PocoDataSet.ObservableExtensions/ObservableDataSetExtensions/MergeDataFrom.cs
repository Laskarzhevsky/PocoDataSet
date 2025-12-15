using System.Collections.Generic;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Merges observable data set with data from refreshed data set
        /// </summary>
        /// <param name="currentDataSet">Current data set</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        /// <param name="mergeOptions">Merge options</param>
        public static void MergeDataFrom(this IObservableDataSet currentDataSet, IDataSet refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            List<string> mergedTableNames = new List<string>();
            foreach (IObservableDataTable observableDataTable in currentDataSet.Tables.Values)
            {
                mergedTableNames.Add(observableDataTable.TableName);
                if (mergeOptions != null && mergeOptions.ExcludeTablesFromMerge.Contains(observableDataTable.TableName))
                {
                    continue;
                }

                IDataTable? refreshedDataTable = null;
                refreshedDataSet.TryGetTable(observableDataTable.TableName, out refreshedDataTable);
                if (refreshedDataTable != null)
                {
                    observableDataTable.MergeDataFrom(refreshedDataTable, mergeOptions);
                }
            }

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

                IObservableDataTable observableDataTable = new ObservableDataTable(clonedDataTable);
                currentDataSet.Tables.Add(observableDataTable.TableName, observableDataTable);
            }
        }
        #endregion
    }
}
