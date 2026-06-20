namespace PocoDataSet.SqlServerDataAdapter.IntegrationTests
{
    static class IntegrationTestConnectionString
    {
        public static string GetRequiredConnectionString()
        {
            string connectionString = "Data Source=localhost;Initial Catalog=SqlServerDataAdapterTestDatabase;Integrated Security=True;Persist Security Info=False;Pooling=False;MultipleActiveResultSets=False;Connect Timeout=60;Encrypt=True;TrustServerCertificate=True;Command Timeout=0";
            return connectionString;
        }
    }
}
