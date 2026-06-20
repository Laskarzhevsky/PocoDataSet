using PocoDataSet.IData;

using PocoDataRow = PocoDataSet.Data.DataRow;
using PocoDataTable = PocoDataSet.Data.DataTable;

namespace PocoDataSet.SqlServerDataAdapter.IntegrationTests
{
    static class HostedApplicationLayerTestData
    {
        public static PocoDataTable CreateHostedApplicationLayerRequestTable()
        {
            PocoDataTable table = new PocoDataTable();
            table.TableName = "IHostedApplicationLayer";
            table.AddColumn("ApplicationLayerName", "varchar", true, false, false);
            table.AddColumn("DomainName", "varchar", true, false, false);
            table.AddColumn("Url", "varchar", true, false, false);
            table.AddColumn("UseCaseName", "varchar", true, false, false);
            table.AddColumn("BusinessGuid", "uniqueidentifier", true, false, false);
            table.AddColumn("BusinessStringRepresentation", "nvarchar", true, false, false);
            return table;
        }

        public static void AddHostedApplicationLayer(PocoDataTable table, string domainName, string useCaseName, string applicationLayerName, string url)
        {
#pragma warning disable CS0618
            IDataRow row = new PocoDataRow();
#pragma warning restore CS0618
            row["DomainName"] = domainName;
            row["UseCaseName"] = useCaseName;
            row["ApplicationLayerName"] = applicationLayerName;
            row["Url"] = url;
            row["BusinessStringRepresentation"] = domainName + "." + useCaseName + "." + applicationLayerName;
            table.AddLoadedRow(row);
        }
    }
}
