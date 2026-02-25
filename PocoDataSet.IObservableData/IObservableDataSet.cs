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
        /// RowAdded event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowAdded;

        /// <summary>
        /// RowRemoved event
        /// </summary>
        event EventHandler<RowsChangedEventArgs>? RowRemoved;

        /// <summary>
        /// TableAdded event
        /// </summary>
        event EventHandler<TablesChangedEventArgs>? TableAdded;

        /// <summary>
        /// TableRemoved event
        /// </summary>
        event EventHandler<TablesChangedEventArgs>? TableRemoved;
        #endregion

        #region Methods
        /// <summary>
        /// Adds observable table
        /// </summary>
        /// <param name="dataTable">Data table for addition</param>
        /// <returns>Added observable data table</returns>
        IObservableDataTable AddObservableTable(IDataTable dataTable);

        /// <summary>
        /// Adds relation between parent and child tables in a data set
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnName">Parent column name</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnName">Child column name</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        IDataRelation AddRelation(string relationName, string parentTableName, string parentColumnName, string childTableName, string childColumnName);

        /// <summary>
        /// Adds relation between parent and child tables in a data set.
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnNames">Parent column names</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnNames">Child column names</param>
        /// <returns>Created relation</returns>
        IDataRelation AddRelation(string relationName, string parentTableName, IList<string> parentColumnNames, string childTableName, IList<string> childColumnNames);

        /// <summary>
        /// Gets observable data view
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="rowFilterString">Row filter string</param>
        /// <param name="caseSensitiveRowFilter">Flag indicating whether row filter is case sensitive</param>
        /// <param name="sortString">Sort string</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Observable data table</returns>
        IObservableDataView? GetObservableDataView(string tableName, string? rowFilterString, bool caseSensitiveRowFilter, string? sortString, string requestorName);

        /// <summary>
        /// Removes observable table
        /// </summary>
        /// <param name="tableName">Table name</param>
        void RemoveObservableTable(string tableName);

        /// <summary>
        /// Removes observable data view from cache and disposes it
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Flag indicating whether view was removed</returns>
        bool RemoveObservableDataView(string tableName, string requestorName);

        /// <summary>
        /// Removes all observable data views for the specified requestor and disposes them
        /// </summary>
        /// <param name="requestorName">Requestor name</param>
        /// <returns>Number of removed views</returns>
        int RemoveObservableDataViewsForRequestor(string requestorName);

        /// <summary>
        /// Removes relation by name
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <returns>Flag indicating whether relation was removed</returns>
        bool RemoveRelation(string relationName);
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
        /// Gets or sets name
        /// </summary>
        string? Name
        {
            get; set;
        }

        /// <summary>
        /// Gets relations
        /// </summary>
        IReadOnlyList<IDataRelation> Relations
        {
            get;
        }

        /// <summary>
        /// Gets or sets tables
        /// </summary>
        IReadOnlyDictionary<string, IObservableDataTable> Tables
        {
            get;
        }
        #endregion
    }
}
