using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rapidex.Data;

internal abstract class FilterExpression
{

    public override string ToString()
    {
        return this.ExpressionToString();
    }
}


internal class FilterComparisonExpression : FilterExpression
{
    public string Left { get; }
    public FilterTokens Operator { get; }
    public string Right => string.Join(",", RightArray);
    public bool IsMultipleRight => RightArray.Length > 1;
    public string[] RightArray { get; }

    public FilterComparisonExpression(string left, FilterTokens op, string[] rightArray)
    {
        Left = left;
        Operator = op;
        RightArray = rightArray ?? new string[0];
    }

    public override string ToString()
    {
        return $"{Left} {Operator} {string.Join(", ", RightArray)}";
    }
}

internal class FilterBetweenExpression : FilterExpression
{
    public string Field { get; }
    public string StartValue { get; }
    public string EndValue { get; }

    public FilterBetweenExpression(string field, string startValue, string endValue)
    {
        Field = field;
        StartValue = startValue;
        EndValue = endValue;
    }

    public override string ToString()
    {
        return $"{Field} between {StartValue} and {EndValue}";
    }
}

internal class FilterBinaryExpression : FilterExpression
{
    public FilterExpression Left { get; }
    public FilterTokens Operator { get; }
    public FilterExpression Right { get; }
    public FilterBinaryExpression(FilterExpression left, FilterTokens op, FilterExpression right)
    {
        Left = left;
        Operator = op;
        Right = right;
    }

    public override string ToString()
    {
        return $"{Left.ToString().Crop(20, true)} >> {Operator} << {Right.ToString().Crop(20, true)}";
    }
}

internal class FilterUnaryExpression : FilterExpression
{
    public FilterTokens Operator { get; }
    public FilterExpression Operand { get; }
    public FilterUnaryExpression(FilterTokens op, FilterExpression operand)
    {
        Operator = op;
        Operand = operand;
    }

    public override string ToString()
    {
        return $"{Operator} {Operand.ToString().Crop(20, true)}";
    }
}

internal static class FilterExpressionExtensions
{
    public static string ExpressionToString(this FilterExpression expr)
    {
        return expr switch
        {
            FilterComparisonExpression comp => comp.Operator == FilterTokens.In
                ? $"{comp.Left} {comp.Operator} {string.Join(", ", comp.RightArray)}"
                : $"{comp.Left} {comp.Operator} {comp.Right}",
            FilterBetweenExpression between => $"{between.Field} between {between.StartValue} and {between.EndValue}",
            FilterUnaryExpression unary => $"{unary.Operator}{ExpressionToString(unary.Operand)}",
            FilterBinaryExpression bin => $"({ExpressionToString(bin.Left)} {bin.Operator} {ExpressionToString(bin.Right)})",
            _ => throw new NotSupportedException("Unknown expression type")
        };
    }
}