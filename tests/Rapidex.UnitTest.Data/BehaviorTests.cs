using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.Behaviors;

public class BehaviorTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public class TestBehavior1 : EntityBehaviorBase<TestBehavior1>
    {
        public override IUpdateResult SetupMetadata(IDbEntityMetadata em)
        {
            return new UpdateResult();
        }
    }

    public BehaviorTests( SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    //Json entity behavior
    //Add behavior with name

    //Add behaviors from concrete metadata descriptor
    //Add behaviors from json metadata descriptor

    //Remove behavior
    //Remove sealed behavior

    //Behavior specific tests

    [Fact]
    public void Behaviors_01_Add()
    {

        var dbScope = Database.Dbs.Db();

        var em = dbScope.ParentDbScope.Metadata.ReAdd<ConcreteEntityWithoutBehavior01>() as IDbEntityMetadata;

        em.AddBehavior<TestBehavior1>(false, true);
        em.AddBehavior<TestBehavior1>(false, true);

        Assert.Equal(1, em.BehaviorDefinitions.Count);

    }


    [Fact]
    public void Behaviors_02_BasicAddFields()
    {
        //this.Fixture.ClearCaches();

        var dbScope = Database.Dbs.Db();

        IDbEntityMetadata bem = dbScope.ReAddReCreate<ConcreteEntityWithoutBehavior01>();

        Assert.Equal(0, bem.BehaviorDefinitions.Count);


        //Check table
        DbConnectionInfo dbc = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        SqlConnectionStringBuilder sqlConnectionStringBuilder = new SqlConnectionStringBuilder(dbc.ConnectionString);
        using DbSqlServerConnection connection = new DbSqlServerConnection(sqlConnectionStringBuilder.ConnectionString);

        connection.Execute($"USE [{Database.Dbs.Db().DatabaseName}]");

        DataTable table = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_ConcreteEntityWithoutBehavior01'");
        Assert.Equal(1, table.Rows.Count);


        bem.AddBehavior<ArchiveEntity>(false, true);
        Assert.Equal(1, bem.BehaviorDefinitions.Count);

        dbScope.Structure.ApplyEntityStructure(bem);


        table = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_ConcreteEntityWithoutBehavior01'");
        Assert.Equal(1, table.Rows.Count);

        DataTable columnsTable = connection.Execute($"SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = 'base' AND TABLE_NAME = 'utest_ConcreteEntityWithoutBehavior01'");
        DataView dv = new DataView(columnsTable);

        dv.RowFilter = "COLUMN_NAME = 'IsArchived'";

        Assert.Single(dv);
        Assert.Equal("YES", dv[0]["IS_NULLABLE"]);
        Assert.Equal("bit", dv[0]["DATA_TYPE"]);
    }

}




