using System;
using System.Collections.Generic;

using PocoDataSet.IObjectData;

namespace PocoDataSet.ObjectData
{
    /// <summary>
    /// Default implementation of IObjectDataSet.
    /// </summary>
    public sealed class ObjectDataSet : IObjectDataSet
    {
        #region Data Fields
        /// <summary>
        /// Tables property data field
        /// </summary>
        readonly Dictionary<string, IObjectTable> _tables;
        #endregion

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the ObjectDataSet class.
        /// </summary>
        public ObjectDataSet()
        {
            _tables = new Dictionary<string, IObjectTable>(StringComparer.Ordinal);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets tables
        /// IObjectDataSet interface implementation
        /// </summary>
        public IReadOnlyDictionary<string, IObjectTable> Tables
        {
            get
            {
                return _tables;
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds a new typed table.
        /// </summary>
        /// <typeparam name="T">Item type</typeparam>
        /// <param name="name">Table name</param>
        /// <returns>Created object table</returns>
        public ObjectTable<T> AddTable<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            ObjectTable<T> objectTable = new ObjectTable<T>(name);
            _tables.Add(name, objectTable);
            return objectTable;
        }

        /// <summary>
        /// Attempts to retrieve a table by name.
        /// IObjectDataSet interface implementation
        /// </summary>
        /// <param name="name">Table name</param>
        /// <param name="table">Found table</param>
        /// <returns>True if table exists, otherwise false</returns>
        public bool TryGetTable(string name, out IObjectTable? table)
        {
            return _tables.TryGetValue(name, out table);
        }

        /// <summary>
        /// Attempts to retrieve a typed table by name.
        /// </summary>
        /// <typeparam name="T">Expected item type</typeparam>
        /// <param name="name">Table name</param>
        /// <param name="table">Found typed table</param>
        /// <returns>True if table exists and has matching type, otherwise false</returns>
        public bool TryGetTable<T>(string name, out ObjectTable<T>? table)
        {
            table = null;

            if (!_tables.TryGetValue(name, out IObjectTable? foundTable))
            {
                return false;
            }

            table = foundTable as ObjectTable<T>;
            return table != null;
        }
        #endregion
    }
}
