using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Rapidex.Data.Linq;

/// <summary>
/// Evaluates constant and closure expressions to values for criteria binding.
/// </summary>
internal static class ValueEvaluator
{
    /// <summary>
    /// Resolves a value expression, then normalizes via EnsureLowerValue.
    /// </summary>
    /// <param name="expression">Expression that yields a bound value</param>
    public static object Evaluate(Expression expression)
    {
        object raw = EvaluateRaw(expression);
        return raw.EnsureLowerValue();
    }

    /// <summary>
    /// Evaluates an expression to an object array for In clauses.
    /// </summary>
    /// <param name="expression">Enumerable expression</param>
    public static object[] EvaluateToArray(Expression expression)
    {
        object raw = EvaluateRaw(expression);
        if (raw == null)
            return Array.Empty<object>();

        if (raw is object[] arr)
            return arr.Select(v => v.EnsureLowerValue()).ToArray();

        if (raw is IEnumerable enumerable and not string)
        {
            var list = new List<object>();
            foreach (object item in enumerable)
                list.Add(item.EnsureLowerValue());
            return list.ToArray();
        }

        return new[] { raw.EnsureLowerValue() };
    }

    static object EvaluateRaw(Expression expression)
    {
        expression = MemberPathResolver.StripConverts(expression);

        if (expression is ConstantExpression constant)
            return constant.Value;

        if (expression is MemberExpression member)
        {
            object instance = null;
            if (member.Expression != null)
                instance = EvaluateRaw(member.Expression);

            if (member.Member is FieldInfo field)
                return field.GetValue(instance);
            if (member.Member is PropertyInfo property)
                return property.GetValue(instance);
        }

        if (expression is NewArrayExpression newArray)
        {
            var items = new object[newArray.Expressions.Count];
            for (int i = 0; i < newArray.Expressions.Count; i++)
                items[i] = EvaluateRaw(newArray.Expressions[i]);
            return items;
        }

        if (expression is MethodCallExpression methodCall
            && methodCall.Method.DeclaringType == typeof(MemoryExtensions)
            && methodCall.Method.Name == nameof(MemoryExtensions.AsSpan))
        {
            // numbers.Contains(...) may rewrite to MemoryExtensions over AsSpan(array)
            return EvaluateRaw(methodCall.Arguments[0]);
        }

        try
        {
            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }
        catch (Exception ex)
        {
            throw new NotSupportedException($"Cannot evaluate expression '{expression}': {ex.Message}", ex);
        }
    }
}
