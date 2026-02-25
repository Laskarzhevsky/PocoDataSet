using System;
using System.Collections.Generic;

using PocoDataSet.IData;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data set functionality
    /// </summary>
    public class DataSet : IDataSet
    {
        #region Data Fields
        readonly List<IDataRelation> _relations = new List<IDataRelation>();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets name
        /// IDataSet interface implementation
        /// </summary>
        public string? Name
        {
            get; set;
        }

        /// <summary>
        /// Gets relations
        /// IDataSet interface implementation
        /// </summary>
        public IReadOnlyList<IDataRelation> Relations
        {
            get
            {
                return _relations;
            }
        }

        /// <summary>
        /// Gets or sets tables
        /// IDataSet interface implementation
        /// </summary>
        public Dictionary<string, IDataTable> Tables
        {
            get; set;
        } = new();
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds relation between parent and child tables in a data set
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnName">Parent column name</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnName">Child column name</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        public IDataRelation AddRelation(string relationName, string parentTableName, string parentColumnName, string childTableName, string childColumnName)
        {
            List<string> parentColumnNames = new List<string>();
            parentColumnNames.Add(parentColumnName);

            List<string> childColumnNames = new List<string>();
            childColumnNames.Add(childColumnName);

            return AddRelation(relationName, parentTableName, parentColumnNames, childTableName, childColumnNames);
        }

        /// <summary>
        /// Adds relation between parent and child tables in a data set
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnNames">Parent column names</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnNames">Child column names</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        public IDataRelation AddRelation(string relationName, string parentTableName, IList<string> parentColumnNames, string childTableName, IList<string> childColumnNames)
        {
            if (parentColumnNames.Count == 0)
            {
                throw new ArgumentException("Parent column names must be provided.", nameof(parentColumnNames));
            }

            if (childColumnNames.Count == 0)
            {
                throw new ArgumentException("Child column names must be provided.", nameof(childColumnNames));
            }

            if (parentColumnNames.Count != childColumnNames.Count)
            {
                throw new InvalidOperationException("Parent and child column name counts must match.");
            }

            // Duplicate name throws
            for (int i = 0; i < _relations.Count; i++)
            {
                IDataRelation existing = _relations[i];
                if (existing == null)
                {
                    continue;
                }

                if (string.Equals(existing.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Relation with the same name already exists: " + relationName);
                }
            }

            DataRelation relation = new DataRelation();
            relation.RelationName = relationName;
            relation.ParentTableName = parentTableName;
            relation.ChildTableName = childTableName;
            relation.ParentColumnNames = new List<string>(parentColumnNames);
            relation.ChildColumnNames = new List<string>(childColumnNames);

            _relations.Add(relation);
            return relation;
        }

        /// <summary>
        /// Removes relation by name
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <returns>Flag indicating whether relation was removed</returns>
        public bool RemoveRelation(string relationName)
        {
            for (int i = 0; i < _relations.Count; i++)
            {
                IDataRelation relation = _relations[i];
                if (relation == null)
                {
                    continue;
                }

                if (string.Equals(relation.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    _relations.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
