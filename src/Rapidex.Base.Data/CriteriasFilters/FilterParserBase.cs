using Superpower.Parsers;
using Superpower;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Superpower.Tokenizers;
using Superpower.Display;

namespace Rapidex.Data;


//https://github.com/zzzprojects/System.Linq.Dynamic.Core

internal class FilterParserBase
{
    internal static Tokenizer<FilterTokens> tokenizer = new TokenizerBuilder<FilterTokens>()
        .Ignore(Span.WhiteSpace)
    .Match(Span.EqualTo("()"), FilterTokens.OpenCloseParen)
    .Match(Span.EqualTo("[]"), FilterTokens.OpenCloseParen)
    .Match(Character.EqualTo('('), FilterTokens.LParen)
    .Match(Character.EqualTo(')'), FilterTokens.RParen)
    .Match(Character.EqualTo('['), FilterTokens.LParen)
    .Match(Character.EqualTo(']'), FilterTokens.RParen)
    .Match(Character.EqualTo(','), FilterTokens.Comma)

    .Match(Span.EqualTo("or"), FilterTokens.Or)
    .Match(Character.EqualTo('|'), FilterTokens.Or)
    .Match(Span.EqualTo("and"), FilterTokens.And)
    .Match(Character.EqualTo('&'), FilterTokens.And)

    .Match(Character.EqualTo('~'), FilterTokens.Like)
    .Match(Span.EqualTo("like"), FilterTokens.Like)

    .Match(Character.EqualTo('='), FilterTokens.Equal)
    .Match(Span.EqualTo("eq"), FilterTokens.Equal)
    .Match(Span.EqualTo("=="), FilterTokens.Equal)

    .Match(Span.EqualTo("!="), FilterTokens.NotEqual)
    .Match(Span.EqualTo("<>"), FilterTokens.NotEqual)
    .Match(Span.EqualTo("noteq"), FilterTokens.NotEqual)
    .Match(Span.EqualTo("ne"), FilterTokens.NotEqual)


    .Match(Character.EqualTo('<'), FilterTokens.LessThan)
    .Match(Span.EqualTo("lt"), FilterTokens.LessThan)

    .Match(Character.EqualTo('>'), FilterTokens.GreaterThan)
    .Match(Span.EqualTo("gt"), FilterTokens.GreaterThan)

    .Match(Span.EqualTo("<="), FilterTokens.LessThanOrEqual)
    .Match(Span.EqualTo("lte"), FilterTokens.LessThanOrEqual)

    .Match(Span.EqualTo(">="), FilterTokens.GreaterThanOrEqual)
    .Match(Span.EqualTo("gte"), FilterTokens.GreaterThanOrEqual)

    .Match(Span.EqualTo(":="), FilterTokens.In)
    .Match(Span.EqualTo("in"), FilterTokens.In)
    .Match(Span.EqualTo("contains"), FilterTokens.In)
    .Match(Span.EqualTo("anyof"), FilterTokens.In)

    .Match(Span.EqualTo("notin"), FilterTokens.NotIn)
    .Match(Span.EqualTo("!="), FilterTokens.NotIn)

    .Match(Span.EqualTo("not"), FilterTokens.Not)
    .Match(Character.EqualTo('!'), FilterTokens.Not)

    .Match(Span.EqualTo("between"), FilterTokens.Between)

    .Match(Character.EqualTo(' '), FilterTokens.Whitespace)

    .Match(Character.LetterOrDigit
        //.Or(Character.Digit)
        .Or(Character.EqualTo('*'))
        .Or(Character.EqualTo('/'))
        .AtLeastOnce(), FilterTokens.String)
    //.Match(Span.MatchedBy(Character.AnyChar), MyTokens.String)
    //.Match(Numerics.Natural,MyTokens.Number)
    .Build();

    private static readonly TokenListParser<FilterTokens, string> IdentifierParser =
           Token.EqualTo(FilterTokens.String)
               .Select(t => t.ToStringValue());

    private static readonly TokenListParser<FilterTokens, string[]> IdentifierListParser =
      IdentifierParser.ManyDelimitedBy(Token.EqualTo(FilterTokens.Comma))
          .Select(ids => ids.ToArray());

    private static readonly TokenListParser<FilterTokens, FilterTokens> NotOperatorParser =
    Token.EqualTo(FilterTokens.Not).Select(t => t.Kind);

    private static readonly TokenListParser<FilterTokens, FilterTokens> AndOperatorParser =
    Token.EqualTo(FilterTokens.And).Select(t => t.Kind);

    private static readonly TokenListParser<FilterTokens, FilterTokens> OrOperatorParser =
        Token.EqualTo(FilterTokens.Or).Select(t => t.Kind);

    private static readonly TokenListParser<FilterTokens, FilterExpression> NotExpressionParser =
        from op in NotOperatorParser
        from expr in Superpower.Parse.Ref(() => Factor!)
        select (FilterExpression)new FilterUnaryExpression(op, expr);

