using System;

namespace PocoDataSet.EfCoreBridge
{
    /// <summary>
    /// Optional attribute used by the EF bridge to map a POCO DataSet changeset table name to an entity CLR type,
    /// when the EF table name (from [Table] or fluent mapping) does not match the changeset table name.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class ChangesetTableAttribute : Attribute
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableName">Table name</param>
        public ChangesetTableAttribute(string tableName)
        {
            TableName = tableName;
        }
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName
        {
            get;
        }
        #endregion
    }
}
