using Rapidex.Data.Entities;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class RelationsTestBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        public RelationsTestBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {
        }

        [Fact]
        public virtual async Task One2N_01_Concrete()
        {
            //MasterA/DetailB İki master ve her bir master'a 3'er detail eklenir.
            //Cache temizlenir
            //MasterA ve MasterB için detaylar alınarak kontrol edilir.

            //this.Fixture.ClearCaches();
            var database = Database.Scopes.AddMainDbIfNotExists();

            Database.Metadata.AddIfNotExist<ConcreteEntity03>();
            Database.Metadata.AddIfNotExist<ConcreteEntity04>();

            var dbScope = Database.Scopes.Db();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntity03>();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntity04>();

            var createData = (int scopeId) =>
            {
                ConcreteEntity04 masterEntity = dbScope.New<ConcreteEntity04>();
                masterEntity.Number = RandomHelper.Random(100000);
                masterEntity.Save();

                List<ConcreteEntity03> details = new List<ConcreteEntity03>();
                for (int i = 0; i < 3; i++)
                {
                    ConcreteEntity03 detailEntity = dbScope.New<ConcreteEntity03>();
                    detailEntity.Name = "Detail " + scopeId.ToString("00") + " " + i.ToString("00");
                    detailEntity.Save();

                    details.Add(detailEntity);
                }

                masterEntity.Details01.Add(details);

                return (masterEntity, details);
            };

            var (masterEntity01, details01) = createData(1);
            var (masterEntity02, details02) = createData(2);

            await dbScope.CommitOrApplyChanges();

            //Clear caches


            var ent01Details = masterEntity01.Details01.GetContent();

            Assert.Equal(3, ent01Details.ItemCount);
            Assert.Equal("Detail 01 00", ent01Details[0].Name);
            Assert.Equal("Detail 01 01", ent01Details[1].Name);
            Assert.Equal("Detail 01 02", ent01Details[2].Name);

            var ent02Details = masterEntity02.Details01.GetContent();
            Assert.Equal(3, ent02Details.ItemCount);
            Assert.Equal("Detail 02 00", ent02Details[0].Name);
            Assert.Equal("Detail 02 01", ent02Details[1].Name);
            Assert.Equal("Detail 02 02", ent02Details[2].Name);
        }

        //[Fact]
        //public virtual void One2N_02_ABC_Json()
        //{
        //    //MasterA/DetailB İki master ve her bir master'a 3'er detail eklenir.
        //    //Cache temizlenir
        //    //MasterA ve MasterB için detaylar alınarak kontrol edilir.
        //}

        //[Fact]
        //public virtual void One2N_02_ABC_Mixed()
        //{
        //    //MasterA (Json) /DetailB (Concrete) İki master ve her bir master'a 3'er detail eklenir.
        //    //Cache temizlenir
        //    //MasterA ve MasterB için detaylar alınarak kontrol edilir.
        //}

        ////[Fact]
        //public virtual void One2N_02_ABC_CyclicDependency()
        //{
        //    //EntityA.Prop = EntityB, EntityB.Prop = EntityA
        //    //Bunun master / detail olani
        //}



        //[Fact]
        //public virtual void Reference_0X_ABC_CyclicDependency()
        //{
        //    //EntityA.Prop = EntityB, EntityB.Prop = EntityA
        //}


        [Fact]
        public virtual async Task N2N_01_Concrete()
        {
            var database = Database.Scopes.AddMainDbIfNotExists();

            Database.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();
            Database.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();

            var dbScope = Database.Scopes.Db();
            dbScope.Structure.DropEntity<ConcreteEntityForN2NTest02>();
            dbScope.Structure.DropEntity<ConcreteEntityForN2NTest01>();
            dbScope.Structure.DropEntity<GenericJunction>();

            dbScope.Structure.ApplyEntityStructure<GenericJunction>();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest02>();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntityForN2NTest01>();

            var relatedEntity01 = dbScope.New<ConcreteEntityForN2NTest02>();
            relatedEntity01.Name = "Related Entity 01";
            relatedEntity01.Save();

            var relatedEntity02 = dbScope.New<ConcreteEntityForN2NTest02>();
            relatedEntity02.Name = "Related Entity 02";
            relatedEntity02.Save();


            var entity01 = dbScope.New<ConcreteEntityForN2NTest01>();
            entity01.Name = "Entity 01";
            entity01.Save();

            entity01.Relation01.Add(relatedEntity01);
            entity01.Relation01.Add(relatedEntity02);
            entity01.Save();

            var entity02 = dbScope.New<ConcreteEntityForN2NTest01>();
            entity02.Name = "Entity 02";
            entity02.Save();

            await dbScope.CommitOrApplyChanges();

            long entity1Id = entity01.Id;
            long entity2Id = entity02.Id;

            //Clear cache

            var entity1 = await dbScope.Find<ConcreteEntityForN2NTest01>(entity1Id);
            var relations01 = entity1.Relation01.GetContent();
            Assert.Equal(2, relations01.ItemCount);

            var entity2 = await dbScope.Find<ConcreteEntityForN2NTest01>(entity2Id);
            var relations02 = entity2.Relation01.GetContent();
            Assert.Empty(relations02);




        }

        [Fact]
        public virtual async Task N2N_02_Json()
        {

            var database = Database.Scopes.AddMainDbIfNotExists();

            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity10.forN2N.json");
            var em1 = Database.Metadata.AddFromJson(content);
            content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity11.forN2N.json");
            var em2 = Database.Metadata.AddFromJson(content);

            var dbScope = Database.Scopes.Db();
            dbScope.Structure.DropEntity("myJsonEntity10");
            dbScope.Structure.DropEntity("myJsonEntity11");
            dbScope.Structure.DropEntity<GenericJunction>();

            dbScope.Structure.ApplyEntityStructure<GenericJunction>();
            dbScope.Structure.ApplyEntityStructure(em1);
            dbScope.Structure.ApplyEntityStructure(em2);

            var relatedEntity01 = dbScope.New("myJsonEntity10");
            relatedEntity01["Name"] = "Related Entity 01";
            relatedEntity01.Save();

            var relatedEntity02 = dbScope.New("myJsonEntity10");
            relatedEntity02["Name"] = "Related Entity 02";
            relatedEntity02.Save();


            var entity01 = dbScope.New("myJsonEntity11");
            entity01["Name"] = "Entity 01";
            entity01.Save();

            ((RelationN2N)entity01["Relation01"]).Add(relatedEntity01);
            ((RelationN2N)entity01["Relation01"]).Add(relatedEntity02);
            entity01.Save();

            var entity02 = dbScope.New("myJsonEntity11");
            entity02["Name"] = "Entity 02";
            entity02.Save();

            await dbScope.CommitOrApplyChanges();

            long entity1Id = entity01.GetId().As<long>();
            long entity2Id = entity02.GetId().As<long>();

            //Clear cache

            var entity1 = dbScope.Find("myJsonEntity11", entity1Id);
            var relations01 = (IEntityLoadResult)((RelationN2N)entity01["Relation01"]).GetContent();
            Assert.Equal(2, relations01.ItemCount);

            var entity2 = dbScope.Find("myJsonEntity11", entity2Id);
            var relations02 = (IEntityLoadResult)((RelationN2N)entity02["Relation01"]).GetContent();
            Assert.Empty(relations02);



        }

        //[Fact]
        //public virtual void N2N_03_SelfRelation()
        //{


        //}
    }
}
