using PocoDataSet.Data;
using PocoDataSet.Demo.DataSetExtensions;
using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.IObservableData;
using PocoDataSet.ObservableData;
using PocoDataSet.ObservableExtensions;
using PocoDataSet.Serializer;
using PocoDataSet.SqlServerDataAdapter;

namespace PocoDataSet.Demo
{
    internal static class Program
    {
        private static async Task Main()
        {
/*
            // 1) Create an empty data set
            IDataSet dataSet = DataSetFactoryExamples.CreateDataSet();

            // 2.a) Create an empty data table using data set extension
            IDataTable departmentDataTable = DataSetExtensionExamples.AddNewTable(dataSet, "Department");
            DataTableExtensionExamples.AddColumn(departmentDataTable, "Id", DataTypeNames.INT);
            DataTableExtensionExamples.AddColumn(departmentDataTable, "Name", DataTypeNames.STRING);

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

            IDataTable employeeTable = DataSetExtensionExamples.AddNewTable(dataSet, "Employee", listOfColumnMetadata);

            // 2.c) Change table schema
            DataTableExtensionExamples.AddColumn(employeeTable, "DepartmentId", DataTypeNames.INT);

            // 2.d) Create an empty data table from POCO interface
            IDataTable employmentTypeDataTable = DataSetExtensionExamples.AddNewTableFromPocoInterface(dataSet, "EmploymentType", typeof(IEmploymentType));

            // 3.a) Create a new row and add it to the table. Use it when field values need to be assigned before adding row to the table
            IDataRow departmentDataRow = DataRowExtensionExamples.CreateRowFromColumns(departmentDataTable.Columns);
            departmentDataRow.UpdateDataFieldValue("Id", 1);
            departmentDataRow.UpdateDataFieldValue("Name", "Customer Service");
            departmentDataTable.Rows.Add(departmentDataRow);

            // 3.b) More convenient way to create a new row is to call AddNewRow method on data table
            departmentDataRow = DataTableExtensionExamples.AddNewRow(departmentDataTable);
            departmentDataRow.UpdateDataFieldValue("Id", 2);
            departmentDataRow.UpdateDataFieldValue("Name", "Financial");

            IDataRow employmentTypeDataRow = DataTableExtensionExamples.AddNewRow(employmentTypeDataTable);
            employmentTypeDataRow.UpdateDataFieldValue("Id", 1);
            employmentTypeDataRow.UpdateDataFieldValue("Code", "ET01");
            employmentTypeDataRow.UpdateDataFieldValue("Description", "Full Time");

            // 3.b) Create a new rows and add them to the table via data set. Use it when field values need to be assigned before adding row to the table
            IDataRow employeeDataRow = DataRowExtensionExamples.CreateRowFromColumnsWithDefaultValues(employeeTable.Columns);
            employeeDataRow.UpdateDataFieldValue("Id", 1);
            employeeDataRow.UpdateDataFieldValue("FirstName", "John");
            employeeDataRow.UpdateDataFieldValue("LastName", "Doe");
            employeeDataRow.UpdateDataFieldValue("DepartmentId", 2);
            DataSetExtensionExamples.AddRow(dataSet, "Employee", employeeDataRow);

            employeeDataRow = DataRowExtensionExamples.CreateRowFromColumnsWithDefaultValues(employeeTable.Columns);
            employeeDataRow.UpdateDataFieldValue("Id", 2);
            employeeDataRow.UpdateDataFieldValue("FirstName", "Sara");
            employeeDataRow.UpdateDataFieldValue("LastName", "Gor");
            employeeDataRow.UpdateDataFieldValue("DepartmentId", 2);
            DataSetExtensionExamples.AddRow(dataSet, "Employee", employeeDataRow);

            // 4.a) Read back a value using data set, expected "John"
            string? firstName = DataSetExtensionExamples.GetFieldValue<string>(dataSet, "Employee", rowIndex: 0, columnName: "FirstName");

            // 4.b) Read back a value using data table, expected "Doe"
            string? lastName = employeeTable.GetFieldValue<string>(0, "LastName");

            // 4.c) Read back Full Time value using data row, expected "Full Time"
            string? employmentTypeDescription = DataRowExtensionExamples.GetDataFieldValue<string>(employmentTypeDataRow, "Description");

            // 4.d) Read employment type code from data row using POCO interface, change it from "ET01" to "ET02"
            // and verify that changes are propagated to underlying data row
            IEmploymentType employmentType = DataRowExtensionExamples.AsInterface<IEmploymentType>(employmentTypeDataRow);
            string? employmentTypeCode = employmentType.Code;
            employmentType.Code = "ET02";
            string? updatedEmploymentTypeCode = employmentTypeDataRow["Code"] as string;

            // 4.e) Read employment type code from data row using POCO interface, change it from "ET02" to "ET03"
            // and verify that changes are NOT propagated to underlying data row
            EmploymentType employmentTypePoco = DataRowExtensionExamples.ToPoco<EmploymentType>(employmentTypeDataRow);
            employmentTypePoco.Code = "ET03";
            updatedEmploymentTypeCode = employmentTypeDataRow["Code"] as string;

            // 4.f) Update data row from POCO object
            // and verify that employment type code was updated to "ET03"
            DataRowExtensionExamples.CopyFromPoco<EmploymentType>(employmentTypeDataRow, employmentTypePoco);
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

            // Data sets merge example
            // 7. Remove row from table
            DataSetExtensionExamples.RemoveRow(dataSet, "Employee", 0);

            // 8. Clear table
            DataSetExtensionExamples.ClearTable(dataSet, "Department");

            // 9. Change information of employment type code in copied data set
            copyOfDataSet!.Tables["EmploymentType"].Rows[0]["Id"] = 2;
            copyOfDataSet.Tables["EmploymentType"].Rows[0]["Code"] = "ET02";
            copyOfDataSet.Tables["EmploymentType"].Rows[0]["Description"] = "Part Time";

            // 10. Change information of employee last name in copied data set
            copyOfDataSet.Tables["Employee"].Rows[1]["LastName"] = "Monk";

            // 11) Merge data sets
            // dataSet contains Department table without rows
            // dataSet contains EmploymentType table with 1 row: 1, "ET03", "Full Time"
            // copyOfDataSet contains Employee table with 1 row: 2, "Sara", "Gor", 2

            // copyOfDataSet contains Department table with 2 rows: 1, "Customer Service" and 2, "Financial"
            // copyOfDataSet contains Employee table with 2 rows: 1, "John", "Doe", 2 and 2, "Sara", "Monk", 2
            // copyOfDataSet contains EmploymentType table with 1 row: 1, "ET01", "Full Time"

            // After the merge dataSet will contain:
            // - Department table with 2 rows: 1, "Customer Service" and 2, "Financial"
            // - Employee table with 2 rows: 1, "John", "Doe", 2 and 2, "Sara", "Monk", 2
            // - EmploymentType table with 1 row: 2, "ET02", "Part Time"
            dataSet.AcceptChanges();
            copyOfDataSet.AcceptChanges();
            IDataSetMergeResult dataSetMergeResult = DataSetExtensionExamples.MergeWith(dataSet, copyOfDataSet);

            // Data set and observable data set merge example
            // 12. Create observable data set
            IObservableDataSet observableDataSet = new ObservableDataSet(dataSet);
            observableDataSet.DataFieldValueChanged += ObservableDataSet_DataFieldValueChanged;
            observableDataSet.RowAdded += ObservableDataSet_RowsAdded;
            observableDataSet.RowRemoved += ObservableDataSet_RowsRemoved;
            observableDataSet.TableAdded += ObservableDataSet_TableAdded;
            observableDataSet.TableRemoved += ObservableDataSet_TableRemoved;

            // 13. Create copy of previously merged data set
            string? secondJson = DataSetSerializer.ToJsonString(dataSet);
            IDataSet? copyOfMergedDataSet = DataSetSerializer.FromJsonString(secondJson);

            // 14. Remove table
            copyOfMergedDataSet!.Tables.Remove("EmploymentType");

            // 15. Remove row
            IDataTable copiedDepartmentDataTable = copyOfMergedDataSet.Tables["Department"];
            IDataRow copiedDepartmentDataRowForRemoval = copiedDepartmentDataTable.Rows[0];
            copiedDepartmentDataTable.Rows.Remove(copiedDepartmentDataRowForRemoval);

            // 16. Add row
            IDataRow newCopiedDepartmentDataRow = copiedDepartmentDataTable.AddNewRow();
            newCopiedDepartmentDataRow["Id"] = 5;
            newCopiedDepartmentDataRow["Name"] = "HR";

            // 17. Add table
            IDataTable payrollDataTable = new DataTable();
            payrollDataTable.TableName = "Payroll";
            payrollDataTable.AddColumn("Id", DataTypeNames.INT);
            payrollDataTable.AddColumn("Type", DataTypeNames.STRING);
            IDataRow payrollDataRow = payrollDataTable.AddNewRow();
            payrollDataRow["Id"] = "1";
            payrollDataRow["Type"] = "Monthly";
            copyOfMergedDataSet.Tables.Add(payrollDataTable.TableName, payrollDataTable);

            // 18. Modify row
            copyOfMergedDataSet.UpdateFieldValue<string>("Employee", 0, "FirstName", "Martin");

            // 19. Merge data sets
            // - observable dataSet contains Department table with 2 rows: 1, "Customer Service" and 2, "Financial"
            // - observable dataSet contains Employee table with 2 rows: 1, "John", "Doe", 2 and 2, "Sara", "Monk", 2
            // - observable dataSet contains EmploymentType table with 1 row: 2, "ET02", "Part Time"

            // copyOfMergedDataSet contains Department table with 2 rows: 2, "Financial" and 5, "HR"
            // copyOfMergedDataSet contains Employee table with 2 rows: 1, "John", "Doe", 2 and 2, "Martin", "Monk", 2
            // copyOfMergedDataSet contains Payroll table with 1 row: 1, Monthly

            // After the merge observable dataSet will contain:
            // - Department table with 2 rows: 2, "Financial" and 5, "HR"
            // - Employee table with 2 rows: 1, "John", "Doe", 2 and 2, "Martin", "Monk", 2
            // - EmploymentType table with 1 row: 2, "ET02", "Part Time"
            // - Payroll table with 1 row: 1, "Monthly"
            IObservableDataSetMergeResult observableDataSetMergeResult = observableDataSet.MergeWith(copyOfMergedDataSet);

            // 12) SQL Server data adapter example of loading data from database into data set
            LoadDataFromDatabase().Wait();
*/
            await SaveChangesetExample();
            Console.ReadLine();
        }

