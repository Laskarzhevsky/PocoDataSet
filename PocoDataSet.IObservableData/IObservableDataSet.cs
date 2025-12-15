using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.IObservableData
{
    /// <summary>
    /// Defines observable data set functionality
    /// </summary>
    public interface IObservableDataSet
    {
        #region Events
        /// <summary>
        /// Cell changed notification
        /// </summary>
        event EventHandler<DataFieldValueChangedEventArgs>? DataFieldValueChanged;

        /// <summary>
        /// RowsAdded event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowsAdded;

        /// <summary>
        /// RowsRemoved event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowsRemoved;
        #endregion

        #region Methods
        /// <summary>
        /// Gets observable data view
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="rowFilterString">Row filter string</param>
        /// <param name="caseSensitiveRowFilter">Flag indicating whether row filter is case sensitive</param>
        /// <param name="sortString">Sort string</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Observable data table</returns>
        IObservableDataView? GetObservableDataView(string tableName, string? rowFilterString, bool caseSensitiveRowFilter, string? sortString, string? requestorName);
        #endregion

        #region Properties
        /// <summary>
        /// Gets GUID
        /// </summary>
        Guid Guid
        {
            get;
        }

        /// <summary>
        /// Gets inner data set
        /// </summary>
        IDataSet InnerDataSet
        {
            get;
        }

        /// <summary>
        /// Gets relations
        /// </summary>
        List<IDataRelation> Relations
        {
            get;
        }

        /// <summary>
        /// Gets or sets tables
        /// </summary>
        IDictionary<string, IObservableDataTable> Tables
        {
            get;
        }
        #endregion
    }
}
