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
        /// Adds a column to observable table (delegates to inner table)
        /// </summary>
        /// <param name="observableDataTable">Observable data table</param>
        /// <param name="columnName">Column name</param>
        /// <param name="dataType">Data type</param>
        /// <param name="isNullable">Flag indicating whether column is nullable</param>
        /// <param name="isPrimaryKey">Flag indicating whether column is primary key</param>
        /// <param name="isForeignKey">Flag indicating whether column is foreign key</param>
        /// <returns>Added column to observable table</returns>
        public static IColumnMetadata AddColumn(this IObservableDataTable? observableDataTable, string columnName, string dataType, bool? isNullable = null, bool? isPrimaryKey = null, bool? isForeignKey = null)
        {
            if (observableDataTable == null)
            {
                return default!;
            }

            return observableDataTable.InnerDataTable.AddColumn(columnName, dataType, isNullable, isPrimaryKey, isForeignKey);
        }
        #endregion
    }
}