        private static void ObservableDataSet_TableRemoved(object? sender, TablesChangedEventArgs e)
        {
        }

        private static void ObservableDataSet_TableAdded(object? sender, TablesChangedEventArgs e)
        {
        }

        private static void ObservableDataSet_RowsRemoved(object? sender, RowsChangedEventArgs e)
        {
        }

        private static void ObservableDataSet_RowsAdded(object? sender, RowsChangedEventArgs e)
        {
        }

        private static void ObservableDataSet_DataFieldValueChanged(object? sender, DataFieldValueChangedEventArgs e)
        {
        }

        /// <summary>
        /// Loading data from database example using PocoDataSet.SqlServerDataAdapter.SqlDataAdapter
        /// Create PocoDataSetExamples database first and fill it with data using items located inside DatabaseItems folder
        /// </summary>
        static async Task LoadDataFromDatabase()
        {
            List<string> returnedTableNames = new List<string>();
            returnedTableNames.Add("Department");

            IDataSet requestDataSet = DataSetFactoryExamples.CreateDataSet();
            string connectionString = "Server=localhost;Database=BlazorCoffeeShop;Trusted_Connection=True;Encrypt=Optional;MultipleActiveResultSets=True;Connection Timeout=300";
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

        static async Task SaveChangesetExample()
        {
            // Create SqlDataAdapter
            string connectionString = "Server=localhost;Database=BlazorCoffeeShop;Trusted_Connection=True;Encrypt=Optional;MultipleActiveResultSets=True;Connection Timeout=300";
            SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(connectionString);

            // 1. Create an empty data set
            IDataSet dataSet = DataSetFactoryExamples.CreateDataSet();

            // 2. Create an empty data table
            IDataTable departmentDataTable = DataSetExtensionExamples.AddNewTable(dataSet, "Department");
            DataTableExtensionExamples.AddColumn(departmentDataTable, "Id", DataTypeNames.INT);
            DataTableExtensionExamples.AddColumn(departmentDataTable, "Name", DataTypeNames.STRING);

            // INSERT example
            // Create a new row by AddNewRow method on data table
            IDataRow departmentDataRow = DataTableExtensionExamples.AddNewRow(departmentDataTable);
            departmentDataRow.UpdateDataFieldValue("Name", "Emergency");

            // 4. Create changeset
            IDataSet? changeset = dataSet.CreateChangeset();
            if (changeset == null)
            {
                return;
            }

            // Call SaveChangesAsync method
            await sqlDataAdapter.SaveChangesAsync(changeset);
            dataSet.MergeWith(changeset, MergeMode.PostSave);
            /*
                        // Make data as Deleted
                        departmentDataRow.AcceptChanges();
                        departmentDataRow.Delete();
            */
        }
    }
}
