using System.Threading.Tasks;

using Microsoft.Data.SqlClient;

namespace PocoDataSet.SqlServerDataAdapter.IntegrationTests
{
    public sealed class SqlServerIntegrationTestFixture : IAsyncLifetime
    {
        public SqlConnection Connection
        {
            get; private set;
        } = null!;

        public async ValueTask InitializeAsync()
        {
            string connectionString = IntegrationTestConnectionString.GetRequiredConnectionString();
            Connection = new SqlConnection(connectionString);
            await Connection.OpenAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (Connection != null)
            {
                await Connection.DisposeAsync().ConfigureAwait(false);
            }
        }
    }
}
