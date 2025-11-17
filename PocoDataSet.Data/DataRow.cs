using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json.Serialization;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data row functionality
    /// </summary>
    public class DataRow : Dictionary<string, object?>, IDataRow
    {
        #region Constructors
        /// <summary>
        /// Block "new DataRow()" outside this assembly
        /// </summary>
        [JsonConstructor]
        [EditorBrowsable(EditorBrowsableState.Never)]
        [System.Obsolete("Use DataRowExtensions.CreateRowFromColumns, DataRowExtensions.CreateRowFromColumnsWithDefaultValues, or DataTableExtension.AddNewRow instead.", false)]
        public DataRow() : base(StringComparer.Ordinal)
        {
        }

        /// <summary>
        /// Efficient pre-sizing for adapters (optional but nice)
        /// </summary>
        /// <param name="capacity"></param>
        internal DataRow(int capacity) : base(capacity, StringComparer.Ordinal) 
        {
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets data row state
        /// IDataRow interface implementation
        /// </summary>
        public DataRowState DataRowState
        {
            get; set;
        } = DataRowState.Detached;

        /// <summary>
        /// Gets or sets flag indicating whether data row is selected
        /// IDataRow interface implementation
        /// </summary>
        public bool Selected
        {
            get; set;
        }
        #endregion
    }
}
