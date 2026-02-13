using System;

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
        /// Creates new data row in data table with default values taken from columns metadata
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <returns>New data row created in data table</returns>
        public static IDataRow AddNewRow(this IDataTable? dataTable)
        {
            if (dataTable == null)
            {
                throw new System.ArgumentNullException(nameof(dataTable));
            }

            
            // Ensure __ClientKey column exists in the table schema.
            // AddNewRow() always assigns a client correlation key, and schema must reflect it.
            if (!dataTable.ContainsColumn(SpecialColumnNames.CLIENT_KEY))
            {
                dataTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            }

            IDataRow dataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(dataTable.Columns);
            dataRow[SpecialColumnNames.CLIENT_KEY] = Guid.NewGuid();
            dataRow.SetDataRowState(DataRowState.Added);
            dataTable.AddRow(dataRow);

            return dataRow;
        }
        #endregion
    }
}
