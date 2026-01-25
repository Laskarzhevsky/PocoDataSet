using System;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains set extension methods
    /// </summary>
    public static partial class DataSetExtensions
    {
        #region Public Methods
        /// <summary>
        /// Checks
        /// </summary>
        /// <param name="dataSet">Data set</param>
        /// <param name="relationName">Relation name</param>
        /// <returns></returns>
        public static bool RelationExists(this IDataSet? dataSet, string relationName)
        {
            if (dataSet == null)
            {
                return false;
            }

            if (dataSet.Relations == null)
            {
                return false;
            }

            for (int i = 0; i < dataSet.Relations.Count; i++)
            {
                IDataRelation relation = dataSet.Relations[i];
                if (string.Equals(relation.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
        #endregion
    }
}
