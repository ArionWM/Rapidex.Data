using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.Data;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data;
public class DataUpdateWithJsonTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public DataUpdateWithJsonTests(EachTestClassIsolatedFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
    {
    }

    [Fact]
    public void Serialization_01_UpdateData_Basic()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();

        var work1 = db.BeginWork();

        var ent01 = work1.New<ConcreteEntity01>();
        ent01.Name = RandomHelper.RandomText(10);
        ent01.Address = RandomHelper.RandomText(20);
        ent01.Save();

        work1.CommitChanges();

        long entityId = ent01.Id;
        string address = ent01.Address;

        Assert.Equal(0m, ent01.CreditLimit1.Value);
        Assert.True(ent01.Description.IsNullOrEmpty());
        Assert.True(ent01.Picture.IsNullOrEmpty());


        string rawJson = @"
        {
          ""Entity"": ""ConcreteEntity01"",
          ""Type"": ""Update"",
          ""Values"": {
            ""Id"": 12345,
            ""Name"": ""Updated 1"",
            ""Phone"": ""1234567890"",
            ""Number"": 5,
            ""Picture"": {
              ""value"": ""10006"",
              ""text"": ""Image01.png"",
              ""id"": ""Base.ConcreteEntity01.10041.fields.Picture""
            },
            ""BirthDate"": ""2012-04-21T18:25:43-05:00"",
            ""ContactType"": {
              ""value"": ""32""
            }
          }
        }
        ";

        string json = rawJson.Replace("12345", entityId.ToString());



        var entities = EntityDataJsonConverter.Deserialize(json, db);
        Assert.IsType<IPartialEntity>(entities.Single(), false);
        using var work2 = db.BeginWork();
        entities.Save();
        work2.CommitChanges();

        var entUpdated = db.Find<ConcreteEntity01>(entityId);
        Assert.NotNull(entUpdated);
        Assert.Equal("Updated 1", entUpdated.Name);
        Assert.Equal(address, entUpdated.Address);

    }

    [Fact]
    public void Serialization_07_UpdateData_Complex1()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntity02>();

        string content1 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.01Simple.yml");
        db.Metadata.AddYaml(content1);

        string content2 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.07SimpleWithMultipleReference.yml");
        //Entity.03SimpleWithReference.yml
        db.Metadata.AddYaml(content2);

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntity02>();
        db.Structure.DropEntity("myEntity7");

        db.Structure.ApplyAllStructure();


        string json1 = @"
            [
              {
                ""Entity"": ""ConcreteEntity01"",
                ""Type"": ""New"",
                ""Values"": {
                  ""id"": -83273,
                  ""Name"": ""New 1"",
                  ""Phone"": ""1234567890"",
                  ""Number"": 5,
                  ""BirthDate"": ""2012-04-21T18:25:43-05:00"",
                  ""ContactType"": 32 //Yes, we can use direct value too
                }
              },
              {
                ""Entity"": ""ConcreteEntity02"",
                ""Type"": ""New"",
                ""Values"": {
                  ""MyReference"": -83273,
                }
              },
              {
                ""entity"": ""myEntity7"",
                ""type"": ""new"",
                ""values"": {
                  ""Ref02"": -83273,
                  ""title"": ""Test 1""
                }
              }
            ]
        ";

        var entities = EntityDataJsonConverter.Deserialize(json1, db);

        entities.Select(ent => Assert.IsType<IPartialEntity>(ent, false));

        using var work2 = db.BeginWork();
        entities.Save();
        var updateResult = work2.CommitChanges();

        Assert.NotNull(updateResult);
        Assert.Equal(3, updateResult.AddedItems.Count);

        var resItemConcreteEntity01 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -83273);
        Assert.NotNull(resItemConcreteEntity01);

        var resItemConcreteConcreteEntity02 = updateResult.AddedItems.FirstOrDefault(itm => itm.Name == "ConcreteEntity02");
        Assert.NotNull(resItemConcreteConcreteEntity02);

        var resItemMyEntity7 = updateResult.AddedItems.FirstOrDefault(itm => itm.Name == "myEntity7");
        Assert.NotNull(resItemMyEntity7);

        var ent01 = db.Find<ConcreteEntity01>(resItemConcreteEntity01.Id);
        Assert.NotNull(ent01);
        Assert.Equal("New 1", ent01.Name);
        Assert.Equal(32, (long)ent01.ContactType);
        Assert.Equal("1234567890", ent01.Phone);

        var ent02 = db.Find<ConcreteEntity02>(resItemConcreteConcreteEntity02.Id);
        Assert.NotNull(ent02);
        Assert.False(ent02.MyReference.IsEmpty);
        Assert.Equal(ent01.Id, ent02.MyReference.TargetId);

        var ent07 = db.Find("myEntity7", resItemMyEntity7.Id);
        Assert.NotNull(ent07);
        Assert.Equal(ent01.Id, ((Reference)ent07["Ref02"]).TargetId);
    }

    [Fact]
    public void Serialization_08_UpdateData_Complex2_WithSetN2NRelation()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();

        string content1 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.01Simple.yml");
        db.Metadata.AddYaml(content1);

        string content2 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.07SimpleWithMultipleReference.yml");
        //Entity.03SimpleWithReference.yml
        db.Metadata.AddYaml(content2);

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity("myEntity1");
        db.Structure.DropEntity("myEntity7");

        db.Structure.ApplyAllStructure();


        string json1 = @"
            [
              {
                ""entity"": ""ConcreteEntityForN2NTest02"",
                ""id"": -1111,
                ""type"": ""new"",
                ""values"": {
                  ""name"": ""new cet2 1"",
                  ""number"": 638
                }
              },
              {
                ""entity"": ""ConcreteEntityForN2NTest02"",
                ""id"": -1112,
                ""type"": ""new"",
                ""values"": {
                  ""name"": ""new cet2 1"",
                  ""number"": 638
                }
              },
              {
                ""entity"": ""ConcreteEntityForN2NTest01"",
                ""type"": ""new"",
                ""id"": -87311,
                ""values"": {
                  ""name"": ""new cet1 1"",
                  ""Relation01"": [-1111, -1112] //Setting relation to these entities
                }
              },
            ]

        ";

        using var work2 = db.BeginWork();
        var entities = EntityDataJsonConverter.Deserialize(json1, db);
        entities.Save();
        var updateResult = work2.CommitChanges();

        Assert.NotNull(updateResult);

        var resConcreteEntityForN2NTest01 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -87311);
        Assert.NotNull(resConcreteEntityForN2NTest01);

        var resItemConcreteEntityForN2NTest02 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -1111);
        Assert.NotNull(resItemConcreteEntityForN2NTest02);

        var resItemConcreteEntityForN2NTest01 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -1112);
        Assert.NotNull(resItemConcreteEntityForN2NTest01);

        var resItemMyEntity7 = updateResult.AddedItems.Where(itm => itm.Name == "GenericJunction");
        Assert.Equal(2, resItemMyEntity7.Count());
    }


    [Fact]
    public void Serialization_09_UpdateData_Complex3_WithAddN2NRelation()
    {
        var db = Database.Dbs.AddMainDbIfNotExists();
        db.Metadata.AddIfNotExist<ConcreteEntity01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest01>();
        db.Metadata.AddIfNotExist<ConcreteEntityForN2NTest02>();

        string content1 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.01Simple.yml");
        db.Metadata.AddYaml(content1);

        string content2 = this.Fixture.GetFileContentAsString("TestContent\\yaml\\Entity.07SimpleWithMultipleReference.yml");
        //Entity.03SimpleWithReference.yml
        db.Metadata.AddYaml(content2);

        db.Structure.DropEntity<ConcreteEntity01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity<ConcreteEntityForN2NTest01>();
        db.Structure.DropEntity("myEntity1");
        db.Structure.DropEntity("myEntity7");

        db.Structure.ApplyAllStructure();


        string json1 = @"
            [
              {
                ""Entity"": ""ConcreteEntity01"",
                ""id"": -84372,
                ""Type"": ""New"",
                ""Values"": {
                  ""Name"": ""New 1"",
                  ""Phone"": ""1234567890"",
                  ""Number"": 5,
                  ""BirthDate"": ""2012-04-21T18:25:43-05:00"",
                  ""ContactType"": 32 //Yes, we can use direct value too
                }
              },
              {
                ""entity"": ""ConcreteEntityForN2NTest02"",
                ""id"": -12345,
                ""type"": ""new"",
                ""values"": {
                  ""name"": ""new cet2 1"",
                  ""number"": 638
                }
              },
              {
                ""entity"": ""ConcreteEntityForN2NTest01"",
                ""type"": ""new"",
                ""id"": -87311,
                ""values"": {
                  ""name"": ""new cet1 1"",
                  ""Relation01"": [
                    {
                      //Adding relation to this entity
                      ""type"": ""add"",
                      ""id"": -12345
                    }
                  ]
                }
              },
              {
                ""entity"": ""myEntity7"",
                ""type"": ""new"",
                ""values"": {
                  ""name"": ""new cet1 1"",
                  ""Ref02"": -84372,
                  ""Ref03"": -87311
                }
              }
            ]

        ";

        
        var entities = EntityDataJsonConverter.Deserialize(json1, db);
        using var work2 = db.BeginWork();
        entities.Save();
        var updateResult = work2.CommitChanges();

        Assert.NotNull(updateResult);

        var resItemConcreteEntity01 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -84372);
        Assert.NotNull(resItemConcreteEntity01);

        var resItemConcreteEntityForN2NTest02 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -12345);
        Assert.NotNull(resItemConcreteEntityForN2NTest02);

        var resItemConcreteEntityForN2NTest01 = updateResult.AddedItems.FirstOrDefault(itm => itm.OldId == -87311);
        Assert.NotNull(resItemConcreteEntityForN2NTest01);

        var resItemMyEntity7 = updateResult.AddedItems.FirstOrDefault(itm => itm.Name == "myEntity7");
        Assert.NotNull(resItemMyEntity7);


    }
}
