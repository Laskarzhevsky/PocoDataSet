using System.Collections.Generic;

namespace PocoDataSet.IObjectData
{
    /// <summary>
    /// Represents a named collection of typed object tables.
    /// </summary>
    public interface IObjectDataSet
    {
        #region Properties
        /// <summary>
        /// Gets tables
        /// </summary>
        IReadOnlyDictionary<string, IObjectTable> Tables
        {
            get;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Attempts to retrieve a table by name.
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="table">Found table</param>
        /// <returns>True if table exists, otherwise false</returns>
        bool TryGetTable(string name, out IObjectTable? table);
        #endregion
    }
}
