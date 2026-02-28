using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data set functionality
    /// </summary>
    public interface IDataSet
    {
        #region Properties
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
        IReadOnlyDictionary<string, IDataTable> Tables
        {
            get;
        }
        #endregion

        #region Indexers
        /// <summary>
        /// Gets the data table associated with the specified table name
        /// </summary>
        /// <returns>An instance of IDataTable representing the specified table</returns>
        IDataTable this[string tableName]
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Adds table to data set
        /// </summary>
        /// <param name="dataTable">Data table for addition</param>
        void AddTable(IDataTable dataTable);

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
        /// Adds relation between parent and child tables in a data set
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnNames">Parent column names</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnNames">Child column names</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        IDataRelation AddRelation(string relationName, string parentTableName, IList<string> parentColumnNames, string childTableName, IList<string> childColumnNames);

        /// <summary>
        /// Removes relation by name
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <returns>Flag indicating whether relation was removed</returns>
        bool RemoveRelation(string relationName);

        /// <summary>
        /// Removes table from data set by name
        /// </summary>
        /// <param name="tableName">Table name</param>
        void RemoveTable(string tableName);

        /// <summary>
        /// Attempts to retrieve the data table associated with the specified table name
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve</param>
        /// <param name="dataTable">When this method returns true, contains the data table associated with the specified table name, otherwise null</param>
        /// <returns>True if a table with the specified name was found, otherwise false</returns>
        bool TryGetTable(string tableName, out IDataTable? dataTable);
        #endregion
    }
}
