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
        /// Gets or sets child column names
        /// </summary>
        List<string> ChildColumnNames
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets child table name
        /// </summary>
        string ChildTableName
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
        /// Gets or sets parent column names
        /// </summary>
        List<string> ParentColumnNames
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets parent table name
        /// </summary>
        string ParentTableName
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