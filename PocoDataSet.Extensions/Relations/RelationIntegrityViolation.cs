using System.Collections.Generic;
using System.Text;

namespace PocoDataSet.Extensions.Relations
{
    /// <summary>
    /// Represents a referential-integrity violation detected by relation validation.
    /// </summary>
    public class RelationIntegrityViolation
    {
        #region Public Properties
        /// <summary>
        /// Gets or sets snapshot of child key values (column -> value).
        /// </summary>
        public Dictionary<string, object?> ChildKey
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets child table name.
        /// </summary>
        public string ChildTable
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets violation kind.
        /// </summary>
        public RelationIntegrityViolationKind Kind
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets message.
        /// </summary>
        public string Message
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets snapshot of parent key values (column -> value).
        /// </summary>
        public Dictionary<string, object?> ParentKey
        {
            get; set;
        } = new();

        /// <summary>
        /// Gets or sets parent table name.
        /// </summary>
        public string ParentTable
        {
            get; set;
        } = string.Empty;

        /// <summary>
        /// Gets or sets relation name.
        /// </summary>
        public string RelationName
        {
            get; set;
        } = string.Empty;
        #endregion

        #region Public Methods
        /// <summary>
        /// Returns human-readable representation.
        /// </summary>
        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.Append(RelationName);
            stringBuilder.Append(": ");
            stringBuilder.Append(Message);

            if (ParentKey.Count > 0)
            {
                stringBuilder.Append(" | ParentKey: ");
                AppendKey(stringBuilder, ParentKey);
            }

            if (ChildKey.Count > 0)
            {
                stringBuilder.Append(" | ChildKey: ");
                AppendKey(stringBuilder, ChildKey);
            }

            return stringBuilder.ToString();
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Appends key
        /// </summary>
        /// <param name="stringBuilder">String builder</param>
        /// <param name="key">Key to append</param>
        private static void AppendKey(StringBuilder stringBuilder, Dictionary<string, object?> key)
        {
            bool first = true;
            foreach (KeyValuePair<string, object?> keyValuePair in key)
            {
                if (!first)
                {
                    stringBuilder.Append(", ");
                }

                first = false;
                stringBuilder.Append(keyValuePair.Key);
                stringBuilder.Append("=");
                stringBuilder.Append(keyValuePair.Value == null ? "null" : keyValuePair.Value.ToString());
            }
        }
        #endregion
    }
}
