using System;
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
        public static DataSetsMergeResult MergeWith(this IDataSet? currentDataSet, IDataSet? refreshedDataSet, IMergeOptions? mergeOptions = null)
        {
            List<IDataRow> listOfAddedDataRows = new List<IDataRow>();
            List<IDataRow> listOfDeletedDataRows = new List<IDataRow>();
            List<IDataRow> listOfUpdatedDataRows = new List<IDataRow>();
            DataSetsMergeResult dataSetsMergeResult = new DataSetsMergeResult(listOfAddedDataRows, listOfDeletedDataRows, listOfUpdatedDataRows);
            if (currentDataSet == null || refreshedDataSet == null)
            {
                return dataSetsMergeResult;
            }

            List<string> mergedTableNames = new List<string>();
            foreach (IDataTable dataTable in currentDataSet.Tables.Values)
            {
                mergedTableNames.Add(dataTable.TableName);
                if (mergeOptions != null && mergeOptions.ExcludeTablesFromMerge.Contains(dataTable.TableName))
                {
                    continue;
                }

                IDataTable? refreshedDataTable = null;
                refreshedDataSet.TryGetTable(dataTable.TableName, out refreshedDataTable);
                if (refreshedDataTable != null)
                {
                    dataTable.MergeWith(refreshedDataTable, mergeOptions);
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

                currentDataSet.AddTable(clonedDataTable);
            }

            return dataSetsMergeResult;
        }
        #endregion
    }
}
