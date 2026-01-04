using System;
using System.Collections.Generic;
using System.Reflection;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Builds PocoDataSet column metadata from EF Core model metadata.
    /// </summary>
    internal static class EfColumnMetadataBuilder
    {
        public static List<IColumnMetadata> Build<TEntity>(DbContext dbContext)
            where TEntity : class
        {
            if (dbContext == null)
            {
                throw new ArgumentNullException(nameof(dbContext));
            }

            IEntityType? entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException(
                    "EF Core model does not contain entity type '" + typeof(TEntity).FullName + "'.");
            }

            IKey? primaryKey = entityType.FindPrimaryKey();

            List<IForeignKey> foreignKeys = new List<IForeignKey>();
            foreach (IForeignKey fk in entityType.GetForeignKeys())
            {
                foreignKeys.Add(fk);
            }

            List<IColumnMetadata> result = new List<IColumnMetadata>();

            int displayOrder = 0;

            foreach (IProperty property in entityType.GetProperties())
            {
                PropertyInfo? clrProperty = property.PropertyInfo;

                // Skip shadow properties: we cannot materialize them into POCO rows reliably.
                if (clrProperty == null)
                {
                    if (IsPrimaryKeyProperty(primaryKey, property))
                    {
                        throw new InvalidOperationException(
                            "Entity '" + typeof(TEntity).FullName + "' has a primary key shadow property '" + property.Name + "'. " +
                            "Shadow-key entities are not supported by PocoDataSet.EfCoreBridge for roundtrip updates.");
                    }

                    continue;
                }

                ColumnMetadata column = new ColumnMetadata();
                column.ColumnName = property.Name;
                column.DataType = MapClrTypeToDataTypeName(GetNonNullableClrType(property.ClrType));
                column.IsNullable = property.IsNullable;

                // Primary key
                column.IsPrimaryKey = IsPrimaryKeyProperty(primaryKey, property);

                // Foreign key
                ApplyForeignKeyInfo(column, property, foreignKeys);

                // Display fields
                column.DisplayName = property.Name;
                column.DisplayOrder = displayOrder;
                displayOrder++;

                // Optional description (comment) if available
                column.Description = GetCommentOrNull(property);

                // Optional length / precision / scale
                column.MaxLength = GetMaxLengthOrNull(property);
                column.Precision = GetByteOrNull(GetPrecisionOrNull(property));
                column.Scale = GetByteOrNull(GetScaleOrNull(property));

                // If primary key, make it non-nullable
                if (column.IsPrimaryKey)
                {
                    column.IsNullable = false;
                }

                result.Add(column);
            }

            return result;
        }

        private static bool IsPrimaryKeyProperty(IKey? primaryKey, IProperty property)
        {
            if (primaryKey == null)
            {
                return false;
            }

            foreach (IProperty pkProp in primaryKey.Properties)
            {
                if (string.Equals(pkProp.Name, property.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static void ApplyForeignKeyInfo(
            ColumnMetadata column,
            IProperty property,
            List<IForeignKey> foreignKeys)
        {
            column.IsForeignKey = false;
            column.ReferencedTableName = null;
            column.ReferencedColumnName = null;

            foreach (IForeignKey fk in foreignKeys)
            {
                foreach (IProperty depProp in fk.Properties)
                {
                    if (string.Equals(depProp.Name, property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        column.IsForeignKey = true;

                        // If single-column FK, try to populate referenced table/column.
                        if (fk.PrincipalKey.Properties.Count == 1)
                        {
                            column.ReferencedColumnName = fk.PrincipalKey.Properties[0].Name;
                        }

                        // Table name may require relational provider; fall back to CLR name.
                        string? tableName = TryGetRelationalTableName(fk.PrincipalEntityType);
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            tableName = fk.PrincipalEntityType.ClrType != null
                                ? fk.PrincipalEntityType.ClrType.Name
                                : fk.PrincipalEntityType.Name;
                        }

                        column.ReferencedTableName = tableName;

                        return;
                    }
                }
            }
        }

        private static Type GetNonNullableClrType(Type clrType)
        {
            Type? underlying = Nullable.GetUnderlyingType(clrType);
            if (underlying != null)
            {
                return underlying;
            }

            return clrType;
        }

        private static string MapClrTypeToDataTypeName(Type clrType)
        {
            if (clrType == typeof(byte[]))
                return DataTypeNames.BINARY;

            if (clrType == typeof(bool))
                return DataTypeNames.BOOL;

            if (clrType == typeof(byte))
                return DataTypeNames.BYTE;

            if (clrType == typeof(short))
                return DataTypeNames.INT16;

            if (clrType == typeof(int))
                return DataTypeNames.INT32;

            if (clrType == typeof(long))
                return DataTypeNames.INT64;

            if (clrType == typeof(decimal))
                return DataTypeNames.DECIMAL;

            if (clrType == typeof(double))
                return DataTypeNames.DOUBLE;

            if (clrType == typeof(float))
                return DataTypeNames.SINGLE;

            if (clrType == typeof(Guid))
                return DataTypeNames.GUID;

            if (clrType == typeof(DateTime))
                return DataTypeNames.DATE_TIME;

            if (clrType.FullName == "System.DateOnly")
                return DataTypeNames.DATE;

            if (clrType.FullName == "System.TimeOnly")
                return DataTypeNames.TIME;

            if (clrType == typeof(string))
                return DataTypeNames.STRING;

            return DataTypeNames.OBJECT;
        }

        private static int? GetMaxLengthOrNull(IProperty property)
        {
            try
            {
                return property.GetMaxLength();
            }
            catch
            {
                return null;
            }
        }

        private static int? GetPrecisionOrNull(IProperty property)
        {
            try
            {
                return property.GetPrecision();
            }
            catch
            {
                return null;
            }
        }

        private static int? GetScaleOrNull(IProperty property)
        {
            try
            {
                return property.GetScale();
            }
            catch
            {
                return null;
            }
        }

        private static string? GetCommentOrNull(IProperty property)
        {
            try
            {
                return property.GetComment();
            }
            catch
            {
                return null;
            }
        }

        private static byte? GetByteOrNull(int? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            if (value.Value < 0 || value.Value > 255)
            {
                return null;
            }

            return (byte)value.Value;
        }

        private static string? TryGetRelationalTableName(IEntityType entityType)
        {
            try
            {
                return entityType.GetTableName();
            }
            catch
            {
                return null;
            }
        }
    }
}
