using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Demo.DataSetExtensions
{
    /// <summary>
    /// Provides examples of POCO DataSet functionality
    /// </summary>
    internal static partial class DataRowExtensionExamples
    {
        #region Public Methods
        /// <summary>
        /// DataRowExtensions.CreateRowFromColumns method example
        /// Creates row from columns metadata
        /// </summary>
        /// <param name="columnsMetadata">Columns metadata</param>
        /// <returns>Created row</returns>
        public static IDataRow CreateRowFromColumns(List<IColumnMetadata> columnsMetadata)
        {
            // DataRowExtensions.CreateRowFromColumns method call example
            IDataRow dataRow = DataRowExtensions.CreateRowFromColumns(columnsMetadata);

            return dataRow;
        }
        #endregion
    }
}
