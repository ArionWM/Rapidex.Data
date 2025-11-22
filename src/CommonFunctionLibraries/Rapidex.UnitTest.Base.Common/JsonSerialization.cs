using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Rapidex.UnitTest.Base.Common.Fixtures;


namespace Rapidex.UnitTest.Base.Common;

public class JsonSerialization : IClassFixture<SingletonFixtureFactory<DefaultEmptyFixture>>
{

    DefaultEmptyFixture Fixture { get; }
    public ILogger Logger => this.Fixture.Logger;

    public JsonSerialization(EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture> factory)
    {
        this.Fixture = factory.GetFixture(this.GetType());
        this.Logger?.LogInformation("JsonSerialization initialized.");
    }

    //[Fact]
    //public void T01_InterfaceDerivedTypes()
    //{
    //    JsonTestClassA_A obj01 = new()
    //    {
    //        Name = "Test 01",
    //    };

    //    string json01 = obj01.ToJson();

    //    JsonTestClassA_A desObj01 = json01.FromJson<JsonTestClassA_A>();
    //    Assert.Equal(obj01.Name, desObj01.Name);

    //    JsonTestClassA_A_A_Int obj02 = new()
    //    {
    //        Name = "Test 02",
    //        Value = 123,
    //        DValue = 893
    //    };

    //    string json02 = obj02.ToJson();
    //    JsonTestClassA_A_A_Int desObj02 = json02.FromJson<JsonTestClassA_A_A_Int>();

    //    Assert.Equal(obj02.Name, desObj02.Name);
    //    Assert.Equal(obj02.Value, desObj02.Value);
    //    Assert.Equal(obj02.DValue, desObj02.DValue);


    //    JsonTestClassA_B_A_A_String obj03 = new()
    //    {
    //        Name = "Test 03",
    //        Value = "Hello",
    //        DValue = 874
    //    };

    //    string json03 = obj03.ToJson();
    //    JsonTestClassA_B_A_A_String desObj03 = json03.FromJson<JsonTestClassA_B_A_A_String>();

    //    Assert.Equal(obj03.Name, desObj03.Name);
    //    Assert.Equal(obj03.Value, desObj03.Value);
    //    Assert.Equal(obj03.DValue, desObj03.DValue);
    //}

}
