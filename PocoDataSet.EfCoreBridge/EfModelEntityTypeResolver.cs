using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Resolves entity types for changeset tables using EF Core model metadata.
    /// Resolution order per entity:
    /// 1) [ChangesetTable("X")] attribute (optional)
    /// 2) EF table name mapping (from [Table] or fluent mapping)
    /// 3) CLR type name
    /// </summary>
    public sealed class EfModelEntityTypeResolver : IEntityTypeResolver
    {
        private readonly Dictionary<string, Type> _byName;

        public EfModelEntityTypeResolver(DbContext dbContext)
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            _byName = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);

            IModel model = dbContext.Model;
            if (model == null)
            {
                return;
            }

            foreach (IEntityType entityType in model.GetEntityTypes())
            {
                if (entityType == null)
                {
                    continue;
                }

                Type? clrType = entityType.ClrType;
                if (clrType == null)
                {
                    continue;
                }

                // 1) Custom [ChangesetTable]
                AddFromChangesetTableAttribute(clrType);

                // 2) EF table name mapping (requires Microsoft.EntityFrameworkCore.Relational at runtime)
                string? tableName = entityType.GetTableName();
                if (!string.IsNullOrWhiteSpace(tableName))
                {
                    AddMapping(tableName, clrType);
                }

                // 3) CLR type name fallback
                AddMapping(clrType.Name, clrType);
            }
        }

        public bool TryResolveEntityType(string tableName, out Type entityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                entityType = typeof(object);
                return false;
            }

            Type? resolved;
            if (_byName.TryGetValue(tableName, out resolved))
            {
                entityType = resolved;
                return true;
            }

            entityType = typeof(object);
            return false;
        }

        private void AddFromChangesetTableAttribute(Type clrType)
        {
            object[] attributes = clrType.GetCustomAttributes(typeof(ChangesetTableAttribute), false);
            if (attributes == null || attributes.Length == 0)
            {
                return;
            }

            ChangesetTableAttribute? attribute = attributes[0] as ChangesetTableAttribute;
            if (attribute == null)
            {
                return;
            }

            if (string.IsNullOrWhiteSpace(attribute.TableName))
            {
                return;
            }

            AddMapping(attribute.TableName, clrType);
        }

        private void AddMapping(string name, Type clrType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return;
            }

            // Prefer first registration to keep behavior deterministic.
            if (_byName.ContainsKey(name))
            {
                return;
            }

            _byName.Add(name, clrType);
        }
    }
}
