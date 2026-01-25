using System;
using System.Collections.Generic;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Simple dictionary-backed entity type resolver.
    /// </summary>
    public sealed class DictionaryEntityTypeResolver : IEntityTypeResolver
    {
        private readonly Dictionary<string, Type> _map;

        public DictionaryEntityTypeResolver()
        {
            _map = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        }

        public DictionaryEntityTypeResolver Add(string tableName, Type entityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name must be provided.", nameof(tableName));
            }

            if (entityType == null)
            {
                throw new ArgumentNullException(nameof(entityType));
            }

            _map[tableName] = entityType;
            return this;
        }

        public bool TryResolveEntityType(string tableName, out Type entityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                entityType = typeof(object);
                return false;
            }

            if (_map.TryGetValue(tableName, out entityType))
            {
                return true;
            }

            entityType = typeof(object);
            return false;
        }
    }
}
