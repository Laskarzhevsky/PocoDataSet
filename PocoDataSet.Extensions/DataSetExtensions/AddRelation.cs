using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds relation
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnNames">Parent column names</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnNames">Child column names</param>
        public static void AddRelation(this IDataSet dataSet, string relationName, string parentTableName, IList<string> parentColumnNames, string childTableName, IList<string> childColumnNames)
        {
            if (dataSet == null)
            {
                return;
            }

            if (dataSet.Relations == null)
            {
                dataSet.Relations = new List<IDataRelation>();
            }

            // Prevent duplicates
            for (int i = 0; i < dataSet.Relations.Count; i++)
            {
                IDataRelation existing = dataSet.Relations[i];
                if (existing == null)
                {
                    continue;
                }

                if (string.Equals(existing.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
            }

            DataRelation relation = new DataRelation();
            relation.RelationName = relationName;
            relation.ParentTableName = parentTableName;
            relation.ChildTableName = childTableName;
            relation.ParentColumnNames = new List<string>(parentColumnNames);
            relation.ChildColumnNames = new List<string>(childColumnNames);

            dataSet.Relations.Add(relation);
        }
        #endregion
    }
}
