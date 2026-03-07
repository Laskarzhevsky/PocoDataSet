using System;
using System.Linq;
using System.Threading.Tasks;

using PocoDataSet.ObjectEfCoreBridge;
using PocoDataSet.ObjectData;

using Xunit;

namespace PocoDataSet.ObjectEfCoreBridgeTests
{
    public sealed class EfCoreObjectDataAdapterTests
    {
        [Fact]
        public void LoadData_Creates_ObjectDataSet_With_Populated_Typed_Table()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees.OrderBy(x => x.Id), "Employee");

            Assert.NotNull(objectDataSet);
            Assert.Single(objectDataSet.Tables);

            Assert.True(objectDataSet.TryGetTable<Employee>("Employee", out ObjectTable<Employee>? employeeTable));
            Assert.NotNull(employeeTable);
            Assert.Equal(3, employeeTable.Items.Count);
            Assert.Equal("Alice", employeeTable.Items[0].Name);
            Assert.Equal("Bob", employeeTable.Items[1].Name);
            Assert.Equal("Carol", employeeTable.Items[2].Name);
        }

        [Fact]
        public void LoadData_Into_Existing_ObjectDataSet_Adds_Table()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = new ObjectDataSet();

            EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees.OrderBy(x => x.Id), objectDataSet, "Employee");
            EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Departments.OrderBy(x => x.Id), objectDataSet, "Department");

            Assert.Equal(2, objectDataSet.Tables.Count);

            Assert.True(objectDataSet.TryGetTable<Employee>("Employee", out ObjectTable<Employee>? employeeTable));
            Assert.True(objectDataSet.TryGetTable<Department>("Department", out ObjectTable<Department>? departmentTable));

            Assert.NotNull(employeeTable);
            Assert.NotNull(departmentTable);

            Assert.Equal(3, employeeTable.Items.Count);
            Assert.Equal(2, departmentTable.Items.Count);
        }

        [Fact]
        public async Task LoadDataAsync_Creates_ObjectDataSet_With_Populated_Typed_Table()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = await EfCoreObjectDataAdapter.LoadDataAsync(dbContext, dbContext.Employees.OrderBy(x => x.Id), "Employee");

            Assert.True(objectDataSet.TryGetTable<Employee>("Employee", out ObjectTable<Employee>? employeeTable));
            Assert.NotNull(employeeTable);
            Assert.Equal(3, employeeTable.Items.Count);
            Assert.Equal("Alice", employeeTable.Items[0].Name);
        }

        [Fact]
        public async Task LoadDataAsync_Into_Existing_ObjectDataSet_Adds_Table()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = new ObjectDataSet();

            await EfCoreObjectDataAdapter.LoadDataAsync(dbContext, dbContext.Employees.OrderBy(x => x.Id), objectDataSet, "Employee");
            await EfCoreObjectDataAdapter.LoadDataAsync(dbContext, dbContext.Departments.OrderBy(x => x.Id), objectDataSet, "Department");

            Assert.Equal(2, objectDataSet.Tables.Count);

            Assert.True(objectDataSet.TryGetTable<Employee>("Employee", out ObjectTable<Employee>? employeeTable));
            Assert.True(objectDataSet.TryGetTable<Department>("Department", out ObjectTable<Department>? departmentTable));

            Assert.NotNull(employeeTable);
            Assert.NotNull(departmentTable);
            Assert.Equal(3, employeeTable.Items.Count);
            Assert.Equal(2, departmentTable.Items.Count);
        }

        [Fact]
        public void LoadData_Throws_When_DbContext_Is_Null()
        {
            IQueryable<Employee> query;

            using (TestDbContext dbContext = DbContextFactory.CreateDbContext())
            {
                query = dbContext.Employees;
                Assert.Throws<ArgumentNullException>(() => EfCoreObjectDataAdapter.LoadData<Employee>(null!, query, "Employee"));
            }
        }

        [Fact]
        public void LoadData_Throws_When_Query_Is_Null()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            Assert.Throws<ArgumentNullException>(() => EfCoreObjectDataAdapter.LoadData<Employee>(dbContext, null!, "Employee"));
        }

        [Fact]
        public void LoadData_Throws_When_Target_ObjectDataSet_Is_Null()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            Assert.Throws<ArgumentNullException>(() => EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees, null!, "Employee"));
        }

        [Fact]
        public void LoadData_Throws_When_TableName_Is_Missing()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            Assert.Throws<ArgumentNullException>(() => EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees, ""));
            Assert.Throws<ArgumentNullException>(() => EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees, "   "));
        }

        [Fact]
        public async Task LoadDataAsync_Throws_When_TableName_Is_Missing()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await EfCoreObjectDataAdapter.LoadDataAsync(dbContext, dbContext.Employees, "");
            });
        }

        [Fact]
        public void LoadData_Throws_When_Same_TableName_Is_Added_Twice()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = new ObjectDataSet();

            EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees, objectDataSet, "Employee");

            Assert.Throws<ArgumentException>(() =>
            {
                EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees, objectDataSet, "Employee");
            });
        }

        [Fact]
        public void LoadData_Preserves_Entity_Type_In_ObjectTable()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Departments, "Department");

            Assert.True(objectDataSet.TryGetTable("Department", out PocoDataSet.IObjectData.IObjectTable? table));
            Assert.NotNull(table);
            Assert.Equal(typeof(Department), table.ItemType);
            Assert.Equal(2, table.UntypedItems.Count);
        }

        [Fact]
        public void LoadData_With_Empty_Query_Creates_Empty_Table()
        {
            using TestDbContext dbContext = DbContextFactory.CreateDbContext();

            ObjectDataSet objectDataSet = EfCoreObjectDataAdapter.LoadData(dbContext, dbContext.Employees.Where(x => x.Id < 0), "Employee");

            Assert.True(objectDataSet.TryGetTable<Employee>("Employee", out ObjectTable<Employee>? employeeTable));
            Assert.NotNull(employeeTable);
            Assert.Empty(employeeTable.Items);
        }
    }
}
