using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rapidex.UnitTest.Base.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Base.Common;
public class Logging : IClassFixture<SingletonFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture fixture;

    public Logging(SingletonFixtureFactory<DefaultEmptyFixture> factory)
    {
        fixture = factory.GetFixture();
    }

    [Fact]
    public void Log_01_Info()
    {
        ILoggerFactory factory = this.fixture.ServiceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(factory);
        //Rapidex.Log.Info("Test message");
        //Rapidex.Log.Info("Test category", "Test message");
        //Rapidex.Log.Info("Test category", new Exception("Test exception"), "Test message with exception");
        //Rapidex.Log.Info(new Exception("Test exception"), "Test message with exception");
        //Rapidex.Log.Debug("Test category", "Debug message");
        //Rapidex.Log.Debug("Debug message");
    }

}
