using Rapidex.Data.PostgreServer;

namespace Rapidex.UnitTest.Data.PostgreServer;

public class PostgreServer_01_DatabaseStructureTests : DatabaseStructureTestsBase<PostgreSqlServerProvider>
{
    public PostgreServer_01_DatabaseStructureTests( SingletonFixtureFactory<DbWithProviderFixture<PostgreSqlServerProvider>> factory) : base(factory)
    {
    }

    public override void Structure_01_MasterAndBaseSchema()
    {
        base.Structure_01_MasterAndBaseSchema();

        //using PostgreSqlServerConnection connection = Rapidex.UnitTest.Data.PostgreServer.Library.CreatePostgreServerConnection();

        //DataTable table = connection.Execute($"SELECT SCHEMA_ID('base') AS SchemaId");
        //Assert.Equal(1, table.Rows.Count);

    }
}
