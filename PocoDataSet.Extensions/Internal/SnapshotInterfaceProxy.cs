// File: PocoDataSet.Extensions/Internal/SnapshotInterfaceProxy.cs
using System;
using System.Collections.Generic;
using System.Reflection;

using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Extensions
{
    /// <summary>
    /// DispatchProxy that implements a specific interface by returning values
    /// captured from an IDataRow at initialization time (detached snapshot).
    /// </summary>
    internal class SnapshotInterfaceProxy : DispatchProxy
    {
        IDictionary<string, object?> _values = default!;
        Type _interfaceType = default!;

        public void Initialize(Type interfaceType, IDataRow row)
        {
            Initialize(interfaceType, row, null);
        }

        public void Initialize(Type interfaceType, IDataRow row, IDictionary<string, string>? nameMap)
        {
            if (interfaceType == null)
                throw new ArgumentNullException(nameof(interfaceType));
            if (row == null)
                throw new ArgumentNullException(nameof(row));
            if (!interfaceType.IsInterface)
                throw new ArgumentException("interfaceType must be an interface.", nameof(interfaceType));

            _interfaceType = interfaceType;

            if (interfaceType.ContainsGenericParameters)
            {
                throw new NotSupportedException("Cannot proxy an open generic interface: " + interfaceType.FullName);
            }

            PropertyInfo[] props = interfaceType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            for (int i = 0; i < props.Length; i++)
            {
                var pt = props[i].PropertyType;
                if (pt.ContainsGenericParameters)
                {
                    throw new NotSupportedException(
                        "Property type is open generic: " + interfaceType.FullName + "." + props[i].Name + " : " + pt.FullName);
                }
            }

            _values = CaptureValues(interfaceType, row, nameMap);
        }

        protected override object? Invoke(MethodInfo targetMethod, object?[]? args)
        {
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));

            if (targetMethod.IsSpecialName && targetMethod.Name.StartsWith("get_", StringComparison.Ordinal))
            {
                string propName = targetMethod.Name.Substring(4);
                object? value;
                if (_values.TryGetValue(propName, out value))
                {
                    return value;
                }
                return null;
            }

            if (targetMethod.Name == nameof(object.ToString))
                return _interfaceType.Name + " (detached snapshot)";
            if (targetMethod.Name == nameof(object.GetHashCode))
                return _values.GetHashCode();
            if (targetMethod.Name == nameof(object.Equals))
                return ReferenceEquals(this, args != null && args.Length > 0 ? args[0] : null);

            throw new NotSupportedException("Only property getters are supported on detached snapshot proxies.");
        }

        static IDictionary<string, object?> CaptureValues(
            Type iface, IDataRow row, IDictionary<string, string>? nameMap)
        {
            var dict = new Dictionary<string, object?>(StringComparer.Ordinal);

            PropertyInfo[] props = GetAllInterfaceProperties(iface);

            for (int i = 0; i < props.Length; i++)
            {
                PropertyInfo p = props[i];
                if (!p.CanRead)
                    continue;

                string propName = p.Name;
                string columnName = propName;

                if (nameMap != null && nameMap.TryGetValue(propName, out var mapped))
                {
                    columnName = mapped;
                }

                object? val;
                if (!ValueReader.TryRead(row, columnName, p.PropertyType, out val))
                {
                    val = Defaults.ForType(p.PropertyType); // your default/null helper
                }

                dict[propName] = val;
            }

            return dict;
        }

        // Collect properties from iface + all inherited interfaces (no duplicates by name)
        static PropertyInfo[] GetAllInterfaceProperties(Type iface)
        {
            var list = new List<PropertyInfo>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var q = new Queue<Type>();
            q.Enqueue(iface);

            while (q.Count > 0)
            {
                var t = q.Dequeue();

                // direct properties on this interface
                var props = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                for (int i = 0; i < props.Length; i++)
                {
                    var p = props[i];
                    if (seen.Add(p.Name))
                    {
                        list.Add(p);
                    }
                }

                // inherited interfaces
                var parents = t.GetInterfaces();
                for (int i = 0; i < parents.Length; i++)
                {
                    q.Enqueue(parents[i]);
                }
            }

            return list.ToArray();
        }
    }
}
