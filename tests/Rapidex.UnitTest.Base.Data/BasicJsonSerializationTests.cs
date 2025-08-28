
using Rapidex.Data;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data;

public class BasicJsonSerializationTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public BasicJsonSerializationTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    /*
     * Serialization; Her dataType olması gerektiği gibi EntityData dict item üretiyor mu? (??? doğrudan serileştirsek?)
     * EntityData dict doğru serialize ediliyor mu?
     * 
     Flat data için A, B, C türlerinin olduğu EntityData dan 
     
     */

    [Fact]
    public void Serialization_01_ConvertToEntityDataDto()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();

        db.Metadata.AddIfNotExist<ConcreteEntity04>(); //Master
        db.Metadata.AddIfNotExist<ConcreteEntity03>(); //Detail

        var dbscope = Database.Dbs.AddMainDbIfNotExists();
        dbscope.Structure.DropEntity<ConcreteEntity03>();
        dbscope.Structure.DropEntity<ConcreteEntity04>();

        dbscope.Structure.ApplyEntityStructure<ConcreteEntity03>();
        dbscope.Structure.ApplyEntityStructure<ConcreteEntity04>();


        ConcreteEntity03 ent03_01 = dbscope.New<ConcreteEntity03>();
        ent03_01.Name = "ent03_01";
        ent03_01.Save();

        ConcreteEntity03 ent03_02 = dbscope.New<ConcreteEntity03>();
        ent03_02.Name = "ent03_02";
        ent03_02.Save();

        ConcreteEntity04 ent04 = dbscope.New<ConcreteEntity04>();
        ent04.Number = RandomHelper.Random(100000);
        ent04.Save();

        ent04.Details01.Add(ent03_01);
        ent04.Details01.Add(ent03_02);

        dbscope.ApplyChanges();

        IEntitySerializationDataCreator dataCreator = this.Fixture.ServiceProvider.GetRapidexService<IEntitySerializationDataCreator>() ?? new EntitySerializationDataCreator();

        EntitySerializationOptions serializationOptions = new EntitySerializationOptions();
        serializationOptions.IncludeNestedEntities = true;
        serializationOptions.IncludeBaseFields = true;
        serializationOptions.IncludeTypeName = true;

        //All fields requested
        EntityDataDtoBase dto01 = dataCreator.ConvertToEntityData(ent04, serializationOptions);
        Assert.NotNull(dto01);
        Assert.Equal("ConcreteEntity04", dto01.Entity);


        object detailData = dto01["Details01"];
        Assert.NotNull(detailData);
        Assert.IsAssignableFrom<IEnumerable<EntityDataDtoBase>>(detailData);

        IEnumerable<EntityDataDtoBase> _detailData = (IEnumerable<EntityDataDtoBase>)detailData;
        Assert.Equal(2, _detailData.Count());

        EntityDataDtoBase detail01 = _detailData.First();
        EntityDataDtoBase detail02 = _detailData.Last();

        Assert.Equal("ent03_01", detail01["Name"]);
        Assert.Equal("ent03_02", detail02["Name"]);


        //Only Number requested
        EntityDataDtoBase dto02 = dataCreator.ConvertToEntityData(ent04, serializationOptions, "number"); //<- case insensitive
        Assert.NotNull(dto02);

        Assert.Equal(6, dto02.Keys.Count());
        Assert.Equal(ent04.GetId(), dto02[CommonConstants.FIELD_ID]);
        Assert.Equal(ent04.GetId(), dto02[CommonConstants.DATA_FIELD_ID]);
        Assert.Equal(ent04.DbVersion, dto02[CommonConstants.FIELD_VERSION]);
        Assert.Equal(ent04.Caption(), dto02[CommonConstants.DATA_FIELD_CAPTION]);
        Assert.Equal(ent04.Number, dto02["Number"]);

        Assert.Null(dto02["Details01"]);

    }

    [Fact]
    public void Serialization_01_EntityToJsonAndReverse()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master
        db.Structure.ApplyEntityStructure<ConcreteEntity01>();
        //var dbscope = Database.Databases.AddMainDbIfNotExists();
        //dbscope.Structure.DropEntity<ConcreteEntity01>();
        //dbscope.Structure.ApplyEntityStructure<ConcreteEntity01>();

        ConcreteEntity01 ent01 = db.New<ConcreteEntity01>();
        ent01.Name = "ent01";
        ent01.CreditLimit1 = 1234;
        ent01.Description = "desc 01";
        ent01.ContactType = ContactType.Corporate;
        ent01.Save();

        string json01 = ent01.ToJson();
        Assert.NotNull(json01);

        ConcreteEntity01 ent = json01.FromJson<ConcreteEntity01>(db);
        Assert.NotNull(ent);
        Assert.Equal("ent01", ent.Name);
        Assert.Equal(1234, ent.CreditLimit1.Value);
        Assert.Equal(ContactType.Corporate, (ContactType)ent.ContactType.Value);
    }

    [Fact]
    public void Serialization_02_EntityJsonDataDeserialization()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>(); //Master

        //var dbscope = Database.Databases.AddMainDbIfNotExists();
        //dbscope.Structure.DropEntity<ConcreteEntity01>();

        //dbscope.Structure.ApplyAllStructure();

        //ConcreteEntity01 ent01 = dbscope.New<ConcreteEntity01>();
        //ent01.Name = "ent01";
        //ent01.CreditLimit1 = 1234;
        //ent01.Description = "desc 01";
        //ent01.ContactType = ContactType.Corporate;
        //ent01.Save();

        string json =
        @"{
              ""name"": ""ent01"",
              ""address"": null,
              ""phone"": null,
              ""creditLimit1"": {
                ""value"": 1234,
                ""type"": null
              },
              ""creditLimit2"": {
                ""value"": 0,
                ""type"": null
              },
              ""total"": {
                ""value"": 1234,
                ""type"": null
              },
              ""description"": ""desc 01"",
              ""picture"": ""-10062.Picture"",
              ""birthDate"": ""0001-01-01T00:00:00+00:00"",
              ""contactType"": {
                ""value"": 2,
                ""text"": ""Corporate""
              },
              ""_TypeName"": ""ConcreteEntity01"",
              ""_DbName"": ""Master"",
              ""_SchemaName"": ""Base"",
              ""_IsNew"": true,
              ""id"": -10062,
              ""externalId"": null,
              ""dbVersion"": 0
            }";

        ConcreteEntity01 ent = json.FromJson<ConcreteEntity01>();
        Assert.NotNull(ent);
        Assert.Equal("ent01", ent.Name);
        Assert.Equal(1234, ent.CreditLimit1.Value);
        Assert.Equal(ContactType.Department, (ContactType)ent.ContactType.Value);

    }

}
