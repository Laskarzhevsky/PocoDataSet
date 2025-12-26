namespace PocoDataSet.IData
{
    /// <summary>
    /// Defines data type names
    /// </summary>
    public static class DataTypeNames
    {
        public static string BINARY = "Byte[]";
        public static string BOOL = "Boolean";
        public static string BYTE = "Byte";
        public static string DATE = "DateOnly";   // if you support it
        public static string DATE_TIME = "DateTime";
        public static string DECIMAL = "Decimal";
        public static string DOUBLE = "Double";
        public static string GUID = "Guid";
        public static string INT16 = "Int16";
        public static string INT32 = "Int32";
        public static string INT64 = "Int64";
        public static string JSON = "Json";        // serialized object / string / provider-specific JSON type
        public static string OBJECT = "Object";
        public static string SINGLE = "Single";
        public static string TIME = "TimeOnly";   // if you support it
        public static string SPATIAL = "Spatial";    // geometry / geography object depending on adapter
        public static string STRING = "String";
    }
}
