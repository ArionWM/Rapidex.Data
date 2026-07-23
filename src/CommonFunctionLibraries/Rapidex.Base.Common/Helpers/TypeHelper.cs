using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Rapidex.Base;
using Rapidex.Base.Common.Assemblies;
using Rapidex.Collections;

namespace Rapidex;

public static class TypeHelper
{
    readonly static TwoLevelDictionary2<Type, string, PropertyInfo> Properties = new TwoLevelDictionary2<Type, string, PropertyInfo>(null);
    readonly static TwoLevelList<Type, Type> BaseTypes = new TwoLevelList<Type, Type>();
    readonly static TwoLevelDictionary2<Type, Type, bool?> SupportToCache = new TwoLevelDictionary2<Type, Type, bool?>(null);

    public readonly static Type Type_Int = typeof(int);
    public readonly static Type Type_String = typeof(string);
    public readonly static Type Type_DateTime = typeof(DateTime);
    public readonly static Type Type_DateTimeOffset = typeof(DateTimeOffset);
    public readonly static Type Type_TimeSpan = typeof(TimeSpan);
    public readonly static Type Type_Boolean = typeof(bool);
    public readonly static Type Type_Guid = typeof(Guid);
    public readonly static Type Type_Decimal = typeof(decimal);
    public readonly static Type Type_Double = typeof(double);
    public readonly static Type Type_Float = typeof(float);
    public readonly static Type Type_Long = typeof(long);
    public readonly static Type Type_UInt = typeof(uint);
    public readonly static Type Type_Byte = typeof(byte);
    public readonly static Type Type_ByteArray = typeof(byte[]);
    public readonly static Type Type_XmlNode = typeof(System.Xml.XmlNode);
    public readonly static Type Type_FloatArray = typeof(float[]);

    static TypeHelper()
    {

    }


    public static bool IsSupportTo<T>(this Type type)
    {
        if (type == null)
            throw new BaseArgumentNullException("type");

        Type targetType = typeof(T);

        if (targetType == type)
        {
            return true;
        }

        bool? cachedResult = SupportToCache?.Get(type, targetType);
        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        bool result = targetType.IsAssignableFrom(type);
        SupportToCache?.Set(type, targetType, result);

        return result;
    }

    public static bool IsSupportTo(this Type type, Type targetType)
    {
        if (type == null)
            throw new BaseArgumentNullException("type");

        if (targetType == type)
        {
            return true;
        }

        bool? cachedResult = SupportToCache?.Get(type, targetType);
        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        bool result = targetType.IsAssignableFrom(type);
        SupportToCache?.Set(type, targetType, result);

        return result;
    }

    public static bool IsSupportTo(this object obj, Type targetType)
    {
        Type type = obj?.GetType();
        if (type == null)
            throw new BaseArgumentNullException("type");

        if (targetType == type)
        {
            return true;
        }

        bool? cachedResult = SupportToCache?.Get(type, targetType);
        if (cachedResult.HasValue)
        {
            return cachedResult.Value;
        }

        bool result = targetType.IsAssignableFrom(type);
        SupportToCache?.Set(type, targetType, result);

        return result;
    }

    public static bool IsSupportTo<T>(this object obj)
    {
        Type targetType = typeof(T);
        Type type = obj?.GetType();

        return IsSupportTo(type, targetType);
    }


    public static object CreateInstance(Type type, params object[] parameters)
    {
        //TODO: Remember choice and use future ...

        if (type.IsValueType)
        {
            return Activator.CreateInstance(type);
        }

        if (Rapidex.Common.InternalServiceProvider == null)
        {
            //Henüz sistem ayağa kalkmamış
            return Activator.CreateInstance(type, parameters);
        }

        if (!parameters.Any() && type.GetConstructors().Any(c => !c.GetParameters().Any()))
            return Activator.CreateInstance(type);

