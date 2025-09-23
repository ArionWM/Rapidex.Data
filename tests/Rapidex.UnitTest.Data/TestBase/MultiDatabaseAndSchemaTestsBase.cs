using Rapidex.Data.Entities;
using Rapidex.Data.Scopes;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
public abstract class MultiDatabaseAndSchemaTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
{
    public MultiDatabaseAndSchemaTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
    {
    }

    [Fact]
    public virtual void T01_MultipleSchemas()
    {
        DbConnectionInfo connectionInfo = Database.Configuration.ConnectionInfo.Get(DatabaseConstants.MASTER_DB_ALIAS_NAME);
        DbProviderFactory DbProviderFactory = new DbProviderFactory();
        IDbProvider provider = DbProviderFactory.CreateProvider(connectionInfo);

        this.Fixture.DropAllSchemasInDatabase(provider, true);

        var db = Database.Dbs.AddMainDbIfNotExists();
        db.ReAddReCreate<SchemaInfo>();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.ApplyAllStructure();

        string newSchemaName1 = "Schema" + RandomHelper.RandomText(5);

        var schema1 = db.AddSchemaIfNotExists(newSchemaName1);
        schema1.Structure.ApplyAllStructure();


        var workOnTestSchema = schema1.BeginWork();
        ConcreteEntity01 ce1 = workOnTestSchema.New<ConcreteEntity01>();
        ce1.Name = RandomHelper.RandomText(10);
        ce1.Save();

        workOnTestSchema.CommitChanges();

        long countOnTestSchema = workOnTestSchema.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(1, countOnTestSchema);

        var workOnBaseSchema = db.BeginWork();
        long countOnBaseSchema = workOnBaseSchema.GetQuery<ConcreteEntity01>().Count();
        Assert.Equal(0, countOnBaseSchema);

    }

    protected virtual string GetRandomDbName()
    {
        return "TestDb" + RandomHelper.RandomText(5);
    }

    [Fact]
    public virtual void T01_MultipleDatabases()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();


        string dbName1 = this.GetRandomDbName();

        var newDb1 = Database.Dbs.AddDbIfNotExists(2, dbName1);
        newDb1.Structure.ApplyAllStructure();

        newDb1.Structure.DestroyDatabase(newDb1.DatabaseName);
    }

}
