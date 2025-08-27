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

            db.ApplyChanges();

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

            db.ApplyChanges();

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

            db.ApplyChanges();
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
            entity.CheckIDataTypeAssignments();

            string name = RandomHelper.RandomText(10);

            entity.Name = name;
            entity.Save();
            db.ApplyChanges();

            long id = entity.Id;

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity = db.Find<ConcreteEntity01>(id);
            loadedEntity.CheckIDataTypeAssignments();

            Assert.False(loadedEntity._IsNew);
            Assert.NotNull(loadedEntity);
            Assert.Equal(name, loadedEntity.Name);

            string name2 = RandomHelper.RandomText(10);
            loadedEntity.Name = name2;
            loadedEntity.Save();
            db.ApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity2 = db.Find<ConcreteEntity01>(id);
            Assert.NotNull(loadedEntity2);
            loadedEntity2.CheckIDataTypeAssignments();
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
            entity.CheckIDataTypeAssignments();

            Assert.True((long)entity.GetId() < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            string name = RandomHelper.RandomText(10);

            entity["Subject"] = name;
            entity.Save();
            db.ApplyChanges();

            long id = (long)entity.GetId();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity = db.Find("myJsonEntity03", id);
            Assert.NotNull(loadedEntity);
            Assert.True((string)loadedEntity["Subject"] == name);

            loadedEntity.CheckIDataTypeAssignments();

            string name2 = RandomHelper.RandomText(10);
            loadedEntity["Subject"] = name2;
            loadedEntity.Save();
            db.ApplyChanges();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity2 = db.Find("myJsonEntity03", id);
            loadedEntity2.CheckIDataTypeAssignments();

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

            db.ApplyChanges();

            var entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            db.Delete(entity01);

            //Henüz commit olmadı, veritabanında duruyor
            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            db.ApplyChanges();

            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, entCount);

            db.Delete(entity02);
            db.ApplyChanges();

            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, entCount);


        }

        [Fact]
        public virtual async Task Crud_08_Transaction_Basics()
        {
            var db = Database.Scopes.AddMainDbIfNotExists();

            db.ReAddReCreate<ConcreteEntity01>();
            db.ReAddReCreate<ConcreteEntity02>();

            long count = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, count);

            ConcreteEntity01 ent1 = db.New<ConcreteEntity01>();
            ent1.Name = "Ent 1";
            ent1.Save();

            db.ApplyChanges();
            count = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, count);

            var tran1 = db.Begin();

            ConcreteEntity01 ent2 = db.New<ConcreteEntity01>();
            ent2.Name = "Ent 2";
            ent2.Save();

            tran1.Rollback();

            count = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, count);

            using (var tran2 = db.Begin())
            {
                ConcreteEntity01 ent3 = db.New<ConcreteEntity01>();
                ent3.Name = "Ent 3";
                ent3.Save();
            }

            count = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, count);
        }



    }
}
