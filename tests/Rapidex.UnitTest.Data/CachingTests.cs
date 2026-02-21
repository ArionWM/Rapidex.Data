using Rapidex.Data;
using Rapidex.UnitTest.Data.Helpers;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data;

public class CachedLoadingTests : DbDependedTestsBase<DbSqlServerProvider>
{
    ICache cache;

    public CachedLoadingTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
        this.cache = this.Fixture.ServiceProvider.GetRequiredService<ICache>()
             .ShouldSupportTo<TestCache>();
    }



    [Fact]
    public void CacheKeys()
    {
        var db = Database.Dbs.Db();
        var em = db.Metadata.Get<ConcreteEntity01>();
        DateTimeOffset start = new DateTimeOffset(2000, 01, 01, 01, 01, 01, TimeSpan.Zero);

        var query1 = db.GetQuery<ConcreteEntity01>()
            .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5));

        var query2 = db.GetQuery<ConcreteEntity01>()
            .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5));


        var dmp = db.DbProvider.GetDataModificationProvider();
        var compiler = dmp.GetCompiler();

        var resut1 = compiler.Compile(query1.Query);
        var resut2 = compiler.Compile(query2.Query);


        string key1 = CacheExtensions.GetQueryCacheKey(em, db, resut1);
        string key2 = CacheExtensions.GetQueryCacheKey(em, db, resut2);

        Assert.Equal(key1, key2);

    }


    protected void Common_IdCache01()
    {
        var db = Database.Dbs.Db();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        var em = db.Metadata.Get<ConcreteEntity01>();
        em.CacheOptions.IsIdCacheEnabled = true;
        em.CacheOptions.IsQueryCacheEnabled = false;

        using var work = db.BeginWork();

        var cent01 = work.New<ConcreteEntity01>();
        cent01.Name = "Name 01";
        cent01.Save();

        work.CommitChanges(); //<- After commit, already saved to cache

        long id = cent01.Id;

        var cent01_02 = db.Find<ConcreteEntity01>(id); //<- Load to cache

        Assert.NotNull(cent01_02);
        Assert.Equal(LoadSource.Cache, cent01_02._loadSource);
        Assert.Equal(0, cent01_02.DbVersion);

        using var work2 = db.BeginWork();
        cent01.Name = "Name 01 - 1";
        cent01.Save();

        work2.CommitChanges(); //<- After commit, already saved to cache

        Task.Delay(100).Wait(); //<- Wait a bit to ensure cache update

        var cent01_03 = db.Find<ConcreteEntity01>(id); //<- Load from cache
        Assert.NotNull(cent01_03);
        Assert.Equal(LoadSource.Cache, cent01_03._loadSource);
        Assert.Equal(1, cent01_03.DbVersion);
    }


    protected void Common_QueryCache01()
    {
        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity01>();
        var em = db.Metadata.Get<ConcreteEntity01>();

        em.CacheOptions.IsIdCacheEnabled = true;
        em.CacheOptions.IsQueryCacheEnabled = true;

        try
        {
            using var work = db.BeginWork();

            DateTimeOffset start = new DateTimeOffset(2000, 02, 01, 01, 01, 01, TimeSpan.Zero);

            for (int i = 1; i <= 10; i++)
            {
                var cent01 = work.New<ConcreteEntity01>();
                cent01.Name = $"Name {i:00}";
                cent01.BirthDate = start.AddMonths(i);
                cent01.Save();
            }

            work.CommitChanges(); //<- After commit, already saved to cache



            var result1 = db.GetQuery<ConcreteEntity01>()
                 .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5))
                 .Load();

            Assert.Equal(6, result1.Count);
            Assert.Equal(LoadSource.Database, result1[0]._loadSource);



            var resultWithCache2 = db.GetQuery<ConcreteEntity01>()
                 .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5))
                 .Load();

            Assert.Equal(6, resultWithCache2.Count);
            Assert.Equal(LoadSource.Cache, resultWithCache2[0]._loadSource);


            var resultWithoutCache2 = db.GetQuery<ConcreteEntity01>()
                 .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(6))
                 .Load();

            Assert.Equal(5, resultWithoutCache2.Count);
            Assert.Equal(LoadSource.Database, resultWithoutCache2[0]._loadSource);
        }
        finally
        {
            em.CacheOptions.IsIdCacheEnabled = true;
            em.CacheOptions.IsQueryCacheEnabled = false;
        }
    }




    protected void Commmon_QueryCache02()
    {
        var db = Database.Dbs.Db();
        db.ReAddReCreate<ConcreteEntity01>();
        var em = db.Metadata.Get<ConcreteEntity01>();

        em.CacheOptions.IsIdCacheEnabled = true;
        em.CacheOptions.IsQueryCacheEnabled = false;


        DateTimeOffset start = new DateTimeOffset(2000, 01, 01, 01, 01, 01, TimeSpan.Zero);

        using var work = db.BeginWork();
        for (int i = 1; i <= 10; i++)
        {
            var cent01 = work.New<ConcreteEntity01>();
            cent01.Name = $"Name {i:00}";
            cent01.BirthDate = start.AddMonths(i);
            cent01.Save();
        }
        work.CommitChanges(); //<- After commit, not yet saved to query cache (IsQueryCacheEnabled: false)

        var result1 = db.GetQuery<ConcreteEntity01>()
             .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5))
             .UseQueryCache()
             .Load();

        Assert.Equal(6, result1.Count);
        Assert.Equal(LoadSource.Database, result1[0]._loadSource);



        var resultWithCache2 = db.GetQuery<ConcreteEntity01>()
             .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5))
             .UseQueryCache()
             .Load();

        Assert.Equal(6, resultWithCache2.Count);
        Assert.Equal(LoadSource.Cache, resultWithCache2[0]._loadSource);

        var resultWithCache3 = db.GetQuery<ConcreteEntity01>()
            .GtEq(nameof(ConcreteEntity01.BirthDate), start.AddMonths(5))
            .Load(); //<- Not use query cache (IsQueryCacheEnabled: false)

        Assert.Equal(6, resultWithCache3.Count);
        Assert.Equal(LoadSource.Database, resultWithCache3[0]._loadSource);
    }

    [Fact]
    public void InMemory_IdCache01()
    {
        TestCache.MemoryCacheEnabled = true;
        try
        {
            this.Common_IdCache01();

        }
        finally
        {
            TestCache.MemoryCacheEnabled = false;
        }
    }

    [Fact]
    public void Hybrid_IdCache01()
    {
        return;
        TestCache.HybridCacheEnabled = true;
        cache.RemoveByTag("test");
        CacheExtensions.SetTagContext(null, "test");
        try
        {
            this.Common_IdCache01();

        }
        finally
        {
            cache.RemoveByTag("test");
            TestCache.HybridCacheEnabled = false;
            CacheExtensions.ClearTagContext(null);
        }
    }


    [Fact]
    public void InMemory_QueryCache01()
    {


        TestCache.MemoryCacheEnabled = true;
        try
        {
            this.Common_QueryCache01();
        }
        finally
        {
            TestCache.MemoryCacheEnabled = false;
        }

    }

    [Fact]
    public void Hybrid_QueryCache01()
    {
        return;
        CacheExtensions.SetTagContext(null, "test");
        TestCache.HybridCacheEnabled = true;
        cache.RemoveByTag("test");
        try
        {
            this.Common_QueryCache01();
        }
        finally
        {
            cache.RemoveByTag("test");
            TestCache.HybridCacheEnabled = false;
            CacheExtensions.ClearTagContext(null);
        }

    }

    [Fact]
    public void InMemory_QueryCache02()
    {
        TestCache.MemoryCacheEnabled = true;
        try
        {
            this.Commmon_QueryCache02();
        }
        finally
        {
            TestCache.MemoryCacheEnabled = false;
        }
    }

    [Fact]
    public void Hybrid_QueryCache02()
    {
        return;
        CacheExtensions.SetTagContext(null, "test");
        TestCache.HybridCacheEnabled = true;
        cache.RemoveByTag("test");
        try
        {
            this.Commmon_QueryCache02();
        }
        finally
        {
            cache.RemoveByTag("test");
            TestCache.HybridCacheEnabled = false;
            CacheExtensions.ClearTagContext(null);
        }


    }
}
