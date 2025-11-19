global using Rapidex;
global using Rapidex.Data;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Rapidex.Base.Common.Logging.Serilog.Core8;
using Rapidex.Data.Sample.App1;
using Rapidex.Data.Sample.App1.ConcreteEntities;



#region Classic ASP.NET Core setup
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

// Configure Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Rapidex Data Sample Application",
        Description = "A sample API demonstrating Rapidex.Data framework capabilities",
        Contact = new OpenApiContact
        {
            Name = "Rapidex.Data",
            Url = new Uri("https://github.com/ArionWM/Rapidex.Data")
        }
    });

    // Include XML comments for better documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }

    // Configure for controller actions
    c.TagActionsBy(api => new[] { api.GroupName ?? api.ActionDescriptor.RouteValues["controller"] });
    c.DocInclusionPredicate((name, api) => true);
});

builder.UseRapidexSerilog(conf =>
{
    // Override or extend appsettings.json configuration if needed
    conf.LogDirectory = Path.Combine(builder.Environment.ContentRootPath, "App_Data", "Logs");
    //conf.SetMinimumLogLevelAndOthers(new[] { "Rapidex" }, LogLevel.Debug, LogLevel.Warning);
});
#endregion


builder.Services.AddApplicationServices(); //<- Add own services


//Rapidex ==============================
//See: https://github.com/ArionWM/Rapidex.Data/blob/main/docs/QuickStart.md

builder.Services.AddRapidexDataLevel(configuration: builder.Configuration); //<- Add Rapidex services

//For single database and schema application, this useful ...
builder.Services.AddTransient<IDbSchemaScope>(sp =>
{
    var dbScope = Database.Dbs.Db();
    return dbScope;
});

//If we not use IRapidexAssemblyDefinition supported class (library declaration), we can add manually own assemblies
Common.Assembly.Add(typeof(Program).Assembly);

//See: https://github.com/ArionWM/Rapidex.Data/blob/main/docs/LibraryDeclaration.md
//======================================



//...

var app = builder.Build();


//...

//Rapidex ==============================
app.Services.StartRapidexDataLevel(); //<- Start Rapidex infrastructure

StartMyConfiguration(); //<- Add dynamic metadata etc.
//======================================

//...

#region Classic ASP.NET Core setup

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rapidex Data Sample API v1");
        c.RoutePrefix = "swagger"; // Access via /swagger
        c.DisplayRequestDuration();
        c.EnableTryItOutByDefault();
    });
}
else
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

#endregion

app.Run();



static void StartMyConfiguration()
{
    // Lets access database
    var dbScope = Database.Dbs.AddMainDbIfNotExists();

    //Scan and add metadata from definitions
    //--------------------------------------------
    //Concrete definitions already scanned from assembly

    //Lets scan json or yaml definitions from folder
    dbScope.Metadata.ScanDefinitions(@".\App_Content\MyAppDefinitions");

    // Or you can add each metadata manually
    //--------------------------------------------
    dbScope.Metadata.AddJson(@"{ ""type"": ""EntityDefinition"", ""version"": 1, ""name"": ""mySampleJsonEntity01"", ""dbPrefix"": ""utest"", ""primaryKey"":""Id"", ""fields"": [ { ""name"": ""Id"", ""type"": ""long"" }, { ""name"": ""Subject"", ""type"": ""string"" }, { ""name"": ""ChangeField"", ""type"": ""int"" }]}");
    dbScope.Metadata.AddIfNotExist<Order>();

    //Apply all structure changes to database
    dbScope.Structure.ApplyAllStructure();
}