using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Rapidex;

public class RapidexTypeConverter : IManager
{
    private static readonly TwoLevelDictionary<Type, Type, IDirectConverter> DirectConverters = new();
    private static readonly ConcurrentBag<ICustomTypeConverter> CustomTypeConverters = new();
    private static readonly TwoLevelDictionary<Type, Type, ICustomTypeConverter> CustomTypeConvertersCache = new();


    static RapidexTypeConverter()
    {
        RapidexTypeConverter.RegisterInternal<DateTimeOffsetConverter>();
        RapidexTypeConverter.RegisterInternal<DateTimeOffsetNullableConverter>();
        RapidexTypeConverter.RegisterInternal<DateTimeOffsetToDateTimeConverter>();
        RapidexTypeConverter.RegisterInternal<DateTimeOffsetNullableToDateTimeNullableConverter>();
        RapidexTypeConverter.RegisterInternal<DateTimeOffsetStrConverter>();
    }

    internal static void RegisterInternal(IDirectConverter converter)
    {
        DirectConverters.Set(converter.FromType, converter.ToType, converter);
    }

    internal static void RegisterInternal<T>() where T : IDirectConverter, new()
    {
        IDirectConverter converter = (IDirectConverter)TypeHelper.CreateInstance(typeof(T));
        RegisterInternal(converter);
    }

    public void Register(IDirectConverter converter)
    {
        DirectConverters.Set(converter.FromType, converter.ToType, converter);
    }

    public static void Register<T>()
        where T : IDirectConverter, new()
    {
        RegisterInternal<T>();
    }

    public static void RegisterTypeConverter<T>()
        where T : TypeConverter, new()
    {
        TypeConverter converter = (TypeConverter)TypeHelper.CreateInstance(typeof(T));
        ICustomTypeConverter ctConverter = converter.ShouldSupportTo<ICustomTypeConverter>();
        CustomTypeConverters.Add(ctConverter);
        CustomTypeConvertersCache.Clear();
    }

    private static TypeConverter FindCustomTypeConverter(Type fromType, Type toType)
    {
        ICustomTypeConverter conv = CustomTypeConvertersCache.Get(fromType)?.Get(toType);
        if (conv != null)
        {
            TypeConverter res = conv.ShouldSupportTo<TypeConverter>();
            return res;
        }

        foreach (var converter in CustomTypeConverters)
        {
            if (converter.CanConvert(fromType, toType) )
            {
                CustomTypeConvertersCache.Set(fromType, toType, converter);
                TypeConverter res = converter.ShouldSupportTo<TypeConverter>();
                return res;
            }
        }

        //customTypeConvertersCache.Set(fromType, toType, null);
        return null;
    }


    [Obsolete("Use TryConvert instead. This method will be removed in future versions.")]
    public object Convert(object value, Type targetType)
    {
        try
        {
            if (value == null || value is DBNull)
            {
                if (targetType.IsValueType)
                    return TypeHelper.CreateInstance(targetType);
                return null;
            }

            targetType = targetType.StripNullable();

            if (value.IsSupportTo(targetType))
            {
                return value;
            }

            if (targetType.IsSupportTo<string>())
            {
                string strVal = (string)value;
                value = strVal;
            }

            object convertedValue = default;

            if (targetType.IsEnum)
            {
                if (value is string strVal)
                {
                    value = Enum.Parse(targetType, strVal);
                    return value;
                }
                //else
                //{
                //    value = Enum.ToObject(targetType, value);
                //}
                //return value;
            }

            var fromType = value.GetType();

            var converter = DirectConverters.Get(fromType, targetType);
            if (converter != null)
            {
                return converter.Convert(value, targetType);
            }

            var customTC = FindCustomTypeConverter(fromType, targetType);
            if (customTC != null)
            {
                return customTC.ConvertTo(value, targetType);
            }

            Type sourceType = value.GetType();
            //https://stackoverflow.com/questions/312858/how-can-i-convert-types-at-runtime/312898
            System.ComponentModel.TypeConverter conv = TypeDescriptor.GetConverter(targetType);
            if (conv.CanConvertFrom(sourceType))
            {
                convertedValue = conv.ConvertFrom(value);
            }
            else
            {
                conv = TypeDescriptor.GetConverter(value);


                if (conv.CanConvertTo(targetType))
                {
                    convertedValue = conv.ConvertTo(value, targetType);
                }
                else
                {
                    convertedValue = System.Convert.ChangeType(value, targetType);
                }
            }

            return convertedValue;
        }
        catch (InvalidCastException ice)
        {
            string message = $"Invalid cast from {value.GetType().Name} to {targetType.Name}";
            throw new InvalidCastException(message, ice);
        }
    }

