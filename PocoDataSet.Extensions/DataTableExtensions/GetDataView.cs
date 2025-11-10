using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        public static List<IDataRow> GetDataView(this IDataTable dataTable, List<string> primaryKeyColumnNames, IDataRowFilter? dataRowFilter)
        {
            return new List<IDataRow>();
        }
        #endregion
    }
}
