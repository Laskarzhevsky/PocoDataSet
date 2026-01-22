using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data table extension methods
    /// </summary>
    public static partial class ObservableDataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Replaces rows in the current observable data table with rows from the refreshed table.
        /// Ensures missing columns are created, mirrors refreshed primary keys, ensures the client key column exists,
        /// and baselines all loaded rows.
        /// </summary>
        /// <param name="observableDataTable">Observable table to reload.</param>
        /// <param name="refreshedTable">Refreshed table snapshot.</param>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null.</exception>
        public static void ReloadFrom(this IObservableDataTable? observableDataTable, IDataTable refreshedTable)
        {
            if (observableDataTable == null)
            {
                return;
            }

            observableDataTable.ClearRows();

            // 2) Ensure columns exist based on refreshed schema.
            observableDataTable.AddColumns(refreshedTable.Columns);

            // 3) Mirror refreshed primary keys on the inner (non-observable) table.
            if (refreshedTable.PrimaryKeys != null)
            {
                List<string> primaryKeys = new List<string>();
                for (int i = 0; i < refreshedTable.PrimaryKeys.Count; i++)
                {
                    primaryKeys.Add(refreshedTable.PrimaryKeys[i]);
                }

                observableDataTable.InnerDataTable.PrimaryKeys = primaryKeys;
            }

            for (int i = 0; i < refreshedTable.Rows.Count; i++)
            {
                IDataRow refreshedRow = refreshedTable.Rows[i];
                IObservableDataRow newRow = observableDataTable.AddNewRow();

                // Delegate row initialization/baselining to row-level ReloadFrom.
                newRow.ReloadFrom(refreshedRow, refreshedTable.Columns);
            }
        }
        #endregion
    }
}
