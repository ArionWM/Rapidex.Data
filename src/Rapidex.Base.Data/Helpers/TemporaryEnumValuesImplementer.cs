using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data.Helpers
{
    /// <summary>
    /// Bu sınıf, enumeration'lara başlangıç değerlerini (renk vs.) verebilmek için
    /// geçici oluşturulmuştur.
    /// Bkz: EnumerationDefinitionFactory.Apply
    /// </summary>

    [Obsolete("", true)]
    public class TemporaryEnumValuesImplementer
    {
        public static void Check<T>(IDbScope scope, T value, Action<IEntity> check) where T : Enum
        {
            Type enumType = typeof(T);

            var em = Database.Metadata.Get(enumType.Name);

            int id = Convert.ToInt32(value);

            IEntity ent = scope.GetQuery(em).Find(id).Result;
            check(ent);
            ent.Save();
        }
    }
}
