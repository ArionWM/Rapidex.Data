using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data.TestBase;
internal static class TestExtensions
{
    public static IDbEntityMetadata ReAdd<T>(this IDbMetadataContainer emman, string module = null, string prefix = null) where T : IConcreteEntity
    {
        emman.Remove<T>();
        IDbEntityMetadata metadata = emman.AddIfNotExist<T>(module, prefix);
        return metadata;

    }


    public static IDbEntityMetadata ReAddReCreate<T>(this IDbSchemaScope scope) where T : IConcreteEntity
    {
        var em = scope.ParentDbScope.Metadata.Get<T>();
        if (em != null)
            scope.ParentDbScope.Metadata.Remove<T>();

        IDbEntityMetadata metadata = scope.ParentDbScope.Metadata.AddIfNotExist<T>();
        scope.Structure.DropEntity<T>();
        scope.Structure.ApplyEntityStructure<T>();
        return metadata;
    }

    public static void CheckIDataTypeAssignments(this IEntity entity)
    {
        var em = entity.GetMetadata();

        foreach (IDbFieldMetadata fm in em.Fields.Values)
        {
            if (!fm.Type.IsSupportTo<IDataType>())
                continue;

            IDataType available = entity.GetValue(fm.Name) as IDataType;

            if (available == null)
            {
                throw new InvalidOperationException($"IDataType assignments not available on entity {entity} / {fm.Name}");
            }
        }
    }
}
