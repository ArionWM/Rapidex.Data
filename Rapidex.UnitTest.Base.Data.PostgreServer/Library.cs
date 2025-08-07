

global using Microsoft.Data.SqlClient;
global using Rapidex.Data;
global using Rapidex;
global using Rapidex.UnitTests;
global using Rapidex.UnitTest.Data.Fixtures;
global using Rapidex.UnitTest.Data.TestBase;
global using System;
global using System.Collections.Generic;
global using System.Data;
global using System.Linq;
global using System.Text;
global using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using Rapidex.Data.PostgreServer;
using System.Runtime.CompilerServices;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]
[assembly: InternalsVisibleTo("Rapidex.Base.Application.Common")]

namespace Rapidex.UnitTest.Data.PostgreServer;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "UnitTest.Data.PostgreServer";
    public override string TablePrefix => "utest";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        Rapidex.Common.EnviromentCode = CommonConstants.ENV_UNITTEST;
    }

    public override void SetupMetadata(IServiceCollection services)
    {
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }

    internal static PostgreSqlServerConnection CreatePostgreServerConnection()
    {
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        NpgsqlConnectionStringBuilder sqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder(dbc.ConnectionString);
        sqlConnectionStringBuilder.Database = Database.Scopes.Db().DatabaseName;
        PostgreSqlServerConnection connection = new PostgreSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        //connection.Execute($"USE [{Database.Scopes.Db().DatabaseName}]");
        return connection;
    }
}
