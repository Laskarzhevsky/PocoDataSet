using System.Collections.Generic;

using PocoDataSet.Extensions;
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
        /// Adds columns to observable table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="columns">Columns</param>
        public static void AddColumns(this IObservableDataTable? observableDataTable, List<IColumnMetadata> columns)
        {
            if (observableDataTable == null)
            {
                return;
            }

            observableDataTable.InnerDataTable.AddColumns(columns);
        }
        #endregion
    }
}
