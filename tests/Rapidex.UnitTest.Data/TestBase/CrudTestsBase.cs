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
        public virtual void Crud_01_Insert_Concrete()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work = db.BeginWork();

            ConcreteEntity01 entity = work.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            Assert.True(entity._IsNew);
            Assert.NotNull(entity.CreditLimit1); //IDataType türünden ise boş değer içeren bir nesne atanmış olmalı
            Assert.NotNull(entity.Description);
            Assert.NotNull(entity.Picture);

            entity.Save();

            work.CommitChanges();

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
        public virtual void Crud_02_Insert_Multiple_Concrete()
        {
            this.Fixture.ClearCaches();
            var db = Database.Dbs.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work = db.BeginWork();

            for (int i = 0; i < 10; i++)
            {
                ConcreteEntity01 entity = work.New<ConcreteEntity01>();
                entity.Name = $"Entity Name {i:000}";
                entity.Save();
            }

            work.CommitChanges();

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

            var db = Database.Dbs.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work = db.BeginWork();

            ConcreteEntity01 entity = work.New<ConcreteEntity01>();
            entity.Id = 5;
            //??? Assert.True(entity.Id < 0); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            entity.Save();

            Assert.Equal(5, entity.Id);

            work.CommitChanges();
        }


        [Fact]
        public virtual void Crud_06_Update_Concrete()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.DropEntity("ConcreteEntity01");

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work1 = db.BeginWork();

            ConcreteEntity01 entity = work1.New<ConcreteEntity01>();
            Assert.True(entity.Id < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir
            entity.CheckIDataTypeAssignments();

            string name = RandomHelper.RandomText(10);

            entity.Name = name;
            entity.Save();
            work1.CommitChanges();

            long id = entity.Id;

            //TODO: Cache'ler temizlenecek !!

            using var work2 = db.BeginWork();
            ConcreteEntity01 loadedEntity = db.Find<ConcreteEntity01>(id);
            loadedEntity.CheckIDataTypeAssignments();

            Assert.False(loadedEntity._IsNew);
            Assert.NotNull(loadedEntity);
            Assert.Equal(name, loadedEntity.Name);

            string name2 = RandomHelper.RandomText(10);
            loadedEntity.Name = name2;
            loadedEntity.Save();
            work2.CommitChanges();

            //TODO: Cache'ler temizlenecek !!

            ConcreteEntity01 loadedEntity2 = db.Find<ConcreteEntity01>(id);
            Assert.NotNull(loadedEntity2);
            loadedEntity2.CheckIDataTypeAssignments();
            Assert.Equal(name2, loadedEntity2.Name);

        }

        [Fact]
        public virtual void Crud_06_Update_Json()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();
            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity03.base.json");

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();
            var ems1 = db.Metadata.AddJson(content);

            db.Structure.DropEntity("ConcreteEntity01");
            db.Structure.DropEntity(ems1.First());

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();
            db.Structure.ApplyEntityStructure(ems1.First());

            using var work1 = db.BeginWork();

            IEntity entity = work1.New("myJsonEntity03");
            entity.CheckIDataTypeAssignments();

            Assert.True((long)entity.GetId() < -9999); //Yeni entity'lerde Id verilir ancak eksi bir değer alır. Commit sırasında bu değer ve ilişkili kayıtlar için güncellenir

            string name = RandomHelper.RandomText(10);

            entity["Subject"] = name;
            entity.Save();
            work1.CommitChanges();

            long id = (long)entity.GetId();

            //TODO: Cache'ler temizlenecek !!

            using var work2 = db.BeginWork();
            IEntity loadedEntity = db.Find("myJsonEntity03", id);
            Assert.NotNull(loadedEntity);
            Assert.True((string)loadedEntity["Subject"] == name);

            loadedEntity.CheckIDataTypeAssignments();

            string name2 = RandomHelper.RandomText(10);
            loadedEntity["Subject"] = name2;
            loadedEntity.Save();
            work2.CommitChanges();

            //TODO: Cache'ler temizlenecek !!

            IEntity loadedEntity2 = db.Find("myJsonEntity03", id);
            loadedEntity2.CheckIDataTypeAssignments();

            Assert.NotNull(loadedEntity2);
            Assert.Equal(name2, (string)loadedEntity2["Subject"]);
        }

        [Fact]
        public virtual void Crud_07_SimpleDelete()
        {
            var db = Database.Dbs.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();

            db.Structure.DropEntity<ConcreteEntity01>();

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();

            using var work1 = db.BeginWork();

            ConcreteEntity01 entity01 = work1.New<ConcreteEntity01>();
            entity01.Name = "Entity 001";
            entity01.Save();

            ConcreteEntity01 entity02 = work1.New<ConcreteEntity01>();
            entity02.Name = "Entity 002";
            entity02.Save();

            work1.CommitChanges();

            using var work2 = db.BeginWork();
            var entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            work2.Delete(entity01);

            //Henüz commit olmadı, veritabanında duruyor
            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);

            work2.CommitChanges();

            using var work3 = db.BeginWork();
            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(1, entCount);

            work3.Delete(entity02);
            work3.CommitChanges();

            entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(0, entCount);
        }


        [Fact]
        public virtual void Crud_08_NewEntityCreation()
        {
            var db = Database.Dbs.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();

            db.Structure.DropEntity<ConcreteEntity01>();

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work1 = db.BeginWork();

            //Prefer to use work.New<T> method to create new entities within a work scope
            ConcreteEntity01 entity01 = work1.New<ConcreteEntity01>();
            entity01.Name = "Entity 001";
            entity01.Save();

            //But also can use constructor directly
            ConcreteEntity01 entity02 = new ConcreteEntity01();
            entity02.Name = "Entity 002";
            entity02.Save();

            work1.CommitChanges();

            var entCount = db.GetQuery<ConcreteEntity01>().Count();
            Assert.Equal(2, entCount);
        }



    }
}
