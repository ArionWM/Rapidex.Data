using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data.Entities;
using Rapidex.Data.Exceptions;
using Rapidex.Data.SerializationAndMapping;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data;

public class DataSerializationTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public DataSerializationTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public void Serialization_01_Basic_ConvertToJson01()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();


        using var work = db.BeginWork();

        ConcreteEntity01 ent01_01 = work.New<ConcreteEntity01>();
        ent01_01.ContactType = ContactType.Personal;
        ent01_01.BirthDate = new DateTimeOffset(1990, 1, 1, 0, 0, 0, TimeSpan.Zero);
        ent01_01.Name = "ent01_01";
        ent01_01.Phone = "555-1234";
        ent01_01.Number = 123;
        ent01_01.CreditLimit1 = 10000;
        ent01_01.CreditLimit1.Type = "USD";
        ent01_01.Description = "Description for ent01";
        ent01_01.Description.Type = TextType.Plain;
        ent01_01.Picture.LoadFromFile(this.Fixture.GetPath("TestContent\\Image01.png"));
        ent01_01.Save();


        work.CommitChanges();

        long entityId = ent01_01.GetId().As<long>();


        string json01 = ((IEntity)ent01_01).ToJson();

        var dict01 = json01.FromJsonToDictionary();

        Assert.NotNull(dict01);
        Assert.Equal("ConcreteEntity01", dict01["_entity"]);

        var values01 = dict01["Values"] as IDictionary<string, object>;
        Assert.NotNull(values01);

        var desc01 = values01["Description"] as IDictionary<string, object>;
        Assert.NotNull(desc01);
        Assert.Equal("Description for ent01", desc01["value"]);
        Assert.Equal("Plain", desc01["type"]);

        var contactType01 = values01["ContactType"] as IDictionary<string, object>;
        Assert.NotNull(contactType01);
        Assert.Equal(16, contactType01["value"]);
        Assert.Equal("Personal", contactType01["text"]);

        var picture01 = values01["Picture"] as IDictionary<string, object>;
        Assert.NotNull(picture01);
        Assert.Equal("Image01.png", picture01["text"]);
        Assert.Equal($"Base.ConcreteEntity01.{entityId}.fields.Picture", picture01["id"]);


    }

    [Fact]
    public void Serialization_02_Basic_ConvertToJson02()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity04>(); //Master
        db.Metadata.AddIfNotExist<ConcreteEntity03>(); //Detail

        db.Structure.DropEntity<ConcreteEntity03>();
        db.Structure.DropEntity<ConcreteEntity04>();

        db.Structure.ApplyEntityStructure<ConcreteEntity04>();
        db.Structure.ApplyEntityStructure<ConcreteEntity03>();


        using var work = db.BeginWork();

        ConcreteEntity01 ent01_01 = work.New<ConcreteEntity01>();
        ent01_01.Name = "ent01_01";
        ent01_01.Save();

        ConcreteEntity03 ent03_01 = work.New<ConcreteEntity03>();
        ent03_01.Ref1 = ent01_01;
        ent03_01.Name = "ent03_01";
        ent03_01.Save();

        ConcreteEntity03 ent03_02 = work.New<ConcreteEntity03>();
        ent03_02.Name = "ent03_02";
        ent03_02.Save();

        ConcreteEntity04 ent04 = work.New<ConcreteEntity04>();
        ent04.Number = RandomHelper.Random(100000);
        ent04.Description = "Description for ent04";

        ent04.Save();

        ent04.Details01.Add(ent03_01);
        ent04.Details01.Add(ent03_02);

        work.CommitChanges();

        long detailId1 = ent03_01.GetId().As<long>();
        long detailId2 = ent03_02.GetId().As<long>();


        string json01 = ((IEntity)ent04).ToJson();

        var dict01 = json01.FromJsonToDictionary();

        Assert.NotNull(dict01);
        Assert.Equal("ConcreteEntity04", dict01["_entity"]);

        var values01 = dict01["Values"] as IDictionary<string, object>;
        Assert.NotNull(values01);

        var desc01 = values01["Description"] as IDictionary<string, object>;
        Assert.NotNull(desc01);

        Assert.Equal("Description for ent04", desc01["value"]);
        Assert.Equal("Plain", desc01["type"]);

        var detais01 = values01["Details01"] as IList<object>;
        Assert.NotNull(detais01);
        Assert.Equal(2, detais01.Count());

        var detail01 = detais01.FirstOrDefault(x => ((IDictionary<string, object>)x)["id"].As<long>() == detailId1) as IDictionary<string, object>;
        Assert.NotNull(detail01);

        var detail01Values = detail01["Values"] as IDictionary<string, object>;
        Assert.Equal("ent03_01", detail01Values["Name"].As<string>());
    }


    [Fact]
    public void Serialization_03_Complex_ConvertToJson02()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<GenericJunction>();
        db.Metadata.AddIfNotExist<ConcreteEntityForSerializationTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForSerializationTest02>();
        db.Metadata.AddIfNotExist<ConcreteEntityForSerializationTest03>();
        db.Structure.ApplyAllStructure();

        using var work = db.BeginWork();
        ConcreteEntityForSerializationTest01 ent01_01 = work.New<ConcreteEntityForSerializationTest01>();
        ent01_01.Name = "ent01_01";
        ent01_01.Save();

        ConcreteEntityForSerializationTest01 ent01_02 = work.New<ConcreteEntityForSerializationTest01>();
        ent01_02.Name = "ent01_02";
        ent01_02.Save();

        ConcreteEntityForSerializationTest02 ent02_01 = work.New<ConcreteEntityForSerializationTest02>();
        ent02_01.TimeField = RandomHelper.RandomDate(true);
        ent02_01.TextField = "<p>This is <b>HTML</b> text</p>";
        ent02_01.TextField.Type = TextType.Html;
        ent02_01.Save();


        ConcreteEntityForSerializationTest03 ent03_01 = work.New<ConcreteEntityForSerializationTest03>();
        ent03_01.Name = "ent03_01";
        ent03_01.NumberField = 123;
        ent03_01.ReferenceTo02 = ent02_01;
        ent03_01.Save();

        ConcreteEntityForSerializationTest03 ent03_02 = work.New<ConcreteEntityForSerializationTest03>();
        ent03_02.Name = "ent03_02";
        ent03_02.NumberField = 456;
        ent03_02.ReferenceTo02 = ent02_01;
        ent03_02.Save();

        ent01_01.Relation01.Add(ent03_01);
        ent01_01.Relation01.Add(ent03_02);

        ent01_02.Relation01.Add(ent03_02);
        work.CommitChanges();

        var ent01s = new ConcreteEntityForSerializationTest01[] { ent01_01, ent01_02 };

        string json01 = EntityDataJsonConverter.Serialize(ent01s);

        var listDict01 = json01.FromJsonToListOfDictionary();

        Assert.NotNull(listDict01);
        Assert.Equal(2, listDict01.Count());

        var dict01 = listDict01.FirstOrDefault(x => x["_id"].As<long>() == ent01_01.Id);
        Assert.NotNull(dict01);
        Assert.Equal("ConcreteEntityForSerializationTest01", dict01["_entity"]);

    }


    [Fact]
    public void Serialization_03_EntityToJsonAndReverse()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();

        using var work = db.BeginWork();

        ConcreteEntity01 ent01 = work.New<ConcreteEntity01>();
        ent01.Name = "ent01";
        ent01.CreditLimit1 = 1234;
        ent01.Description = "desc 01";
        ent01.ContactType = ContactType.Corporate;
        ent01.Save();

        string json01 = ent01.ToJson();
        Assert.NotNull(json01);

        ConcreteEntity01 ent = EntityDataJsonConverter.Deserialize<ConcreteEntity01>(json01, db).FirstOrDefault();
        Assert.NotNull(ent);
        Assert.Equal("ent01", ent.Name);
        Assert.Equal(1234, ent.CreditLimit1.Value);
        Assert.Equal(ContactType.Corporate, (ContactType)ent.ContactType.Value);
    }

    [Fact]
    public void Serialization_04_EntityJsonDataDeserialization()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master

        string json =
        @"{
              ""Entity"": ""ConcreteEntity01"",
              ""Values"": {
                ""Id"": 123,
                ""Name"": ""Test Entity"",
                ""Address"": ""abc abc"",
                ""Phone"": ""1234567890"",
                ""Number"": 5,
                ""CreditLimit1"": 1000.50,
                ""CreditLimit1Currency"": ""USD"",
                ""Total"": 1500.75,
                ""TotalCurrency"": ""USD"",
                ""Description"": ""This is a test entity."",
                ""Picture"": {
                  ""value"": ""10006"",
                  ""text"": ""Image01.png"",
                  ""id"": ""Base.ConcreteEntity01.10041.fields.Picture""
                },
                ""BirthDate"": ""2012-04-21T18:25:43-05:00"",
                ""ContactType"": {
                  ""value"": ""1"",
                  ""text"": ""Customer""
                    }
                }
            }";


        IEntity entDynamic = EntityDataJsonConverter.Deserialize(json, db).FirstOrDefault();
        Assert.NotNull(entDynamic);
        Assert.Equal("ConcreteEntity01", entDynamic._TypeName);
        Assert.Equal("Test Entity", entDynamic["Name"].As<string>());
        Assert.Equal(1000.50m, (entDynamic["CreditLimit1"].As<Currency>()).Value);

        ConcreteEntity01 entConcrete = EntityDataJsonConverter.Deserialize<ConcreteEntity01>(json, db).FirstOrDefault();
        Assert.NotNull(entConcrete);
        Assert.Equal("Test Entity", entConcrete.Name);
        Assert.Equal(1000.50m, entConcrete.CreditLimit1.Value);
    }

    [Fact]
    public void Serialization_05_MultipleEntityJsonDataDeserialization()
    {
        string json = @"
            [
              {
                ""Entity"": ""ConcreteEntity01"",
                ""Values"": {
                  ""Id"": 123,
                  ""Name"": ""Test Entity 1"",
                  ""Address"": ""abc abc"",
                  ""Phone"": ""1234567890"",
                  ""Number"": 5,
                  ""CreditLimit1"": 1000.50,
                  ""CreditLimit1Currency"": ""USD"",
                  ""Total"": 1500.75,
                  ""TotalCurrency"": ""USD"",
                  ""Description"": ""This is a test entity."",
                  ""Picture"": {
                    ""value"": ""10006"",
                    ""text"": ""Image01.png"",
                    ""id"": ""Base.ConcreteEntity01.10041.fields.Picture""
                  },
                  ""BirthDate"": ""2012-04-21T18:25:43-05:00"",
                  ""ContactType"": {
                    ""value"": ""16"",
                    ""text"": ""Customer""
                  }
                }
              },
              {
                ""Entity"": ""ConcreteEntity01"",
                ""Values"": {
                  ""Id"": 124,
                  ""Name"": ""Test Entity 2"",
                  ""Address"": ""abc abc"",
                  ""Phone"": ""98765421"",
                  ""Number"": 4,
                  ""CreditLimit1"": 500.50,
                  ""CreditLimit1Currency"": ""USD"",
                  ""Total"": 500.75,
                  ""TotalCurrency"": ""USD"",
                  ""Description"": ""This is a test entity 2"",
                  ""Picture"": null,
                  ""BirthDate"": ""2004-03-01T18:25:43+03:00"",
                  ""ContactType"": {
                    ""value"": ""1""
                  }
                }
              }

            ]";

        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master
        var entDynamics = EntityDataJsonConverter.Deserialize(json, db);

        Assert.NotNull(entDynamics);
        Assert.Equal(2, entDynamics.Count());

        var entDynamic1 = entDynamics.FirstOrDefault(x => x.GetId().As<long>() == 123);
        var entDynamic2 = entDynamics.FirstOrDefault(x => x.GetId().As<long>() == 124);

        Assert.NotNull(entDynamic1);
        Assert.IsType<ConcreteEntity01>(entDynamic1);   
        Assert.Equal("Test Entity 1", entDynamic1["Name"].As<string>());
        Assert.Equal(1000.50m, (entDynamic1["CreditLimit1"].As<decimal>()));
        Assert.Equal(ContactType.Personal, (ContactType)entDynamic1["ContactType"].As<Enumeration>().Value);

        Assert.NotNull(entDynamic2);
        Assert.Equal("Test Entity 2", entDynamic2["Name"].As<string>());
        Assert.Equal(500.50m, (entDynamic2["CreditLimit1"].As<decimal>()));
        Assert.Equal(ContactType.Employee, (ContactType)entDynamic2["ContactType"].As<Enumeration>().Value);



    }

    [Fact]
    public void Serialization_06_ForceUpdateJsonToConcreteTypeRaiseError()
    {
        //error scenario 
    }



}