    public bool TryConvert(object value, Type targetType, out object convertedValue)
    {

        if (value == null || value is DBNull)
        {
            convertedValue = null;
            if (targetType.IsValueType)
                convertedValue = TypeHelper.CreateInstance(targetType);
            return true;
        }

        targetType = targetType.StripNullable();

        if (value.IsSupportTo(targetType))
        {
            convertedValue = value;
        }

        if (targetType.IsSupportTo<string>())
        {
            string strVal = (string)value;
            value = strVal;
        }

        convertedValue = null;

        try
        {
            if (targetType.IsEnum)
            {
                if (value is string strVal)
                {
                    value = Enum.TryParse(targetType, strVal, true, out convertedValue);
                }
                else
                {
                    convertedValue = Enum.ToObject(targetType, value);
                }

                return true;
            }


            var fromType = value.GetType();

            var converter = DirectConverters.Get(fromType, targetType);
            if (converter != null)
            {
                return converter.TryConvert(value, targetType, out convertedValue);
            }

            Type sourceType = value.GetType();
            //https://stackoverflow.com/questions/312858/how-can-i-convert-types-at-runtime/312898
            System.ComponentModel.TypeConverter conv = TypeDescriptor.GetConverter(targetType);

            if (conv.CanConvertFrom(sourceType))
            {
                convertedValue = conv.ConvertFrom(value);
                return true;
            }
            else
            {
                conv = TypeDescriptor.GetConverter(value);


                if (conv.CanConvertTo(targetType))
                {
                    convertedValue = conv.ConvertTo(value, targetType);
                }
                else
                {
                    convertedValue = System.Convert.ChangeType(value, targetType);
                }
                return true;
            }

        }
        catch (InvalidCastException ice)
        {
            ice.Log();
            return false;
            //string message = $"Invalid cast from {value.GetType().Name} to {targetType.Name}";
            //throw new InvalidCastException(message, ice);
        }
    }


    public void Setup(IServiceCollection services)
    {
        Type[] types = Common.Assembly.FindDerivedClassTypes(typeof(IDirectConverter));
        foreach (var type in types)
        {
            //IBaseConverter converter = (IBaseConverter)TypeHelper.CreateInstance(type);
            //Register(converter);
        }
    }

    public void Start(IServiceProvider serviceProvider)
    {

    }
}


public abstract class ConverterBase : IDirectConverter
{
    public abstract Type FromType { get; }
    public abstract Type ToType { get; }

    public abstract object Convert(object from, Type toType);
    public abstract bool TryConvert(object from, Type toType, out object to);
}

public abstract class ConverterBase<TFrom, TTo> : ConverterBase, IDirectConverter<TFrom, TTo>
{
    public override Type FromType => typeof(TFrom);
    public override Type ToType => typeof(TTo);

    public virtual TTo Convert(TFrom from, TTo to)
    {
        return (TTo)this.Convert(from, typeof(TTo));
    }

    public virtual bool TryConvert(TFrom from, Type toType, out TTo to)
    {
        object obj = null;
        bool result = this.TryConvert(from, toType, out obj);
        to = (TTo)obj;
        return result;
    }
}
