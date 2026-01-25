using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Represents data relation
    /// </summary>
    public class DataRelation : IDataRelation
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets child column names
        /// IDataRelation interface implementation
        /// </summary>
        public List<string> ChildColumnNames 
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets child table name
        /// IDataRelation interface implementation
        /// </summary>
        public string ChildTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets display field
        /// IDataRelation interface implementation
        /// </summary>
        public string? DisplayField
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets parent column names
        /// IDataRelation interface implementation
        /// </summary>
        public List<string> ParentColumnNames 
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets parent table name
        /// IDataRelation interface implementation
        /// </summary>
        public string ParentTableName
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets relation name
        /// IDataRelation interface implementation
        /// </summary>
        public string RelationName
        {
            get; set;
        } = string.Empty;
        #endregion
    }
}