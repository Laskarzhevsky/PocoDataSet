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
        /// Builds primary key index (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="primaryKeyColumnNames">Primary key column names</param>
        /// <returns>Built data row index for inner table</returns>
        public static Dictionary<string, IDataRow> BuildPrimaryKeyIndex(this IObservableDataTable? observableDataTable, List<string> primaryKeyColumnNames)
        {
            if (observableDataTable == null)
            {
                return new Dictionary<string, IDataRow>(System.StringComparer.Ordinal);
            }

            return observableDataTable.InnerDataTable.BuildPrimaryKeyIndex(primaryKeyColumnNames);
        }
        #endregion
    }
}