    private static readonly TokenListParser<FilterTokens, FilterTokens> ComparisonOperatorParser =
    Token
        .EqualTo(FilterTokens.Equal).Select(t => FilterTokens.Equal)
        .Or(Token.EqualTo(FilterTokens.Like).Select(t => t.Kind))
        .Or(Token.EqualTo(FilterTokens.In).Select(t => t.Kind))
        .Or(Token.EqualTo(FilterTokens.GreaterThanOrEqual).Select(t => t.Kind))
        .Or(Token.EqualTo(FilterTokens.LessThanOrEqual).Select(t => t.Kind))
        .Or(Token.EqualTo(FilterTokens.GreaterThan).Select(t => t.Kind))
        .Or(Token.EqualTo(FilterTokens.LessThan).Select(t => t.Kind)
        );

    private static readonly TokenListParser<FilterTokens, FilterExpression> ComparisonParser =
           (from left in IdentifierParser
            from op in ComparisonOperatorParser
            from right in IdentifierListParser
            select (FilterExpression)new FilterComparisonExpression(left, op, right));


    private static readonly TokenListParser<FilterTokens, FilterExpression> Factor =
         ComparisonParser
            .Or(NotExpressionParser)
            .Or(Superpower.Parse.Ref(() => ParenExpressionParser!))
            .Named("factor");

    private static readonly TokenListParser<FilterTokens, FilterExpression> ParenExpressionParser =
            from lparen in Token.EqualTo(FilterTokens.LParen)
            from expr in Superpower.Parse.Ref(() => ExpressionParser!)
            from rparen in Token.EqualTo(FilterTokens.RParen)
            select expr;

    private static readonly TokenListParser<FilterTokens, FilterExpression> ExpressionParser =
        Superpower.Parse.Chain(AndOperatorParser.Or(OrOperatorParser), Factor,
            (op, left, right) => new FilterBinaryExpression(left, op, right));


    public FilterExpression ParseToExpression(string input)
    {
        var tokens = FilterParserBase.tokenizer.Tokenize(input);
        var result = FilterParserBase.ExpressionParser.Parse(tokens);
        return result;
    }



    /// <summary>
    /// Predefined value checker
    /// </summary>
    /// <param name="query"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    protected object CheckValue(string value, IDbFieldMetadata? fm)
    {
        value = value?.Trim();
        object _value = value;



        switch (value)
        {
            case "true":
                _value = true;
                break;
            case "false":
                _value = false;
                break;
            case "today":
                _value = DateTimeOffset.Now.ToString("yyyy-MM-dd");
                break;
            case "now":
                _value = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:ss");
                break;
            case "yesterday":
                _value = DateTimeOffset.Now.AddDays(-1).ToString("yyyy-MM-dd");
                break;
            case "tomorrow":
                _value = DateTimeOffset.Now.AddDays(1).ToString("yyyy-MM-dd");
                break;
            case "thisweek":
                _value = DateTimeOffset.Now.WeekStart().ToString("yyyy-MM-dd");
                break;
            case "lastweek":
                _value = DateTimeOffset.Now.AddDays(-7).WeekStart().ToString("yyyy-MM-dd");
                break;
            case "nextweek":
                _value = DateTimeOffset.Now.AddDays(7).WeekStart().ToString("yyyy-MM-dd");
                break;
            case "thismonth":
                _value = DateTimeOffset.Now.MonthStart().ToString("yyyy-MM-dd");
                break;
            case "lastmonth":
                _value = DateTimeOffset.Now.AddMonths(-1).MonthStart().ToString("yyyy-MM-dd");
                break;
            case "nextmonth":
                _value = DateTimeOffset.Now.AddMonths(1).MonthStart().ToString("yyyy-MM-dd");
                break;
            case "thisyear":
                _value = DateTimeOffset.Now.YearStart().ToString("yyyy-MM-dd");
                break;
            case "lastyear":
                _value = DateTimeOffset.Now.AddYears(-1).YearStart().ToString("yyyy-MM-dd");
                break;
            case "nextyear":
                _value = DateTimeOffset.Now.AddYears(1).YearStart().ToString("yyyy-MM-dd");
                break;

            case "user":
                throw new NotImplementedException();
                break;
        }

        if (fm != null && fm.Type.IsSupportTo<IDataType>())
        {
            if (value != null && !value.IsSupportTo(fm.Type))
            {
                IDataType dt = (IDataType)EntityMapper.EnsureValueType(fm, null, value, false);
                _value = dt.GetValueLower();
            }
        }

        return _value;
    }


    // Nested vs. desteklemez, daha sonra değişecek

    protected IDbFieldMetadata CheckLeft(IDbEntityMetadata em, FilterComparisonExpression fce)
    {
        string leftValue = fce.Left;

        IDbFieldMetadata fm = null;
        switch (leftValue.ToLowerInvariant())
        {
            case "caption":
                fm = em.Caption;
                if (fm == null)
                {
                    throw new InvalidOperationException($"{em.Name} has no caption field");
                }
                return fm;
        }

        fm = em.Fields.Get(leftValue, false);
        if (fm == null)
        {
            //TODO: Check nested or ?

            throw new InvalidOperationException($"Field not found with '{leftValue}' on filter part: '{fce}'");
        }
        return fm;
    }


}


