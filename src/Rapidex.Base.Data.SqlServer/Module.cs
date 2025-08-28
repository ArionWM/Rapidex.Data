using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Data.SqlServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]

namespace Rapidex.Data.SqlServer;

internal class Module : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library SqlServer extensions";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IExceptionTranslator, DbSqlServerExceptionTranslator>();
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }

    internal static DbSqlServerConnection CreateSqlServerConnection()
    {
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);
        DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        connection.Execute($"USE [{Database.Dbs.Db().DatabaseName}]");
        return connection;
    }
}
