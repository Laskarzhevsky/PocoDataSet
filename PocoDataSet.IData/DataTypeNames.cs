namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data type names
    /// </summary>
    public static class DataTypeNames
    {
        #region Text and character types
        public static string CHAR = "char";
        public static string NCHAR = "nchar";
        public static string NTEXT = "ntext";
        public static string NVARCHAR = "nvarchar";
        public static string TEXT = "text";
        public static string VARCHAR = "varchar";
        public static string XML = "xml";
        #endregion

        #region Binary types
        public static string BINARY = "binary";
        public static string IMAGE = "image";
        public static string ROW_VERSION = "rowversion";
        public static string TIME_STAMP = "timestamp";
        public static string VARBINARY = "varbinary";
        #endregion

        #region Date and time types
        public static string DATE = "date";
        public static string DATE_TIME = "datetime";
        public static string DATE_TIME_2 = "datetime2";
        public static string DATE_TIME_OFFSET = "datetimeoffset";
        public static string SMALL_DATE_TIME = "smalldatetime";
        public static string TIME = "time";
        #endregion

        #region Numeric types
        public static string BIGINT = "bigint";
        public static string BIT = "bit";
        public static string DECIMAL = "decimal";
        public static string FLOAT = "float";
        public static string INT = "int";
        public static string MONEY = "money";
        public static string NUMERIC = "numeric";
        public static string REAL = "real";
        public static string SMALL_INT = "smallint";
        public static string SMALL_MONEY = "smallmoney";
        public static string TINY_INT = "tinyint";
        #endregion

        #region SQL CLR and special types
        public static string GEOGRAPHY = "geography";
        public static string GEOMETRY = "geometry";
        public static string HIERARCHY_ID = "hierarchyid";
        public static string SQL_VARIANT = "sql_variant";
        public static string UNIQUE_IDENTIFIER = "uniqueidentifier";
        #endregion

        #region Optional CLR aliases
        public static string BOOL = "bool";
        public static string BYTE = "byte";
        public static string DOUBLE = "double";
        public static string LONG = "long";
        public static string OBJECT = "object";
        public static string SBYTE = "sbyte";
        public static string SHORT = "short";
        public static string STRING = "string";
        public static string UINT = "uint";
        public static string ULONG = "ulong";
        public static string USHORT = "ushort";
        #endregion
    }
}
