using System;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Optional attribute used by the EF bridge to map a POCO DataSet changeset table name to an entity CLR type,
    /// when the EF table name (from [Table] or fluent mapping) does not match the changeset table name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public sealed class ChangesetTableAttribute : Attribute
    {
        public ChangesetTableAttribute(string tableName)
        {
            if (string.IsNullOrWhiteSpace(tableName))
            {
                throw new ArgumentException("Table name must not be empty.", nameof(tableName));
            }

            TableName = tableName;
        }

        public string TableName
        {
            get;
        }
    }
}
