global using Rapidex;
global using Rapidex.Data;
using Microsoft.Extensions.DependencyInjection;


namespace Rapidex.Data.Sample.Library;

public class LibraryDeclaration : AssemblyDefinitionBase, IRapidexMetadataReleatedAssemblyDefinition
{
    public override string Name => "SampleRapidexDataLibrary";

    public override string TablePrefix => null;

    public override int Index => 1000;


    public void SetupMetadata(IDbScope db)
    {
        
    }

    public override void SetupServices(IServiceCollection services)
    {
        
    }

    public override void Initialize(IServiceProvider serviceProvider)
    {

    }


    public override void Start(IServiceProvider serviceProvider)
    {
        
    }
}
