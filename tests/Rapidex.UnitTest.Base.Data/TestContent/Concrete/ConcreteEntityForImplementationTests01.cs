using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent.Concrete;
internal class ConcreteEntityForImplementationTests01 : DbConcreteEntityBase
{
    public string Name { get; set; }
    public Percent Percent { get; set; }
}

internal class ConcreteEntityForImplementationTests01Implementer : IConcreteEntityImplementer<ConcreteEntityForImplementationTests01>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false)
            .MarkOnlyBaseSchema()
            .AddFieldIfNotExist("newField", typeof(string), "New Field");
    }
}

