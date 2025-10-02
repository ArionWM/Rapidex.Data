using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rapidex.UnitTest.Data.TestContent;

namespace Rapidex.UnitTest.Data;
public class DataUpdateWithJsonTests : DbDependedTestsBase<DbSqlServerProvider>
{
    public DataUpdateWithJsonTests(SingletonFixtureFactory<DbWithProviderFixture<DbSqlServerProvider>> factory) : base(factory)
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
        ent01.Save();

        work1.CommitChanges();

        long entityId = ent01.Id;
        Assert.Equal(0m, ent01.CreditLimit1.Value);
        Assert.True(ent01.Description.IsNullOrEmpty());
        Assert.True(ent01.Picture.IsNullOrEmpty());


        string rawJson = @"
        {
          ""Entity"": ""ConcreteEntity01"",
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
        using var work2 = db.BeginWork();
        entities.Save();
        work2.CommitChanges();

        var entUpdated = db.Find<ConcreteEntity01>(entityId);
        Assert.NotNull(entUpdated);
        Assert.Equal("Updated 1", entUpdated.Name);

    }

    [Fact]
    public void Serialization_07_UpdateData_Complex()
    {

    }
}
