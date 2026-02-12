using System;
using System.Collections.Generic;

using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableTortureTests
    {
        [Fact]
        public void Torture_RandomOperations_KeepViewConsistent_WithUnderlyingObservableTable()
        {
            // Arrange
            IDataSet dataSet = DataSetFactory.CreateDataSet();
            IDataTable table = dataSet.AddNewTable("Department");
            table.AddColumn("Id", DataTypeNames.INT32);
            table.AddColumn("Name", DataTypeNames.STRING);

            // Seed initial loaded rows (represents data coming from DB)
            for (int i = 1; i <= 5; i++)
            {
                DataRow loadedRow = new DataRow();
                loadedRow["Id"] = i;
                loadedRow["Name"] = "Dept" + i.ToString();
                ((DataTable)table).AddLoadedRow(loadedRow);
            }

            ObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            IObservableDataTable observableTable = observableDataSet.Tables["Department"];

            IObservableDataView? view = observableDataSet.GetObservableDataView("Department", null, false, null, "TortureView");
            Assert.NotNull(view);

            int nextId = 6;
            Random random = new Random(12345);

            // Act + Assert (invariants after each operation)
            for (int step = 0; step < 200; step++)
            {
                int op = random.Next(0, 3);

                if (op == 0)
                {
                    // Add new row
                    DataRow newRow = new DataRow();
                    newRow["Id"] = nextId;
                    newRow["Name"] = "Dept" + nextId.ToString();
                    nextId++;

                    observableTable.AddRow(newRow);
                }
                else if (op == 1)
                {
                    // Remove random row
                    if (observableTable.Rows.Count > 0)
                    {
                        int indexToRemove = random.Next(0, observableTable.Rows.Count);
                        observableTable.RemoveRowAt(indexToRemove);
                    }
                }
                else
                {
                    // Update random row
                    if (view!.Rows.Count > 0)
                    {
                        int indexToUpdate = random.Next(0, view.Rows.Count);
                        IObservableDataRow row = view.Rows[indexToUpdate];

                        int id = (int)row["Id"];
                        string newName = "Dept" + id.ToString() + "_U" + step.ToString();

                        row["Name"] = newName;
                    }
                }

                AssertViewMatchesTable(view!, observableTable);
            }
        }

        static void AssertViewMatchesTable(IObservableDataView view, IObservableDataTable table)
        {
            Assert.Equal(table.Rows.Count, view.Rows.Count);

            HashSet<int> tableIds = new HashSet<int>();
            for (int i = 0; i < table.Rows.Count; i++)
            {
                IObservableDataRow row = table.Rows[i];
                tableIds.Add((int)row["Id"]);
            }

            HashSet<int> viewIds = new HashSet<int>();
            for (int i = 0; i < view.Rows.Count; i++)
            {
                IObservableDataRow row = view.Rows[i];
                viewIds.Add((int)row["Id"]);
            }

            Assert.Equal(tableIds, viewIds);
        }
    }
}
