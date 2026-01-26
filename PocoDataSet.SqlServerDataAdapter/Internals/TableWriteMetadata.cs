using System;
using System.Collections.Generic;

namespace PocoDataSet.SqlServerDataAdapter
{
    internal class TableWriteMetadata
    {
        public string TableName
        {
            get; set;
        } = string.Empty;

        public HashSet<string> ColumnNames
        {
            get;
        } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public List<string> PrimaryKeyColumns
        {
            get;
        } = new List<string>();

        public HashSet<string> PrimaryKeys
        {
            get;
        } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> IdentityColumns
        {
            get;
        } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> ComputedColumns
        {
            get;
        } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public HashSet<string> RowVersionColumns
        {
            get;
        } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    }
}
