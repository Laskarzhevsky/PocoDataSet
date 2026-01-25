using System;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Exception thrown when an optimistic concurrency check fails.
    /// </summary>
    public class PocoConcurrencyException : Exception
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="tableName">Table name</param>
        /// <param name="operation">Operation (UPDATE / DELETE)</param>
        /// <param name="primaryKeyText">Primary key values as text</param>
        public PocoConcurrencyException(string tableName, string operation, string primaryKeyText)
            : base($"Optimistic concurrency conflict. Table=\"{tableName}\", Operation=\"{operation}\", Key=\"{primaryKeyText}\"")
        {
            TableName = tableName;
            Operation = operation;
            PrimaryKeyText = primaryKeyText;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets table name
        /// </summary>
        public string TableName
        {
            get;
        }

        /// <summary>
        /// Gets operation (UPDATE / DELETE)
        /// </summary>
        public string Operation
        {
            get;
        }

        /// <summary>
        /// Gets primary key text
        /// </summary>
        public string PrimaryKeyText
        {
            get;
        }
        #endregion
    }
}
