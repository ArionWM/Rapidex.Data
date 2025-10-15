using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;


namespace Rapidex;

public static class AssertionHelper
{
    //see https://stackoverflow.com/questions/361468/can-net-source-code-hard-code-a-debugging-breakpoint
    [DebuggerHidden]
    [Conditional("DEBUG")]
    public static void DebugBreak()
    {
        if (System.Diagnostics.Debugger.IsAttached)
            System.Diagnostics.Debugger.Break();
    }

    //see https://stackoverflow.com/questions/61384377/how-to-suppress-possible-null-reference-warnings
    public static T NotNull<T>([NotNull] this T obj, string? message = null)
    {
        if (obj == null)
        {
            DebugBreak();
            throw new ArgumentNullException(message ?? "Object is null");
        }

        return obj;
    }

    public static T NotNull<T, E>([NotNull] this T obj, string? message = null) where E : Exception, new()
    {
        if (obj == null)
        {
            var exception = new E();
            if (!string.IsNullOrEmpty(message))
            {
                var messageField = typeof(E).GetField("_message", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic);
                if (messageField != null)
                {
                    messageField.SetValue(exception, message);
                }
            }
            DebugBreak();
            throw exception;
        }

        return obj;
    }

    public static T NotEmpty<T>([NotNull] this T obj, string? message = null)
    {
        obj.NotNull(message);

        if (obj.IsNullOrEmpty())
        {
            DebugBreak();
            throw new ArgumentNullException(message ?? "Object is empty");
        }

        return obj;
    }

    public static T ShouldBeSuccess<T>(this T obj, string? message = null) where T : IResult
    {
        obj.NotNull(message);

        if (!obj.Success)
        {
            DebugBreak();
            throw new BaseValidationException(message ?? "Result fail");
        }

        return obj;
    }

    public static TObj ShouldSupportTo<TObj>(this object obj, string? message = null)
    {
        message = message ?? $"Object does not support to {typeof(TObj).Name}, obj type is '{obj?.GetType().Name}'";

        if (!obj.IsSupportTo(typeof(TObj)))
        {
            DebugBreak();
            throw new BaseValidationException(message ?? $"Object '{obj}' is not support to {typeof(TObj).Name}");
        }

        return (TObj)obj;
    }

    public static TObj NotZero<TObj>(this TObj obj, string? message = null) where TObj : struct, IComparable
    {
        if (obj.CompareTo(default(TObj)) == 0)
        {
            DebugBreak();
            throw new BaseValidationException(message ?? "Object is zero");
        }
        return obj;
    }   

    public static Type ShouldSupportTo(this Type type, Type desiredType, string? message = null)
    {
        message = message ?? $"Type does not support to {desiredType.Name}, type is '{type.Name}'";

        if (!type.IsSupportTo(desiredType))
        {
            DebugBreak();
            throw new BaseValidationException(message ?? $"Type '{type.Name}' is not support to {desiredType.Name}");
        }
        return type;
    }

    public static Type ShouldSupportTo<TType>(this Type type, string? message = null)
    {
        message = message ?? $"Type does not support to {typeof(TType).Name}, type is '{type.Name}'";

        Type desiredType = typeof(TType);
        type.ShouldSupportTo(desiredType, message);
        return type;
    }

    public static void ShouldNotSupportTo<TObj>(this object obj, string? message = null)
    {
        message = message ?? $"Object does support to {typeof(TObj).Name}, obj type is '{obj?.GetType().Name}'";

        if (obj.IsSupportTo(typeof(TObj)))
        {
            DebugBreak();
            throw new BaseValidationException(message);
        }

    }

    public static T MustBe<T>(this T obj, Func<T, bool> clause, string? message = null)
    {
        obj.NotNull("Object is null");
        clause.NotNull("Clause is null");

        bool result = clause.Invoke(obj);
        if (!result)
        {
            DebugBreak();
            throw new BaseValidationException(message ?? string.Format("Object is not valid"));
        }

        return obj;
    }

    public static T ShouldEquals<T>(this T obj, T other, string? message = null)
    {
        if (!object.Equals(obj, other))
        {
            DebugBreak();
            throw new BaseValidationException(message ?? string.Format("Object '{0}' is not equal to '{1}'", obj, other));
        }
        return obj;
    }
}
