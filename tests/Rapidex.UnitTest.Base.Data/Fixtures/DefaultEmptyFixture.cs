using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapidex.UnitTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Base.Common.Fixtures;

public class DefaultEmptyFixture : ICoreTestFixture
{
    protected bool initialized = false;
    public IConfiguration Configuration { get; set; }
    public virtual IServiceProvider ServiceProvider { get; set; }

    public DefaultEmptyFixture()
    {
        this.Init();
    }

    protected virtual void Setup(IServiceCollection services)
    {
        //Rapidex.Library commonLib = new Rapidex.Library();
        //commonLib.SetupServices(services);
    }

    public virtual void Init()
    {
        if (initialized)
            return;

        HostApplicationBuilder builder = Host.CreateApplicationBuilder();

        //Database.Configuration.DefaultDatabaseNamePrefix = "RapidexAppUnitTest";

        string logDir = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs");
        builder.UseSerilog(logDir, Serilog.Events.LogEventLevel.Verbose);

        Rapidex.Common.Setup(AppContext.BaseDirectory, AppContext.BaseDirectory, builder.Services);

        this.Configuration = Rapidex.Common.Configuration;

        this.Setup(builder.Services);

        IHost host = builder.Build();
        this.ServiceProvider = host.Services;

        initialized = true;
    }

    public virtual void ClearCaches()
    {

    }

    public virtual void Reinit()
    {
        this.initialized = false;
        this.ClearCaches();
        this.Init();
    }

    public string GetPath(string relativeFilePath)
    {
        string filePath = Path.Combine(Rapidex.Common.RootFolder, relativeFilePath);
        return filePath;
    }

    public string GetFileContentAsString(string relativeFilePath)
    {
        string filePath = Path.Combine(Rapidex.Common.RootFolder, relativeFilePath);
        return System.IO.File.ReadAllText(filePath);
    }

    public byte[] GetFileContentAsBinary(string relativeFilePath)
    {
        string filePath = Path.Combine(Rapidex.Common.RootFolder, relativeFilePath);
        return System.IO.File.ReadAllBytes(filePath);
    }
}
