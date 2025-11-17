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
        /// DataRowExtensions.CreateRowFromColumnsWithDefaultValues method example
        /// Creates row from columns with default values
        /// </summary>
        /// <param name="columnsMetadata">Columns metadata</param>
        /// <returns>Created row</returns>
        public static IDataRow CreateRowFromColumnsWithDefaultValues(List<IColumnMetadata> columnsMetadata)
        {
            // DataRowExtensions.CreateRowFromColumnsWithDefaultValues method call example
            IDataRow dataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(columnsMetadata);

            return dataRow;
        }
        #endregion
    }
}
