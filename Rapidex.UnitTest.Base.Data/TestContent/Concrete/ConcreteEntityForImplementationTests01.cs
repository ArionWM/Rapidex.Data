using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestContent.Concrete;
internal class ConcreteEntityForImplementationTests01 : DbConcreteEntityBase
{

}

internal class ConcreteEntityForImplementationTests01Implementer : IConcreteEntityImplementer<ConcreteEntityForImplementationTests01>
{
    public void SetupMetadata(IDbScope owner, IDbEntityMetadata metadata)
    {
        metadata
            .AddBehavior<ArchiveEntity>(true, false)
            .AddBehavior<HasTags>(true, false)
            .MarkOnlyBaseSchema();
    }
}

