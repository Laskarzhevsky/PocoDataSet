using System;
using System.Collections.Generic;
using System.Reflection;

using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// Contains data table extension methods
    /// </summary>
    public static partial class DataTableExtensions
    {
        #region Public Methods
        /// <summary>
        /// Adds columns from interface with inheritance chain
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="interfaceType">Interface type</param>
        /// <exception cref="ArgumentException">Exception is thrown if specified type is not an interface</exception>
        public static void AddColumnsFromInterface(this IDataTable? dataTable, Type interfaceType)
        {
            if (dataTable == null)
            {
                return;
            }

            if (!interfaceType.IsInterface)
            {
                throw new ArgumentException("Type must be an interface.", nameof(interfaceType));
            }

            // Collect interface + its inherited interfaces
            List<Type> listOfInterfaces = new List<Type>();
            listOfInterfaces.Add(interfaceType);
            AddInheritedInterfacesToList(interfaceType, listOfInterfaces);

            // To avoid duplicate-property additions
            HashSet<string> alreadyAddedInterfaces = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < listOfInterfaces.Count; i++)
            {
                interfaceType = listOfInterfaces[i];
                AddColumnFromInterface(dataTable, interfaceType, alreadyAddedInterfaces);
            }
        }
        #endregion

        #region Private Methods
        /// <summary>
        /// Adds column from interface
        /// </summary>
        /// <param name="dataTable">Data table</param>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="alreadyAddedInterfaces">Already added interfaces</param>
        static void AddColumnFromInterface(IDataTable dataTable, Type interfaceType, HashSet<string> alreadyAddedInterfaces)
        {
            PropertyInfo[] interfacePropertyInfos = interfaceType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            if (interfacePropertyInfos == null)
            {
                return;
            }

            for (int i = 0; i < interfacePropertyInfos.Length; i++)
            {
                PropertyInfo propertyInfo = interfacePropertyInfos[i];
                if (propertyInfo == null)
                {
                    continue;
                }

                string propertyName = propertyInfo.Name;
                if (alreadyAddedInterfaces.Contains(propertyName))
                {
                    continue;
                }

                string columnName = propertyName;
                string clrTypeName = PropertyInfoToClrNameConverter.GetClrTypeName(propertyInfo);
                bool isNullable = PropertyNullabilityChecker.IsNullableProperty(propertyInfo);

                dataTable.AddColumn(columnName, clrTypeName, isNullable);
                alreadyAddedInterfaces.Add(propertyName);
            }
        }

        /// <summary>
        /// Adds inherited interfaces to list
        /// </summary>
        /// <param name="interfaceType">Interface type</param>
        /// <param name="listOfInterfaces">List of interfaces</param>
        static void AddInheritedInterfacesToList(Type interfaceType, List<Type> listOfInterfaces)
        {
            Type[] inherited = interfaceType.GetInterfaces();
            if (inherited != null)
            {
                for (int i = 0; i < inherited.Length; i++)
                {
                    if (inherited[i] != null)
                    {
                        listOfInterfaces.Add(inherited[i]);
                        AddInheritedInterfacesToList(inherited[i], listOfInterfaces);
                    }
                }
            }
        }
        #endregion
    }
}
