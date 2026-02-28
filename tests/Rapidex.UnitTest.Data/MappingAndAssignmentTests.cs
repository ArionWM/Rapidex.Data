using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data;
using Rapidex.Data;
using Rapidex.Data.Exceptions;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data
{
    public class MappingAndAssignmentTests : DbDependedTestsBase<DbSqlServerProvider>
    {
        public MappingAndAssignmentTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
        {
        }

        //Reference null assign set id to zero
        //Reference<T> can assign but not obj reference, only id 
        //Relation try add null ref (exception)

        [Fact]
        public void T01_EntityInitialization_Concrete_AdvancedDataTypes_ShouldBe_Assigned()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();
            var dbScope = db.AddSchemaIfNotExists("myTestSchema01");

            db.Metadata.AddIfNotExist<ConcreteEntity01>();


            var workOnSchema = dbScope.BeginWork();

            IEntity entity = workOnSchema.New<ConcreteEntity01>();
            entity.CheckIDataTypeAssignments();
        }

        [Fact]
        public void T02_EntityInitialization_Json_AdvancedDataTypes_ShouldBe_Assigned()
        {
            var db = Database.Dbs.AddMainDbIfNotExists();

            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity05.base.json");
            db.Metadata.AddJson(content);

            using var work = db.BeginWork();

            IEntity entity = work.New("myJsonEntity05");
            entity.CheckIDataTypeAssignments();
        }

        [Fact]
        public void T03_FieldAssignment_Json_WrongTypesThrowException()
        {
            var db = Database.Dbs.AddMainDbIfNotExists();

            string content = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity05.base.json");
            db.Metadata.AddJson(content);

            using var work = db.BeginWork();

            IEntity entity = work.New("myJsonEntity05");

            //Assert.Throws<InvalidOperationException>(() => entity["Id"] = 1.334d);

            entity["ConcreteReference01"] = 1;

            //Assert.Throws<InvalidOperationException>(() => entity["ConcreteReference01"] = "1");

            Assert.Throws<InvalidCastException>(() => entity["Email"] = 1);

            entity["Email"] = "test@test.com";

            Assert.Throws<InvalidCastException>(() => entity["Description"] = 999);


        }


        /// <summary>
        /// Json entity'lerde de değerlerin saklandığı dictionary içerisinde
        /// (Advanced Data Types için) ilgili DataType nesnesi bulunmalıdır.
        /// </summary>
        [Fact]
        public void T04_FieldAssignment_Json_AdvancedDataTypes01()
        {

            var db = Database.Dbs.AddMainDbIfNotExists();

            string content04 = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity04.base.json");
            string content06 = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity06.base.json");
            db.Metadata.AddJson(content04);
            db.Metadata.AddJson(content06);

            using var work = db.BeginWork();

            IEntity entity = work.New("myJsonEntity06");

            object valueRef = entity["ConcreteReference01"];
            Assert.IsAssignableFrom<Reference>(valueRef); //null da olsa Reference nesnesi almalıyız

            object valueImg = entity["Picture"];
            Assert.IsAssignableFrom<Image>(valueImg);

            object valueMail = entity["Email"];
            Assert.IsAssignableFrom<EMail>(valueMail);

            object valueDesc = entity["Description"];
            Assert.IsAssignableFrom<Text>(valueDesc);

            object valuePrice = entity["Price"];
            Assert.IsAssignableFrom<Currency>(valuePrice);

            object reference01 = entity["Details01"];
            Assert.IsAssignableFrom<RelationOne2N>(reference01);



            entity["ConcreteReference01"] = 1; //Int ataması
            valueRef = entity["ConcreteReference01"];
            Assert.IsAssignableFrom<Reference>(valueRef); //ancak okur iken Reference nesnesi almalıyız

            entity["Email"] = "test@ccc.com"; //String ataması
            valueMail = entity["Email"];
            Assert.IsAssignableFrom<EMail>(valueMail); //ancak okur iken Email nesnesi almalıyız

            entity["Price"] = 100;
            valuePrice = entity["Price"];
            Assert.IsAssignableFrom<Currency>(valuePrice);

            entity["Price"] = 111m;
            valuePrice = entity["Price"];
            Assert.IsAssignableFrom<Currency>(valuePrice);

        }

        [Fact]
        public void T05_ConcreteReferenceAssignment()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();
            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();

            using var work1 = db.BeginWork();

            ConcreteEntity01 entity01 = work1.New<ConcreteEntity01>();
            entity01.Save();
            work1.CommitChanges();

            using var work2 = db.BeginWork();
            ConcreteEntity02 entity02 = work2.New<ConcreteEntity02>();
            entity02.MyReference = entity01;
            entity02.Save();

            work2.CommitChanges();

            long id1 = entity01.Id;
            long id2 = entity02.Id;

            //Clear entity cache

            ConcreteEntity02 entity02_1 = db.Find<ConcreteEntity02>(id2);
            Assert.Equal(id1, (long)entity02_1.MyReference.TargetId);
        }

        [Fact]
        public void T06_SameEntityWithDifferentLoadShouldHaveDifferentReferences_Concrete()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();

            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity01>();

            using var work = db.BeginWork();

            ConcreteEntity01 entity01 = work.New<ConcreteEntity01>();
            entity01.CreditLimit1 = 100;
            entity01.CreditLimit2 = 200;
            entity01.Description = "Cust01 description";
            entity01.Name = "Cust01";
            entity01.Address = "Cust01 address";
            entity01.Phone = "1234567890";
            entity01.Picture.Set(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 }, "aaa", "bbb");
            entity01.Save();
            work.CommitChanges();

            long id1 = entity01.Id;

            //Clear entity cache

            ConcreteEntity01 entity01_1 = db.Find<ConcreteEntity01>(id1);
            entity01_1.CheckIDataTypeAssignments();

            ConcreteEntity01 entity01_2 = db.Find<ConcreteEntity01>(id1);
            entity01_2.CheckIDataTypeAssignments();

            //entity01_1 ve entity01_2 farklı referanslara sahip olmalı (aynı bilgiyi tutan farklı nesneler)
            Assert.NotSame(entity01_1, entity01_2);

            //Sadece entity nesneleri değil, içerikler de farklı referanslarda (aynı bilgiyi tutan farklı nesneler) olmalı
            Assert.NotSame(entity01_1.CreditLimit1, entity01_2.CreditLimit1);
            Assert.NotSame(entity01_1.CreditLimit2, entity01_2.CreditLimit2);
            Assert.NotSame(entity01_1.Description, entity01_2.Description);
            Assert.NotSame(entity01_1.Name, entity01_2.Name);
            Assert.NotSame(entity01_1.Address, entity01_2.Address);
            Assert.NotSame(entity01_1.Phone, entity01_2.Phone);
            Assert.NotSame(entity01_1.Picture, entity01_2.Picture);
        }

        [Fact]
        public void T06_SameEntityWithDifferentLoadShouldHaveDifferentReferences_Json()
        {
            this.Fixture.Reinit();

            var db = Database.Dbs.AddMainDbIfNotExists();

            string content05 = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity05.base.json");
            var ems1 = db.Metadata.AddJson(content05);
            Assert.NotNull(ems1);
            Assert.Single(ems1);
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();
            db.Structure.ApplyEntityStructure(ems1.First());

            using var work1 = db.BeginWork();
            ConcreteEntity02 entityForRef = work1.New<ConcreteEntity02>();
            //entityForRef.Name = RandomHelper.RandomText(10);
            entityForRef.Save();
            work1.CommitChanges();

            using var work2 = db.BeginWork();
            IEntity jsonEntity01 = work2.New("myJsonEntity05");
            jsonEntity01["Subject"] = "Subject01";
            jsonEntity01["StartTime"] = DateTimeOffset.Now;
            jsonEntity01["Tags"] = "Tag01,Tag02";
            jsonEntity01["ConcreteReference01"] = entityForRef;
            jsonEntity01["Email"] = "mail@blabla.com";
            jsonEntity01["MobilePhone"] = "1234567890";
            jsonEntity01["Picture"] = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 0 };
            jsonEntity01["Description"] = RandomHelper.RandomTextWithRandomSpaces(100);
            jsonEntity01["Price"] = 173233;

            jsonEntity01.Save();
            work2.CommitChanges();

            long id1 = (long)jsonEntity01.GetId();
            //Clear entity cache

            IEntity entity01_1 = db.Find("myJsonEntity05", id1);
            entity01_1.CheckIDataTypeAssignments();

            IEntity entity01_2 = db.Find("myJsonEntity05", id1);
            entity01_2.CheckIDataTypeAssignments();

            //entity01_1 ve entity01_2 farklı referanslara sahip olmalı (aynı bilgiyi tutan farklı nesneler)
            Assert.NotSame(entity01_1, entity01_2);

            //Sadece entity nesneleri değil, içerikler de farklı referanslarda (aynı bilgiyi tutan farklı nesneler) olmalı
            Assert.NotSame(entity01_1["Subject"], entity01_2["Subject"]);
            Assert.NotSame(entity01_1["StartTime"], entity01_2["StartTime"]);
            Assert.NotSame(entity01_1["Tags"], entity01_2["Tags"]);
            Assert.NotSame(entity01_1["ConcreteReference01"], entity01_2["ConcreteReference01"]);
            Assert.NotSame(entity01_1["Email"], entity01_2["Email"]);
            Assert.NotSame(entity01_1["MobilePhone"], entity01_2["MobilePhone"]);
            Assert.NotSame(entity01_1["Picture"], entity01_2["Picture"]);
            Assert.NotSame(entity01_1["Description"], entity01_2["Description"]);
            Assert.NotSame(entity01_1["Price"], entity01_2["Price"]);
        }


        [Fact]
        public void T07_ReferenceX()
        {
            this.Fixture.ClearCaches();

            var db = Database.Dbs.AddMainDbIfNotExists();

            string content05 = this.Fixture.GetFileContentAsString("TestContent\\json\\jsonEntity05.base.json");
            db.Metadata.AddJson(content05);
            db.Metadata.AddIfNotExist<ConcreteEntity01>();
            db.Metadata.AddIfNotExist<ConcreteEntity02>();

            db.Structure.ApplyEntityStructure<ConcreteEntity01>();
            db.Structure.ApplyEntityStructure<ConcreteEntity02>();

            using var work1 = db.BeginWork();

            ConcreteEntity01 concreteEntity01 = work1.New<ConcreteEntity01>();
            concreteEntity01.Name = "Cust01";
            concreteEntity01.Save();

            work1.CommitChanges();

            using var work2 = db.BeginWork();
            ConcreteEntity02 concreteEntity02 = work2.New<ConcreteEntity02>();
            concreteEntity02.MyReference = concreteEntity01.Id; //id can assign direct
            concreteEntity02.Save();

            work2.CommitChanges();

        }


        [Fact]
        public void T08_OnlyBaseEntityReferenceAssignment()
        {
            this.Fixture.ClearCaches();
            var db = Database.Dbs.AddMainDbIfNotExists();

            var schemaBase = db.Schema("base");
            var schema02 = db.AddSchemaIfNotExists("schema02");

            db.Metadata.AddIfNotExist<ConcreteOnlyBaseEntity01>().MarkOnlyBaseSchema();
            db.Metadata.AddIfNotExist<ConcreteOnlyBaseReferencedEntity02>();

            schemaBase.Structure.DropEntity<ConcreteOnlyBaseEntity01>();
            schemaBase.Structure.DropEntity<ConcreteOnlyBaseReferencedEntity02>();

            schema02.Structure.DropEntity<ConcreteOnlyBaseReferencedEntity02>();

            schemaBase.Structure.ApplyAllStructure();
            schema02.Structure.ApplyAllStructure();

            //---------------------------------------------------------------------------------


            var workOnBase = schemaBase.BeginWork();

            ConcreteOnlyBaseEntity01 baseEnt01 = workOnBase.New<ConcreteOnlyBaseEntity01>();
            baseEnt01.Name = "CEnt 01";
            baseEnt01.Save();

            ConcreteOnlyBaseEntity01 baseEnt02 = workOnBase.New<ConcreteOnlyBaseEntity01>();
            baseEnt02.Name = "CEnt 02";
            baseEnt02.Save();

            workOnBase.CommitChanges();


            Assert.False(baseEnt01.HasPrematureOrEmptyId());

            long baseId01 = (long)baseEnt01.GetId();
            long baseId02 = (long)baseEnt02.GetId();

            var workOnSchema02 = schema02.BeginWork();

            ConcreteOnlyBaseReferencedEntity02 refEntity01 = workOnSchema02.New<ConcreteOnlyBaseReferencedEntity02>();
            refEntity01.Name = "REnt 01";
            refEntity01.Reference = baseEnt01;
            refEntity01.Save();

            workOnSchema02.CommitChanges();

            long refId01 = (long)refEntity01.GetId();
            //----------------------------------------------------------------------------------


            this.Fixture.ClearCaches();
            db = Database.Dbs.AddMainDbIfNotExists();

            schemaBase = db.Schema("base");
            schema02 = db.AddSchemaIfNotExists("schema02");

            db.Metadata.AddIfNotExist<ConcreteOnlyBaseEntity01>().MarkOnlyBaseSchema();
            db.Metadata.AddIfNotExist<ConcreteOnlyBaseReferencedEntity02>();

            ConcreteOnlyBaseReferencedEntity02 refEnt01_02 = schema02.GetQuery<ConcreteOnlyBaseReferencedEntity02>().Find(refId01);
            Assert.NotNull(refEnt01_02);
            Assert.Equal(baseId01, refEnt01_02.Reference.TargetId);
            ConcreteOnlyBaseEntity01 referencedEntity = refEnt01_02.Reference.GetContent();
            Assert.NotNull(referencedEntity);
            Assert.Equal("base", referencedEntity._SchemaName.ToLowerInvariant());

        }
    }
}
