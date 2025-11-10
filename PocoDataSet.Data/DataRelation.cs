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
        /// Gets or sets child columns
        /// IDataRelation interface implementation
        /// </summary>
        public List<string> ChildColumns 
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets child table
        /// IDataRelation interface implementation
        /// </summary>
        public string ChildTable
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
        /// Gets or sets parent columns
        /// IDataRelation interface implementation
        /// </summary>
        public List<string> ParentColumns 
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets parent table
        /// IDataRelation interface implementation
        /// </summary>
        public string ParentTable
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