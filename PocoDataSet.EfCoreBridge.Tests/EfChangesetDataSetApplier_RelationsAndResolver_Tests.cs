using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using PocoDataSet.Data;
using PocoDataSet.EfCoreBridge;
using PocoDataSet.Extensions;
using PocoDataSet.IData;

using Xunit;

namespace PocoDataSet.EfCoreBridge.Tests;

public class EfChangesetDataSetApplier_RelationsAndResolver_Tests
{
    [Fact]
    public void ApplyChangesetAndSave_InsertsParentBeforeChild_WhenRelationExists_Sqlite()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<ParentChildDbContext> options = new DbContextOptionsBuilder<ParentChildDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using ParentChildDbContext db = new ParentChildDbContext(options);
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

        // Arrange: build POCO DataSet with Parent/Child tables and a relation Parent.Id -> Child.ParentId
        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable parent = ds.AddNewTable("Parent");
        parent.AddColumn("Id", DataTypeNames.INT32);
        parent.AddColumn("Name", DataTypeNames.STRING);
        parent.PrimaryKeys = new List<string> { "Id" };

        IDataTable child = ds.AddNewTable("Child");
        child.AddColumn("Id", DataTypeNames.INT32);
        child.AddColumn("ParentId", DataTypeNames.INT32);
        child.AddColumn("Name", DataTypeNames.STRING);
        child.PrimaryKeys = new List<string> { "Id" };

        AddRelation(ds, "ParentChild", "Parent", new List<string> { "Id" }, "Child", new List<string> { "ParentId" });

        IDataRow p1 = parent.AddNewRow();
        p1["Id"] = 1;
        p1["Name"] = "P1";

        IDataRow c1 = child.AddNewRow();
        c1["Id"] = 10;
        c1["ParentId"] = 1;
        c1["Name"] = "C1";

        IDataSet cs = ds.CreateChangeset();

