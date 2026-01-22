using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using PocoDataSet.IObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Initializes (loads) an observable data row from a refreshed row snapshot.
        /// Copies values using refreshed schema columns (schema-evolution safe), optionally assigns a new client key,
        /// and baselines the row (<see cref="IObservableDataRow.AcceptChanges"/> is called).
        /// </summary>
        /// <param name="observableDataRow">Observable row to load.</param>
        /// <param name="refreshedRow">Refreshed row snapshot.</param>
        /// <param name="refreshedColumns">Refreshed table columns.</param>
        /// <exception cref="ArgumentNullException">Thrown when arguments are null.</exception>
        public static void ReloadFrom(this IObservableDataRow? observableDataRow, IDataRow refreshedRow, IList<IColumnMetadata> refreshedColumns)
        {
            if (observableDataRow == null)
            {
                return;
            }

            // Copy refreshed values using refreshed schema columns (schema-evolution safe).
            observableDataRow.CopyFrom(refreshedRow, refreshedColumns);

            // Loaded baseline.
            observableDataRow.AcceptChanges();
        }
        #endregion
    }
}
