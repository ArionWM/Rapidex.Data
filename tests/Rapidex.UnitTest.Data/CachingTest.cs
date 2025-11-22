using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.UnitTest.Data.Helpers;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data;

public class CachedLoadingTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public CachedLoadingTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {

    }

    [Fact]
    public void CachedLoading01()
    {
        TestCache.CacheEnabled = true;
        try
        {
            var db = Database.Dbs.Db();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work = db.BeginWork();

            var cent01 = work.New<ConcreteEntity01>();
            cent01.Name = "Name 01";
            cent01.Save();

            work.CommitChanges(); //<- After commit, already saved to cache

            long id = cent01.Id;

            var cent01_02 = db.Find<ConcreteEntity01>(id); //<- Load to cache

            Assert.NotNull(cent01_02);
            Assert.Equal(LoadSource.Cache, cent01_02._loadSource);


        }
        finally
        {
            TestCache.CacheEnabled = false;
        }


    }



}
