using PocoDataSet.Data;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using PocoDataSet.SqlServerDataAdapter;

namespace PocoDataSet.DemoWithNugetPackage
{
    internal static class Program
    {
        private static void Main()
        {
            // 1) Create an empty data set
            IDataSet dataSet = DataSetFactory.CreateDataSet();

            // 2.a) Create an empty data table using data set extension
            IDataTable departmentDataTable = dataSet.AddNewTable("Department");
            departmentDataTable.AddColumn("Id", DataTypeNames.INT);
            departmentDataTable.AddColumn("Name", DataTypeNames.STRING);

            // 2.b) Create an empty data table manually specifying column metadata.
            // You are on your own to define the columns correctly including primary keys, foreign keys, nullability, etc.
            List<IColumnMetadata> listOfColumnMetadata = new List<IColumnMetadata>();

            IColumnMetadata columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "Id";
            columnMetadata.DataType = DataTypeNames.INT;
            columnMetadata.IsNullable = false;
            columnMetadata.IsPrimaryKey = true;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "FirstName";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            columnMetadata = new ColumnMetadata();
            columnMetadata.ColumnName = "LastName";
            columnMetadata.DataType = DataTypeNames.STRING;
            listOfColumnMetadata.Add(columnMetadata);

            IDataTable employeeTable = dataSet.AddNewTable("Employee", listOfColumnMetadata);

            // 2.c) Change table schema
            employeeTable.AddColumn("DepartmentId", DataTypeNames.INT);

            // 2.d) Create an empty data table from POCO interface
            IDataTable employmentTypeDataTable = dataSet.AddNewTableFromPocoInterface("EmploymentType", typeof(IEmploymentType));

            // 3.a) Create a new row and add it to the table. Use it when field values need to be assigned before adding row to the table
            IDataRow departmentDataRow = DataRowExtensions.CreateRowFromColumns(departmentDataTable.Columns);
            departmentDataRow.UpdateDataFieldValue("Id", 1);
            departmentDataRow.UpdateDataFieldValue("Name", "Customer Service");
            departmentDataTable.Rows.Add(departmentDataRow);

            // 3.b) More convenient way to create a new row is to call AddNewRow method on data table
            departmentDataRow = departmentDataTable.AddNewRow();
            departmentDataRow.UpdateDataFieldValue("Id", 2);
            departmentDataRow.UpdateDataFieldValue("Name", "Financial");

            IDataRow employmentTypeDataRow = employmentTypeDataTable.AddNewRow();
            employmentTypeDataRow.UpdateDataFieldValue("Id", 1);
            employmentTypeDataRow.UpdateDataFieldValue("Code", "ET01");
            employmentTypeDataRow.UpdateDataFieldValue("Description", "Full Time");

            // 3.b) Create a new rows and add them to the table via data set. Use it when field values need to be assigned before adding row to the table
            IDataRow employeeDataRow = DataRowExtensions.CreateRowFromColumnsWithDefaultValues(employeeTable.Columns);
            employeeDataRow.UpdateDataFieldValue("Id", 1);
            employeeDataRow.UpdateDataFieldValue("FirstName", "John");
            employeeDataRow.UpdateDataFieldValue("LastName", "Doe");
            employeeDataRow.UpdateDataFieldValue("DepartmentId", 2);
            dataSet.AddRow("Employee", employeeDataRow);

            // 4.a) Read back a value using data set, expected "John"
            string? firstName = dataSet.GetFieldValue<string>("Employee", rowIndex: 0, columnName: "FirstName");

            // 4.b) Read back a value using data table, expected "Doe"
            string? lastName = employeeTable.GetFieldValue<string>(0, "LastName");

            // 4.c) Read back Full Time value using data row, expected "Full Time"
            string? employmentTypeDescription = employmentTypeDataRow.GetDataFieldValue<string>("Description");

            // 4.d) Read employment type code from data row using POCO interface, change it from "ET01" to "ET02"
            // and verify that changes are propagated to underlying data row
            IEmploymentType employmentType = employmentTypeDataRow.AsInterface<IEmploymentType>();
            string? employmentTypeCode = employmentType.Code;
            employmentType.Code = "ET02";
            string? updatedEmploymentTypeCode = employmentTypeDataRow["Code"] as string;

            // 4.e) Read employment type code from data row using POCO interface, change it from "ET02" to "ET03"
            // and verify that changes are NOT propagated to underlying data row
            EmploymentType employmentTypePoco = employmentTypeDataRow.ToPoco<EmploymentType>();
            employmentTypePoco.Code = "ET03";
            updatedEmploymentTypeCode = employmentTypeDataRow["Code"] as string;

            // 4.f) Update data row from POCO object
            // and verify that employment type code was updated to "ET03"
            employmentTypeDataRow.CopyFromPoco<EmploymentType>(employmentTypePoco);
            updatedEmploymentTypeCode = employmentTypeDataRow["Code"] as string;

            // 5) Serialize to JSON
            string? json = DataSetSerializer.ToJsonString(dataSet);
            Console.WriteLine("Serialized DataSet:\n");
            Console.WriteLine(json);

            // 6) Deserialize back
            IDataSet? restored = DataSetSerializer.FromJsonString(json);
            if (restored != null)
            {
                // Read back a value using restored data set, expected "John"
                string? restoredFirstName = restored.GetFieldValue<string>("Employee", 0, "FirstName");
                Console.WriteLine($"\nDataSet restored. Employee first name: {restoredFirstName}");
            }

            // Create copy of data set from JSON string directly
            IDataSet? copyOfDataSet = DataSetSerializer.FromJsonString(json);

            // 7. Remove row from table
            dataSet.RemoveRow("Employee", 0);

            // 8. Remove table from data set
            dataSet.RemoveTable("Employee");

            // 9. Clear table
            dataSet.ClearTable("Department");

            // 10) Change information of employment type code in copied data set
            copyOfDataSet!.Tables["EmploymentType"].Rows[0]["Id"] = 2;
            copyOfDataSet!.Tables["EmploymentType"].Rows[0]["Code"] = "ET02";
            copyOfDataSet!.Tables["EmploymentType"].Rows[0]["Description"] = "Part Time";

            // 11) Merge data sets
            // dataSet contains Department table without rows
            // dataSet contains EmploymentType table with 1 row: 1, "ET03", "Full Time"

            // copyOfDataSet contains Department table with 2 rows: 1, "Customer Service" and 2, "Financial"
            // copyOfDataSet contains Employee table with 1 row: 1, "John", "Doe", 2
            // copyOfDataSet contains EmploymentType table with 1 row: 1, "ET01", "Full Time"

            // After the merge dataSet will contain:
            // - Department table with 2 rows: 1, "Customer Service" and 2, "Financial"
            // - Employee table with 1 row: 1, "John", "Doe", 2
            // - EmploymentType table with 1 row: 2, "ET02", "Part Time"
            dataSet.MergeWith(copyOfDataSet!);

            // 12) SQL Server data adapter example of loading data from database into data set
            LoadDataFromDatabase().Wait();

            Console.ReadLine();
        }

