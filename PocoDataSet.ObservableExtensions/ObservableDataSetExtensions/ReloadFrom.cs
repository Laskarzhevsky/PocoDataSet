using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data set extension methods
    /// </summary>
    public static partial class ObservableDataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces rows in the current observable data set with rows from the refreshed data set.
        /// Keeps existing schema where possible and ensures missing tables/columns are created.
        /// After reload, all rows are baselined (<see cref="IObservableDataRow.AcceptChanges"/> is called).
        /// </summary>
        /// <param name="observableDataSet">Observable data set to reload.</param>
        /// <param name="refreshedDataSet">Refreshed data set</param>
        public static void ReloadFrom(this IObservableDataSet? observableDataSet, IDataSet refreshedDataSet)
        {
            if (observableDataSet == null)
            {
                return;
            }

            // 2) Ensure all refreshed tables exist and reload each table.
            foreach (KeyValuePair<string, IDataTable> kvp in refreshedDataSet.Tables)
            {
                IDataTable refreshedTable = kvp.Value;
                if (refreshedTable == null)
                {
                    continue;
                }

                IObservableDataTable observableTable;
                if (observableDataSet.Tables.ContainsKey(refreshedTable.TableName))
                {
                    observableTable = observableDataSet.Tables[refreshedTable.TableName];
                }
                else
                {
                    observableTable = observableDataSet.AddNewTable(refreshedTable.TableName);
                }

                observableTable.ReloadFrom(refreshedTable);
            }
        }
        #endregion
    }
}
