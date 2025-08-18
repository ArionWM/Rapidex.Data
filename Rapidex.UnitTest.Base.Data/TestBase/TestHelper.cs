using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
internal static class TestHelper
{
    public static IDbEntityMetadata ReAdd<T>(this IDbMetadataContainer emman, string module = null, string prefix = null) where T : IConcreteEntity
    {
        emman.Remove<T>();
        IDbEntityMetadata metadata = emman.AddIfNotExist<T>(module, prefix);
        return metadata;

    }


    public static IDbEntityMetadata ReAddReCreate<T>(this IDbSchemaScope scope) where T : IConcreteEntity
    {
        scope.ParentDbScope.Metadata.Remove<T>();
        IDbEntityMetadata metadata = scope.ParentDbScope.Metadata.AddIfNotExist<T>();
        scope.Structure.DropEntity<T>();
        scope.Structure.ApplyEntityStructure<T>();
        return metadata;
    }
}
