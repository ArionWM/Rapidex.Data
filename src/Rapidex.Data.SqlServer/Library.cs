using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Data.SqlServer")]
[assembly: InternalsVisibleTo("Rapidex.UnitTest.Base.Application")]

namespace Rapidex.Data.SqlServer;

internal class Library : AssemblyDefinitionBase, IRapidexAssemblyDefinition
{
    public override string Name => "Data / Orm Library SqlServer extensions";
    public override string TablePrefix => "data";
    public override int Index => 1000;

    public override void SetupServices(IServiceCollection services)
    {
        services.AddSingletonForProd<IExceptionTranslator, DbSqlServerExceptionTranslator>();
        services.AddKeyedTransient<IDbProvider, DbSqlServerProvider>("DbSqlServerProvider");
        services.AddKeyedTransient<IDbProvider, DbSqlServerProvider>("Rapidex.Data.SqlServer.DbSqlServerProvider");
        services.AddKeyedTransient<IDbStructureProvider, DbSqlStructureProvider>("mssql");
    }

    public override void Initialize(IServiceProvider serviceProvider)
    {
    }

    public override void Start(IServiceProvider serviceProvider)
    {

    }

    /// <summary>
    /// For tests and tools only.
    /// </summary>
    /// <returns></returns>
    internal static DbSqlServerConnection CreateSqlServerConnection()
    {
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);
        DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        connection.Execute($"USE [{Database.Dbs.Db().DatabaseName}]");
        return connection;
    }
}
