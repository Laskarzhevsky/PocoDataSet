using System.Collections.Generic;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Provides data type name to CLR name converter functionality
    /// </summary>
    public static class DataTypeNameToClrNameConverter
    {
        #region Constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        static DataTypeNameToClrNameConverter() {
            InitializeDataTypeNameToClrNameDictionary();
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Gets CLR name
        /// </summary>
        /// <param name="dataTypeName">Data type name</param>
        /// <returns>CLR name</returns>
        public static string? GetClrName(string dataTypeName)
        {
            string? clrName = null;
            if (string.IsNullOrWhiteSpace(dataTypeName))
            {
                return clrName;
            }

            if (dataTypeName.Contains('.'))
            {
                string[] parts = dataTypeName.Split('.');
                clrName = parts[parts.Length - 1];
            }
            else
            {
                clrName = dataTypeName;
            }

            if (DataTypeNameToClrNameDictionary.TryGetValue(clrName, out var mapped))
            {
                clrName = mapped;
            }

            return clrName;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Initializes data type name to CLR name dictionary
        /// </summary>
        static void InitializeDataTypeNameToClrNameDictionary()
        {
            // Text and character types
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.CHAR, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.NCHAR, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.NTEXT, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.NVARCHAR, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.TEXT, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.VARCHAR, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.XML, ClrTypeNames.STRING);

            // Binary types
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.BINARY, ClrTypeNames.BYTE_ARRAY);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.IMAGE, ClrTypeNames.BYTE_ARRAY);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.ROW_VERSION, ClrTypeNames.BYTE_ARRAY);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.TIME_STAMP, ClrTypeNames.BYTE_ARRAY);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.VARBINARY, ClrTypeNames.BYTE_ARRAY);

            // Date and time types
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DATE, ClrTypeNames.DATE_TIME);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DATE_TIME, ClrTypeNames.DATE_TIME);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DATE_TIME_2, ClrTypeNames.DATE_TIME);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DATE_TIME_OFFSET, ClrTypeNames.DATE_TIME_OFFSET);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SMALL_DATE_TIME, ClrTypeNames.DATE_TIME);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.TIME, ClrTypeNames.TIME_SPAN);

            // Numeric types
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.BIGINT, ClrTypeNames.INT64);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.BIT, ClrTypeNames.BOOLEAN);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DECIMAL, ClrTypeNames.DECIMAL);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.FLOAT, ClrTypeNames.DOUBLE);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.INT, ClrTypeNames.INT32);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.MONEY, ClrTypeNames.DECIMAL);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.NUMERIC, ClrTypeNames.DECIMAL);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.REAL, ClrTypeNames.SINGLE);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SMALL_INT, ClrTypeNames.INT16);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SMALL_MONEY, ClrTypeNames.DECIMAL);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.TINY_INT, ClrTypeNames.BYTE);

            // SQL CLR and special types
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.GEOGRAPHY, ClrTypeNames.OBJECT);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.GEOMETRY, ClrTypeNames.OBJECT);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.HIERARCHY_ID, ClrTypeNames.OBJECT);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SQL_VARIANT, ClrTypeNames.OBJECT);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.UNIQUE_IDENTIFIER, ClrTypeNames.GUID);

            // Optional CLR aliases
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.BOOL, ClrTypeNames.BOOLEAN);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.BYTE, ClrTypeNames.BYTE);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.DOUBLE, ClrTypeNames.DOUBLE);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.LONG, ClrTypeNames.INT64);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.OBJECT, ClrTypeNames.OBJECT);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SBYTE, ClrTypeNames.SBYTE);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.SHORT, ClrTypeNames.INT16);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.STRING, ClrTypeNames.STRING);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.UINT, ClrTypeNames.UINT32);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.ULONG, ClrTypeNames.UINT64);
            DataTypeNameToClrNameDictionary.Add(DataTypeNames.USHORT, ClrTypeNames.UINT16);
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets data type name to CLR name dictionary
        /// </summary>
        static Dictionary<string, string> DataTypeNameToClrNameDictionary
        {
            get; set;
        } = new Dictionary<string, string>();
        #endregion
    }
}
