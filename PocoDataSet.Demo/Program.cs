using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;

namespace PocoDataSet.Demo
{
    internal static class Program
    {
        private static void Main()
        {
            // 1) Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2) Define a table schema
            IDataTable employeeTable = new PocoDataSet.Data.DataTable();
            employeeTable.TableName = "Employee";

            // Add columns using metadata extensions
            employeeTable.AddColumn("Id", DataTypeNames.INT, isPrimaryKey: true, isForeignKey: false);
            employeeTable.AddColumn("FirstName", DataTypeNames.STRING, isPrimaryKey: false, isForeignKey: false);
            employeeTable.AddColumn("LastName", DataTypeNames.STRING, isPrimaryKey: false, isForeignKey: false);

            // Add the table to the data set
            dataSet.AddTable(employeeTable);

            // 3) Create a new row with default values
            IDataRow newRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(employeeTable.Columns);

            newRow["Id"] = 1;
            newRow["FirstName"] = "John";
            newRow["LastName"] = "Doe";

            // Add the row to the table via the data set
            dataSet.AddRow("Employee", newRow);

            // 4) Read back a value using a typed helper
            string? firstName = dataSet.GetFieldValue<string>("Employee", rowIndex: 0, columnName: "FirstName");
            Console.WriteLine($"First employee: {firstName}");

            // 5) Serialize to JSON
            string? json = DataSetSerializer.ToJsonString(dataSet);
            Console.WriteLine("Serialized DataSet:\n");
            Console.WriteLine(json);

            // 6) Deserialize back
            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            if (restored != null)
            {
                string? restoredFirstName = restored.GetFieldValue<string>("Employee", 0, "FirstName");
                Console.WriteLine($"\nDataSet restored. Employee first name: {restoredFirstName}");
            }

            Console.ReadLine();
        }
    }
}
