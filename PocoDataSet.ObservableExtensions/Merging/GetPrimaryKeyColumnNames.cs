using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Gets primary key column names
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="mergeOptions">Merge options</param>
        /// <returns>Primary keys</returns>
        public static List<string> GetPrimaryKeyColumnNames(this IObservableDataTable? observableDataTable, IObservableMergeOptions? mergeOptions)
        {
            List<string> listOfPrimaryKeys = new List<string>();
            if (observableDataTable == null)
            {
                return listOfPrimaryKeys;
            }

            // 1) Explicit overrides win
            if (mergeOptions != null)
            {
                List<string>? overrideKeys;
                if (mergeOptions.OverriddenPrimaryKeyNames.TryGetValue(observableDataTable.TableName, out overrideKeys))
                {
                    if (overrideKeys != null && overrideKeys.Count > 0)
                    {
                        return overrideKeys;
                    }
                }
            }

            // 2) Use table primary keys (single source of truth)
            if (observableDataTable.PrimaryKeys != null && observableDataTable.PrimaryKeys.Count > 0)
            {
                for (int i = 0; i < observableDataTable.PrimaryKeys.Count; i++)
                {
                    listOfPrimaryKeys.Add(observableDataTable.PrimaryKeys[i]);
                }
            }

            return listOfPrimaryKeys;
        }
        #endregion
    }
}
