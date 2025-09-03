global using Rapidex;
global using Rapidex.Data;
using Microsoft.Extensions.DependencyInjection;
using Rapidex.Data.Sample.App1.ConcreteEntities;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();


builder.Services.UseSerilog(Path.Combine(AppContext.BaseDirectory, "Logs"));

//...

//Rapidex ==============================
builder.Services.AddRapidexDataLevel(); //<- Add Rapidex services

//If we not use IRapidexAssemblyDefinition class (library declaration), we can add manually own assemblies
Common.Assembly.Add(typeof(Program).Assembly);
//======================================

//...

var app = builder.Build();

//...

//Rapidex ==============================
app.Services.StartRapidexDataLevel(); //<- Start Rapidex infrastructure

StartMyConfiguration(); //<- Add dynamic metadata eg.
//======================================

//...

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();




static void StartMyConfiguration()
{
    // Lets access database
    var dbScope = Database.Dbs.Db();

    //Scan and add metadata from definitions
    //--------------------------------------------
    //Concrete definitions already scanned from assembly
    //Lets scan json or yaml definitions from folder
    dbScope.Metadata.ScanDefinitions(@".\App_Content\MyAppDefinitions");

    // Or you can add each metadata manually
    //--------------------------------------------
    dbScope.Metadata.AddJson("myJsonContent");
    dbScope.Metadata.AddIfNotExist<Order>();
}