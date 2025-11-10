using System.Collections.Generic;

namespace PocoDataSet.IData
{
    /// <summary>
    /// Represents data relation
    /// </summary>
    public interface IDataRelation
    {
        #region Properties
        /// <summary>
        /// Gets or sets child columns
        /// </summary>
        List<string> ChildColumns
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets child table
        /// </summary>
        string ChildTable
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets display field
        /// </summary>
        string? DisplayField
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets parent columns
        /// </summary>
        List<string> ParentColumns
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets parent table
        /// </summary>
        string ParentTable
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets relation name 
        /// </summary>
        string RelationName
        {
            get; set;
        }
        #endregion
    }
}