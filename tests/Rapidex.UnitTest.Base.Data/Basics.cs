using Microsoft.Extensions.DependencyInjection;
using Rapidex.UnitTest.Data.TestContent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data;
public class Basics //With no fixture
{
    [Fact]
    public void ActivateDatabaseInfrastructure()
    {
        ServiceCollection services = new();
        services.AddRapidexDataLevel();


        var serviceProvider = services.BuildServiceProvider();

        serviceProvider.StartRapidexDataLevel();

        var masterBaseScope = Database.Dbs.AddMainDbIfNotExists();

        masterBaseScope.Metadata.Add<ConcreteEntity01>();
        masterBaseScope.Structure.ApplyEntityStructure<ConcreteEntity01>();

    }

}
