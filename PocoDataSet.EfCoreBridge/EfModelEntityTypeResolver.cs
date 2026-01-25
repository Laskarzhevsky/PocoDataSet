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
    public class EfModelEntityTypeResolver : IEntityTypeResolver
    {
        #region Data Fields
        /// <summary>
        /// Holds types by name
        /// </summary>
        private readonly Dictionary<string, Type> _byName;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="dbContext">Db context</param>
        public EfModelEntityTypeResolver(DbContext dbContext)
        {
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
        #endregion

        #region Public Methods
        /// <summary>
        /// Tries to resolve entity type
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="resolvedEntityType">Resolved entity type</param>
        /// <returns></returns>
        public bool TryResolveEntityType(string tableName, out Type resolvedEntityType)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                resolvedEntityType = typeof(object);
                return false;
            }

            Type? resolved;
            if (_byName.TryGetValue(tableName, out resolved))
            {
                resolvedEntityType = resolved;
                return true;
            }

            resolvedEntityType = typeof(object);
            return false;
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds table attribute from changeset
        /// </summary>
        /// <param name="clrType">CLR type</param>
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

        /// <summary>
        /// Adds mapping between name and CLR type
        /// </summary>
        /// <param name="name">Name to map</param>
        /// <param name="clrType">CLR type</param>
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
        #endregion
    }
}
