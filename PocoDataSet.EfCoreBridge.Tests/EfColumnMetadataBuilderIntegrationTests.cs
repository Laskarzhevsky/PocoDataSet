using System;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using PocoDataSet.IData;
using PocoDataSet.Extensions;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests
{
    public class EfColumnMetadataBuilderIntegrationTests
    {
        [Fact]
        public void ToDataTable_Sets_PrimaryKey_Metadata_And_TablePrimaryKeys()
        {
            using TestDbContext db = CreateContext();

            db.Departments.Add(new Department { Id = 1, Name = "Sales" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Departments.AsQueryable().ToDataTable(db, dataSet, "Department");

            Assert.Equal("Department", table.TableName);
            Assert.True(table.Columns.Any(c => c.ColumnName == "Id" && c.IsPrimaryKey));
            Assert.Contains("Id", table.PrimaryKeys);

            Assert.Equal(1, table.Rows.Count);
            Assert.Equal(1, (int)table.Rows[0]["Id"]!);
        }

        [Fact]
        public void ToDataTable_Sets_ForeignKey_Metadata()
        {
            using TestDbContext db = CreateContext();

            db.Departments.Add(new Department { Id = 1, Name = "Sales" });
            db.Employees.Add(new Employee { Id = 10, Name = "John", DepartmentId = 1, Salary = 12.34m });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Employees.AsQueryable().ToDataTable(db, dataSet, "Employee");

            IColumnMetadata? fk = FindColumn(table, "DepartmentId");
            Assert.NotNull(fk);

            Assert.True(fk!.IsForeignKey);
            Assert.Equal("Department", fk.ReferencedTableName);
            Assert.Equal("Id", fk.ReferencedColumnName);
        }

        [Fact]
        public void ToDataTable_Sets_Nullability_MaxLength_Precision_And_Scale()
        {
            using TestDbContext db = CreateContext();

            db.Departments.Add(new Department { Id = 1, Name = "Sales" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = db.Employees.AsQueryable().ToDataTable(db, dataSet, "Employee");

            IColumnMetadata? name = FindColumn(table, "Name");
            Assert.NotNull(name);
            Assert.False(name!.IsNullable);
            Assert.Equal(100, name.MaxLength);

            IColumnMetadata? salary = FindColumn(table, "Salary");
            Assert.NotNull(salary);
            Assert.Equal((byte)18, salary!.Precision);
            Assert.Equal((byte)2, salary.Scale);
        }

        [Fact]
        public void ToDataTable_Throws_When_PrimaryKey_Is_ShadowProperty()
        {
            using ShadowKeyDbContext db = CreateShadowKeyContext();

            ShadowEntity e = new ShadowEntity();
            db.ShadowEntities.Add(e);
            db.Entry(e).Property("Id").CurrentValue = 1;
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            Assert.Throws<InvalidOperationException>(() =>
            {
                db.ShadowEntities.AsQueryable().ToDataTable(db, dataSet, "Shadow");
            });
        }

        [Fact]
        public async Task ToDataTableAsync_Works_And_Uses_BuiltSchema()
        {
            using TestDbContext db = CreateContext();

            db.Departments.Add(new Department { Id = 1, Name = "Sales" });
            db.SaveChanges();

            IDataSet dataSet = DataSetFactory.CreateDataSet();

            IDataTable table = await db.Departments.AsQueryable().ToDataTableAsync(db, dataSet, "Department");

            Assert.Equal(1, table.Rows.Count);
            Assert.Contains("Id", table.PrimaryKeys);
        }

        private static IColumnMetadata? FindColumn(IDataTable table, string columnName)
        {
            for (int i = 0; i < table.Columns.Count; i++)
            {
                IColumnMetadata c = table.Columns[i];
                if (string.Equals(c.ColumnName, columnName, StringComparison.OrdinalIgnoreCase))
                {
                    return c;
                }
            }

            return null;
        }

        private static TestDbContext CreateContext()
        {
            DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new TestDbContext(options);
        }

        private static ShadowKeyDbContext CreateShadowKeyContext()
        {
            DbContextOptions<ShadowKeyDbContext> options = new DbContextOptionsBuilder<ShadowKeyDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new ShadowKeyDbContext(options);
        }

        private class TestDbContext : DbContext
        {
            public TestDbContext(DbContextOptions<TestDbContext> options)
                : base(options)
            {
            }

            public DbSet<Department> Departments => Set<Department>();
            public DbSet<Employee> Employees => Set<Employee>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Department>(b =>
                {
                    b.ToTable("Department");
                    b.HasKey(x => x.Id);
                    b.Property(x => x.Id).ValueGeneratedNever();
                    b.Property(x => x.Name).HasMaxLength(50).IsRequired();
                });

                modelBuilder.Entity<Employee>(b =>
                {
                    b.ToTable("Employee");
                    b.HasKey(x => x.Id);
                    b.Property(x => x.Id).ValueGeneratedNever();

                    b.Property(x => x.Name).HasMaxLength(100).IsRequired();

                    b.Property(x => x.Salary).HasPrecision(18, 2);

                    b.HasOne<Department>()
                        .WithMany()
                        .HasForeignKey(x => x.DepartmentId);
                });
            }
        }

        private class ShadowKeyDbContext : DbContext
        {
            public ShadowKeyDbContext(DbContextOptions<ShadowKeyDbContext> options)
                : base(options)
            {
            }

            public DbSet<ShadowEntity> ShadowEntities => Set<ShadowEntity>();

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<ShadowEntity>(b =>
                {
                    b.ToTable("Shadow");
                    b.Property<int>("Id").ValueGeneratedNever();
                    b.HasKey("Id");
                    b.Property(x => x.Name).HasMaxLength(10);
                });
            }
        }

        private class Department
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }

        private class Employee
        {
            public int Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int DepartmentId { get; set; }
            public decimal Salary { get; set; }
        }

        private class ShadowEntity
        {
            public string? Name { get; set; }
        }
    }
}
