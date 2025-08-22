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

            var db = Database.Scopes.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            Assert.True(entity._IsNew);
            Assert.NotNull(entity.CreditLimit1); //IDataType türünden ise boş değer içeren bir nesne atanmış olmalı
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Picture);

            entity.Save();

            await db.CommitOrApplyChanges();

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
            var db = Database.Scopes.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            for (int i = 0; i < 10; i++)
            {
                ConcreteEntity01 entity = db.New<ConcreteEntity01>();
                entity.Name = $"Entity Name {i:000}";
                entity.Save();
            }

            await db.CommitOrApplyChanges();

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

            var db = Database.Scopes.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            entity.Id = 5;
            //??? Assert.True(entity.Id < 0); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            entity.Save();

            Assert.Equal(5, entity.Id);

            db.CommitOrApplyChanges();
        }


        [Fact]
        public virtual async Task Crud_06_Update_Concrete()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            ConcreteEntity01 entity = db.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            entity.TestIDataTypeAssignments();

            string name = RandomHelper.RandomText(10);

            entity.Name = name;
            entity.Save();
            await db.CommitOrApplyChanges();

            long id = entity.Id;

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity = await db.Find<ConcreteEntity01>(id);
            loadedEntity.TestIDataTypeAssignments();

            Assert.False(loadedEntity._IsNew);
            Assert.NotNull(loadedEntity);
            Assert.Equal(name, loadedEntity.Name);

            string name2 = RandomHelper.RandomText(10);
            loadedEntity.Name = name2;
            loadedEntity.Save();
            await db.CommitOrApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity2 = await db.Find<ConcreteEntity01>(id);
            Assert.NotNull(loadedEntity2);
            loadedEntity2.TestIDataTypeAssignments();
            Assert.Equal(name2, loadedEntity2.Name);

        }

        [Fact]
        public virtual async Task Crud_06_Update_Json()
        {
            this.Fixture.ClearCaches();

            var db = Database.Scopes.AddMainDbIfNotExists();
            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity03.base.json");

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();
            var ems1 = db.Metadata.AddJson(content);

            db.Structure.DropEntity("ConcreteEntity01");
            db.Structure.DropEntity(ems1.First());

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();
            db.Structure.ApplyEntityStructure(ems1.First());

            IEntity entity = db.New("myJsonEntity03");
            entity.TestIDataTypeAssignments();

            Assert.True((long)entity.GetId() < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            string name = RandomHelper.RandomText(10);

            entity["Subject"] = name;
            entity.Save();
            await db.CommitOrApplyChanges();

            long id = (long)entity.GetId();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity = await db.Find("myJsonEntity03", id);
            Assert.NotNull(loadedEntity);
            Assert.True((string)loadedEntity["Subject"] == name);

            loadedEntity.TestIDataTypeAssignments();

            string name2 = RandomHelper.RandomText(10);
            loadedEntity["Subject"] = name2;
            loadedEntity.Save();
            await db.CommitOrApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity2 = await db.Find("myJsonEntity03", id);
            loadedEntity2.TestIDataTypeAssignments();

            Assert.NotNull(loadedEntity2);
            Assert.Equal(name2, (string)loadedEntity2["Subject"]);
        }

        [Fact]
        public virtual async Task Crud_07_SimpleDelete()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();

            db.Structure.DropEntity<ConcreteEntity01>();

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();

            ConcreteEntity01 entity01 = db.New<ConcreteEntity01>();
            entity01.Name = "Entity 001";
            entity01.Save();

            ConcreteEntity01 entity02 = db.New<ConcreteEntity01>();
            entity02.Name = "Entity 002";
            entity02.Save();

            await db.CommitOrApplyChanges();

            var entCount = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            db.Delete(entity01);

            //Henüz commit olmadı, veritabanında duruyor
            entCount = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            await db.CommitOrApplyChanges();

            entCount = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, entCount);

            db.Delete(entity02);
            await db.CommitOrApplyChanges();

            entCount = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, entCount);


        }

        [Fact]
        public virtual async Task Crud_08_Transaction_Basics()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();

            db.ReAddReCreate<ConcreteEntity01>();
            db.ReAddReCreate<ConcreteEntity02>();

            long count = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, count);

            ConcreteEntity01 ent1 = db.New<ConcreteEntity01>();
            ent1.Name = "Ent 1";
            ent1.Save();

            await db.CommitOrApplyChanges();
            count = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, count);

            db.Begin();

            ConcreteEntity01 ent2 = db.New<ConcreteEntity01>();
            ent2.Name = "Ent 2";
            ent2.Save();

            await db.Rollback();

            count = await db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, count);
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
