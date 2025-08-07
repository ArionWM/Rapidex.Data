using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase
{
    public abstract class CrudTestsBase<T> : DbDependedTestsBase<T> where T : IDbProvider
    {
        protected CrudTestsBase(SingletonFixtureFactory<DbWithProviderFixture<T>> factory) : base(factory)
        {
        }

        //Delete tests: basic delete, delete with criteria, update and delete in same scope, delete related entity

        [Fact]
        public virtual async Task Crud_01_Insert_Concrete()
        {
            this.Fixture.ClearCaches();

            var dbScope = Database.Scopes.AddMainDbIfNotExists();
            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            dbScope.Structure.DropEntity("ConcreteEntity01");

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            Assert.True(entity._IsNew);
            Assert.NotNull(entity.CreditLimit1); //IDataType türünden ise boş değer içeren bir nesne atanmış olmalı
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Picture);

            entity.Save();

            await dbScope.CommitOrApplyChanges();

            Assert.True(entity.Id > 9999); //Yeni entity'ler 10 k dan büyük bir Id alır
            Assert.False(entity._IsNew);

            Assert.NotNull(entity.CreditLimit1); //IDataType türünden ise boş değer içeren bir nesne atanmış olmalı
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Picture);

            //var entity2 = dbScope.Data.Select<TestEntity>(1);

            //Assert.True(entity2.Name == entity.Name);
            //Assert.True(entity2.Description == entity.Description);
        }

        [Fact]
        public virtual async Task Crud_02_Insert_Multiple_Concrete()
        {
            this.Fixture.ClearCaches();
            var dbScope = Database.Scopes.AddMainDbIfNotExists();

            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            dbScope.Structure.DropEntity("ConcreteEntity01");

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

            for (int i = 0; i < 10; i++)
            {
                ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
                entity.Name = $"Entity Name {i:000}";
                entity.Save();
            }

            await dbScope.CommitOrApplyChanges();

            //Check table
        }

        //[Fact]
        //public virtual void Crud_03_Insert_Multiple_Json()
        //{
        //    throw new NotImplementedException();
        //}

        //[Fact]
        //public virtual void Crud_04_Insert_Json()
        //{
        //    throw new NotImplementedException();
        //}

        [Fact]
        public virtual void Crud_05_ManualInsertsCanUseIdsAsBelow10K_Concrete()
        {
            this.Fixture.ClearCaches();
            //Elle bir entity'nin Id'si 10k altında verilebilir (ön tanımlı kayıtlar bu şekilde çalışır)

            var dbScope = Database.Scopes.AddMainDbIfNotExists();
            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            dbScope.Structure.DropEntity("ConcreteEntity01");

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
            entity.Id = 5;
            //??? Assert.True(entity.Id < 0); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            entity.Save();

            Assert.Equal(5, entity.Id);
        }


        [Fact]
        public virtual async Task Crud_06_Update_Concrete()
        {
            this.Fixture.ClearCaches();

            var dbScope = Database.Scopes.AddMainDbIfNotExists();
            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            dbScope.Structure.DropEntity("ConcreteEntity01");

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = dbScope.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            entity.TestIDataTypeAssignments();

            string name = RandomHelper.RandomText(10);

            entity.Name = name;
            entity.Save();
            await dbScope.CommitOrApplyChanges();

            long id = entity.Id;

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity = await dbScope.Find<ConcreteEntity01>(id);
            loadedEntity.TestIDataTypeAssignments();

            Assert.False(loadedEntity._IsNew);
            Assert.NotNull(loadedEntity);
            Assert.Equal(name, loadedEntity.Name);

            string name2 = RandomHelper.RandomText(10);
            loadedEntity.Name = name2;
            loadedEntity.Save();
            await dbScope.CommitOrApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity2 = await dbScope.Find<ConcreteEntity01>(id);
            Assert.NotNull(loadedEntity2);
            loadedEntity2.TestIDataTypeAssignments();
            Assert.Equal(name2, loadedEntity2.Name);

        }

        [Fact]
        public virtual async Task Crud_06_Update_Json()
        {
            this.Fixture.ClearCaches();

            var dbScope = Database.Scopes.AddMainDbIfNotExists();
            string content = this.Fixture.GetFileContentAsString("TestContent\\jsonEntity03.base.json");

            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            Database.Metadata.AddIfNotExist<ConcreteEntity02>();
            var em1 = Database.Metadata.AddFromJson(content);

            dbScope.Structure.DropEntity("ConcreteEntity01");
            dbScope.Structure.DropEntity(em1);

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntity02>();
            dbScope.Structure.ApplyEntityStructure(em1);

            IEntity entity = dbScope.New("myJsonEntity03");
            entity.TestIDataTypeAssignments();

            Assert.True((long)entity.GetId() < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            string name = RandomHelper.RandomText(10);

            entity["Subject"] = name;
            entity.Save();
            await dbScope.CommitOrApplyChanges();

            long id = (long)entity.GetId();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity = await dbScope.Find("myJsonEntity03", id);
            Assert.NotNull(loadedEntity);
            Assert.True((string)loadedEntity["Subject"] == name);

            loadedEntity.TestIDataTypeAssignments();

            string name2 = RandomHelper.RandomText(10);
            loadedEntity["Subject"] = name2;
            loadedEntity.Save();
            await dbScope.CommitOrApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity2 = await dbScope.Find("myJsonEntity03", id);
            loadedEntity2.TestIDataTypeAssignments();

            Assert.NotNull(loadedEntity2);
            Assert.Equal(name2, (string)loadedEntity2["Subject"]);
        }

        [Fact]
        public virtual async Task Crud_07_SimpleDelete()
        {
            var dbScope = Database.Scopes.AddMainDbIfNotExists();

            Database.Metadata.AddIfNotExist<ConcreteEntity01>();
            Database.Metadata.AddIfNotExist<ConcreteEntity02>();

            dbScope.Structure.DropEntity<ConcreteEntity01>();

            dbScope.Structure.ApplyEntityStructure<ConcreteEntity01>();
            dbScope.Structure.ApplyEntityStructure<ConcreteEntity02>();

            ConcreteEntity01 entity01 = dbScope.New<ConcreteEntity01>();
            entity01.Name = "Entity 001";
            entity01.Save();

            ConcreteEntity01 entity02 = dbScope.New<ConcreteEntity01>();
            entity02.Name = "Entity 002";
            entity02.Save();

            await dbScope.CommitOrApplyChanges();

            var entCount = await dbScope.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            dbScope.Delete(entity01);

            //Henüz commit olmadı, veritabanında duruyor
            entCount = await dbScope.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            await dbScope.CommitOrApplyChanges();

            entCount = await dbScope.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, entCount);

            dbScope.Delete(entity02);
            await dbScope.CommitOrApplyChanges();

            entCount = await dbScope.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, entCount);


        }

        //[Fact]
        //public virtual void Crud_06_ImageFrieldsIsLazyLoaded()
        //{
        //    //Picture Values içerisinde boş olacak
        //}

        //[Fact]
        //public virtual void Crud_06_Update_Bulk()
        //{
        //}



    }
}
