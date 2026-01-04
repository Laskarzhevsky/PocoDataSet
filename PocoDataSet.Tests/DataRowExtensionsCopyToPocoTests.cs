using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataRowExtensionsCopyToPocoTests
    {
        [Fact]
        public void CopyToPoco_WhenDataRowIsNull_DoesNothing()
        {
            // Arrange
            IDataRow? row = null;
            SamplePoco poco = new SamplePoco();
            poco.Id = 123;

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(123, poco.Id);
        }

        [Fact]
        public void CopyToPoco_WhenPocoInstanceIsNull_DoesNothing()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 7;

            SamplePoco? poco = null;

            // Act (should not throw)
            row.CopyToPoco(poco);

            // Assert
            Assert.True(true);
        }

        [Fact]
        public void CopyToPoco_CopiesMatchingColumns_AndIgnoresMissingOnRow()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 10;
            row["Name"] = "Alice";

            SamplePoco poco = new SamplePoco();

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(10, poco.Id);
            Assert.Equal("Alice", poco.Name);

            // This property has no corresponding column in the row; must remain default.
            Assert.Equal(0, poco.Age);
        }

        [Fact]
        public void CopyToPoco_DoesNotWriteReadOnlyProperties()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("ReadOnlyValue", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["ReadOnlyValue"] = "X";

            SamplePoco poco = new SamplePoco();

            // Act (should not throw; should ignore read-only property)
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal("RO", poco.ReadOnlyValue);
        }

        [Fact]
        public void CopyToPoco_WhenValueIsNull_SetsNullOnNullableProperty()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("NullableInt", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["NullableInt"] = null;

            SamplePoco poco = new SamplePoco();
            poco.NullableInt = 5;

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Null(poco.NullableInt);
        }

        [Fact]
        public void CopyToPoco_ConvertsToNullableUnderlyingType()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("NullableInt", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["NullableInt"] = "42";

            SamplePoco poco = new SamplePoco();

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(42, poco.NullableInt);
        }

        [Fact]
        public void CopyToPoco_ParsesEnum_CaseInsensitive()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Status", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Status"] = "active";

            SamplePoco poco = new SamplePoco();
            poco.Status = SampleStatus.Unknown;

            // Act
            row.CopyToPoco(poco);

            // Assert
            Assert.Equal(SampleStatus.Active, poco.Status);
        }

        [Fact]
        public void CopyToPoco_WhenConversionFails_SwallowsAndLeavesExistingValue()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = "NotANumber";

            SamplePoco poco = new SamplePoco();
            poco.Id = 77;

            // Act (Convert.ChangeType will throw; method must swallow)
            row.CopyToPoco(poco);

            // Assert: unchanged
            Assert.Equal(77, poco.Id);
        }

        private enum SampleStatus
        {
            Unknown = 0,
            Active = 1,
            Inactive = 2
        }

        private sealed class SamplePoco
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public int Age { get; set; }

            public int? NullableInt { get; set; }

            public SampleStatus Status { get; set; }

            public string ReadOnlyValue
            {
                get { return "RO"; }
            }
        }
    }
}
