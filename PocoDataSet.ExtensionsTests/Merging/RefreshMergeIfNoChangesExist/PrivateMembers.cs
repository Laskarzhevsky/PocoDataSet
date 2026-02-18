using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.ExtensionsTests.Merging
{
    /// <summary>
    /// Additional coverage for RefreshIfNoChangesExist after the "no MergeMode / no policies" refactor.
    /// Focus: dirty-detection matrix + PK-null behavior lock-in.
    /// </summary>
    public partial class RefreshMergeIfNoChangesExist
    {
        private static IDataTable AddCompositePkTable(IDataSet ds)
        {
            IDataTable t = ds.AddNewTable("T");

            // Mark both columns as PK parts.
            // Kept nullable=true so we can construct null-part refreshed rows for contract tests.
            t.AddColumn("A", DataTypeNames.INT32, true, true);
            t.AddColumn("B", DataTypeNames.STRING, true, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            return t;
        }

        private static void AddLoadedRow(IDataTable t, object a, object? b, string name)
        {
            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["A"] = a;
            row["B"] = b;
            row["Name"] = name;
            t.AddLoadedRow(row);
        }

        private static IDataSet BuildCurrentWithPrimaryKey()
        {
            IDataSet current = DataSetFactory.CreateDataSet();
            IDataTable t = current.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 1;
            row["Name"] = "One";
            t.AddLoadedRow(row);

            return current;
        }

        private static IDataSet BuildRefreshedWithSameSchemaAndRow(object? id, string name)
        {
            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable t = refreshed.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32, false, true);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = id;
            row["Name"] = name;
            t.AddLoadedRow(row);

            return refreshed;
        }

        private static bool ContainsCompositePk(IDataTable t, int a, string b)
        {
            for (int i = 0; i < t.Rows.Count; i++)
            {
                IDataRow r = t.Rows[i];
                if ((int)r["A"]! == a && (string)r["B"]! == b)
                {
                    return true;
                }
            }

            return false;
        }

        private static IDataSet CreateCurrentCompositePk(int a, string b, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, a, b, name);

            return ds;
        }

        private static IDataSet CreateRefreshedCompositePk(object objectA, object? objectB, string name)
        {
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = AddCompositePkTable(ds);

            AddLoadedRow(t, objectA, objectB, name);

            return ds;
        }
    }
}
