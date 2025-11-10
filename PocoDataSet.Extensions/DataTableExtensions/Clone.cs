using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Clones data table
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>Cloned data table</returns>
        public static IDataTable? Clone(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                return dataTable;
            }

            IDataTable clonedDataTable = new DataTable();
            clonedDataTable.TableName = dataTable.TableName;
            clonedDataTable.CloneColumnsFrom(dataTable);
            clonedDataTable.CloneRowsFrom(dataTable);

            return clonedDataTable;
        }
        #endregion
    }
}
