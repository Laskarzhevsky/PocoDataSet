using System;
using System.Globalization;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class PocoMappingEdgeCaseTests
    {
        [Fact]
        public void CopyToPoco_NullableEnum_IsAssigned_WhenRowHasStringValue()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("NullableStatus", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["NullableStatus"] = "Active";

            PocoWithNullableEnum poco = new PocoWithNullableEnum();
            poco.NullableStatus = SampleStatus.Inactive;

            // Act
            row.CopyToPoco(poco);

            // Assert: case-insensitive enum parsing + Nullable<Enum> supported
            Assert.Equal(SampleStatus.Active, poco.NullableStatus);
        }

        [Fact]
        public void CopyToPoco_DateTimeFromString_ParsesAndPreservesInstantInTime()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("WhenUtc", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["WhenUtc"] = "2026-01-03T12:34:56Z";

            PocoWithDateTime poco = new PocoWithDateTime();
            poco.WhenUtc = new DateTime(2026, 1, 1, 1, 2, 3, DateTimeKind.Utc);

            // Act
            row.CopyToPoco(poco);

            // Assert (compare instants)
            DateTime expected = DateTime.Parse(
                "2026-01-03T12:34:56Z",
                CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind);

            Assert.Equal(expected.ToUniversalTime(), poco.WhenUtc.ToUniversalTime());
        }

        [Fact]
        public void CopyToPoco_GuidFromString_ParsesGuid()
        {
            // Arrange
            Guid incoming = Guid.NewGuid();

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("ClientKey", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["ClientKey"] = incoming.ToString();

            PocoWithGuid poco = new PocoWithGuid();
            poco.ClientKey = Guid.Empty;

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(incoming, poco.ClientKey);
        }

        [Fact]
        public void ToPoco_GuidFromString_ParsesGuid()
        {
            // Arrange
            Guid g = Guid.NewGuid();

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("ClientKey", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["ClientKey"] = g.ToString();

            // Act
            PocoWithGuid poco = row.ToPoco<PocoWithGuid>();

            // Assert
            Assert.Equal(g, poco.ClientKey);
        }

        [Fact]
        public void CaseSensitivity_CopyToPoco_Copies_WhenRowHasLowercaseKey()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");

            // Create schema with "name" lowercase.
            t.AddColumn("name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["name"] = "alice";

            PocoWithName poco = new PocoWithName();
            poco.Name = "KeepMe";

            // Act
            row.CopyToPoco(poco);

            // Assert: case-insensitive mapping
            Assert.Equal("alice", poco.Name);
        }

        [Fact]
        public void CaseSensitivity_CopyFromPoco_UsesExistingKey_WhenSchemaUsesLowercase()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");

            // Schema column is lowercase
            t.AddColumn("name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);

            PocoWithName source = new PocoWithName();
            source.Name = "Alice";

            // Act
            row.CopyFromPoco(source);

            // Assert: should populate existing "name" key, not add "Name"
            Assert.True(row.ContainsKey("name"));
            Assert.False(row.ContainsKey("Name"));

            object? v;
            row.TryGetValue("name", out v);
            Assert.Equal("Alice", (string?)v);
        }

        private enum SampleStatus
        {
            Unknown = 0,
            Active = 1,
            Inactive = 2
        }

        private sealed class PocoWithNullableEnum
        {
            public SampleStatus? NullableStatus { get; set; }
        }

        private sealed class PocoWithDateTime
        {
            public DateTime WhenUtc { get; set; }
        }

        private sealed class PocoWithGuid
        {
            public Guid ClientKey { get; set; }
        }

        private sealed class PocoWithName
        {
            public string? Name { get; set; }
        }
    }
}
