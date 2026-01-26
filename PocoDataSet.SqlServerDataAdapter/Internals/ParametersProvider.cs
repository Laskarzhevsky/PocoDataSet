using System;

using Microsoft.Data.SqlClient;

namespace PocoDataSet.SqlServerDataAdapter
{
    /// <summary>
    /// Provides paramenters
    /// </summary>
    internal static class ParametersProvider
    {
        #region Public Methods
        /// <summary>
        /// Creates SQL parameter
        /// </summary>
        /// <param name="parameterName">Parameter name</param>
        /// <param name="parameterValue">Parameter value</param>
        /// <returns>SQL parameter</returns>
        public static SqlParameter CreateSqlParameter(string parameterName, object? parameterValue)
        {
            SqlParameter sqlParameter = new SqlParameter();
            sqlParameter.ParameterName = parameterName;
            if (parameterValue == null)
            {
                sqlParameter.Value = DBNull.Value;
            }
            else
            {
                sqlParameter.Value = parameterValue;
            }

            return sqlParameter;
        }
        #endregion
    }
}
