using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex
{
    public static class EnumHelper
    {
        //https://stackoverflow.com/questions/1799370/getting-attributes-of-enums-value
        public static T GetAttribute<T>(this Enum enumVal) where T : System.Attribute
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());
            if (memInfo.Length == 0) //Flags kullanımında memInfo bulunamıyor
                return null;

            var attributes = memInfo[0].GetCustomAttributes(typeof(T), false);
            return (attributes.Length > 0) ? (T)attributes[0] : null;
        }

        public static bool IsExists<T>(this Enum enumVal) where T : System.Attribute //TODO: Cache !!!
        {
            T attr = enumVal.GetAttribute<T>();
            return attr != null;
        }

        //https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
        public static T Next<T>(this T v) where T : struct, Enum
        {
            return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).SkipWhile(e => !v.Equals(e)).Skip(1).First();
        }

        //https://stackoverflow.com/questions/642542/how-to-get-next-or-previous-enum-value-in-c-sharp
        public static T Previous<T>(this T v) where T : struct, Enum
        {
            return Enum.GetValues(v.GetType()).Cast<T>().Concat(new[] { default(T) }).Reverse().SkipWhile(e => !v.Equals(e)).Skip(1).First();
        }
    }
}
