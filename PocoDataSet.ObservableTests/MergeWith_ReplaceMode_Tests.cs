using System;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;

using Xunit;

namespace PocoDataSet.ObservableTests
{
    public class ObservableMergeReplaceModeTests
    {
        [Fact]
        public void MergeWith_ReplaceMode_DiscardsLocalChanges_AndMatchesRefreshedState()
        {
            // Arrange
            IObservableDataSet current = new ObservableDataSet();

            IObservableDataTable currentDept = current.AddNewTable("Department");
            currentDept.AddColumn("Id", DataTypeNames.INT32, false, true); // PK
            currentDept.AddColumn("Name", DataTypeNames.STRING);

            IObservableDataRow r1 = currentDept.AddNewRow();
            r1["Id"] = 1;
            r1["Name"] = "Sales";
            r1.AcceptChanges();

            r1["Name"] = "Sales - Local Edit";

            IObservableDataRow rAdded = currentDept.AddNewRow();
            rAdded["Name"] = "Temp Row";

            IDataSet refreshed = DataSetFactory.CreateDataSet();
            IDataTable refreshedDept = refreshed.AddNewTable("Department");
            refreshedDept.AddColumn("Id", DataTypeNames.INT32, false, true);
            refreshedDept.AddColumn("Name", DataTypeNames.STRING);

            IDataRow s1 = refreshedDept.AddNewRow();
            s1["Id"] = 1;
            s1["Name"] = "Sales - Server";

            IDataRow s2 = refreshedDept.AddNewRow();
            s2["Id"] = 2;
            s2["Name"] = "Engineering";

            IObservableMergeOptions options = new ObservableMergeOptions();
            // Act
            current.DoReplaceMerge(refreshed, options);

            // Assert
            Assert.Equal(2, current.Tables["Department"].Rows.Count);

            IObservableDataRow found1 = FindById(current.Tables["Department"], 1);
            Assert.Equal("Sales - Server", (string)found1["Name"]!);
            Assert.Equal(DataRowState.Unchanged, found1.InnerDataRow.DataRowState);

            IObservableDataRow found2 = FindById(current.Tables["Department"], 2);
            Assert.Equal("Engineering", (string)found2["Name"]!);
            Assert.Equal(DataRowState.Unchanged, found2.InnerDataRow.DataRowState);
        }

        static IObservableDataRow FindById(IObservableDataTable table, int id)
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
    }
}
