using System;
using System.Collections.Generic;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Simple dictionary-backed entity type resolver.
    /// </summary>
    public sealed class DictionaryEntityTypeResolver : IEntityTypeResolver
    {
        #region Data Fields
        /// <summary>
        /// 
        /// </summary>
        private readonly Dictionary<string, Type> _map;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public DictionaryEntityTypeResolver()
        {
            _map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds mappin between table name and entry type
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="entityType">Entry type</param>
        /// <returns>The entire dictionary with newly-added mapping</returns>
        public DictionaryEntityTypeResolver Add(string tableName, Type entityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name must be provided.", nameof(tableName));
            }

            _map[tableName] = entityType;

            return this;
        }

        /// <summary>
        /// Tries to resolve entity type
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="resolvedEntityType">Resolved entry type</param>
        /// <returns>True if entity type was resolved successfully, otherwise false</returns>
        public bool TryResolveEntityType(string tableName, out Type resolvedEntityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                resolvedEntityType = typeof(object);
                return false;
            }

            Type? foundType = null;
            if (_map.TryGetValue(tableName, out foundType))
            {
                resolvedEntityType = foundType;
                return true;
            }

            resolvedEntityType = typeof(object);
            return false;
        }
        #endregion
    }
}
