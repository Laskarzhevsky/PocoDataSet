using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    public class InterfaceRowProxy<TInterface> : System.Reflection.DispatchProxy where TInterface : class
    {
        IDataRow _row;
        IDictionary<string, string>? _nameMap; // property name -> column name (case-sensitive unless you pass a CI dictionary)

        public InterfaceRowProxy()
        {
        }

        public void Initialize(IDataRow row, IDictionary<string, string>? nameMap)
        {
            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            _row = row;

            if (nameMap == null)
            {
                _nameMap = null;
            }
            else
            {
                // Make a defensive copy; keep caller’s comparer if any
                IDictionary<string, string> copy;

                if (nameMap is Dictionary<string, string> concrete)
                {
                    copy = new Dictionary<string, string>(concrete, concrete.Comparer);
                }
                else
                {
                    // Default to case-insensitive keys if the input isn’t a Dictionary
                    copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (KeyValuePair<string, string> kv in nameMap)
                    {
                        copy[kv.Key] = kv.Value;
                    }
                }

                _nameMap = copy;
            }
        }

        protected override object? Invoke(MethodInfo? targetMethod, object?[]? args)
        {
            if (targetMethod == null)
            {
                throw new ArgumentNullException(nameof(targetMethod));
            }
            if (_row == null)
            {
                throw new InvalidOperationException("Proxy not initialized with IDataRow.");
            }

            if (!targetMethod.IsSpecialName)
            {
                throw new NotSupportedException("Only property get/set is supported.");
            }

            string name = targetMethod.Name;

            if (name.StartsWith("get_", StringComparison.Ordinal))
            {
                string prop = name.Substring(4);

                string columnName = ResolveColumnName(prop);

                object? raw;
                bool found = _row.TryGetValue(columnName, out raw);
                if (!found)
                {
                    return RuntimeDefaults.GetDefault(targetMethod.ReturnType);
                }
                if (raw == null)
                {
                    return RuntimeDefaults.GetDefault(targetMethod.ReturnType);
                }

                object? converted;
                bool ok = CompositeValueConverterFactory.Default.TryConvert(raw, targetMethod.ReturnType, CultureInfo.InvariantCulture, out converted);
                if (!ok)
                {
                    return RuntimeDefaults.GetDefault(targetMethod.ReturnType);
                }
                if (converted == null)
                {
                    return RuntimeDefaults.GetDefault(targetMethod.ReturnType);
                }

                return converted;
            }

            if (name.StartsWith("set_", StringComparison.Ordinal))
            {
                string prop = name.Substring(4);
                object? val = null;

                if (args != null)
                {
                    if (args.Length > 0)
                    {
                        val = args[0];
                    }
                }

                string columnName = ResolveColumnName(prop);

                try
                {
                    _row[columnName] = val;
                }
                catch
                {
                    // ignore write failures
                }

                return null!;
            }

            throw new NotSupportedException("Unsupported accessor.");
        }

        string ResolveColumnName(string propertyName)
        {
            if (_nameMap == null)
            {
                return propertyName;
            }

            string mapped;
            bool ok = _nameMap.TryGetValue(propertyName, out mapped);
            if (ok)
            {
                if (mapped != null)
                {
                    return mapped;
                }
                else
                {
                    return propertyName;
                }
            }
            else
            {
                return propertyName;
            }
        }

        // ---------------------------
        // Factory methods (renamed)
        // ---------------------------

        public static TInterface CreateProxy(IDataRow dataRow)
        {
            if (dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow));
            }
            if (!typeof(TInterface).IsInterface)
            {
                throw new ArgumentException("TInterface must be an interface.", nameof(TInterface));
            }

            TInterface proxy = System.Reflection.DispatchProxy.Create<TInterface, InterfaceRowProxy<TInterface>>();

            InterfaceRowProxy<TInterface> core = (InterfaceRowProxy<TInterface>)(object)proxy;
            core.Initialize(dataRow, null);

            return proxy;
        }

        public static TInterface CreateProxy(IDataRow dataRow, IDictionary<string, string> nameMap)
        {
            if (dataRow == null)
            {
                throw new ArgumentNullException(nameof(dataRow));
            }
            if (!typeof(TInterface).IsInterface)
            {
                throw new ArgumentException("TInterface must be an interface.", nameof(TInterface));
            }

            TInterface proxy = System.Reflection.DispatchProxy.Create<TInterface, InterfaceRowProxy<TInterface>>();

            InterfaceRowProxy<TInterface> core = (InterfaceRowProxy<TInterface>)(object)proxy;
            core.Initialize(dataRow, nameMap);

            return proxy;
        }
    }
}
