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
        /// <param name="columnMetadata">Column metadata</param>
        public static void AddColumn(this IObservableDataTable? observableDataTable, IColumnMetadata columnMetadata)
        {
            if (observableDataTable == null)
            {
                return;
            }

                        observableDataTable.InnerDataTable.AddColumn(
                columnMetadata.ColumnName,
                columnMetadata.DataType,
                columnMetadata.IsNullable,
                columnMetadata.IsPrimaryKey,
                columnMetadata.IsForeignKey);
}
        #endregion
    }
}
