using MetaTable.Domain.Enums;
using MetaTable.Domain.Models;
using MetaTable.Infrastructure;
using MetaTable.Tests.Support;

namespace MetaTable.Tests.Integration;

public sealed class MetaTableStoreShould
{
    [Fact]
    public async Task Define_tables_via_metadata_and_persist_typed_rows_with_association()
    {
        string databasePath = Path.Combine(Path.GetTempPath(), $"metatable-{Guid.NewGuid():N}.db");
        MetaTableTestContextFactory factory = new($"Data Source={databasePath}");

        try
        {
            await using (MetaTableDbContext context = factory.CreateDbContext())
            {
                MetaTableStore store = new(context);

                await store.DefineTableAsync("department", "Department");
                await store.DefineColumnAsync("department", "Name", MetaColumnKind.Text, isRequired: true, sortOrder: 1);
                await store.DefineColumnAsync("department", "Code", MetaColumnKind.Text, sortOrder: 2);

                await store.DefineTableAsync("employee", "Employee");
                await store.DefineColumnAsync("employee", "Name", MetaColumnKind.Text, isRequired: true, sortOrder: 1);
                await store.DefineColumnAsync("employee", "Age", MetaColumnKind.Integer, sortOrder: 2);
                await store.DefineColumnAsync(
                    "employee",
                    "Department",
                    MetaColumnKind.Association,
                    isRequired: true,
                    sortOrder: 3,
                    associationTargetTableKey: "department");

                MetaRowEntity departmentRow = await store.InsertRowAsync(
                    "department",
                    textValues: new Dictionary<string, string?>
                    {
                        ["Name"] = "Engineering",
                        ["Code"] = "ENG",
                    },
                    integerValues: new Dictionary<string, int?>(),
                    associationValues: new Dictionary<string, Guid?>());

                MetaRowEntity employeeRow = await store.InsertRowAsync(
                    "employee",
                    textValues: new Dictionary<string, string?> { ["Name"] = "Alex" },
                    integerValues: new Dictionary<string, int?> { ["Age"] = 34 },
                    associationValues: new Dictionary<string, Guid?> { ["Department"] = departmentRow.Id });

                MetaRowSnapshot? snapshot = await store.GetRowSnapshotAsync(employeeRow.Id);
                MetaTableDefinition? employeeDefinition = await store.GetTableDefinitionAsync("employee");

                Assert.NotNull(snapshot);
                Assert.NotNull(employeeDefinition);
                Assert.Equal("Alex", snapshot.TextValues["Name"]);
                Assert.Equal(34, snapshot.IntegerValues["Age"]);
                Assert.Equal(departmentRow.Id, snapshot.AssociationValues["Department"]);
                Assert.Equal(3, employeeDefinition.Columns.Count);
                Assert.Contains(
                    employeeDefinition.Columns,
                    column => column.Name == "Department" && column.Kind == MetaColumnKind.Association);
            }
        }
        finally
        {
            if (File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
