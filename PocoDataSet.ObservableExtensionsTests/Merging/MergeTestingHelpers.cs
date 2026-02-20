using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using System;

namespace PocoDataSet.ObservableExtensionsTests.Merging
{
    public static class MergeTestingHelpers
    {

        public static void AddAcceptedIdNameRow(IObservableDataTable t, int id, string name)
        {
            IObservableDataRow r = t.AddNewRow();
            r["Id"] = id;
            r["Name"] = name;
            r.AcceptChanges();
        }

        public static void AddAcceptedRow(IObservableDataTable t, int a, string b, string name)
        {
            IObservableDataRow r = t.AddNewRow();
            r["A"] = a;
            r["B"] = b;
            r["Name"] = name;
            r.AcceptChanges();
        }

        public static IDataTable AddCompositePkTable(IDataSet ds)
        {
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);
            return t;
        }

        public static void AddLoadedRow(IDataTable t, object a, object? b, string name)
        {
            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);
        }

        public static void AddLoadedIdNameRow(IDataTable t, int id, string name)
        {
            IDataRow r = DataRowExtensions.CreateRowFromColumns(t.Columns);
            r["Id"] = id;
            r["Name"] = name;
            t.AddLoadedRow(r);
        }

        public static bool ContainsCompositePk(IObservableDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["A"]! == a && (string)t.Rows[i]["B"]! == b)
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsRow(IObservableDataSetMergeResult result, string tableName, IObservableDataRow row)
        {
            foreach (IObservableDataSetMergeResultEntry entry in result.UpdatedObservableDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.ObservableDataRow, row))
                {
                    return true;
                }
            }

            foreach (IObservableDataSetMergeResultEntry entry in result.AddedObservableDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.ObservableDataRow, row))
                {
                    return true;
                }
            }

            foreach (IObservableDataSetMergeResultEntry entry in result.DeletedObservableDataRows)
            {
                if (entry.TableName == tableName && object.ReferenceEquals(entry.ObservableDataRow, row))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool ContainsRowWithId(IObservableDataTable table, int id)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                object? value = table.Rows[i]["Id"];
                if (value is int v && v == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static int CountUserColumns(IObservableDataTable table)
        {
            int count = 0;

            for (int i = 0; i < table.Columns.Count; i++)
            {
                string name = table.Columns[i].ColumnName;

                if (name == SpecialColumnNames.CLIENT_KEY)
                {
                    continue;
                }

                count++;
            }

            return count;
        }

        public static IObservableDataSet CreateCurrentObservableCompositePk(int a, string b, string name)
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(inner);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return new ObservableDataSet(inner);
        }

        public static ObservableDataSet CreateCurrentObservableDepartmentDataSet()
        {
            IDataSet currentInner = DataSetFactory.CreateDataSet();
            ObservableDataSet current = new ObservableDataSet(currentInner);

            IObservableDataTable currentTable = current.AddNewTable("Department");
            currentTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            currentTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentTable.AddColumn("Name", DataTypeNames.STRING);

            // Deterministic client keys so tests are stable.
            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            Guid clientKey2 = Guid.Parse("22222222-2222-2222-2222-222222222222");

            IObservableDataRow r1 = currentTable.AddNewRow();
            r1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1.AcceptChanges();

            IObservableDataRow r2 = currentTable.AddNewRow();
            r2[SpecialColumnNames.CLIENT_KEY] = clientKey2;
            r2["Id"] = 2;
            r2["Name"] = "HR";
            r2.AcceptChanges();

            currentTable.AcceptChanges();

            return current;
        }

        public static IDataSet CreateDepartmentRefreshedSnapshot(string id1Name)
        {
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("Department");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow r1 = new DataRow();
            r1["Id"] = 1;
            r1["Name"] = id1Name;
            t.AddRow(r1);

            t.AcceptChanges();
            return refreshed;
        }

        public static IDataSet CreateRefreshedCompositePk(object objectA, object? objectB, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);
            AddLoadedRow(t, objectA, objectB, name);
            return ds;
        }

        public static IObservableDataSet CreateCurrentObservableWithCompositePkRow(object a, object? b)
        {
            IDataSet inner = DataSetFactory.CreateDataSet();
            IDataTable t = inner.AddNewTable("T");

            // Make PK parts nullable so we can construct the invalid cases.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = "Current";
            t.AddLoadedRow(row);

            return new ObservableDataSet(inner);
        }

        public static IDataSet CreateRefreshedCompositePkSnapshot(int a, string b, string name)
        {
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable t = dataSet.AddNewTable("T");

            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            DataRow row = new DataRow();
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return dataSet;
        }

        public static IDataSet CreateRefreshedDepartmentDataSet()
        {
            // Refreshed:
            // - Id=1 updated Name
            // - Id=2 missing (deleted)
            // - Id=3 added
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedTable = refreshed.AddNewTable("Department");
            refreshedTable.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
            refreshedTable.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            refreshedTable.AddColumn("Name", DataTypeNames.STRING);

            Guid clientKey1 = Guid.Parse("11111111-1111-1111-1111-111111111111");

            DataRow refreshedRow1 = new DataRow();
            refreshedRow1[SpecialColumnNames.CLIENT_KEY] = clientKey1;
            refreshedRow1["Id"] = 1;
            refreshedRow1["Name"] = "SalesUpdated";
            refreshedTable.AddRow(refreshedRow1);

            DataRow refreshedRow3 = new DataRow();
            refreshedRow3[SpecialColumnNames.CLIENT_KEY] = Guid.Parse("33333333-3333-3333-3333-333333333333");
            refreshedRow3["Id"] = 3;
            refreshedRow3["Name"] = "IT";
            refreshedTable.AddRow(refreshedRow3);

            refreshedTable.AcceptChanges();

            return refreshed;
        }

        public static IObservableDataRow FindById(IObservableDataTable table, int id)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IObservableDataRow row = table.Rows[i];
                if ((int)row["Id"]! == id)
                {
                    return row;
                }
            }

            throw new InvalidOperationException("Row with Id=" + id.ToString() + " was not found.");
        }

        public static IObservableDataRow? FindRowByClientKey(IObservableDataTable table, Guid clientKey)
        {
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IObservableDataRow row = table.Rows[i];

                object? value;
                row.InnerDataRow.TryGetValue(SpecialColumnNames.CLIENT_KEY, out value);

                if (value is Guid g && g == clientKey)
                {
                    return row;
                }
            }

            return null;
        }

        public static bool HasColumn(IObservableDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, columnName, StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool RowExistsById(IObservableDataTable t, int id)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                if ((int)t.Rows[i]["Id"]! == id)
                {
                    return true;
                }
            }

            return false;
        }

        public static IObservableDataRow GetObservableRowById(IObservableDataTable table, int id)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                object? value = table.Rows[i]["Id"];
                if (value is int v && v == id)
                {
                    return table.Rows[i];
                }
            }

            throw new InvalidOperationException("Row with Id '" + id + "' was not found in table '" + table.TableName + "'.");
        }

        public static bool ContainsRowInstance(IObservableDataTable table, IObservableDataRow row)
        {
            if (table == null)
            {
                throw new ArgumentNullException(nameof(table));
            }

            if (row == null)
            {
                throw new ArgumentNullException(nameof(row));
            }

            for (int i = 0; i < table.Rows.Count; i++)
            {
                if (object.ReferenceEquals(table.Rows[i], row))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

