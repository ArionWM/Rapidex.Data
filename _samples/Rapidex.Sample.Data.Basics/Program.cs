// See https://aka.ms/new-console-template for more information
global using Rapidex;
global using Rapidex.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapidex.Sample.Data.Basics.ConcreteEntitites;

Console.WriteLine("This is single database basic application");

//This mini sample .. You can use Host eg buil
ServiceCollection services = new();

// Configure Serilog for logging
services.UseSerilog(Path.Combine(AppContext.BaseDirectory, "Logs"));

// Init Rapidex infrastructure
Rapidex.Common.Setup(AppContext.BaseDirectory, AppContext.BaseDirectory, services);

Database.Setup(services);
//......

var serviceProvider = services.BuildServiceProvider();
Rapidex.Common.Start(serviceProvider);


// Lets access database
var dbScope = Database.Scopes.AddMainDbIfNotExists();


dbScope.Metadata.AddJson("myJsonContent");
dbScope.Metadata.ScanDefinitions("myDefinitionsFolder");
dbScope.Metadata.Add<MyEntity1>();
dbScope.Metadata.ScanConcreteDefinitions();
