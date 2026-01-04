using System;
using System.Reflection;

using Xunit;

using PocoDataSet.Extensions;
using PocoDataSet.IData;

namespace PocoDataSet.Tests
{
    public sealed class DataRowExtensionsCopyToPocoRoundTripTests
    {
        [Fact]
        public void CopyFromPoco_ThenCopyToPoco_RoundTripsValues_ForSimpleTypes()
        {
            // Arrange
            SourcePoco source = new SourcePoco();
            source.Id = 5;
            source.Name = "Alice";
            source.Amount = 99.95m;
            source.IsActive = true;

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);
            t.AddColumn("Name", DataTypeNames.STRING);
            t.AddColumn("Amount", DataTypeNames.DECIMAL);
            t.AddColumn("IsActive", DataTypeNames.BOOL);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);

            // Act: POCO -> row -> POCO(existing)
            row.CopyFromPoco(source);

            TargetPoco target = new TargetPoco();
            target.Id = 0;
            target.Name = null;
            target.Amount = 0m;
            target.IsActive = false;

            row.CopyToPoco(target);

            // Assert
            Assert.Equal(5, target.Id);
            Assert.Equal("Alice", target.Name);
            Assert.Equal(99.95m, target.Amount);
            Assert.True(target.IsActive);
        }

        [Fact]
        public void CopyFromPoco_AddsKeysEvenWhenRowSchemaDoesNotHaveColumns()
        {
            // Arrange
            SourcePoco source = new SourcePoco();
            source.Id = 5;
            source.Name = "Alice";
            source.Amount = 99.95m;
            source.IsActive = true;

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);

            // Act
            row.CopyFromPoco(source);

            // Assert: Id exists (schema column)
            object? id;
            row.TryGetValue("Id", out id);
            Assert.Equal(5, (int)id!);

            // Assert: other keys are added (even though schema has no such columns)
            Assert.True(row.ContainsKey("Name"));
            Assert.True(row.ContainsKey("Amount"));
            Assert.True(row.ContainsKey("IsActive"));
        }

        [Fact]
        public void CopyToPoco_IgnoresProperties_WhenRowDoesNotHaveColumns()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Id"] = 9;

            TargetPoco target = new TargetPoco();
            target.Name = "KeepMe";
            target.Amount = 123m;
            target.IsActive = true;

            // Act
            row.CopyToPoco(target);

            // Assert
            Assert.Equal(9, target.Id);
            Assert.Equal("KeepMe", target.Name);
            Assert.Equal(123m, target.Amount);
            Assert.True(target.IsActive);
        }

        [Fact]
        public void CopyFromPoco_WhenPropertyGetterThrows_ThrowsTargetInvocationExceptionWithInnerException()
        {
            // Arrange
            PocoWithThrowingGetter source = new PocoWithThrowingGetter();

            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Id", DataTypeNames.INT32);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);

            // Act
            TargetInvocationException ex = Assert.Throws<TargetInvocationException>(
                delegate
                {
                    row.CopyFromPoco(source);
                });

            // Assert
            Assert.NotNull(ex.InnerException);
            Assert.IsType<InvalidOperationException>(ex.InnerException);
            Assert.Equal("boom", ex.InnerException!.Message);
        }

        [Fact]
        public void CopyToPoco_DoesNotThrow_WhenPropertySetterThrows()
        {
            // Arrange
            IDataSet ds = DataSetFactory.CreateDataSet();
            IDataTable t = ds.AddNewTable("T");
            t.AddColumn("Name", DataTypeNames.STRING);

            IDataRow row = DataRowExtensions.CreateRowFromColumns(t.Columns);
            row["Name"] = "X";

            PocoWithThrowingSetter target = new PocoWithThrowingSetter();

            // Act (should swallow)
            row.CopyToPoco(target);

            // Assert
            Assert.True(true);
        }

        private sealed class SourcePoco
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public decimal Amount { get; set; }

            public bool IsActive { get; set; }
        }

        private sealed class TargetPoco
        {
            public int Id { get; set; }

            public string? Name { get; set; }

            public decimal Amount { get; set; }

            public bool IsActive { get; set; }
        }

        private sealed class PocoWithThrowingGetter
        {
            public int Id
            {
                get { throw new InvalidOperationException("boom"); }
            }
        }

        private sealed class PocoWithThrowingSetter
        {
            private string? _name;

            public string? Name
            {
                get { return _name; }
                set { throw new InvalidOperationException("boom"); }
            }
        }
    }
}
