using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class AggregationTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        protected AggregationTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {
        }


        protected async Task GenerateEntities()
        {
            var db = Database.Scopes.Db();
            db.Metadata.AddIfNotExist<AggrTestEntity01>();

            db.Structure.DropEntity<AggrTestEntity01>();

            db.Structure.ApplyEntityStructure<AggrTestEntity01>();

            //100 entity
            //"No" birer artıyor (1'den başlayarak)
            //"Age" 20'den başlayarak 1'er artıyor
            //Name: "Entity Name 001" şeklinde
            //Amount: i % 10
            //IsActive: i % 2 == 0
            //BirthDate: 01.01.2000 + i gün


            for (int i = 0; i < 100; i++)
            {
                AggrTestEntity01 entity = db.New<AggrTestEntity01>();
                entity.No = i + 1;
                entity.Age = 20 + i;
                entity.Name = $"Entity Name {i + 1:000}";
                entity.Value = 10000 * (i + 1);
                entity.Amount = i % 10;
                entity.BirthDate = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(i);

                entity.Save();
            }

            await db.CommitOrApplyChanges();



        }

        [Fact]
        public virtual async Task Aggr_01_Test01()
        {
            await this.GenerateEntities();

            var db = Database.Scopes.Db();

            float val01 = await db.GetQuery<AggrTestEntity01>().Avg<float>(nameof(AggrTestEntity01.Amount));
            Assert.Equal(4.5f, val01);

            double val02 = await db.GetQuery<AggrTestEntity01>().Sum<double>(nameof(AggrTestEntity01.Value));
            Assert.Equal(50500000, val02);

            long val03 = await db.GetQuery<AggrTestEntity01>().Count();
            Assert.Equal(100, val03);

            DateTimeOffset val04 = await db.GetQuery<AggrTestEntity01>().Min<DateTimeOffset>(nameof(AggrTestEntity01.BirthDate));
            Assert.Equal(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), val04);

            DateTimeOffset val05 = await db.GetQuery<AggrTestEntity01>().Max<DateTimeOffset>(nameof(AggrTestEntity01.BirthDate));
            Assert.Equal(new DateTimeOffset(2000, 4, 9, 0, 0, 0, TimeSpan.Zero), val05);

            float val06 = await db.GetQuery<AggrTestEntity01>()
                .Gt(nameof(AggrTestEntity01.Age), 50)
                .Avg<float>(nameof(AggrTestEntity01.Amount));
            Assert.Equal(4.5f, val01);





        }
    }
}