        /// <summary>
        /// Loading data from database example using PocoDataSet.SqlServerDataAdapter.SqlDataAdapter
        /// Create PocoDataSetExamples database first and fill it with data using items located inside DatabaseItems folder
        /// </summary>
        static async Task LoadDataFromDatabase()
        {
            List<string> returnedTableNames = new List<string>();
            returnedTableNames.Add("Department");

            IDataSet requestDataSet = DataSetFactory.CreateDataSet();
            string connectionString = "Server=localhost;Database=PocoDataSetExamples;Trusted_Connection=True;Encrypt=Optional;MultipleActiveResultSets=True;Connection Timeout=300";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(connectionString);

            // a) Load Department (no parameters)
            IDataSet responseDataSet = await sqlDataAdapter.FillAsync("SELECT * FROM Department", false, null, returnedTableNames, null, requestDataSet);

            // b) Load employees for department #2 using a parameter dictionary
            returnedTableNames = new List<string>();
            returnedTableNames.Add("Employee");

            string employeeSql = "SELECT * FROM Employee WHERE DepartmentId = @DepartmentId";
            Dictionary<string, object?> parameters = new Dictionary<string, object?>(StringComparer.Ordinal);
            parameters.Add("@DepartmentId", 2);

            responseDataSet = await sqlDataAdapter.FillAsync(employeeSql, false, parameters, returnedTableNames, null, responseDataSet);
        }
    }
}
