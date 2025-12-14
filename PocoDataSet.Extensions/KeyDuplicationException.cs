using System;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides a way to throw an exception when adding an element with a key to a collection
    /// when the collection already contains an element with the specified key
    /// </summary>
    public class KeyDuplicationException : Exception
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        public KeyDuplicationException()
        {
        }

        /// <summary>
        /// Creates new exception taking message as an argument
        /// </summary>
        /// <param name="message">Exception message</param>
        public KeyDuplicationException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates new exception taking message and inner exception as arguments
        /// </summary>
        /// <param name="message">Exception message</param>
        /// <param name="innerException">Inner exception</param>
        public KeyDuplicationException(string message, Exception innerException) : base(message, innerException)
        {
        }
        #endregion
    }
}
