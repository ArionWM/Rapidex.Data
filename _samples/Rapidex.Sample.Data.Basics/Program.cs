// See https://aka.ms/new-console-template for more information
global using Rapidex;
global using Rapidex.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rapidex.Sample.Data.Basics.ConcreteEntitites;

Console.WriteLine("This is single database basic application");

//This mini sample .. You can use Host eg build
ServiceCollection services = new();

// Configure Serilog for logging
services.UseSerilog(Path.Combine(AppContext.BaseDirectory, "Logs"));

services.AddRapidexDataLevel(); //<- Add Rapidex services

//If we not use IRapidexAssemblyDefinition class, we can add manually own assemblies
Common.Assembly.Add(typeof(Program).Assembly);

//......

var serviceProvider = services.BuildServiceProvider();

//......

serviceProvider.StartRapidexDataLevel(); //<- Start Rapidex infrastructure


// Lets access database
var dbScope = Database.Scopes.AddMainDbIfNotExists();

//Scan and add metadata from definitions
dbScope.Metadata.ScanDefinitions(@".\App_Content\MyAppDefinitions");
dbScope.Metadata.ScanConcreteDefinitions();

// Or you can add metadata manually
dbScope.Metadata.AddJson("myJsonContent");
dbScope.Metadata.AddIfNotExist<MyEntity1>();
