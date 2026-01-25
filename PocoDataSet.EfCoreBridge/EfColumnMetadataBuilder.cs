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
    /// Provides Entiry Framework column mMetadata builder functionality
    /// </summary>
    internal static class EfColumnMetadataBuilder
    {
        #region Public Methods
        /// <summary>
        /// Builds PocoDataSet column metadata from EF Core model metadata
        /// </summary>
        /// <typeparam name="TEntity">Entity type</typeparam>
        /// <param name="dbContext">Db context</param>
        /// <returns>Built PocoDataSet column metadata from EF Core model metadata</returns>
        public static List<IColumnMetadata> Build<TEntity>(DbContext dbContext) where TEntity : class
        {
            IEntityType? entityType = dbContext.Model.FindEntityType(typeof(TEntity));
            if (entityType == null)
            {
                throw new InvalidOperationException("EF Core model does not contain entity type '" + typeof(TEntity).FullName + "'.");
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
        #endregion

        #region Private Methods
        /// <summary>
        /// Applies foreign key info
        /// </summary>
        /// <param name="columnMetadata">Column metadata</param>
        /// <param name="property">Property</param>
        /// <param name="foreignKeys">Foreign keys</param>
        private static void ApplyForeignKeyInfo(ColumnMetadata columnMetadata, IProperty property, List<IForeignKey> foreignKeys)
        {
            columnMetadata.IsForeignKey = false;
            columnMetadata.ReferencedTableName = null;
            columnMetadata.ReferencedColumnName = null;

            foreach (IForeignKey foreignKey in foreignKeys)
            {
                foreach (IProperty depProp in foreignKey.Properties)
                {
                    if (string.Equals(depProp.Name, property.Name, StringComparison.OrdinalIgnoreCase))
                    {
                        columnMetadata.IsForeignKey = true;

                        // If single-column FK, try to populate referenced table/column.
                        if (foreignKey.PrincipalKey.Properties.Count == 1)
                        {
                            columnMetadata.ReferencedColumnName = foreignKey.PrincipalKey.Properties[0].Name;
                        }

                        // Table name may require relational provider; fall back to CLR name.
                        string? tableName = TryGetRelationalTableName(foreignKey.PrincipalEntityType);
                        if (string.IsNullOrWhiteSpace(tableName))
                        {
                            tableName = foreignKey.PrincipalEntityType.ClrType != null
                                ? foreignKey.PrincipalEntityType.ClrType.Name
                                : foreignKey.PrincipalEntityType.Name;
                        }

                        columnMetadata.ReferencedTableName = tableName;

                        return;
                    }
                }
            }
        }

        /// <summary>
        /// Gets byte
        /// </summary>
        /// <param name="value">Value for conversion</param>
        /// <returns>Value converted into byte</returns>
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

        /// <summary>
        /// Gets property comment
        /// </summary>
        /// <param name="property">Property to inspect</param>
        /// <returns>Property comment</returns>
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

        /// <summary>
        /// Gets property max length
        /// </summary>
        /// <param name="property">Property to inspect</param>
        /// <returns>Property max length</returns>
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

        /// <summary>
        /// Gets non-nullable CLR type
        /// </summary>
        /// <param name="clrType">CLR type</param>
        /// <returns>Non-nullable CLR type</returns>
        private static Type GetNonNullableClrType(Type clrType)
        {
            Type? underlying = Nullable.GetUnderlyingType(clrType);
            if (underlying != null)
            {
                return underlying;
            }

            return clrType;
        }

        /// <summary>
        /// Gets property precision
        /// </summary>
        /// <param name="property">Property to inspect</param>
        /// <returns>Property precision</returns>
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

        /// <summary>
        /// Gets property scale
        /// </summary>
        /// <param name="property">Property to inspect</param>
        /// <returns>Property scale</returns>
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

        /// <summary>
        /// Checks whether property is a primary key
        /// </summary>
        /// <param name="primaryKey">Primary key</param>
        /// <param name="property">Property to check</param>
        /// <returns>True if property is a primary key, otherwise false</returns>
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

        /// <summary>
        /// Maps CLR type to data type name
        /// </summary>
        /// <param name="clrType">CLR type</param>
        /// <returns>Mapped CLR type to data type name</returns>
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

        /// <summary>
        /// Tries to get relational table name
        /// </summary>
        /// <param name="entityType">Entity type</param>
        /// <returns>Relational table name</returns>
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
        #endregion
    }
}
