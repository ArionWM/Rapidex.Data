using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.UnitTest.Data
{
    public static class DataTestExtensions
    {
        public static void TestIDataTypeAssignments(this IEntity entity)
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
}
