using System;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Resolves an Entity Framework entity CLR type for a PocoDataSet table name.
    /// </summary>
    public interface IEntityTypeResolver
    {
        /// <summary>
        /// Attempts to resolve an entity CLR type for the provided table name.
        /// </summary>
        bool TryResolveEntityType(string tableName, out Type entityType);
    }
}