        // Act: relation ordering should apply Parent before Child.
        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);
        // Assert
        Assert.Equal(1, db.Parents.Count());
        Assert.Equal(1, db.Children.Count());
        Assert.Equal(1, db.Parents.Single().Id);
        Assert.Equal(1, db.Children.Single().ParentId);
    }

    [Fact]
    public void ApplyChangesetAndSave_DeletesChildBeforeParent_WhenBothDeleted_Sqlite()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<ParentChildDbContext> options = new DbContextOptionsBuilder<ParentChildDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using ParentChildDbContext db = new ParentChildDbContext(options);
        db.Database.EnsureCreated();
        db.Database.ExecuteSqlRaw("PRAGMA foreign_keys=ON;");

        // Seed baseline
        db.Parents.Add(new ParentEntity { Id = 1, Name = "P1" });
        db.Children.Add(new ChildEntity { Id = 10, ParentId = 1, Name = "C1" });
        db.SaveChanges();

        // Arrange dataset baseline (loaded rows)
        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable parent = ds.AddNewTable("Parent");
        parent.AddColumn("Id", DataTypeNames.INT32);
        parent.AddColumn("Name", DataTypeNames.STRING);
        parent.PrimaryKeys = new List<string> { "Id" };

        IDataTable child = ds.AddNewTable("Child");
        child.AddColumn("Id", DataTypeNames.INT32);
        child.AddColumn("ParentId", DataTypeNames.INT32);
        child.AddColumn("Name", DataTypeNames.STRING);
        child.PrimaryKeys = new List<string> { "Id" };

        AddRelation(ds, "ParentChild", "Parent", new List<string> { "Id" }, "Child", new List<string> { "ParentId" });

        IDataRow pLoaded = DataRowExtensions.CreateRowFromColumns(parent.Columns);
        pLoaded["Id"] = 1;
        pLoaded["Name"] = "P1";
        parent.AddLoadedRow(pLoaded);

        IDataRow cLoaded = DataRowExtensions.CreateRowFromColumns(child.Columns);
        cLoaded["Id"] = 10;
        cLoaded["ParentId"] = 1;
        cLoaded["Name"] = "C1";
        child.AddLoadedRow(cLoaded);

        // Mark both deleted (if parent were deleted first in persistence, SQLite FK would reject)
        child.DeleteRow(cLoaded);
        parent.DeleteRow(pLoaded);

        IDataSet cs = ds.CreateChangeset();

        // Act
        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);
        // Assert
        Assert.Empty(db.Children.ToList());
        Assert.Empty(db.Parents.ToList());
    }

    [Fact]
    public void ApplyChangesetAndSave_ResolvesEntityType_ByTableAttribute_DefaultPath()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<TableAttributeDbContext> options = new DbContextOptionsBuilder<TableAttributeDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using TableAttributeDbContext db = new TableAttributeDbContext(options);
        db.Database.EnsureCreated();

        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable t = ds.AddNewTable("Employee");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = t.AddNewRow();
        r["Id"] = 1;
        r["Name"] = "Alice";

        IDataSet cs = ds.CreateChangeset();

        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);
        Assert.Equal(1, db.Employees.Count());
        Assert.Equal("Alice", db.Employees.Single().Name);
    }

    [Fact]
    public void ApplyChangesetAndSave_ResolvesEntityType_ByChangesetTableAttribute_OverridePath()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<ChangesetTableOverrideDbContext> options = new DbContextOptionsBuilder<ChangesetTableOverrideDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using ChangesetTableOverrideDbContext db = new ChangesetTableOverrideDbContext(options);
        db.Database.EnsureCreated();

        IDataSet ds = DataSetFactory.CreateDataSet();

        // Changeset table is "Employee" but EF table is "HR_EMP_T"
        IDataTable t = ds.AddNewTable("Employee");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.AddColumn("Name", DataTypeNames.STRING);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = t.AddNewRow();
        r["Id"] = 7;
        r["Name"] = "Bob";

        IDataSet cs = ds.CreateChangeset();

        EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);
        Assert.Equal(1, db.EmployeeEntities.Count());
        Assert.Equal(7, db.EmployeeEntities.Single().Id);
        Assert.Equal("Bob", db.EmployeeEntities.Single().Name);
    }

    [Fact]
    public void ApplyChangesetAndSave_ThrowsWithHelpfulMessage_WhenEntityCannotBeResolved()
    {
        using SqliteConnection connection = CreateOpenSqliteConnection();
        DbContextOptions<TableAttributeDbContext> options = new DbContextOptionsBuilder<TableAttributeDbContext>()
            .UseSqlite(connection)
            .EnableSensitiveDataLogging()
            .Options;

        using TableAttributeDbContext db = new TableAttributeDbContext(options);
        db.Database.EnsureCreated();

        IDataSet ds = DataSetFactory.CreateDataSet();

        IDataTable t = ds.AddNewTable("Unknown");
        t.AddColumn("Id", DataTypeNames.INT32);
        t.PrimaryKeys = new List<string> { "Id" };

        IDataRow r = t.AddNewRow();
        r["Id"] = 1;

        IDataSet cs = ds.CreateChangeset();

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
        {
            EfChangesetDataSetApplier.ApplyChangesetAndSave(db, cs);
        });

        Assert.Contains("no entity type mapping for table 'Unknown'", ex.Message);
        Assert.Contains("Table(\"Unknown\")", ex.Message);
    }

    private static SqliteConnection CreateOpenSqliteConnection()
    {
        SqliteConnection connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();
        return connection;
    }



    private static void AddRelation(
        IDataSet dataSet,
        string relationName,
        string parentTable,
        List<string> parentColumns,
        string childTable,
        List<string> childColumns)
    {
        if (dataSet == null)
        {
            throw new ArgumentNullException(nameof(dataSet));
        }

        IDataRelation relation = new DataRelation();
        relation.RelationName = relationName;
        relation.ParentTableName = parentTable;
        relation.ChildTableName = childTable;
        relation.ParentColumnNames = parentColumns;
        relation.ChildColumnNames = childColumns;

        dataSet.Relations.Add(relation);
    }


    // DbContexts and entities used for tests

    private sealed class ParentChildDbContext : DbContext
    {
        public ParentChildDbContext(DbContextOptions<ParentChildDbContext> options) : base(options)
        {
        }

        public DbSet<ParentEntity> Parents => Set<ParentEntity>();
        public DbSet<ChildEntity> Children => Set<ChildEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ParentEntity>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ChildEntity>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<ChildEntity>()
                .HasOne<ParentEntity>()
                .WithMany()
                .HasForeignKey(x => x.ParentId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }

    [Table("Parent")]
    private sealed class ParentEntity
    {
        public int Id
        {
            get; set;
        }
        public string? Name
        {
            get; set;
        }
    }

    [Table("Child")]
    private sealed class ChildEntity
    {
        public int Id
        {
            get; set;
        }
        public int ParentId
        {
            get; set;
        }
        public string? Name
        {
            get; set;
        }
    }

    private sealed class TableAttributeDbContext : DbContext
    {
        public TableAttributeDbContext(DbContextOptions<TableAttributeDbContext> options) : base(options)
        {
        }

        public DbSet<Employee> Employees => Set<Employee>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Employee>().HasKey(x => x.Id);
        }
    }

    [Table("Employee")]
    private sealed class Employee
    {
        public int Id
        {
            get; set;
        }
        public string? Name
        {
            get; set;
        }
    }

    private sealed class ChangesetTableOverrideDbContext : DbContext
    {
        public ChangesetTableOverrideDbContext(DbContextOptions<ChangesetTableOverrideDbContext> options) : base(options)
        {
        }

        public DbSet<EmployeeEntity> EmployeeEntities => Set<EmployeeEntity>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<EmployeeEntity>().HasKey(x => x.Id);
        }
    }

    [Table("HR_EMP_T")]
    [ChangesetTable("Employee")]
    private sealed class EmployeeEntity
    {
        public int Id
        {
            get; set;
        }
        public string? Name
        {
            get; set;
        }
    }
}