        if (type.ContainsGenericParameters)
        {
            Type[] genericTypes = type.GenericTypeArguments;
            Type genericType = type.GetGenericTypeDefinition();
            Type reqType = genericType.MakeGenericType(genericTypes);
            return ActivatorUtilities.CreateInstance(Common.ServiceProvider, reqType, parameters);
            //return Activator.CreateInstance(reqType, parameters);
        }
        else
        {
            return ActivatorUtilities.CreateInstance(Common.ServiceProvider, type, parameters);
            //return Activator.CreateInstance(type, parameters);
        }
    }

    public static T CreateInstance<T>(Type type, params object[] parameters)
    {
        return (T)CreateInstance(type, parameters);
    }

    public static T CreateInstance<T>(params object[] parameters)
    {
        return (T)CreateInstance(typeof(T), parameters);
    }

    public static object CreateInstanceWithDI(this IServiceProvider sp, Type type, params object[] parameters)
    {
        if (type.ContainsGenericParameters)
        {
            Type[] genericTypes = type.GenericTypeArguments;
            Type genericType = type.GetGenericTypeDefinition();
            Type reqType = genericType.MakeGenericType(genericTypes);
            return ActivatorUtilities.CreateInstance(sp, reqType, parameters);
        }
        else
        {
            return ActivatorUtilities.CreateInstance(sp, type, parameters);
        }
    }

    public static object CreateInstanceWithDI(Type type, params object[] parameters)
    {
        return CreateInstanceWithDI(Common.ServiceProvider, type, parameters);
    }

    public static T CreateInstanceWithDI<T>(this IServiceProvider sp, Type type, params object[] parameters)
    {
        return (T)CreateInstanceWithDI(sp, type, parameters);
    }

    public static T CreateInstanceWithDI<T>(Type type, params object[] parameters)
    {
        return (T)CreateInstanceWithDI(Common.ServiceProvider, type, parameters);
    }

    public static T CreateInstanceWithDI<T>(this IServiceProvider sp, params object[] parameters)
    {
        return (T)CreateInstanceWithDI(sp, typeof(T), parameters);
    }

    public static T CreateInstanceWithDI<T>(params object[] parameters)
    {
        return (T)CreateInstanceWithDI(Common.ServiceProvider, typeof(T), parameters);
    }

    public static PropertyInfo GetPropertyCached(this Type type, string propertyName)
    {
        var propDict = Properties.Get(type);

        propDict.TryGetValue(propertyName, out PropertyInfo propInfo);
        if (propInfo == null)
        {
            propInfo = type.GetProperty(propertyName, BindingFlags.Instance | BindingFlags.Public);
            if (propInfo != null)
                Properties.Set(type, propertyName, propInfo);

        }

        return propInfo;
    }

    public static IList<Type> GetBaseTypesChainCached(this Type type, bool includeInterfaces)
    {
        if (BaseTypes.ContainsKey(type) && BaseTypes[type] != null)
        {
            return BaseTypes[type];
        }

        List<Type> list = new List<Type>();
        Type bType = type.BaseType;
        while (bType != null)
        {
            list.Add(bType);
            bType = bType.BaseType;
        }
        BaseTypes.Set(type, list);

        if (includeInterfaces)
        {
            Type[] interfaces = type.GetInterfaces();
            foreach (Type iface in interfaces)
            {
                if (!list.Contains(iface))
                    list.Add(iface);
            }
        }
        return list;
    }

    public static string ToStringAdv(this Type type)
    {
        if (type == null)
            return string.Empty;

        StringBuilder sb = new StringBuilder();

        if (type.IsGenericType)
        {
            sb.Append(type.BaseType.Name);
            sb.Append("<");
            Type[] genericTypes = type.GenericTypeArguments;
            for (int i = 0; i < genericTypes.Length; i++)
            {
                Type gType = genericTypes[i];
                sb.Append(gType.Name);
                if (i < genericTypes.Length - 1)
                    sb.Append(",");
            }
            sb.Append(">");
        }
        else
        {
            switch (type.Name)
            {
                case "Void":
                    sb.Append("void");
                    break;
                default:
                    sb.Append(type.Name);
                    break;
            }
        }
        return sb.ToString();
    }

    public static Type StripNullable(this Type type)
    {
        Type undType = Nullable.GetUnderlyingType(type);
        if (undType != null)
            return undType;

        return type;
    }
}
