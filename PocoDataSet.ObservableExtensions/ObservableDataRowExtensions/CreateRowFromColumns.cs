using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;

namespace PocoDataSet.ObservableExtensions
{
    /// <summary>
    /// Contains observable data row extension methods
    /// </summary>
    public static partial class ObservableDataRowExtensions
    {
        #region Public Methods
        /// <summary>
        /// Creates detached observable row without any values from columns metadata
        /// </summary>
        /// <param name="listOfColumnMetadata">List of column metadata</param>
        /// <returns>Created row</returns>
        public static IObservableDataRow CreateRowFromColumns(IList<IColumnMetadata> listOfColumnMetadata)
        {
            IDataRow dataRow = DataRowFactory.CreateEmpty(listOfColumnMetadata.Count);
            foreach (IColumnMetadata column in listOfColumnMetadata)
            {
                dataRow[column.ColumnName] = null;
            }

            IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);
            return observableDataRow;
        }
        #endregion
    }
}
