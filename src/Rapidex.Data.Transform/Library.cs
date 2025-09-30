//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Microsoft.Extensions.DependencyInjection;

//namespace Rapidex.Data.Transform;
//internal class Library: AssemblyDefinitionBase
//{
//    public override string Name => "Data / Orm Library Transform Addons";
//    public override string TablePrefix => "data";
//    public override int Index => 100;

//    public override void SetupServices(IServiceCollection services)
//    {
//        services.AddTransientForProd<IEntitySerializationDataCreator, EntitySerializationDataCreator>();
//    }

//    public override void Start(IServiceProvider serviceProvider)
//    {
        
//    }
//}
