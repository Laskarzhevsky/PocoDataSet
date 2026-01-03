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
        /// Creates observable row from columns
        /// </summary>
        /// <param name="columnsMetadata">Columns metadata</param>
        /// <returns>Created row</returns>
        public static IObservableDataRow CreateRowFromColumns(IList<IColumnMetadata> columnsMetadata)
        {
            IDataRow dataRow = DataRowFactory.CreateEmpty(columnsMetadata.Count);
            foreach (IColumnMetadata column in columnsMetadata)
            {
                dataRow[column.ColumnName] = null;
            }

            IObservableDataRow observableDataRow = new ObservableDataRow(dataRow);
            return observableDataRow;
        }
        #endregion
    }
}
