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



internal class FilterTextParser : FilterParserBase, IDbCriteriaParser
{
  
    public bool IsYours(string filterOrSqlClause)
    {
        return true;
    }


    // Nested vs. desteklemez, daha sonra değişecek

    protected void ParseFieldExpression(IQueryCriteria query, FilterComparisonExpression fce)
    {
        string leftValue = fce.Left;
        IDbFieldMetadata fm = this.CheckLeft(query.EntityMetadata, fce);

        string fieldName = fm.Name;

        switch (fce.Operator)
        {
            case FilterTokens.Equal:
                string rVal = fce.Right.Trim().ToLower();
                if (rVal == "null")
                    rVal = null;
                query.Eq(fieldName, this.CheckValue(rVal, fm));
                break;
            case FilterTokens.Like:
                query.Like(fieldName, this.CheckValue(fce.Right, fm));
                break;
            case FilterTokens.In:
                query.In(fieldName, fce.RightArray);
                break;
            case FilterTokens.GreaterThan:
                query.Gt(fieldName, this.CheckValue(fce.Right, fm));
                break;
            case FilterTokens.GreaterThanOrEqual:
                query.GtEq(fieldName, this.CheckValue(fce.Right, fm));
                break;
            case FilterTokens.LessThan:
                query.Lt(fieldName, this.CheckValue(fce.Right, fm));
                break;
            case FilterTokens.LessThanOrEqual:
                query.LtEq(fieldName, this.CheckValue(fce.Right, fm));
                break;
            case FilterTokens.NotEqual:
                string rVal2 = fce.Right.Trim().ToLower();
                if (rVal2 == "null")
                    rVal2 = null;
                query.Not(q=> q.Eq(fieldName, this.CheckValue(rVal2, fm)));
                break;
            case FilterTokens.NotIn:
                query.Not(q => q.In(fieldName, fce.RightArray));
                break;
            default:
                throw new NotSupportedException($"Operator {fce.Operator} is not supported.");
        }
    }

    protected void ParseBetweenExpression(IQueryCriteria query, FilterBetweenExpression fbe)
    {
        IDbFieldMetadata fm = this.CheckLeft(query.EntityMetadata, fbe);
        string fieldName = fm.Name;

        object startValue = this.CheckValue(fbe.StartValue, fm);
        object endValue = this.CheckValue(fbe.EndValue, fm);

        query.And(
            q => q.GtEq(fieldName, startValue),
            q => q.LtEq(fieldName, endValue));
    }

    protected void ParseRelationExpression(IQueryCriteria query, FilterComparisonExpression fce)
    {
        if (fce.Operator != FilterTokens.Equal)
            throw new NotSupportedException($"Only 'equal' / '=' operator supported for relation expressions ({fce.Operator} invalid)");

        //Related = EntityA/123/MyField

        string relativeNav = fce.Right.Trim().TrimStart('/').TrimEnd('/');
        //Buradan navigation'a erişilemiyor...
        string[] parts = relativeNav.Split('/');
        if (parts.Length != 3)
            throw new InvalidOperationException($"Invalid relation expression '{fce}'");

        string entityName = parts[0];
        string entityId = parts[1];
        string fieldName = parts[2];

        var em = query.Schema.ParentDbScope.Metadata.Get(entityName);
        em.NotNull($"Metadata for '{entityName}' not found");

        var fm = em.Fields.Get(fieldName, false);
        fm.NotNull($"Field '{fieldName}' not found in '{entityName}' metadata");

        IEntity parentEntity = query.Schema.Find(em, entityId.As<long>());
        parentEntity.NotNull($"Entity '{entityName}' with id '{entityId}' not found");

        query.Related(parentEntity, fieldName);
    }

    protected void Parse(IQueryCriteria query, FilterComparisonExpression fce)
    {
        string leftValue = fce.Left;

        switch (leftValue)
        {
            case "relation":
            case "releated":
            case "related":
                this.ParseRelationExpression(query, fce);
                break;

            case "caption":
            default:
                this.ParseFieldExpression(query, fce);
                return;

        }

    }

    protected void Parse(IQueryCriteria query, FilterBetweenExpression fbe)
    {
        this.ParseBetweenExpression(query, fbe);
    }

    protected void Parse(IQueryCriteria query, FilterBinaryExpression fce)
    {
        switch (fce.Operator)
        {
            case FilterTokens.And:
                query.And(
                    c => this.Parse(c, fce.Left),
                    c => this.Parse(c, fce.Right));
                break;
            case FilterTokens.Or:
                query.Or(
                    c => this.Parse(c, fce.Left),
                    c => this.Parse(c, fce.Right));
                break;
            default:
                throw new NotSupportedException($"Operator {fce.Operator} is not supported.");
        }
    }

    protected void Parse(IQueryCriteria query, FilterUnaryExpression fce)
    {
        switch (fce.Operator)
        {
            case FilterTokens.Not:
                query.Not(c => this.Parse(c, fce.Operand));
                break;
            default:
                throw new NotSupportedException($"Operator {fce.Operator} is not supported.");
        }
    }
    protected void Parse(IQueryCriteria query, FilterExpression filterExpression)
    {
        switch (filterExpression)
        {
            case FilterComparisonExpression comparison:
                this.Parse(query, comparison);
                break;
            case FilterBetweenExpression between:
                this.Parse(query, between);
                break;
            case FilterBinaryExpression binary:
                this.Parse(query, binary);
                break;
            case FilterUnaryExpression unary:
                this.Parse(query, unary);
                break;
            default:
                throw new NotSupportedException($"Filter expression type {filterExpression.GetType()} is not supported.");
        }
    }

    public IQueryCriteria Parse(IQueryCriteria query, string filterOrSqlClause)
    {
        FilterExpression filterExpression = this.ParseToExpression(filterOrSqlClause);
        this.Parse(query, filterExpression);

        return query;
    }

}


