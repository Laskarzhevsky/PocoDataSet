using System;
using System.Globalization;

using PocoDataSet.Extensions;
using PocoDataSet.IData;
using PocoDataSet.Serializer;
using Xunit;

namespace PocoDataSet.SerializerTests
{
    public sealed class SerializationDateTimeCultureInvarianceTests
    {
        [Fact]
        public void FromJsonString_Parses_DateTime_IndependentlyOfCurrentCulture()
        {
            CultureInfo originalCulture = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("fr-CA");

                IDataSet dataSet = DataSetFactory.CreateDataSet();
                IDataTable table = dataSet.AddNewTable("Dates");
                table.AddColumn("Id", DataTypeNames.INT32);
                table.AddColumn("Created", DataTypeNames.DATE_TIME);

                DateTime utc = new DateTime(2025, 12, 29, 13, 45, 59, DateTimeKind.Utc);

                IDataRow row = DataRowExtensions.CreateRowFromColumns(table.Columns);
                row["Id"] = 1;
                row["Created"] = utc;

                table.AddLoadedRow(row);

                string json = DataSetSerializer.ToJsonString(dataSet)!;
                IDataSet? restored = DataSetSerializer.FromJsonString(json);

                DateTime restoredUtc = (DateTime)restored!.Tables["Dates"].Rows[0]["Created"]!;
                Assert.Equal(utc, restoredUtc);
            }
            finally
            {
                CultureInfo.CurrentCulture = originalCulture;
            }
        }
    }
}
