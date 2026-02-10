using System;

namespace PocoDataSet.Data
{
    public static class RuntimeDefaults
    {
        #region Public Methods
        /// <summary>
        /// Returns the runtime default for a System.Type (null for refs, default(T) for value types).
        /// </summary>
        public static object? GetDefault(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            return ForType(t);
        }

        /// <summary>
        /// Same as GetDefault(Type). Kept for callers that already use ForType.
        /// </summary>
        public static object? ForType(Type t)
        {
            if (t == null)
            {
                throw new ArgumentNullException(nameof(t));
            }

            if (t.IsValueType)
            {
                try
                {
                    return Activator.CreateInstance(t);
                }
                catch
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }
       #endregion
    }
}
