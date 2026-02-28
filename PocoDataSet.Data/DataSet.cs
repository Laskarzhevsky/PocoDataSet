using System;
using System.Collections.Generic;

using PocoDataSet.IData;
using System.Text.Json.Serialization;

namespace PocoDataSet.Data
{
    /// <summary>
    /// Provides data set functionality
    /// </summary>
    public class DataSet : IDataSet, IJsonOnDeserialized
    {
        #region Data Fields
        readonly List<IDataRelation> _relations = new List<IDataRelation>();

        readonly Dictionary<string, IDataTable> _tables = new();
        #endregion

        #region Public Properties
        /// <summary>
        /// Gets or sets name
        /// IDataSet interface implementation
        /// </summary>
        public string? Name
        {
            get; set;
        }

        /// <summary>
        /// Gets relations
        /// IDataSet interface implementation
        /// </summary>
        public IReadOnlyList<IDataRelation> Relations
        {
            get
            {
                return _relations;
            }
        }

        /// <summary>
        /// Gets tables
        /// IDataSet interface implementation
        /// </summary>
        [JsonIgnore]
        public IReadOnlyDictionary<string, IDataTable> Tables
        {
            get
            {
                return _tables;
            }
        }

        /// <summary>
        /// Serialization helper for tables.
        /// System.Text.Json cannot populate IReadOnlyDictionary<string, IDataTable> directly,
        /// and it cannot instantiate interface types. This property is used for JSON only.
        /// </summary>
        [JsonInclude]
        [JsonPropertyName("Tables")]
        public Dictionary<string, DataTable> TablesJson
        {
            get; private set;
        } = new Dictionary<string, DataTable>();

        #endregion

        #region Indexers
        /// <summary>
        /// Gets the data table associated with the specified table name
        /// IDataSet interface implementation
        /// </summary>
        /// <returns>An instance of IDataTable representing the specified table</returns>
        public IDataTable this[string tableName]
        {
            get
            {
                if (_tables.TryGetValue(tableName, out IDataTable? dataTable))
                {
                    return dataTable;
                }
                else
                {
                    throw new KeyNotFoundException($"DataSet does not contain table with name {tableName}");
                }
            }
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// Adds relation between parent and child tables in a data set
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnName">Parent column name</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnName">Child column name</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        public IDataRelation AddRelation(string relationName, string parentTableName, string parentColumnName, string childTableName, string childColumnName)
        {
            List<string> parentColumnNames = new List<string>();
            parentColumnNames.Add(parentColumnName);

            List<string> childColumnNames = new List<string>();
            childColumnNames.Add(childColumnName);

            return AddRelation(relationName, parentTableName, parentColumnNames, childTableName, childColumnNames);
        }

        /// <summary>
        /// Adds relation between parent and child tables in a data set
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <param name="parentTableName">Parent table name</param>
        /// <param name="parentColumnNames">Parent column names</param>
        /// <param name="childTableName">Child table name</param>
        /// <param name="childColumnNames">Child column names</param>
        /// <returns>Added relation between parent and child tables in a data set</returns>
        public IDataRelation AddRelation(string relationName, string parentTableName, IList<string> parentColumnNames, string childTableName, IList<string> childColumnNames)
        {
            if (parentColumnNames.Count == 0)
            {
                throw new ArgumentException("Parent column names must be provided.", nameof(parentColumnNames));
            }

            if (childColumnNames.Count == 0)
            {
                throw new ArgumentException("Child column names must be provided.", nameof(childColumnNames));
            }

            if (parentColumnNames.Count != childColumnNames.Count)
            {
                throw new InvalidOperationException("Parent and child column name counts must match.");
            }

            // Duplicate name throws
            for (int i = 0; i < _relations.Count; i++)
            {
                IDataRelation existing = _relations[i];
                if (existing == null)
                {
                    continue;
                }

                if (string.Equals(existing.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException("Relation with the same name already exists: " + relationName);
                }
            }

            DataRelation relation = new DataRelation();
            relation.RelationName = relationName;
            relation.ParentTableName = parentTableName;
            relation.ChildTableName = childTableName;
            relation.ParentColumnNames = new List<string>(parentColumnNames);
            relation.ChildColumnNames = new List<string>(childColumnNames);

            _relations.Add(relation);
            return relation;
        }


        /// <summary>
        /// Adds table to data set
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="dataTable">Data table for addition</param>
        public void AddTable(IDataTable dataTable)
        {
            if (string.IsNullOrEmpty(dataTable.TableName))
            {
                throw new System.ArgumentException("Table name cannot be null");
            }

            if (_tables.ContainsKey(dataTable.TableName))
            {
                throw new KeyDuplicationException($"DataSet contains table with name {dataTable.TableName} already");
            }

            _tables.Add(dataTable.TableName, dataTable);

            DataTable? concrete = dataTable as DataTable;
            if (concrete != null)
            {
                TablesJson[dataTable.TableName] = concrete;
            }
        }

        /// <summary>
        /// Removes relation by name
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="relationName">Relation name</param>
        /// <returns>Flag indicating whether relation was removed</returns>
        public bool RemoveRelation(string relationName)
        {
            for (int i = 0; i < _relations.Count; i++)
            {
                IDataRelation relation = _relations[i];
                if (relation == null)
                {
                    continue;
                }

                if (string.Equals(relation.RelationName, relationName, StringComparison.OrdinalIgnoreCase))
                {
                    _relations.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Removes table from data set by name
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="tableName">Table name</param>
        public void RemoveTable(string tableName)
        {
            if (_tables.ContainsKey(tableName))
            {
                _tables.Remove(tableName);

                TablesJson.Remove(tableName);
            }
            else
            {
                throw new System.Collections.Generic.KeyNotFoundException($"DataSet does not contain table with name {tableName}.");
            }
        }

        /// <summary>
        /// Attempts to retrieve the data table associated with the specified table name
        /// IDataSet interface implementation
        /// </summary>
        /// <param name="tableName">The name of the table to retrieve</param>
        /// <param name="dataTable">When this method returns true, contains the data table associated with the specified table name, otherwise null</param>
        /// <returns>True if a table with the specified name was found, otherwise false</returns>
        public bool TryGetTable(string tableName, out IDataTable? dataTable)
        {
            if (_tables.TryGetValue(tableName, out IDataTable? foundDataTable))
            {
                dataTable = foundDataTable;
                return true;
            }
            else
            {
                dataTable= null;
                return false;
            }
        }
        #endregion

        #region JSON Serializer
        /// <summary>
        /// Called by System.Text.Json after deserialization (.NET 8+ / .NET 9)
        /// </summary>
        public void OnDeserialized()
        {
            _tables.Clear();

            if (TablesJson == null)
            {
                return;
            }

            foreach (KeyValuePair<string, DataTable> kvp in TablesJson)
            {
                if (kvp.Key == null)
                {
                    continue;
                }

                if (kvp.Value == null)
                {
                    continue;
                }

                // Ensure table name consistency if your DataTable has a TableName property
                // kvp.Value.TableName = kvp.Key; // only if needed/allowed

                _tables.Add(kvp.Key, kvp.Value);
            }
        }
        #endregion
    }
}