using System;
using System.Collections.Generic;
using System.Text;

namespace Rapidex.Data.Parsing;

[Obsolete("Use FilterTextParser", true)]
public class SimpleFlatCriteriaParser : IDbCriteriaParser
{
    static char[] likeCommands = { '*', '?' };
    static string[] operators = { "=", ">", ">=", "<", "<=", "[", "~", ":=" };

    protected void ParsePart(IQueryCriteria query, string @operator, string leftValue, string rightValue)
    {
        @operator = @operator.Trim();
        leftValue = leftValue.Trim();
        rightValue = rightValue.Trim();

        rightValue = rightValue.Replace('+', ' ');
        switch (@operator.ToLower())
        {
            case ":=":
                string[] rightParts = rightValue.Split(',');
                if (rightParts.Length != 2)
                    throw new NotSupportedException($"Operator := requires two values separated by comma.");
                query.Between(leftValue, rightParts.First(), rightParts.Last());
                break;
            case "=":

                if (rightValue.Any(c => likeCommands.Contains(c)))
                {
                    this.ParsePart(query, "~", leftValue, rightValue.Replace('*', '%').Replace('?', '_'));
                    return;
                }
                query.Eq(leftValue, rightValue);
                break;
            case ">":
                query.Gt(leftValue, rightValue);
                break;
            case ">=":
                query.GtEq(leftValue, rightValue);
                break;
            case "<":
                query.Lt(leftValue, rightValue);
                break;
            case "<=":
                query.LtEq(leftValue, rightValue);
                break;
            case "[":
                rightValue = rightValue.Trim('[', ']');
                query.In(leftValue, rightValue.Split(','));
                break;
            case "~":
                query.Like(leftValue, rightValue);
                break;
            default:
                throw new NotSupportedException($"Operator {@operator} is not supported.");
        }
    }

    protected void ParsePart(IQueryCriteria query, string part)
    {
        //part = part.Trim().Clear(' ');

        string @operator = operators.FirstOrDefault(opr => part.Contains(opr));
        if (@operator.IsNullOrEmpty())
            throw new NotSupportedException($"Operator not found in {part}.");
        string[] parts = part.Split(@operator, StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length != 2)
            throw new NotSupportedException($"Invalid part {part}.");

        this.ParsePart(query, @operator, parts.First(), parts.Last());
    }


    public IQueryCriteria Parse(IQueryCriteria query, string filterOrSqlClause)
    {
        if (filterOrSqlClause.IsNullOrEmpty())
            return query;

        //TODO: Paranthesis

        filterOrSqlClause = filterOrSqlClause.Replace(" and ", " & ").Replace(" or ", "|");
        filterOrSqlClause = filterOrSqlClause.Replace("&&", "&").Replace("||", "|");
        filterOrSqlClause = filterOrSqlClause.Replace(" like ", " ~ ");

        var parts = filterOrSqlClause.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries)
            .DistinctWithTrimElements();

        foreach (string part in parts)
            this.ParsePart(query, part);

        return query;

    }

    public bool IsYours(string filterOrSqlClause)
    {
        //
        return true;
    }
}
