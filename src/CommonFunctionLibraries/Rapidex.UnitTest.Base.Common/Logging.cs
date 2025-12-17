using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Rapidex.UnitTest.Base.Common.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Base.Common;
public class Logging : IClassFixture<EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture>>
{
    DefaultEmptyFixture Fixture { get; }
    public ILogger Logger => this.Fixture.Logger;

    public Logging(EachTestClassIsolatedFixtureFactory<DefaultEmptyFixture> factory)
    {
        this.Fixture = factory.GetFixture(this.GetType());
        this.Logger?.LogInformation("Logging initialized.");
    }

    [Fact]
    public void Log_01_Info()
    {
        ILoggerFactory factory = this.Fixture.ServiceProvider.GetService<ILoggerFactory>();
        Assert.NotNull(factory);
        //Rapidex.Log.Info("Test message");
        //Rapidex.Log.Info("Test category", "Test message");
        //Rapidex.Log.Info("Test category", new Exception("Test exception"), "Test message with exception");
        //Rapidex.Log.Info(new Exception("Test exception"), "Test message with exception");
        //Rapidex.Common.DefaultLogger?.LogDebug("Test category", "Debug message");
        //Rapidex.Common.DefaultLogger?.LogDebug("Debug message");
    }

}
