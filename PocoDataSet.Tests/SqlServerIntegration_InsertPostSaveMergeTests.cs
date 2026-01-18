using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public  class SqlServerIntegration_InsertPostSaveMergeTests
    {
        [Fact(Skip = "Temporarily disabled feature")]
        public async Task Insert_Then_PostSaveMerge_PropagatesIdentityAndRowVersion()
        {
            //            string? masterConnectionString = Environment.GetEnvironmentVariable("POCODATASET_TEST_SQLSERVER");
            string? masterConnectionString = "Server=localhost;Database=master;Trusted_Connection=True;Encrypt=Optional;MultipleActiveResultSets=True;Connection Timeout=300";
            if (string.IsNullOrWhiteSpace(masterConnectionString))
            {
                // Integration test intentionally skipped.
                // Set env var POCODATASET_TEST_SQLSERVER to a SQL Server connection string (to master) to run.
                return;
            }

            string dbName = "PocoDataSet_Test_" + Guid.NewGuid().ToString("N");
            string dbConnectionString = BuildDatabaseConnectionString(masterConnectionString, dbName);

            try
            {
                await CreateDatabaseAsync(masterConnectionString, dbName);
                await CreateDepartmentTableAsync(dbConnectionString);

                // Arrange: local dataset row before save
                IDataSet current = DataSetFactory.CreateDataSet();
                IDataTable dept = current.AddNewTable("Department");
                dept.AddColumn("Id", DataTypeNames.INT32);
                dept.AddColumn("Name", DataTypeNames.STRING);
                dept.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);
                dept.AddColumn("RowVersion", DataTypeNames.BINARY);
                dept.PrimaryKeys = new List<string> { "Id" };

                Guid clientKey = Guid.NewGuid();

                IDataRow local = DataRowExtensions.CreateRowFromColumns(dept.Columns);
                local["Id"] = 0; // temporary identity placeholder
                local["Name"] = "HR";
                local[SpecialColumnNames.CLIENT_KEY] = clientKey;
                local["RowVersion"] = null;
                dept.AddRow(local);

                Assert.Equal(DataRowState.Added, local.DataRowState);

                // Create changeset to send to DB
                IDataSet? changeset = current.CreateChangeset();
                Assert.NotNull(changeset);
                Assert.True(changeset!.Tables.ContainsKey("Department"));
                Assert.Single(changeset.Tables["Department"].Rows);

                // Act: save changeset to SQL Server (mutates changeset row with OUTPUT Id + RowVersion)
                PocoDataSet.SqlServerDataAdapter.SqlDataAdapter adapter = new PocoDataSet.SqlServerDataAdapter.SqlDataAdapter(dbConnectionString);
                int affected = await adapter.SaveChangesAsync(changeset, dbConnectionString);

                Assert.Equal(1, affected);

                // Act: merge server-confirmed changes (identity + rowversion) back into local dataset
                current.MergeWith(changeset, MergeMode.PostSave);

                // Assert: local row has identity + rowversion and is normalized to Unchanged
                Assert.Equal(DataRowState.Unchanged, local.DataRowState);

                int newId = (int)local["Id"]!;
                Assert.True(newId > 0);

                object? rvObj;
                local.TryGetValue("RowVersion", out rvObj);
                Assert.NotNull(rvObj);

                byte[] rvBytes = (byte[])rvObj!;
                Assert.True(rvBytes.Length > 0);

                // Assert: row exists in DB
                int dbCount = await CountDepartmentsAsync(dbConnectionString, newId, "HR");
                Assert.Equal(1, dbCount);
            }
            finally
            {
                try
                {
                    await DropDatabaseAsync(masterConnectionString, dbName);
                }
                catch
                {
                    // ignore cleanup failures
                }
            }
        }

        private static string BuildDatabaseConnectionString(string masterConnectionString, string databaseName)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(masterConnectionString);
            builder.InitialCatalog = databaseName;
            return builder.ToString();
        }

        private static async Task CreateDatabaseAsync(string masterConnectionString, string dbName)
        {
            using (SqlConnection cn = new SqlConnection(masterConnectionString))
            {
                await cn.OpenAsync();

                string sql = "CREATE DATABASE [" + dbName + "]";
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task CreateDepartmentTableAsync(string dbConnectionString)
        {
            string sql =
@"CREATE TABLE [dbo].[Department](
    [Id] INT IDENTITY(1,1) NOT NULL CONSTRAINT [PK_Department] PRIMARY KEY,
    [Name] NVARCHAR(50) NULL,
    [__ClientKey] UNIQUEIDENTIFIER NULL,
    [RowVersion] ROWVERSION NOT NULL
);";

            using (SqlConnection cn = new SqlConnection(dbConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        private static async Task<int> CountDepartmentsAsync(string dbConnectionString, int id, string name)
        {
            string sql = "SELECT COUNT(1) FROM [dbo].[Department] WHERE [Id] = @id AND [Name] = @name";

            using (SqlConnection cn = new SqlConnection(dbConnectionString))
            {
                await cn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@name", name);

                    object? value = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(value);
                }
            }
        }

        private static async Task DropDatabaseAsync(string masterConnectionString, string dbName)
        {
            using (SqlConnection cn = new SqlConnection(masterConnectionString))
            {
                await cn.OpenAsync();

                string sql =
@"IF DB_ID(@db) IS NOT NULL
BEGIN
    DECLARE @sql NVARCHAR(MAX);
    SET @sql = N'ALTER DATABASE [' + @db + N'] SET SINGLE_USER WITH ROLLBACK IMMEDIATE; DROP DATABASE [' + @db + N'];';
    EXEC(@sql);
END";

                using (SqlCommand cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.AddWithValue("@db", dbName);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }
    }
}
