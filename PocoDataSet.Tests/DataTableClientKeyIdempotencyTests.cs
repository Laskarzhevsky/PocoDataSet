using System;
using Xunit;
using PocoDataSet.Data;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public class DataTableClientKeyIdempotencyTests
    {
        [Fact]
        public void EnsureClientKeyColumnExists_WhenCalledTwice_DoesNotDuplicate()
        {
            DataTable table = new DataTable();
            table.TableName = "Department";

            table.AddColumn("Id", DataTypeNames.INT32);

            int before = table.Columns.Count;

            table.EnsureClientKeyColumnExists();
            int afterFirst = table.Columns.Count;

            table.EnsureClientKeyColumnExists();
            int afterSecond = table.Columns.Count;

            Assert.Equal(before + 1, afterFirst);
            Assert.Equal(afterFirst, afterSecond);

            Assert.True(table.ContainsColumn(SpecialColumnNames.CLIENT_KEY));
        }

        [Fact]
        public void EnsureClientKeyColumnExists_WhenColumnAlreadyPresent_BindsToExistingInstance()
        {
            DataTable table = new DataTable();
            table.TableName = "Department";

            table.AddColumn("Id", DataTypeNames.INT32);

            IColumnMetadata existing = table.AddColumn(SpecialColumnNames.CLIENT_KEY, DataTypeNames.GUID);

            table.EnsureClientKeyColumnExists();

            // Must still be a single column and the schema lookup must return the same instance.
            Assert.Equal(2, table.Columns.Count);

            IColumnMetadata? fromLookup = null;
            for (int i = 0; i < table.Columns.Count; i++)
            {
                if (string.Equals(table.Columns[i].ColumnName, SpecialColumnNames.CLIENT_KEY, StringComparison.OrdinalIgnoreCase))
                {
                    fromLookup = table.Columns[i];
                    break;
                }
            }

            Assert.NotNull(fromLookup);
            Assert.Same(existing, fromLookup);
        }
    }
}
